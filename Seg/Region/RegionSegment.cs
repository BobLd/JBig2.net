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
* RegionSegment.java
* ---------------
*/
#endregion
using JBig2.Decoders;
using JBig2.Util;
using System.Diagnostics;

namespace JBig2.Seg.Region
{
    internal abstract class RegionSegment : Segment
    {
        #region Variables and properties

        protected int regionBitmapWidth, regionBitmapHeight;
        protected int regionBitmapXLocation, regionBitmapYLocation;

        protected RegionFlags regionFlags = new RegionFlags();

        #endregion

        #region Init

        public RegionSegment(JBIG2StreamDecoder streamDecoder)
            : base(streamDecoder)
        {
        }

        #endregion

        public override void readSegment()
        {
            byte[] buff = new byte[4];
            decoder.readByte(buff);
            regionBitmapWidth = BinaryOperation.getInt32(buff);

            buff = new byte[4];
            decoder.readByte(buff);
            regionBitmapHeight = BinaryOperation.getInt32(buff);
#if DEBUG
            if (JBIG2StreamDecoder.DEBUG)
                Debug.WriteLine("Bitmap size = " + regionBitmapWidth + 'x' + regionBitmapHeight);
#endif
            buff = new byte[4];
            decoder.readByte(buff);
            regionBitmapXLocation = BinaryOperation.getInt32(buff);

            buff = new byte[4];
            decoder.readByte(buff);
            regionBitmapYLocation = BinaryOperation.getInt32(buff);
#if DEBUG
            if (JBIG2StreamDecoder.DEBUG)
                Debug.WriteLine("Bitmap location = " + regionBitmapXLocation + ',' + regionBitmapYLocation);
#endif
            /** extract region Segment flags */
            short regionFlagsField = decoder.ReadByte();

            regionFlags.setFlags(regionFlagsField);
#if DEBUG
            if (JBIG2StreamDecoder.DEBUG)
                Debug.WriteLine("region Segment flags = " + regionFlagsField);
#endif
        }
    }
}
