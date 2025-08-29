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
* HuffmanDecoder.java
* ---------------
*/
#endregion
using StreamReader = JBig2.IO.StreamReader;

namespace JBig2.Decoders
{
    public class HuffmanDecoder
    {
        #region Variables and properties

        public const uint JBig2HuffmanLOW = 0xfffffffd;
        public const uint JBig2HuffmanOOB = 0xfffffffe;
        public const uint JBig2HuffmanEOT = 0xffffffff;

        private StreamReader _reader;

        #endregion

        #region Init

        public HuffmanDecoder(StreamReader reader)
        {
            _reader = reader;
        }

        #endregion

        public DecodeIntResult decodeInt(uint[][] table)
        {
            int length = 0, prefix = 0;

            for (int i = 0; table[i][2] != JBig2HuffmanEOT; i++)
            {
                for (; length < table[i][1]; length++)
                {
                    int bit = (int) _reader.ReadBit();
                    prefix = (prefix << 1) | bit;
                }

                if (prefix == table[i][3])
                {
                    if (table[i][2] == JBig2HuffmanOOB)
                    {
                        return new DecodeIntResult(-1, false);
                    }
                    uint decodedInt;
                    if (table[i][2] == JBig2HuffmanLOW)
                    {
                        uint readBits = _reader.ReadBits(32);
                        decodedInt = table[i][0] - readBits;
                    }
                    else if (table[i][2] > 0)
                    {
                        uint readBits = _reader.ReadBits(table[i][2]);
                        decodedInt = table[i][0] + readBits;
                    }
                    else
                    {
                        decodedInt = table[i][0];
                    }
                    return new DecodeIntResult(unchecked((int)decodedInt), true);
                }
            }

            return new DecodeIntResult(-1, false);
        }

        public static uint[][] buildTable(uint[][] table, int length)
        {
            int i, j, k;
            uint prefix;
            uint[] tab;

            for (i = 0; i < length; i++)
            {
                for (j = i; j < length && table[j][1] == 0; j++) ;

                if (j == length)
                {
                    break;
                }
                for (k = j + 1; k < length; k++)
                {
                    if (table[k][1] > 0 && table[k][1] < table[j][1])
                    {
                        j = k;
                    }
                }
                if (j != i)
                {
                    tab = table[j];
                    for (k = j; k > i; k--)
                    {
                        table[k] = table[k - 1];
                    }
                    table[i] = tab;
                }
            }
            table[i] = table[length];

            i = 0;
            prefix = 0;
            table[i++][3] = prefix++;
            for (; table[i][2] != JBig2HuffmanEOT; i++)
            {
                prefix <<= (int) (table[i][1] - table[i - 1][1]);
                table[i][3] = prefix++;
            }

            return table;
        }

