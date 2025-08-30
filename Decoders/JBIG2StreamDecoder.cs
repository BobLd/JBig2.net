#region License
/**
* ===========================================
* Java Pdf Extraction Decoding Access Library
* ===========================================
*
* Project Info:  http://www.jpedal.org
* (C) Copyright 1997-2008, IDRsolutions and Contributors.
* Main Developer: Simon Barnett
*
* 	This file is part of JPedal
*
* Copyright (c) 2008, IDRsolutions
* All rights reserved.
*
* Redistribution and use in source and binary forms, with or without
* modification, are permitted provided that the following conditions are met:
*     * Redistributions of source code must retain the above copyright
*       notice, this list of conditions and the following disclaimer.
*     * Redistributions in binary form must reproduce the above copyright
*       notice, this list of conditions and the following disclaimer in the
*       documentation and/or other materials provided with the distribution.
*     * Neither the name of the IDRsolutions nor the
*       names of its contributors may be used to endorse or promote products
*       derived from this software without specific prior written permission.
*
* THIS SOFTWARE IS PROVIDED BY IDRsolutions ``AS IS'' AND ANY
* EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED
* WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE
* DISCLAIMED. IN NO EVENT SHALL IDRsolutions BE LIABLE FOR ANY
* DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES
* (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES;
* LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND
* ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT
* (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS
* SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
*
* Other JBIG2 image decoding implementations include
* jbig2dec (http://jbig2dec.sourceforge.net/)
* xpdf (http://www.foolabs.com/xpdf/)
* 
* The final draft JBIG2 specification can be found at http://www.jpeg.org/public/fcd14492.pdf
* 
* All three of the above resources were used in the writing of this software, with methodologies,
* processes and inspiration taken from all three.
*
* ---------------
* JBIG2StreamDecoder.java
* ---------------
*/
#endregion
using JBig2.Image;
using JBig2.Seg;
using JBig2.Seg.Extensions;
using JBig2.Seg.PageInformation;
using JBig2.Seg.Pattern;
using JBig2.Seg.Region.Generic;
using JBig2.Seg.Region.Halftone;
using JBig2.Seg.Region.Refinement;
using JBig2.Seg.Region.Text;
using JBig2.Seg.Stripes;
using JBig2.Seg.SymbolDictionary;
using JBig2.Util;
using System.Diagnostics;
using StreamReader = JBig2.IO.StreamReader;

namespace JBig2.Decoders
{
    public class JBIG2StreamDecoder
    {
        #region Variables and properties

        private StreamReader _reader;

        /// <summary>
        /// Number of pages known
        /// </summary>
        private bool _noOfPagesKnown;
        private bool _randomAccessOrganisation;

        private int _noOfPages = -1;

        private List<Segment> segments = new List<Segment>();
        private List<JBIG2Bitmap> bitmaps = new List<JBIG2Bitmap>();

        private Memory<byte>? globalData;

        private ArithmeticDecoder arithmeticDecoder;

        private HuffmanDecoder huffmanDecoder;

        private MMRDecoder mmrDecoder;

        internal ArithmeticDecoder ArithmeticDecoder { get { return arithmeticDecoder; } }

        public HuffmanDecoder HuffmanDecoder { get { return huffmanDecoder; } }

        public MMRDecoder MMRDecoder { get { return mmrDecoder; } }

        public List<Segment> Segments
        {
            get { return segments; }
        }

#if DEBUG
        public static bool DEBUG = false;
#endif

        #endregion

        #region Init

        #endregion

        public bool isNumberOfPagesKnown()
        {
            return _noOfPagesKnown;
        }

        public int getNumberOfPages()
        {
            return _noOfPages;
        }

        public void setGlobalData(Memory<byte> data)
        {
            globalData = data;
        }

