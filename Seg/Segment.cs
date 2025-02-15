#region Licsens
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
* Segment.java
* ---------------
*/
#endregion
using JBig2.Decoders;

namespace JBig2.Seg
{
    public abstract class Segment
    {
        #region Variables and properties

        protected SegmentHeader _seg_head;

        protected HuffmanDecoder huffmanDecoder;

        protected ArithmeticDecoder arithmeticDecoder;

        protected MMRDecoder mmrDecoder;

        protected JBIG2StreamDecoder decoder;

        public SegmentHeader SegmentHeader
        {
            get { return _seg_head; }
            set { _seg_head = value; }
        }

        #endregion

        #region Init

        public Segment(JBIG2StreamDecoder streamDecoder)
        {
            this.decoder = streamDecoder;

            huffmanDecoder = decoder.HuffmanDecoder;
            arithmeticDecoder = decoder.ArithmeticDecoder;
            mmrDecoder = decoder.MMRDecoder;

        }

        #endregion

        protected short readATValue()
        {
            short atValue;
            short c0 = atValue = decoder.ReadByte();

            if ((c0 & 0x80) != 0)
            {
                atValue |= -1 - 0xff;
            }

            return atValue;
        }

        public abstract void readSegment();
    }

    public enum SegType
    {
        SYMBOL_DICTIONARY = 0,
        INTERMEDIATE_TEXT_REGION = 4,
        IMMEDIATE_TEXT_REGION = 6,
        IMMEDIATE_LOSSLESS_TEXT_REGION = 7,
        PATTERN_DICTIONARY = 16,
        INTERMEDIATE_HALFTONE_REGION = 20,
        IMMEDIATE_HALFTONE_REGION = 22,
        IMMEDIATE_LOSSLESS_HALFTONE_REGION = 23,
        INTERMEDIATE_GENERIC_REGION = 36,
        IMMEDIATE_GENERIC_REGION = 38,
        IMMEDIATE_LOSSLESS_GENERIC_REGION = 39,
        INTERMEDIATE_GENERIC_REFINEMENT_REGION = 40,
        IMMEDIATE_GENERIC_REFINEMENT_REGION = 42,
        IMMEDIATE_LOSSLESS_GENERIC_REFINEMENT_REGION = 43,
        PAGE_INFORMATION = 48,
        END_OF_PAGE = 49,
        END_OF_STRIPE = 50,
        END_OF_FILE = 51,
        PROFILES = 52,
        TABLES = 53,
        EXTENSION = 62,
        BITMAP = 70
    }
}
