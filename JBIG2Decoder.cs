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
* JBIG2Decoder.java
* ---------------
*/
#endregion
using JBig2.Decoders;
using JBig2.Image;
using JBig2.Seg;
using System.Collections.Generic;

namespace JBig2
{
    public class JBIG2Decoder
    {
        #region Variables and properties

        private JBIG2StreamDecoder streamDecoder;

        public bool IsNumberOfPagesKnown
        {
            get { return streamDecoder.isNumberOfPagesKnown(); }
        }

        public int NumberOfPages
        {
            get
            {
                int pages = streamDecoder.getNumberOfPages();
                if (streamDecoder.isNumberOfPagesKnown() && pages != 0)
                    return pages;

                int noOfPages = 0;

                var segments = Segments;
                foreach (Segment segment in segments)
                {
                    if (segment.SegmentHeader.SegmentType == SegType.PAGE_INFORMATION)
                        noOfPages++;
                }

                return noOfPages;
            }
        }

        public List<Segment> Segments
        {
            get { return streamDecoder.Segments; }
        }

        #endregion

        #region Init

        /// <summary>
        /// Constructor
        /// </summary>
        public JBIG2Decoder()
        {
            streamDecoder = new JBIG2StreamDecoder();
        }

        /// <summary>
        /// If the data stream is taken from a PDF, there may be some global data. 
        /// Pass any global data in here.  Call this method before decodeJBIG2(...)
        /// </summary>
        /// <param name="data">The global data</param>
        public void setGlobalData(byte[] data) {
		    streamDecoder.setGlobalData(data);
	    }

        #endregion

        public void decodeJBIG2(byte[] data)
        {
            streamDecoder.decodeJBIG2(data);
        }

        public JBIG2Bitmap GetPage(int page)
        {
            page++;
            return streamDecoder.findPageSegement(page).getPageBitmap();
        }
    }
}