        public void decodeJBIG2(Memory<byte> data)
        {
            _reader = new StreamReader(data);

            resetDecoder();

            bool validFile = checkHeader();
#if DEBUG
            if (JBIG2StreamDecoder.DEBUG)
                Debug.WriteLine("validFile = " + validFile);
#endif

            if (!validFile)
            {
                /**
                 * Assume this is a stream from a PDF so there is no file header,
                 * end of page segments, or end of file segments. Organisation must
                 * be sequential, and the number of pages is assumed to be 1.
                 */

                _noOfPagesKnown = true;
                _randomAccessOrganisation = false;
                _noOfPages = 1;

                /** check to see if there is any global data to be read */
                if (globalData.HasValue)
                {
                    /** set the reader to read from the global data */
                    _reader = new StreamReader(globalData.Value);

                    huffmanDecoder = new HuffmanDecoder(_reader);
                    mmrDecoder = new MMRDecoder(_reader);
                    arithmeticDecoder = new ArithmeticDecoder(_reader);

                    /** read in the global data segments */
                    readSegments();

                    /** set the reader back to the main data */
                    _reader = new StreamReader(data);
                }
                else
                {
                    /**
                     * There's no global data, so move the file pointer back to the
                     * start of the stream
                     */
                    _reader.MovePointer(-8);
                }
            }
            else
            {
                /**
                 * We have the file header, so assume it is a valid stand-alone
                 * file.
                 */
#if DEBUG
                if (JBIG2StreamDecoder.DEBUG)
                    Debug.WriteLine("==== File Header ====");
#endif

                setFileHeaderFlags();

#if DEBUG
                if (JBIG2StreamDecoder.DEBUG)
                {
                    Debug.WriteLine("randomAccessOrganisation = " + _randomAccessOrganisation);
                    Debug.WriteLine("noOfPagesKnown = " + _noOfPagesKnown);
                }
#endif

                if (_noOfPagesKnown)
                {
                    _noOfPages = getNoOfPages();
#if DEBUG
                    if (JBIG2StreamDecoder.DEBUG)
                        Debug.WriteLine("noOfPages = " + _noOfPages);
#endif
                }
            }

            huffmanDecoder = new HuffmanDecoder(_reader);
            mmrDecoder = new MMRDecoder(_reader);
            arithmeticDecoder = new ArithmeticDecoder(_reader);

            /** read in the main segment data */
            readSegments();
        }

        private bool checkHeader()
        {
            byte[] controlHeader = new byte[] { 151, 74, 66, 50, 13, 10, 26, 10 };
            byte[] actualHeader = new byte[8];
            _reader.ReadByte(actualHeader);

            return Array.Equals(controlHeader, actualHeader);
        }

