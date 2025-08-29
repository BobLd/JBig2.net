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
* RefinementRegionSegment.java
* ---------------
*/
#endregion
using JBig2.Decoders;
using JBig2.Image;
using JBig2.Seg.PageInformation;
using System.Diagnostics;

namespace JBig2.Seg.Region.Refinement
{
    internal class RefinementRegionSegment : RegionSegment
    {
        #region Variables and properties

        private RefinementRegionFlags refinementRegionFlags = new RefinementRegionFlags();

        private bool inlineImage;

        private int noOfReferedToSegments;

        int[] referedToSegments;

        #endregion

        #region Init

        public RefinementRegionSegment(JBIG2StreamDecoder streamDecoder, bool inlineImage, int[] referedToSegments, int noOfReferedToSegments)
            : base(streamDecoder)
        {
            this.inlineImage = inlineImage;
            this.referedToSegments = referedToSegments;
            this.noOfReferedToSegments = noOfReferedToSegments;
        }

        #endregion

        public override void readSegment()
        {
#if DEBUG
            if (JBIG2StreamDecoder.DEBUG)
                Debug.WriteLine("==== Reading Generic Refinement Region ====");
#endif
            base.readSegment();

            /** read text region segment flags */
            readGenericRegionFlags();

            short[] genericRegionAdaptiveTemplateX = new short[2];
            short[] genericRegionAdaptiveTemplateY = new short[2];

            int template = refinementRegionFlags.getFlagValue(RefinementRegionFlags.GR_TEMPLATE);
            if (template == 0)
            {
                genericRegionAdaptiveTemplateX[0] = readATValue();
                genericRegionAdaptiveTemplateY[0] = readATValue();
                genericRegionAdaptiveTemplateX[1] = readATValue();
                genericRegionAdaptiveTemplateY[1] = readATValue();
            }

            if (noOfReferedToSegments == 0 || inlineImage)
            {
                PageInformationSegment pageSegment = decoder.findPageSegement(_seg_head.PageAssociation);
                JBIG2Bitmap pageBitmap = pageSegment.getPageBitmap();

                if (pageSegment.getPageBitmapHeight() == -1 && regionBitmapYLocation + regionBitmapHeight > pageBitmap.getHeight())
                {
                    pageBitmap.expand(regionBitmapYLocation + regionBitmapHeight, pageSegment.getPageInformationFlags().getFlagValue(PageInformationFlags.DEFAULT_PIXEL_VALUE));
                }
            }

            if (noOfReferedToSegments > 1)
            {
#if DEBUG
                if (JBIG2StreamDecoder.DEBUG)
                    Debug.WriteLine("Bad reference in JBIG2 generic refinement Segment");
#endif

                return;
            }

            JBIG2Bitmap referedToBitmap;
            if (noOfReferedToSegments == 1)
            {
                referedToBitmap = decoder.findBitmap(referedToSegments[0]);
            }
            else
            {
                PageInformationSegment pageSegment = decoder.findPageSegement(_seg_head.PageAssociation);
                JBIG2Bitmap pageBitmap = pageSegment.getPageBitmap();

                referedToBitmap = pageBitmap.getSlice(regionBitmapXLocation, regionBitmapYLocation, regionBitmapWidth, regionBitmapHeight);
            }

            arithmeticDecoder.resetRefinementStats(template, null);
            arithmeticDecoder.start();

            bool typicalPredictionGenericRefinementOn = refinementRegionFlags.getFlagValue(RefinementRegionFlags.TPGDON) != 0;

            JBIG2Bitmap bitmap = new JBIG2Bitmap(regionBitmapWidth, regionBitmapHeight, arithmeticDecoder, huffmanDecoder, mmrDecoder);

            bitmap.readGenericRefinementRegion(template, typicalPredictionGenericRefinementOn, referedToBitmap, 0, 0, genericRegionAdaptiveTemplateX, genericRegionAdaptiveTemplateY);

            if (inlineImage)
            {
                PageInformationSegment pageSegment = decoder.findPageSegement(_seg_head.PageAssociation);
                JBIG2Bitmap pageBitmap = pageSegment.getPageBitmap();

                int extCombOp = regionFlags.getFlagValue(RegionFlags.EXTERNAL_COMBINATION_OPERATOR);

                pageBitmap.combine(bitmap, regionBitmapXLocation, regionBitmapYLocation, extCombOp);
            }
            else
            {
                bitmap.setBitmapNumber(_seg_head.SegmentNumber);
                decoder.appendBitmap(bitmap);
            }
        }

        private void readGenericRegionFlags()
        {
            /** extract text region Segment flags */
            byte refinementRegionFlagsField = decoder.ReadByte();

            refinementRegionFlags.setFlags(refinementRegionFlagsField);
#if DEBUG
            if (JBIG2StreamDecoder.DEBUG)
                Debug.WriteLine("generic region Segment flags = " + refinementRegionFlagsField);
#endif
        }

        public RefinementRegionFlags getGenericRegionFlags()
        {
            return refinementRegionFlags;
        }
    }
}
