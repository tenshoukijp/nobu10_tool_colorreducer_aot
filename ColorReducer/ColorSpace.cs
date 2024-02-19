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

namespace iZYINS;


public struct MRX
{
    public double m11, m12,
           m21, m22;
};

public struct GCS
{
    public int X, Y, Z;
};


public class CColorSpace
{
    public int maskRGB(int rgb)
    {
        int r, g, b, vr, vg, vb, nrgb, rrgb = 0, nr, ng, nb;
        GCS gcs1, gcs2;
        double nd, d;

        convRGBtoGCS(out gcs1, (uint)rgb);
        b = (rgb & 0xff);
        g = (rgb & 0xff00) >> 8;
        r = (rgb & 0xff0000) >> 16;

        d = double.MaxValue;
        for (vr = 0; vr < 2; vr++) for (vg = 0; vg < 2; vg++) for (vb = 0; vb < 2; vb++)
                {
                    nr = maskRGBsub(masktabler, maskcountr, r, vr);
                    ng = maskRGBsub(masktableg, maskcountg, g, vg);
                    nb = maskRGBsub(masktableb, maskcountb, b, vb);
                    nrgb = ((nb) | (ng << 8) | (nr << 16));
                    convRGBtoGCS(out gcs2, (uint)nrgb);
                    if (d > (nd = distanceGCS(ref gcs1, ref gcs2)))
                    {
                        rrgb = nrgb;
                        d = nd;
                    }
                }
        return rrgb;
    }

    public double distanceGCSd(ref GCS g1, int x2, int y2, int z2)
    {
        double x, y, z;

        z = (g1.Z - z2);
        x = (g1.X - x2);
        y = (g1.Y - y2);

        return (x * x) + (y * y) + (z * z);
    }

    public double distanceGCS(ref GCS g1, ref GCS g2)
    {
        double x, y, z;

        z = (g1.Z - g2.Z);
        x = (g1.X - g2.X);
        y = (g1.Y - g2.Y);

        return (x * x) + (y * y) + (z * z);
    }

    private double colortrim2(double n)
    {
        return (Math.Max(0.0, Math.Min(1.0, n)));
    }

    public int convGCStoRGB(int X, int Y, int Z)
    {

        double R, G, B, x, y, z, rr, gg, bb, r3, g3, b3;
        int r, g, b;

        x = M2.m11 * (double)X + M2.m12 * (double)Y;
        y = M2.m21 * (double)X + M2.m22 * (double)Y;
        z = Z;

        R = z * 0.9998247134019967 + x * -0.000044452970991771824 + y * 1.7528659800326482;
        G = z * 1.0000893144198046 + x * -0.4320813565838843 + y * -0.89314419804726;
        B = z * 1.0000000115762946 + x * 2.213731098385467 + y * -0.00011576294529099052;


        r3 = colortrim2(R / 255 / 16384);
        if (r3 >= 0.0540269630587776405948631399435028)
            rr = Math.Pow(r3, 2.2 / 3.0) * 255;
        else
            rr = Math.Pow(r3 / 0.142913647595774056881018286010431, 2.2) * 255;

        g3 = colortrim2(G / 255 / 16384);
        if (g3 >= 0.0540269630587776405948631399435028)
            gg = Math.Pow(g3, 2.2 / 3.0) * 255;
        else
            gg = Math.Pow(g3 / 0.142913647595774056881018286010431, 2.2) * 255;

        b3 = colortrim2(B / 255 / 16384);
        if (b3 >= 0.0540269630587776405948631399435028)
            bb = Math.Pow(b3, 2.2 / 3.0) * 255;
        else
            bb = Math.Pow(b3 / 0.142913647595774056881018286010431, 2.2) * 255;

        r = (int)Math.Floor(rr + 0.5);
        g = (int)Math.Floor(gg + 0.5);
        b = (int)Math.Floor(bb + 0.5);

        if (r < 0) r = 0; else if (r > 255) r = 255;
        if (g < 0) g = 0; else if (g > 255) g = 255;
        if (b < 0) b = 0; else if (b > 255) b = 255;

        return (b & 0xff) | ((g & 0xff) << 8) | ((r & 0xff) << 16);
    }

