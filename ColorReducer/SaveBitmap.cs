/*
iZYINS : Color Reducer on .NET Framework or MONO
Copyright (C) 2005-2008 Y.Nomura

This program is free software; you can redistribute it and/or
modify it under the terms of the GNU General Public License
as published by the Free Software Foundation; either version 2
of the License, or (at your option) any later version.

This program is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
GNU General Public License for more details.

You should have received a copy of the GNU General Public License
along with this program; if not, write to the Free Software
Foundation, Inc., 59 Temple Place - Suite 330, Boston, MA  02111-1307, USA.
*/
using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace ZGUtils.Image;

public class CSaveBitmap
{
    private int mWidth = 0;
    private int mHeight = 0;

    public int Width
    {
        set { this.mWidth = value; }
        get { return this.mWidth; }
    }

    public int Height
    {
        set { this.mHeight = value; }
        get { return this.mHeight; }
    }

    private int[] mData = null;

    private Int32[] mPalette = null;

    private MemoryStream mStream = null;

    public bool setData(int aWidth, int aHeight, Int32[] aPalette,
                        Int32[] aData)
    {
        if ((aPalette.Length < 1) || (aPalette.Length > 256))
        {
            return false;
        }

        if (aData.Length != aWidth * aHeight)
        {
            return false;
        }

        mPalette = new Int32[aPalette.Length];
        aPalette.CopyTo(mPalette, 0);
        mData = new Int32[aWidth * aHeight];
        aData.CopyTo(mData, 0);
        mWidth = aWidth;
        mHeight = aHeight;
        return true;
    }

    private byte[] makeBITMAPFILEHEADER
    (
        ushort bfType,
        uint bfSize,
        ushort bfReserved1,
        ushort bfReserved2,
        uint bfOffBits
    )
    {
        byte[] lbta = new byte[14];
        lbta[0] = (byte)(bfType & 0x00FF);
        lbta[1] = (byte)(bfType >> 8);

        lbta[2] = (byte)(bfSize & 0x000000FF);
        lbta[3] = (byte)((bfSize & 0x0000FF00) >> 8);
        lbta[4] = (byte)((bfSize & 0x00FF0000) >> 16);
        lbta[5] = (byte)(bfSize >> 24);

        lbta[6] = (byte)(bfReserved1 & 0x00FF);
        lbta[7] = (byte)(bfReserved1 >> 8);

        lbta[8] = (byte)(bfReserved2 & 0x00FF);
        lbta[9] = (byte)(bfReserved2 >> 8);

        lbta[10] = (byte)(bfOffBits & 0x000000FF);
        lbta[11] = (byte)((bfOffBits & 0x0000FF00) >> 8);
        lbta[12] = (byte)((bfOffBits & 0x00FF0000) >> 16);
        lbta[13] = (byte)(bfOffBits >> 24);

        return lbta;
    }