        private void readSegments()
        {
#if DEBUG
            if (JBIG2StreamDecoder.DEBUG)
                Debug.WriteLine("==== Segments ====");
#endif

            bool finished = false;
            while (!_reader.IsFinished && !finished)
            {

                SegmentHeader segmentHeader = new SegmentHeader();

#if DEBUG
                if (JBIG2StreamDecoder.DEBUG)
                    Debug.WriteLine("==== Segment Header ====");
#endif

                readSegmentHeader(segmentHeader);

                // read the Segment data
                Segment segment = null;

                SegType segmentType = segmentHeader.SegmentType;
                int[] referredToSegments = segmentHeader.ReferredToSegments;
                int noOfReferredToSegments = segmentHeader.ReferredToSegmentCount;

                switch (segmentType)
                {
                    case SegType.SYMBOL_DICTIONARY:
#if DEBUG
                        if (JBIG2StreamDecoder.DEBUG)
                            Console.WriteLine("==== Segment Symbol Dictionary ====");
#endif

                        segment = new SymbolDictionarySegment(this);

                        segment.SegmentHeader = segmentHeader;

                        break;

                    case SegType.INTERMEDIATE_TEXT_REGION:
#if DEBUG
                        if (JBIG2StreamDecoder.DEBUG)
                            Debug.WriteLine("==== Intermediate Text Region ====");
#endif

                        segment = new TextRegionSegment(this, false);

                        segment.SegmentHeader = segmentHeader;

                        break;

                    case SegType.IMMEDIATE_TEXT_REGION:
#if DEBUG
                        if (JBIG2StreamDecoder.DEBUG)
                            Debug.WriteLine("==== Immediate Text Region ====");
#endif

                        segment = new TextRegionSegment(this, true);

                        segment.SegmentHeader = segmentHeader;

                        break;

                    case SegType.IMMEDIATE_LOSSLESS_TEXT_REGION:
#if DEBUG
                        if (JBIG2StreamDecoder.DEBUG)
                            Debug.WriteLine("==== Immediate Lossless Text Region ====");
#endif

                        segment = new TextRegionSegment(this, true);

                        segment.SegmentHeader = segmentHeader;

                        break;

                    case SegType.PATTERN_DICTIONARY:
#if DEBUG
                        if (JBIG2StreamDecoder.DEBUG)
                            Debug.WriteLine("==== Pattern Dictionary ====");
#endif

                        segment = new PatternDictionarySegment(this);

                        segment.SegmentHeader = segmentHeader;

                        break;

                    case SegType.INTERMEDIATE_HALFTONE_REGION:
#if DEBUG
                        if (JBIG2StreamDecoder.DEBUG)
                            Debug.WriteLine("==== Intermediate Halftone Region ====");
#endif

                        segment = new HalftoneRegionSegment(this, false);

                        segment.SegmentHeader = segmentHeader;

                        break;

                    case SegType.IMMEDIATE_HALFTONE_REGION:
#if DEBUG
                        if (JBIG2StreamDecoder.DEBUG)
                            Debug.WriteLine("==== Immediate Halftone Region ====");
#endif

                        segment = new HalftoneRegionSegment(this, true);

                        segment.SegmentHeader = segmentHeader;

                        break;

                    case SegType.IMMEDIATE_LOSSLESS_HALFTONE_REGION:
#if DEBUG
                        if (JBIG2StreamDecoder.DEBUG)
                            Debug.WriteLine("==== Immediate Lossless Halftone Region ====");
#endif

                        segment = new HalftoneRegionSegment(this, true);

                        segment.SegmentHeader = segmentHeader;

                        break;

                    case SegType.INTERMEDIATE_GENERIC_REGION:
#if DEBUG
                        if (JBIG2StreamDecoder.DEBUG)
                            Debug.WriteLine("==== Intermediate Generic Region ====");
#endif

                        segment = new GenericRegionSegment(this, false);

                        segment.SegmentHeader = segmentHeader;

                        break;

                    case SegType.IMMEDIATE_GENERIC_REGION:
#if DEBUG
                        if (JBIG2StreamDecoder.DEBUG)
                            Debug.WriteLine("==== Immediate Generic Region ====");
#endif

                        segment = new GenericRegionSegment(this, true);

                        segment.SegmentHeader = segmentHeader;

                        break;

                    case SegType.IMMEDIATE_LOSSLESS_GENERIC_REGION:
#if DEBUG
                        if (JBIG2StreamDecoder.DEBUG)
                            Debug.WriteLine("==== Immediate Lossless Generic Region ====");
#endif

                        segment = new GenericRegionSegment(this, true);

                        segment.SegmentHeader = segmentHeader;

                        break;

                    case SegType.INTERMEDIATE_GENERIC_REFINEMENT_REGION:
#if DEBUG
                        if (JBIG2StreamDecoder.DEBUG)
                            Debug.WriteLine("==== Intermediate Generic Refinement Region ====");
#endif

                        segment = new RefinementRegionSegment(this, false, referredToSegments, noOfReferredToSegments);

                        segment.SegmentHeader = segmentHeader;

                        break;

                    case SegType.IMMEDIATE_GENERIC_REFINEMENT_REGION:
#if DEBUG
                        if (JBIG2StreamDecoder.DEBUG)
                            Debug.WriteLine("==== Immediate Generic Refinement Region ====");
#endif

                        segment = new RefinementRegionSegment(this, true, referredToSegments, noOfReferredToSegments);

                        segment.SegmentHeader = segmentHeader;

                        break;

                    case SegType.IMMEDIATE_LOSSLESS_GENERIC_REFINEMENT_REGION:
#if DEBUG
                        if (JBIG2StreamDecoder.DEBUG)
                            Debug.WriteLine("==== Immediate lossless Generic Refinement Region ====");
#endif

                        segment = new RefinementRegionSegment(this, true, referredToSegments, noOfReferredToSegments);

                        segment.SegmentHeader = segmentHeader;

                        break;

                    case SegType.PAGE_INFORMATION:
#if DEBUG
                        if (JBIG2StreamDecoder.DEBUG)
                            Debug.WriteLine("==== Page Information Dictionary ====");
#endif

                        segment = new PageInformationSegment(this);

                        segment.SegmentHeader = segmentHeader;

                        break;

                    case SegType.END_OF_PAGE:
                        continue;

                    case SegType.END_OF_STRIPE:
#if DEBUG
                        if (JBIG2StreamDecoder.DEBUG)
                            Debug.WriteLine("==== End of Stripes ====");
#endif

                        segment = new EndOfStripeSegment(this);

                        segment.SegmentHeader = segmentHeader;
                        break;

                    case SegType.END_OF_FILE:
#if DEBUG
                        if (JBIG2StreamDecoder.DEBUG)
                            Debug.WriteLine("==== End of File ====");
#endif

                        finished = true;

                        continue;

                    case SegType.PROFILES:
#if DEBUG
                        if (JBIG2StreamDecoder.DEBUG)
                            Debug.WriteLine("PROFILES UNIMPLEMENTED");
#endif
                        break;

                    case SegType.TABLES:
#if DEBUG
                        if (JBIG2StreamDecoder.DEBUG)
                            Debug.WriteLine("TABLES UNIMPLEMENTED");
#endif
                        break;

                    case SegType.EXTENSION:
#if DEBUG
                        if (JBIG2StreamDecoder.DEBUG)
                            Debug.WriteLine("==== Extensions ====");
#endif

                        segment = new ExtensionSegment(this);

                        segment.SegmentHeader = segmentHeader;

                        break;

                    default:
                        Debug.WriteLine("Unknown Segment type in JBIG2 stream");

                        break;
                }

                if (!_randomAccessOrganisation)
                {
                    segment.readSegment();
                }

                segments.Add(segment);
            }

            if (_randomAccessOrganisation)
            {
                foreach (Segment segment in segments)
                    segment.readSegment();
            }
        }

