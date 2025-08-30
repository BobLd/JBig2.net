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
* StreamReader.java
* ---------------
*/
#endregion
namespace JBig2.IO
{
    public class StreamReader
    {
        #region Variables and properties

        private Memory<byte> _data;

        /// <summary>
        /// Bit pointer
        /// </summary>
        private int _bit_ptr = 7;

        /// <summary>
        /// Byte pointer
        /// </summary>
        private int _byte_ptr = 0;

        public bool IsFinished
        {
            get { return _byte_ptr == _data.Length; }
        }

        #endregion

        #region Init

        public StreamReader(Memory<byte> data)
        {
            _data = data;
        }

        #endregion

        public void MovePointer(int ammount)
        {
            _byte_ptr += ammount;
        }

        public byte ReadByte()
        {
            return _data.Span[_byte_ptr++];
        }

        public void ReadByte(ushort[] buf)
        {
            for (int i = 0; i < buf.Length; i++)
            {
                buf[i] = _data.Span[_byte_ptr++];
            }
        }

        public void ReadByte(byte[] buf)
        {
            for (int i = 0; i < buf.Length; i++)
            {
                buf[i] = _data.Span[_byte_ptr++];
            }
        }

        public uint ReadBit()
        {
            byte buf = ReadByte();
            uint mask = ((uint)1) << _bit_ptr;

            uint bit = (buf & mask) >> _bit_ptr;

            _bit_ptr--;
            if (_bit_ptr == -1)
                _bit_ptr = 7;
            else
                _byte_ptr--;

            return bit;
        }

        public uint ReadBits(uint num)
        {
            uint result = 0;

            for (int i = 0; i < num; i++)
                result = (result << 1) | ReadBit();

            return result;
        }

        public uint ReadBits(int num)
        {
            uint result = 0;

            for (int i = 0; i < num; i++)
                result = (result << 1) | ReadBit();

            return result;
        }

        public void consumeRemainingBits()
        {
            if (_bit_ptr != 7)
                ReadBits(_bit_ptr + 1);
        }
    }
}