    public void convRGBtoGCS(out GCS gcs, uint rgb)
    {
        double r, g, b, n1, n2, n3, n22, n33;

        b = gtable[(rgb & 0xff)];
        g = gtable[(rgb & 0xff00) >> 8];
        r = gtable[(rgb & 0xff0000) >> 16];



        n1 = (double)r * 0.2989 + (double)g * 0.5866 + (double)b * 0.1145;
        n2 = -(double)r * 0.1350 - (double)g * 0.2650 + (double)b * 0.4000;
        n3 = (double)r * 0.4000 - (double)g * 0.3346 - (double)b * 0.0653;


        n22 = M1.m11 * n2 + M1.m12 * n3;
        n33 = M1.m21 * n2 + M1.m22 * n3;

        gcs.Z = (int)Math.Floor(n1 * 16384.0 * 255.0 + 0.5);
        gcs.X = (int)Math.Floor(n22 * 16384.0 * 255.0 + 0.5);
        gcs.Y = (int)Math.Floor(n33 * 16384.0 * 255.0 + 0.5);

    }

    public void Create(int b, int r, int g, int p)
    {

        int i;
        int fr = 256, fg = 256, fb = 256;
        double n, m;

        switch (p)
        {
            case 0://web
                fr = fg = fb = 6;
                break;
            case 1://333
                fr = fg = 8;
                fb = 4;
                break;
            case 2://333
                fr = fg = fb = 8;
                break;
            case 3://444
                fr = fg = fb = 16;
                break;
            case 4://555
                fr = fg = fb = 32;
                break;
            case 5://655
                fr = fb = 32; fg = 64;
                break;
            case 6://666
                fr = fg = fb = 64;
                break;
            case 7://777
                fr = fg = fb = 128;
                break;
            case 8://888
                fr = fg = fb = 256;
                break;
        }

        {
            maskcountr = fr;
            maskcountg = fg;
            maskcountb = fb;
            for (i = 0; i < fr; i++)
                masktabler[i] = Math.Min((int)Math.Floor((double)i * 256.0 / (double)(fr - 1)), 255);
            for (i = 0; i < fg; i++)
                masktableg[i] = Math.Min((int)Math.Floor((double)i * 256.0 / (double)(fg - 1)), 255);
            for (i = 0; i < fb; i++)
                masktableb[i] = Math.Min((int)Math.Floor((double)i * 256.0 / (double)(fb - 1)), 255);
        }

        for (i = 0; i < 256; i++)
        {
            n = i / 255.0;

            if (n >= 0.117647058823529411764705882352941)
                m = Math.Pow(n, 3.0 / 2.2);
            else
                m = Math.Pow(n, 1.0 / 2.2) * 0.142913647595774056881018286010431;

            gtable[i] = m;

            if (n >= 0.20198703620241161355887485889543)
                m = Math.Pow(n, 3.0 / 2.2);
            else
                m = Math.Pow(n, 2.2) * 0.112905177848716256193056297677849 / 0.029628638340582134493141518902643;
            r2table[i] = m;

            if (n >= 0.148670653233635731053932622069695)
                m = Math.Pow(n, 3.0 / 2.2);
            else
                m = Math.Pow(n, 2.2) * 0.0743389203713377581101520770121405 / 0.0150971701329696556426866689396522;
            g2table[i] = m;

            if (n >= 0.312422190652192596516505331287941)
                m = Math.Pow(n, 3.0 / 2.2);
            else
                m = Math.Pow(n, 2.2) * 0.204649951175241102069736654437528 / 0.07734497816593886462882096069869;
            b2table[i] = m;

        }

        makeMatrix(ref M1, ref M2, (double)b / 100.0, (double)r / 100.0, (double)g / 100.0);



    }

    public CColorSpace()
    {
    }

    ~CColorSpace()
    {

    }