        private void resetDecoder()
        {
            _noOfPagesKnown = false;
            _randomAccessOrganisation = false;

            _noOfPages = -1;

            segments.Clear();
            bitmaps.Clear();
        }

        public uint readBits(int num)
        {
            return _reader.ReadBits(num);
        }

        public void movePointer(int i)
        {
            _reader.MovePointer(i);
        }

        public uint readBit()
        {
            return _reader.ReadBit();
        }

        public byte ReadByte()
        {
            return _reader.ReadByte();
        }

        public void appendBitmap(JBIG2Bitmap bitmap)
        {
            bitmaps.Add(bitmap);
        }

        public JBIG2Bitmap findBitmap(int bitmapNumber)
        {
            foreach (JBIG2Bitmap bitmap in bitmaps)
            {
                if (bitmap.getBitmapNumber() == bitmapNumber)
                {
                    return bitmap;
                }
            }

            return null;
        }

	    public void readByte(byte[] buff) {
		    _reader.ReadByte(buff);
	    }

        public void consumeRemainingBits()
        {
            _reader.consumeRemainingBits();
        }

        internal PageInformationSegment findPageSegement(int page)
        {
            foreach (Segment segment in segments)
            {
                SegmentHeader segmentHeader = segment.SegmentHeader;
                if (segmentHeader.SegmentType == SegType.PAGE_INFORMATION && segmentHeader.PageAssociation == page)
                {
                    return (PageInformationSegment)segment;
                }
            }

            return null;
        }

        public Segment findSegment(int segmentNumber)
        {
            foreach (Segment segment in segments)
            {
                if (segment.SegmentHeader.SegmentNumber == segmentNumber)
                {
                    return segment;
                }
            }

            return null;
        }

        private void readSegmentHeader(SegmentHeader segmentHeader)
        {
            handleSegmentNumber(segmentHeader);

            handleSegmentHeaderFlags(segmentHeader);

            handleSegmentReferredToCountAndRententionFlags(segmentHeader);

            handleReferedToSegmentNumbers(segmentHeader);

            handlePageAssociation(segmentHeader);

            if (segmentHeader.SegmentType != SegType.END_OF_FILE)
                handleSegmentDataLength(segmentHeader);
        }

        private void handleSegmentDataLength(SegmentHeader segmentHeader)
        {
            byte[] buf = new byte[4];
            _reader.ReadByte(buf);

            int dateLength = BinaryOperation.getInt32(buf);
            segmentHeader.DataLength = dateLength;

#if DEBUG
            if (JBIG2StreamDecoder.DEBUG)
                Debug.WriteLine("dateLength = " + dateLength);
#endif
        }

        private void handlePageAssociation(SegmentHeader segmentHeader)
        {
            int pageAssociation;

            bool isPageAssociationSizeSet = segmentHeader.IsPageAssociationSizeSet;
            if (isPageAssociationSizeSet)
            { // field is 4 bytes long
                byte[] buf = new byte[4];
                _reader.ReadByte(buf);
                pageAssociation = BinaryOperation.getInt32(buf);
            }
            else
            { // field is 1 byte long
                pageAssociation = _reader.ReadByte();
            }

            segmentHeader.PageAssociation = pageAssociation;
#if DEBUG
            if (JBIG2StreamDecoder.DEBUG)
                Debug.WriteLine("pageAssociation = " + pageAssociation);
#endif
        }

        private void handleSegmentNumber(SegmentHeader segmentHeader)
        {
            byte[] segmentBytes = new byte[4];
            _reader.ReadByte(segmentBytes);

            int segmentNumber = BinaryOperation.getInt32(segmentBytes);
#if DEBUG
            if (JBIG2StreamDecoder.DEBUG)
                Debug.WriteLine("SegmentNumber = " + segmentNumber);
#endif
            segmentHeader.SegmentNumber = segmentNumber;
        }

        private void handleSegmentHeaderFlags(SegmentHeader segmentHeader)
        {
            SegType segmentHeaderFlags = (SegType)_reader.ReadByte();
            // Debug.WriteLine("SegmentHeaderFlags = " + SegmentHeaderFlags);
            segmentHeader.SetSegmentHeaderFlags(segmentHeaderFlags);
        }

