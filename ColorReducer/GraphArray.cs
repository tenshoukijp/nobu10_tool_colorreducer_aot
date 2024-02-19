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
using System.Drawing;

namespace iZYINS;

public class CGraphArray
{
    public void ClearArray()
    {
        m_array = null;
    }

    public int[] m_palette = new int[256];


    public int PaletteVolume;

    public Bitmap SrcBMP;
    public Bitmap DestBMP;
    public int[] m_array;
    public int Height;
    public int Width;

    public bool MakeArray()
    {
        PaletteVolume = 0;
        m_array = new int[Width * Height];
        Bitmap lsrc = SrcBMP;

        for (int y = 0; y < Height; y++)
        {
            int yWidth = y * Width;
            for (int x = 0; x < Width; x++)
            {
                Color pixel = lsrc.GetPixel(x, y);
                int ado = pixel.ToArgb();
                m_array[x + yWidth] = ado;

            }
        }

        return true;

    }

    public bool ReverseArray()
    {

        if (PaletteVolume <= 0) return false;
        return true;
    }

    public CGraphArray()
    {
        m_array = null;
        Width = 0;
        Height = 0;
        for (int i = 0; i < 256; i++) m_palette[i] = i * 256;
    }

    ~CGraphArray()
    {
        m_array = null;
    }
}