    private byte[] makeBITMAPINFOHEADER
    (
        uint biSize,
        int biWidth,
        int biHeight,
        ushort biPlanes,
        ushort biBitCount,
        uint biCompression,
        uint biSizeImage,
        int biXPelsPerMeter,
        int biYPelsPerMeter,
        uint biClrUsed,
        uint biClrImportant
    )
    {
        byte[] lbta = new byte[40];

        lbta[0] = (byte)(biSize & 0x000000FF);
        lbta[1] = (byte)((biSize & 0x0000FF00) >> 8);
        lbta[2] = (byte)((biSize & 0x00FF0000) >> 16);
        lbta[3] = (byte)(biSize >> 24);

        lbta[4] = (byte)(biWidth & 0x000000FF);
        lbta[5] = (byte)((biWidth & 0x0000FF00) >> 8);
        lbta[6] = (byte)((biWidth & 0x00FF0000) >> 16);
        lbta[7] = (byte)(biWidth >> 24);

        lbta[8] = (byte)(biHeight & 0x000000FF);
        lbta[9] = (byte)((biHeight & 0x0000FF00) >> 8);
        lbta[10] = (byte)((biHeight & 0x00FF0000) >> 16);
        lbta[11] = (byte)(biHeight >> 24);

        lbta[12] = (byte)(biPlanes & 0x00FF);
        lbta[13] = (byte)(biPlanes >> 8);

        lbta[14] = (byte)(biBitCount & 0x00FF);
        lbta[15] = (byte)(biBitCount >> 8);

        lbta[16] = (byte)(biCompression & 0x000000FF);
        lbta[17] = (byte)((biCompression & 0x0000FF00) >> 8);
        lbta[18] = (byte)((biCompression & 0x00FF0000) >> 16);
        lbta[19] = (byte)(biCompression >> 24);

        lbta[20] = (byte)(biSizeImage & 0x000000FF);
        lbta[21] = (byte)((biSizeImage & 0x0000FF00) >> 8);
        lbta[22] = (byte)((biSizeImage & 0x00FF0000) >> 16);
        lbta[23] = (byte)(biSizeImage >> 24);

        lbta[24] = (byte)(biXPelsPerMeter & 0x000000FF);
        lbta[25] = (byte)((biXPelsPerMeter & 0x0000FF00) >> 8);
        lbta[26] = (byte)((biXPelsPerMeter & 0x00FF0000) >> 16);
        lbta[27] = (byte)(biXPelsPerMeter >> 24);

        lbta[28] = (byte)(biYPelsPerMeter & 0x000000FF);
        lbta[29] = (byte)((biYPelsPerMeter & 0x0000FF00) >> 8);
        lbta[30] = (byte)((biYPelsPerMeter & 0x00FF0000) >> 16);
        lbta[31] = (byte)(biYPelsPerMeter >> 24);

        lbta[32] = (byte)(biClrUsed & 0x000000FF);
        lbta[33] = (byte)((biClrUsed & 0x0000FF00) >> 8);
        lbta[34] = (byte)((biClrUsed & 0x00FF0000) >> 16);
        lbta[35] = (byte)(biClrUsed >> 24);

        lbta[36] = (byte)(biClrImportant & 0x000000FF);
        lbta[37] = (byte)((biClrImportant & 0x0000FF00) >> 8);
        lbta[38] = (byte)((biClrImportant & 0x00FF0000) >> 16);
        lbta[39] = (byte)(biClrImportant >> 24);

        return lbta;
    }

    private byte[] makeRGBQUAD(int[] aPalette)
    {
        byte[] lbta = new byte[aPalette.Length * 4];
        for (int i = 0; i < aPalette.Length; i++)
        {
            lbta[i * 4 + 0] = (byte)(aPalette[i] & 0x000000FF);
            lbta[i * 4 + 1] = (byte)((aPalette[i] & 0x0000FF00) >> 8);
            lbta[i * 4 + 2] = (byte)((aPalette[i] & 0x00FF0000) >> 16);
            //                lbta[i * 4 + 3] = (aPalette[i] & 0xFF000000) >> 24;
            lbta[i * 4 + 3] = 0;
        }
        return lbta;
    }