        private void handleSegmentReferredToCountAndRententionFlags(SegmentHeader segmentHeader)
        {
            short referedToSegmentCountAndRetentionFlags = _reader.ReadByte();

            int referredToSegmentCount = (referedToSegmentCountAndRetentionFlags & 224) >> 5; // 224
            // =
            // 11100000

            ushort[] retentionFlags = null;
            /** take off the first three bits of the first byte */
            ushort firstByte = (ushort)(referedToSegmentCountAndRetentionFlags & 31); // 31 =
            // 00011111

            if (referredToSegmentCount <= 4)
            { // short form

                retentionFlags = new ushort[1];
                retentionFlags[0] = firstByte;

            }
            else if (referredToSegmentCount == 7)
            { // long form

                ushort[] longFormCountAndFlags = new ushort[4];
                /** add the first byte of the four */
                longFormCountAndFlags[0] = firstByte;

                for (int i = 1; i < 4; i++)
                    // add the next 3 bytes to the array
                    longFormCountAndFlags[i] = _reader.ReadByte();

                /** get the count of the referred to Segments */
                referredToSegmentCount = BinaryOperation.getInt32(longFormCountAndFlags);

                /** calculate the number of bytes in this field */
                int noOfBytesInField = (int)Math.Ceiling(4 + ((referredToSegmentCount + 1) / 8d));
                // Debug.WriteLine("noOfBytesInField = " + noOfBytesInField);

                int noOfRententionFlagBytes = noOfBytesInField - 4;
                retentionFlags = new ushort[noOfRententionFlagBytes];
                _reader.ReadByte(retentionFlags);

            }
            else
            { // error
                throw new JBIG2Exception("Error, 3 bit Segment count field = " + referredToSegmentCount);
            }

            segmentHeader.ReferredToSegmentCount = referredToSegmentCount;
#if DEBUG
            if (JBIG2StreamDecoder.DEBUG)
                Debug.WriteLine("referredToSegmentCount = " + referredToSegmentCount);
#endif

            segmentHeader.RententionFlags = retentionFlags;

#if DEBUG
            if (JBIG2StreamDecoder.DEBUG)
                Debug.WriteLine("retentionFlags = ");

            if (JBIG2StreamDecoder.DEBUG)
            {
                for (int i = 0; i < retentionFlags.Length; i++)
                    Debug.WriteLine(retentionFlags[i] + " ");
                Debug.WriteLine("");
            }
#endif
        }

        private void handleReferedToSegmentNumbers(SegmentHeader segmentHeader)
        {
            int referredToSegmentCount = segmentHeader.ReferredToSegmentCount;
            int[] referredToSegments = new int[referredToSegmentCount];

            int segmentNumber = segmentHeader.SegmentNumber;

            if (segmentNumber <= 256)
            {
                for (int i = 0; i < referredToSegmentCount; i++)
                    referredToSegments[i] = _reader.ReadByte();
            }
            else if (segmentNumber <= 65536)
            {
                byte[] buf = new byte[2];
                for (int i = 0; i < referredToSegmentCount; i++)
                {
                    _reader.ReadByte(buf);
                    referredToSegments[i] = BinaryOperation.getInt16(buf);
                }
            }
            else
            {
                byte[] buf = new byte[4];
                for (int i = 0; i < referredToSegmentCount; i++)
                {
                    _reader.ReadByte(buf);
                    referredToSegments[i] = BinaryOperation.getInt32(buf);
                }
            }

            segmentHeader.ReferredToSegments = referredToSegments;
#if DEBUG
            if (JBIG2StreamDecoder.DEBUG)
            {
                Debug.Write("referredToSegments = ");
                for (int i = 0; i < referredToSegments.Length; i++)
                    Debug.Write(referredToSegments[i] + " ");
                Debug.WriteLine("");
            }
#endif
        }

        private void setFileHeaderFlags()
        {
            short headerFlags = _reader.ReadByte();

            if ((headerFlags & 0xfc) != 0)
            {
                Debug.WriteLine("Warning, reserved bits (2-7) of file header flags are not zero " + headerFlags);
            }

            int fileOrganisation = headerFlags & 1;
            _randomAccessOrganisation = fileOrganisation == 0;

            int pagesKnown = headerFlags & 2;
            _noOfPagesKnown = pagesKnown == 0;
        }

        private int getNoOfPages()
        {
            byte[] noOfPages = new byte[4];
            _reader.ReadByte(noOfPages);

            return BinaryOperation.getInt32(noOfPages);
        }
    }
}