        public static uint[][] huffmanTableA = { new uint[] { 0, 1, 4, 0x000 }, new uint[] { 16, 2, 8, 0x002 }, new uint[] { 272, 3, 16, 0x006 }, new uint[] { 65808, 3, 32, 0x007 }, new uint[] { 0, 0, JBig2HuffmanEOT, 0 } };
        public static uint[][] huffmanTableB = { new uint[] { 0, 1, 0, 0x000 }, new uint[] { 1, 2, 0, 0x002 }, new uint[] { 2, 3, 0, 0x006 }, new uint[] { 3, 4, 3, 0x00e }, new uint[] { 11, 5, 6, 0x01e }, new uint[] { 75, 6, 32, 0x03e }, new uint[] { 0, 6, JBig2HuffmanOOB, 0x03f }, new uint[] { 0, 0, JBig2HuffmanEOT, 0 } };
        public static uint[][] huffmanTableC = { new uint[] { 0, 1, 0, 0x000 }, new uint[] { 1, 2, 0, 0x002 }, new uint[] { 2, 3, 0, 0x006 }, new uint[] { 3, 4, 3, 0x00e }, new uint[] { 11, 5, 6, 0x01e }, new uint[] { 0, 6, JBig2HuffmanOOB, 0x03e }, new uint[] { 75, 7, 32, 0x0fe }, new uint[] { unchecked((uint)-256), 8, 8, 0x0fe }, new uint[] { unchecked((uint)-257), 8, JBig2HuffmanLOW, 0x0ff }, new uint[] { 0, 0, JBig2HuffmanEOT, 0 } };
        public static uint[][] huffmanTableD = { new uint[] { 1, 1, 0, 0x000 }, new uint[] { 2, 2, 0, 0x002 }, new uint[] { 3, 3, 0, 0x006 }, new uint[] { 4, 4, 3, 0x00e }, new uint[] { 12, 5, 6, 0x01e }, new uint[] { 76, 5, 32, 0x01f }, new uint[] { 0, 0, JBig2HuffmanEOT, 0 } };
        public static uint[][] huffmanTableE = { new uint[] { 1, 1, 0, 0x000 }, new uint[] { 2, 2, 0, 0x002 }, new uint[] { 3, 3, 0, 0x006 }, new uint[] { 4, 4, 3, 0x00e }, new uint[] { 12, 5, 6, 0x01e }, new uint[] { 76, 6, 32, 0x03e }, new uint[] { unchecked((uint)-255), 7, 8, 0x07e }, new uint[] { unchecked((uint)-256), 7, JBig2HuffmanLOW, 0x07f }, new uint[] { 0, 0, JBig2HuffmanEOT, 0 } };
        public static uint[][] huffmanTableF = { new uint[] { 0, 2, 7, 0x000 }, new uint[] { 128, 3, 7, 0x002 }, new uint[] { 256, 3, 8, 0x003 }, new uint[] { unchecked((uint)-1024), 4, 9, 0x008 }, new uint[] { unchecked((uint)-512), 4, 8, 0x009 }, new uint[] { unchecked((uint)-256), 4, 7, 0x00a }, new uint[] { unchecked((uint)-32), 4, 5, 0x00b }, new uint[] { 512, 4, 9, 0x00c }, new uint[] { 1024, 4, 10, 0x00d }, new uint[] { unchecked((uint)-2048), 5, 10, 0x01c }, new uint[] { unchecked((uint)-128), 5, 6, 0x01d }, new uint[] { unchecked((uint)-64), 5, 5, 0x01e }, new uint[] { unchecked((uint)-2049), 6, JBig2HuffmanLOW, 0x03e }, new uint[] { 2048, 6, 32, 0x03f }, new uint[] { 0, 0, JBig2HuffmanEOT, 0 } };
        public static uint[][] huffmanTableG = { new uint[] { unchecked((uint)-512), 3, 8, 0x000 }, new uint[] { 256, 3, 8, 0x001 }, new uint[] { 512, 3, 9, 0x002 }, new uint[] { 1024, 3, 10, 0x003 }, new uint[] { unchecked((uint)-1024), 4, 9, 0x008 }, new uint[] { unchecked((uint)-256), 4, 7, 0x009 }, new uint[] { unchecked((uint)-32), 4, 5, 0x00a }, new uint[] { 0, 4, 5, 0x00b }, new uint[] { 128, 4, 7, 0x00c }, new uint[] { unchecked((uint)-128), 5, 6, 0x01a }, new uint[] { unchecked((uint)-64), 5, 5, 0x01b }, new uint[] { 32, 5, 5, 0x01c }, new uint[] { 64, 5, 6, 0x01d }, new uint[] { unchecked((uint)-1025), 5, JBig2HuffmanLOW, 0x01e }, new uint[] { 2048, 5, 32, 0x01f }, new uint[] { 0, 0, JBig2HuffmanEOT, 0 } };
        public static uint[][] huffmanTableH = { new uint[] { 0, 2, 1, 0x000 }, new uint[] { 0, 2, JBig2HuffmanOOB, 0x001 }, new uint[] { 4, 3, 4, 0x004 }, new uint[] { unchecked((uint)-1), 4, 0, 0x00a }, new uint[] { 22, 4, 4, 0x00b }, new uint[] { 38, 4, 5, 0x00c }, new uint[] { 2, 5, 0, 0x01a }, new uint[] { 70, 5, 6, 0x01b }, new uint[] { 134, 5, 7, 0x01c }, new uint[] { 3, 6, 0, 0x03a }, new uint[] { 20, 6, 1, 0x03b }, new uint[] { 262, 6, 7, 0x03c }, new uint[] { 646, 6, 10, 0x03d }, new uint[] { unchecked((uint)-2), 7, 0, 0x07c }, new uint[] { 390, 7, 8, 0x07d }, new uint[] { unchecked((uint)-15), 8, 3, 0x0fc }, new uint[] { unchecked((uint)-5), 8, 1, 0x0fd }, new uint[] { unchecked((uint)-7), 9, 1, 0x1fc }, new uint[] { unchecked((uint)-3), 9, 0, 0x1fd }, new uint[] { unchecked((uint)-16), 9, JBig2HuffmanLOW, 0x1fe }, new uint[] { 1670, 9, 32, 0x1ff }, new uint[] { 0, 0, JBig2HuffmanEOT, 0 } };
        public static uint[][] huffmanTableI = { new uint[] { 0, 2, JBig2HuffmanOOB, 0x000 }, new uint[] { unchecked((uint)-1), 3, 1, 0x002 }, new uint[] { 1, 3, 1, 0x003 }, new uint[] { 7, 3, 5, 0x004 }, new uint[] { unchecked((uint)-3), 4, 1, 0x00a }, new uint[] { 43, 4, 5, 0x00b }, new uint[] { 75, 4, 6, 0x00c }, new uint[] { 3, 5, 1, 0x01a }, new uint[] { 139, 5, 7, 0x01b }, new uint[] { 267, 5, 8, 0x01c }, new uint[] { 5, 6, 1, 0x03a }, new uint[] { 39, 6, 2, 0x03b }, new uint[] { 523, 6, 8, 0x03c }, new uint[] { 1291, 6, 11, 0x03d }, new uint[] { unchecked((uint)-5), 7, 1, 0x07c }, new uint[] { 779, 7, 9, 0x07d }, new uint[] { unchecked((uint)-31), 8, 4, 0x0fc }, new uint[] { unchecked((uint)-11), 8, 2, 0x0fd }, new uint[] { unchecked((uint)-15), 9, 2, 0x1fc }, new uint[] { unchecked((uint)-7), 9, 1, 0x1fd }, new uint[] { unchecked((uint)-32), 9, JBig2HuffmanLOW, 0x1fe }, new uint[] { 3339, 9, 32, 0x1ff }, new uint[] { 0, 0, JBig2HuffmanEOT, 0 } };
        public static uint[][] huffmanTableJ = { new uint[] { unchecked((uint)-2), 2, 2, 0x000 }, new uint[] { 6, 2, 6, 0x001 }, new uint[] { 0, 2, JBig2HuffmanOOB, 0x002 }, new uint[] { unchecked((uint)-3), 5, 0, 0x018 }, new uint[] { 2, 5, 0, 0x019 }, new uint[] { 70, 5, 5, 0x01a }, new uint[] { 3, 6, 0, 0x036 }, new uint[] { 102, 6, 5, 0x037 }, new uint[] { 134, 6, 6, 0x038 }, new uint[] { 198, 6, 7, 0x039 }, new uint[] { 326, 6, 8, 0x03a }, new uint[] { 582, 6, 9, 0x03b }, new uint[] { 1094, 6, 10, 0x03c }, new uint[] { unchecked((uint)-21), 7, 4, 0x07a }, new uint[] { unchecked((uint)-49), 7, 0, 0x07b }, new uint[] { 4, 7, 0, 0x07c }, new uint[] { 2118, 7, 11, 0x07d }, new uint[] { unchecked((uint)-5), 8, 0, 0x0fc }, new uint[] { 5, 8, 0, 0x0fd }, new uint[] { unchecked((uint)-22), 8, JBig2HuffmanLOW, 0x0fe }, new uint[] { 4166, 8, 32, 0x0ff }, new uint[] { 0, 0, JBig2HuffmanEOT, 0 } };
        public static uint[][] huffmanTableK = { new uint[] { 1, 1, 0, 0x000 }, new uint[] { 2, 2, 1, 0x002 }, new uint[] { 4, 4, 0, 0x00c }, new uint[] { 5, 4, 1, 0x00d }, new uint[] { 7, 5, 1, 0x01c }, new uint[] { 9, 5, 2, 0x01d }, new uint[] { 13, 6, 2, 0x03c }, new uint[] { 17, 7, 2, 0x07a }, new uint[] { 21, 7, 3, 0x07b }, new uint[] { 29, 7, 4, 0x07c }, new uint[] { 45, 7, 5, 0x07d }, new uint[] { 77, 7, 6, 0x07e }, new uint[] { 141, 7, 32, 0x07f }, new uint[] { 0, 0, JBig2HuffmanEOT, 0 } };
        public static uint[][] huffmanTableL = { new uint[] { 1, 1, 0, 0x000 }, new uint[] { 2, 2, 0, 0x002 }, new uint[] { 3, 3, 1, 0x006 }, new uint[] { 5, 5, 0, 0x01c }, new uint[] { 6, 5, 1, 0x01d }, new uint[] { 8, 6, 1, 0x03c }, new uint[] { 10, 7, 0, 0x07a }, new uint[] { 11, 7, 1, 0x07b }, new uint[] { 13, 7, 2, 0x07c }, new uint[] { 17, 7, 3, 0x07d }, new uint[] { 25, 7, 4, 0x07e }, new uint[] { 41, 8, 5, 0x0fe }, new uint[] { 73, 8, 32, 0x0ff }, new uint[] { 0, 0, JBig2HuffmanEOT, 0 } };
        public static uint[][] huffmanTableM = { new uint[] { 1, 1, 0, 0x000 }, new uint[] { 2, 3, 0, 0x004 }, new uint[] { 7, 3, 3, 0x005 }, new uint[] { 3, 4, 0, 0x00c }, new uint[] { 5, 4, 1, 0x00d }, new uint[] { 4, 5, 0, 0x01c }, new uint[] { 15, 6, 1, 0x03a }, new uint[] { 17, 6, 2, 0x03b }, new uint[] { 21, 6, 3, 0x03c }, new uint[] { 29, 6, 4, 0x03d }, new uint[] { 45, 6, 5, 0x03e }, new uint[] { 77, 7, 6, 0x07e }, new uint[] { 141, 7, 32, 0x07f }, new uint[] { 0, 0, JBig2HuffmanEOT, 0 } };
        public static uint[][] huffmanTableN = { new uint[] { 0, 1, 0, 0x000 }, new uint[] { unchecked((uint)-2), 3, 0, 0x004 }, new uint[] { unchecked((uint)-1), 3, 0, 0x005 }, new uint[] { 1, 3, 0, 0x006 }, new uint[] { 2, 3, 0, 0x007 }, new uint[] { 0, 0, JBig2HuffmanEOT, 0 } };
        public static uint[][] huffmanTableO = { new uint[] { 0, 1, 0, 0x000 }, new uint[] { unchecked((uint)-1), 3, 0, 0x004 }, new uint[] { 1, 3, 0, 0x005 }, new uint[] { unchecked((uint)-2), 4, 0, 0x00c }, new uint[] { 2, 4, 0, 0x00d }, new uint[] { unchecked((uint)-4), 5, 1, 0x01c }, new uint[] { 3, 5, 1, 0x01d }, new uint[] { unchecked((uint)-8), 6, 2, 0x03c }, new uint[] { 5, 6, 2, 0x03d }, new uint[] { unchecked((uint)-24), 7, 4, 0x07c }, new uint[] { 9, 7, 4, 0x07d }, new uint[] { unchecked((uint)-25), 7, JBig2HuffmanLOW, 0x07e }, new uint[] { 25, 7, 32, 0x07f }, new uint[] { 0, 0, JBig2HuffmanEOT, 0 } };
    }
}
