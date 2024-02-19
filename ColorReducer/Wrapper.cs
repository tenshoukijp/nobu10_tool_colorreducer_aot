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

class CWrapper
{
    public bool gdwrap(Bitmap aSrcBmp, out Bitmap aDestBmp,
                        ref TiZYINSoption aOption,
                        iZYINSProgress aProgressFunc,
                        byte[] aExt)
    {
        int minpal = 0;
        CQuant q = new CQuant();
        switch (aOption.inlevel + 5)
        {
            case 5:
                q.picturemask = 7;
                break;
            case 6:
                q.picturemask = 3;
                break;
            case 7:
                q.picturemask = 1;
                break;
            default:
                q.picturemask = 0;
                break;
        }

        q.indexvolume = aOption.palletnum;
        q.paletteprecision = aOption.outlevel;
        q.CbMag = aOption.yblevel * 5 + 50 - 15;
        q.CrMag = aOption.crlevel * 5 + 50 + 5;
        q.CgMag = aOption.gmlevel * 5 + 50 + 25;

        q.ditherlevel = aOption.ditherlevel;
        q.addpalettenum = aOption.addfix256num;
        for (int i = 0; i < aOption.addfix256num; i++)
        {
            q.addpalette[i] = aOption.addfix256[i];
        }

        q.transparencycolor = aOption.addpalettealpha;
        q.dithermode = aOption.dithermode;

        minpal += q.addpalettenum;
        if (q.transparencycolor >= 0) minpal++;
        if (q.indexvolume < minpal) q.indexvolume = minpal;

        q.SrcBitmap = aSrcBmp;
        q.picx = aSrcBmp.Width;
        q.picy = aSrcBmp.Height;
        q.progress = aProgressFunc;
        CGraphArray cg = new CGraphArray();
        q.m_GraphArray = cg;

        bool res = q.Execute();
        aDestBmp = q.DestBitmap;

        return res;
    }
}