    protected bool makeMatrix(ref MRX a, ref MRX b,
                                double Cbm, double Crm, double Cgm)
    {

        const double Cbx = 0.5000, Cby = 0.0816;
        const double Crx = -0.1687, Cry = -0.5000;
        const double Cgx = 0.3312, Cgy = -0.4183;
        double d;

        MRX M, N;

        double Csin, Ccos, Clen;

        Clen = Math.Sqrt(Cbx * Cbx + Cby * Cby);
        if (Clen == 0) goto diverr;
        Csin = Cby / Clen;
        Ccos = Cbx / Clen;

        M.m11 = (Cbm - 1.0) * Ccos * Ccos + 1;
        M.m12 = (1.0 - Cbm) * Ccos * Csin;
        M.m21 = (1.0 - Cbm) * Ccos * Csin;
        M.m22 = (Cbm - 1.0) * Csin * Csin + 1;

        Clen = Math.Sqrt(Crx * Crx + Cry * Cry);
        if (Clen == 0) goto diverr;
        Csin = Cry / Clen;
        Ccos = Crx / Clen;

        N.m11 = (Crm - 1.0) * Ccos * Ccos + 1;
        N.m12 = (1.0 - Crm) * Ccos * Csin;
        N.m21 = (1.0 - Crm) * Ccos * Csin;
        N.m22 = (Crm - 1.0) * Csin * Csin + 1;

        makeMatrixsub(ref M, ref N);

        Clen = Math.Sqrt(Cgx * Cgx + Cgy * Cgy);
        if (Clen == 0) goto diverr;
        Csin = Cgy / Clen;
        Ccos = Cgx / Clen;

        N.m11 = (Cgm - 1.0) * Ccos * Ccos + 1;
        N.m12 = (1.0 - Cgm) * Ccos * Csin;
        N.m21 = (1.0 - Cgm) * Ccos * Csin;
        N.m22 = (Cgm - 1.0) * Csin * Csin + 1;

        makeMatrixsub(ref M, ref N);

        a.m11 = M.m11;
        a.m12 = M.m12;
        a.m21 = M.m21;
        a.m22 = M.m22;

        d = M.m11 * M.m22 - M.m12 * M.m21;
        if (d == 0) goto diverr;

        b.m11 = M.m22 / d;
        b.m12 = -M.m12 / d;
        b.m21 = -M.m21 / d;
        b.m22 = M.m11 / d;

        return true;

    diverr:
        a.m11 = 1.0;
        a.m12 = 0.0;
        a.m21 = 0.0;
        a.m22 = 1.0;

        b.m11 = 1.0;
        b.m12 = 0.0;
        b.m21 = 0.0;
        b.m22 = 1.0;

        return false;

    }

    protected int maskRGBsub(int[] m, int mc, int ain, int s)
    {
        int r, i;
        if (s == 0)
        {
            r = 0;
            for (i = 0; i < mc; i++) if (r < m[i] && m[i] <= ain) r = m[i];
        }
        else
        {
            r = 255;
            for (i = 0; i < mc; i++) if (r > m[i] && m[i] >= ain) r = m[i];
        }
        return r;

    }

    protected void makeMatrixsub(ref MRX M, ref MRX N)
    {
        MRX a;

        a.m11 = N.m11 * M.m11 + N.m12 * M.m21;
        a.m12 = N.m11 * M.m12 + N.m12 * M.m22;
        a.m21 = N.m21 * M.m11 + N.m22 * M.m21;
        a.m22 = N.m21 * M.m12 + N.m22 * M.m22;

        M.m11 = a.m11;
        M.m12 = a.m12;
        M.m21 = a.m21;
        M.m22 = a.m22;

    }

    protected MRX M1, M2;

    protected int[] masktabler = new int[256];
    protected int[] masktableg = new int[256];
    protected int[] masktableb = new int[256];
    protected int maskcountr, maskcountg, maskcountb;

    protected double[] gtable = new double[256];
    protected double[] r2table = new double[256],
                        g2table = new double[256],
                        b2table = new double[256];

    protected int palettebitmask = -1, palettebitmaskadd = -1;
}