    private byte[] makePixelData(int[] aData, int aBitcount)
    {
        switch (aBitcount)
        {
            case 1:
                int lLineSize = ((mWidth * 1 + 31) & ~31) >> 3;
                int lPadUnit = lLineSize * 8 - mWidth;

                byte[] lbta = new byte[lLineSize * mHeight];
                byte[] ltmpbta = new byte[lLineSize * 8];

                for (int y2 = 0; y2 < mHeight; y2++)
                {
                    int y = mHeight - 1 - y2;
                    for (int x = 0; x < mWidth; x++)
                    {
                        ltmpbta[x] = (byte)aData[x + y * mWidth];
                    }
                    for (int x = 0; x < lPadUnit; x++)
                    {
                        ltmpbta[lLineSize * 8 - 1 - x] = 0;
                    }
                    for (int x = 0; x < lLineSize; x++)
                    {
                        byte bt = 0;
                        for (int i = 0; i < 8; i++)
                        {
                            bt <<= 1;
                            bt = (byte)(bt | ltmpbta[x * 8 + i]);
                        }
                        lbta[x + y2 * lLineSize] = bt;
                    }
                }
                return lbta;

            case 4:
                lLineSize = ((mWidth * 4 + 31) & ~31) >> 3;
                lPadUnit = lLineSize * 2 - mWidth;

                lbta = new byte[lLineSize * mHeight];
                ltmpbta = new byte[lLineSize * 2];

                for (int y2 = 0; y2 < mHeight; y2++)
                {
                    int y = mHeight - 1 - y2;
                    for (int x = 0; x < mWidth; x++)
                    {
                        ltmpbta[x] = (byte)aData[x + y * mWidth];
                    }
                    for (int x = 0; x < lPadUnit; x++)
                    {
                        ltmpbta[lLineSize * 2 - 1 - x] = 0;
                    }
                    for (int x = 0; x < lLineSize; x++)
                    {
                        byte bt = 0;
                        bt = ltmpbta[x * 2 + 0];
                        bt <<= 4;
                        bt = (byte)(bt | ltmpbta[x * 2 + 1]);
                        lbta[x + y2 * lLineSize] = bt;
                    }
                }
                return lbta;
            case 8:
                lLineSize = (mWidth + 3) & ~3;
                lbta = new byte[lLineSize * mHeight];
                int lPad = lLineSize - mWidth;
                for (int y2 = 0; y2 < mHeight; y2++)
                {
                    int y = mHeight - 1 - y2;
                    for (int x = 0; x < mWidth; x++)
                    {
                        lbta[x + y2 * lLineSize] = (byte)aData[x + y * mWidth];
                    }
                    for (int x = 0; x < lPad; x++)
                    {
                        lbta[(y2 + 1) * lLineSize - 1 - x] = 0;
                    }
                }
                return lbta;
            default:
                return null;
        }


    }

    private bool createStream()
    {
        if (mWidth <= 0) return false;
        if (mHeight <= 0) return false;
        if (mPalette == null) return false;
        if ((mPalette.Length < 1) || (mPalette.Length) > 256)
        {
            return false;
        }
        if (mData == null) return false;
        if (mData.Length != mWidth * mHeight) return false;

        int lBitSize = 0;

        if (mPalette.Length <= 2)
        {
            lBitSize = 1;
        }
        else if (mPalette.Length <= 16)
        {
            lBitSize = 4;
        }
        else
        {
            lBitSize = 8;
        }

        int lDataSize = ((lBitSize * mWidth + 31) & ~31) >> 3;

        int lOffset = 14 + 40 + mPalette.Length * 4;
        int lSize = lOffset + lDataSize;

        byte[] lbta;
        lbta = makeBITMAPFILEHEADER
        (
            0x4D42,
            (uint)lSize,
            0,
            0,
            (uint)lOffset
        );

        MemoryStream ms = new MemoryStream(lSize);
        ms.Position = 0;
        ms.Write(lbta, 0, lbta.Length);

        lbta = makeBITMAPINFOHEADER
        (
            40,
            mWidth,
            mHeight,
            1,
            (ushort)lBitSize,
            0,
            (uint)lDataSize,
            3780,
            3780,
            (uint)mPalette.Length,
            0
        );

        ms.Write(lbta, 0, lbta.Length);

        lbta = makeRGBQUAD(mPalette);
        ms.Write(lbta, 0, lbta.Length);

        lbta = makePixelData(mData, lBitSize);
        ms.Write(lbta, 0, lbta.Length);
        ms.Flush();
        mStream = ms;
        ms.Position = 0;
        return true;
    }

    public bool saveToStream(Stream aStream)
    {
        bool b = true;

        if (mStream == null)
        {
            b = createStream();
        }

        if (b)
        {
            mStream.Position = 0;
            aStream.Position = 0;
            mStream.WriteTo(aStream);
        }
        return b;
    }

}
