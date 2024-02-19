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
using System.Collections;
using System.Drawing;
using ZGUtils.Image;
using System.IO;

namespace iZYINS;

public delegate bool iZYINSProgress(int n);


public struct TiZYINSoption
{
    public int palletnum;
    public uint[] addfix256;
    public int addfix256num;
    public int dithermode;
    public int ditherlevel;
    public int wblevel;
    public int yblevel;
    public int crlevel;
    public int gmlevel;
    public int outlevel;
    public int inlevel;
    public int addpalettealpha;
    public int priority;
    public bool BenchMark;
}

public class PALSTR : IComparable
{
    public int c;
    public int z;


    public int CompareTo(object obj)
    {
        PALSTR a = (PALSTR)this;
        PALSTR b = (PALSTR)obj;

        return a.z - b.z;
    }

}

public class Anime2line
{
    public int pal;
    public int d;
}

public class Anime2lineComparer : IComparer
{
    public int Compare(object a, object b)
    {
        int n = (((Anime2line)a).d) - (((Anime2line)b).d);
        if (n > 0) return 1;
        if (n < 0) return -1;
        return (((Anime2line)a).pal) - (((Anime2line)b).pal);
    }
}


public class CQuant
{
    protected const int BARCO = 0;
    protected const int BARCR = 20;
    protected const int BARDO = 20;
    protected const int BARDR = 80;
    protected const int BAREO = 100;
    protected const int BARER = 300;
    protected const int BARFO = 400;
    protected const int BARFR = 50;
    protected const int BARAO = 450;
    protected const int BARAR = 350;
    protected const int BARBO = 800;
    protected const int BARBR = 200;

    protected const int CUBEX = 40;
    protected const int CUBEY = 40;
    protected const int CUBEZ = 40;
    protected const int CUBEZO = 64 * 512 * 16;
    protected const int CUBEALL = CUBEX * CUBEY * CUBEZ;
    protected const int CUBEH = 64 * 2560 * 16;
    protected const int CUBESTEP = 64 * 128 * 16;
    protected const int CUBESTEPH = 64 * 64 * 16;


    public uint m_CRC;

    public CGraphArray m_GraphArray;

    public int paletteprecision;
    public int CbMag;
    public int CrMag;
    public int CgMag;
    public int indexvolume;
    public int ditherlevel;
    public uint[] addpalette = new uint[256];
    public int addpalettenum;

    public int picturemask;
    public int transparencycolor;
    public int dithermode;

    public MEDBOX[] medbox = new MEDBOX[768 + 1];
    public GCS[] gcstable = new GCS[256];
    public MED[] smedbuf;
    public MED[] medbuf;
    public int transpal;
    public byte[] palbuf;
    public int[] picbuf;
    public int[] orgbuf;
    public int picx, picy;
    public int gpsx, gpsy, gpsc;
    public int gpsdx, gpsdy, gpsdz;

    public int mxdx, mxdy, mxdz;
    public int makepalettenum;
    public Int16[][] qcube = new Int16[CUBEALL][];
    public int trans;


    static int[,] fbpattern = {
                                    { 3,5,1 },
                                    { 7,0,0 }
                                };


    static int[,] maempattern ={
                                        { 1,3,5,3,1 },
                                    { 3,5,7,5,3 },
                                    { 5,7,0,0,0 }
                                };

    static protected void array_hsort(int[] ab, int idx, int n)
    {
        int a = idx - 1;

        int i, j, k;
        int x;

        for (k = n / 2; k >= 1; k--)
        {
            i = k;
            x = ab[a + i];
            while ((j = 2 * i) <= n)
            {
                if (j < n && (((ab[a + j]) & 0xfeffffff) < ((ab[a + j + 1]) & 0xfeffffff))) j++;
                if ((x & 0xfeffffff) >= (ab[a + j] & 0xfeffffff)) break;
                ab[a + i] = ab[a + j];
                i = j;
            }
            ab[a + i] = x;
        }
        while (n > 1)
        {
            x = ab[a + n];
            ab[a + n] = ab[a + 1];
            n--;
            i = 1;
            while ((j = 2 * i) <= n)
            {
                if (j < n && (((ab[a + j]) & 0xfeffffff) < ((ab[a + j + 1]) & 0xfeffffff))) j++;
                if ((x & 0xfeffffff) >= ((ab[a + j]) & 0xfeffffff)) break;
                ab[a + i] = ab[a + j];
                i = j;
            }
            ab[a + i] = x;
        }

    }

    public iZYINSProgress progress = null;

    protected void makeCompressablePalette(PALSTR[] aPal, int aNum)
    {
        int tr = 0, tg = 0, tb = 0;

        int[] lra = new int[aNum];
        int[] lga = new int[aNum];
        int[] lba = new int[aNum];

        for (int i = 0; i < aNum; i++)
        {
            int r = (aPal[i].c & 0x00FF0000) >> 16;
            int g = (aPal[i].c & 0x0000FF00) >> 8;
            int b = (aPal[i].c & 0x000000FF);
            tr += r;
            tg += g;
            tb += b;
            lra[i] = r;
            lga[i] = g;
            lba[i] = b;
        }


        int nr, ng, nb;

        nr = tr & 0xFF;
        ng = tg & 0xFF;
        nb = tb & 0xFF;
        nr = (0x0100 - nr) & 0xFF;
        ng = (0x0100 - ng) & 0xFF;
        nb = (0x0100 - nb) & 0xFF;

        for (int i = 0; i < aNum; i++)
        {
            if (nr == 0) break;
            if (nr >= 0x04)
            {
                if (lra[i] <= (0xFF - 0x04))
                {
                    lra[i] += 0x04;
                    nr -= 0x04;
                }
            }
            else
            {
                if (lra[i] <= (0xFF - 0x04))
                {
                    lra[i] += nr;
                    nr = 0x00;
                }
            }
        }

        if (nr != 0x00)
        {
            for (int i = 0; i < aNum; i++)
            {
                int r = (aPal[i].c & 0x00FF0000) >> 16;
                lra[i] = r;
            }
            nr = tr & 0xFF;
            for (int i = 0; i < aNum; i++)
            {
                if (nr == 0) break;
                if (nr >= 0x04)
                {
                    if (lra[i] >= 0x04)
                    {
                        lra[i] -= 0x04;
                        nr -= 0x04;
                    }
                }
                else
                {
                    if (lra[i] >= 0x04)
                    {
                        lra[i] -= nr;
                        nr = 0x00;
                    }
                }
            }
        }

        for (int i = 0; i < aNum; i++)
        {
            if (ng == 0) break;
            if (ng >= 0x04)
            {
                if (lga[i] <= (0xFF - 0x04))
                {
                    lga[i] += 0x04;
                    ng -= 0x04;
                }
            }
            else
            {
                if (lga[i] <= (0xFF - 0x04))
                {
                    lga[i] += ng;
                    ng = 0x00;
                }
            }
        }

        if (ng != 0x00)
        {
            for (int i = 0; i < aNum; i++)
            {
                int g = (aPal[i].c & 0x0000FF00) >> 8;
                lga[i] = g;
            }
            ng = tg & 0xFF;
            for (int i = 0; i < aNum; i++)
            {
                if (ng == 0) break;
                if (ng >= 0x04)
                {
                    if (lga[i] >= 0x04)
                    {
                        lga[i] -= 0x04;
                        ng -= 0x04;
                    }
                }
                else
                {
                    if (lga[i] >= 0x04)
                    {
                        lga[i] -= ng;
                        ng = 0x00;
                    }
                }
            }
        }

        for (int i = 0; i < aNum; i++)
        {
            if (nb == 0) break;
            if (nb >= 0x04)
            {
                if (lba[i] <= (0xFF - 0x04))
                {
                    lba[i] += 0x04;
                    nb -= 0x04;
                }
            }
            else
            {
                if (lba[i] <= (0xFF - 0x04))
                {
                    lba[i] += nb;
                    nb = 0x00;
                }
            }
        }

        if (nb != 0x00)
        {
            for (int i = 0; i < aNum; i++)
            {
                int b = (aPal[i].c & 0x000000FF);
                lba[i] = b;
            }
            nb = tb & 0xFF;
            for (int i = 0; i < aNum; i++)
            {
                if (nb == 0) break;
                if (nb >= 0x04)
                {
                    if (lba[i] >= 0x04)
                    {
                        lba[i] -= 0x04;
                        nb -= 0x04;
                    }
                }
                else
                {
                    if (lba[i] >= 0x04)
                    {
                        lba[i] -= nb;
                        nb = 0x00;
                    }
                }
            }
        }


        for (int i = 0; i < aNum; i++)
        {
            aPal[i].c = (lra[i] << 16) | (lga[i] << 8) | (lba[i]);
        }

    }

    public void SetProgPos(int i)
    {
        terminated = progress(i);
    }

    public CColorSpace cs = new CColorSpace();
    public CDivideBox dv = new CDivideBox();

    public Bitmap SrcBitmap;
    public Bitmap DestBitmap;

    protected byte colortrim1(double d)
    {
        return (byte)(Math.Max(0.0, Math.Min(255.0, d)) + 0.5);
    }

    public bool Execute()
    {
        this.terminated = false;

        CGraphArray cg = m_GraphArray;
        cg.Width = this.picx;
        cg.Height = this.picy;
        cg.SrcBMP = this.SrcBitmap;
        cg.MakeArray();
        orgbuf = cg.m_array;


        int i;
        int colorcount;
        int[] tmpbuf;
        GCS[][] dt = new GCS[3][];
        PALSTR[] pal = new PALSTR[256];

        int lSrcColorCount = 0;

        double[] wrapmx = new double[13], wrapmn = new double[13];

        double[,] wrapsign = {
                {  1, 0, 0 },
                {  0, 1, 0 },
                {  0, 0, 1 },
                {  1, 1, 0 },
                {  1, 0, 1 },
                {  1, 0,-1 },
                {  1,-1, 0 },
                {  0, 1, 1 },
                {  0, 1,-1 },
                {  1, 1, 1 },
                {  1,-1, 1 },
                {  1,-1,-1 },
                {  1, 1,-1 }
            };

        tmpbuf = null;
        medbuf = null;

        for (i = 0; i < 256; i++)
        {
            pal[i] = new PALSTR();
            pal[i].z = int.MaxValue;
        }

        bool pictureconvertfinish = false;

        SetProgPos(BARCR / 4 + BARCO);

        for (i = 0; i < CUBEALL; i++) qcube[i] = null;
        dt[0] = dt[1] = dt[2] = null;

        {
            palbuf = new byte[picx * picy];
        }

        picbuf = new int[picx * picy];

        makepalettenum = indexvolume;

        makepalettenum -= this.addpalettenum;
        if (transparencycolor >= 0)
        {
            makepalettenum--;
            switch (transparencycolor)
            {
                case 0:
                    trans = orgbuf[0];
                    break;
                case 1:
                    trans = orgbuf[(picy - 1) * picx];
                    break;
                case 2:
                    trans = orgbuf[picx - 1];
                    break;
                default:
                    trans = orgbuf[picx * picy - 1];
                    break;
            }
        }
        else
            trans = 0x01000000;

        cs.Create(CbMag, CrMag, CgMag, paletteprecision);

        {
            int adp = 0;
            int m, r_, rgb;
            int[] t = new int[256];
            int loop, lp;
            int k, j, c, ct;


            loop = 1;

            m = 0xff & ~picturemask;
            k = (picturemask + 1) / 2;
            for (i = 0; i < 256; i++)
            {
                r_ = i + k;
                if (r_ > 255) r_ = 255;
                r_ &= m;
                t[i] = r_;
            }

            for (i = 0; i < makepalettenum; i++) pal[i].c = 0;
            for (lp = 0; lp < loop; lp++)
            {

                {

                    int[] sobel1 =
                    {
                             1, 0,-1,
                             2, 0,-2,
                             1, 0,-1
                    };
                    int[] sobel2 =
                    {
                             1, 2, 1,
                             0, 0, 0,
                            -1,-2,-1
                    };

                    byte[] tmpbuf_byte;
                    int r, g, b, rm, gm, bm;
                    int rx, gx, bx;
                    int dx, dy, x, y;
                    int rmx, rmn, gmx, gmn, bmx, bmn;

                    int ry, gy, by;
                    int sa;

                    tmpbuf_byte = new byte[picx * picy * 6];
                    for (i = 0; i < tmpbuf_byte.Length; i++)
                    {
                        tmpbuf_byte[i] = 0;
                    }

                    for (y = 0; y < picy; y++)
                    {
                        for (x = 0; x < picx; x++)
                        {
                            if (x > 0 && x < picx - 1 && y > 0 && y < picy - 1)
                            {

                                rx = 0;
                                gx = 0;
                                bx = 0;
                                ry = 0;
                                gy = 0;
                                by = 0;
                                rmx = 0;
                                gmx = 0;
                                bmx = 0;
                                rmn = 1000;
                                gmn = 1000;
                                bmn = 1000;

                                c = orgbuf[(x) + (y) * picx];
                                rm = (c) & 0xff;
                                gm = (c >> 8) & 0xff;
                                bm = (c >> 16) & 0xff;

                                sa = 0;
                                for (dy = -1; dy < 2; dy++)
                                {
                                    for (dx = -1; dx < 2; dx++)
                                    {
                                        c = orgbuf[(x + dx) + (y + dy) * picx];
                                        r = (c) & 0xff;
                                        g = (c >> 8) & 0xff;
                                        b = (c >> 16) & 0xff;
                                        rx += sobel1[sa] * (r - rm);
                                        gx += sobel1[sa] * (g - gm);
                                        bx += sobel1[sa] * (b - bm);
                                        ry += sobel2[sa] * (r - rm);
                                        gy += sobel2[sa] * (g - gm);
                                        by += sobel2[sa] * (b - bm);

                                        rmx = Math.Max(r, rmx);
                                        rmn = Math.Min(r, rmn);
                                        gmx = Math.Max(g, gmx);
                                        gmn = Math.Min(g, gmn);
                                        bmx = Math.Max(b, bmx);
                                        bmn = Math.Min(b, bmn);

                                        sa++;
                                    }
                                }

                                byte bt = colortrim1(128.0 - ((double)rx * 64.0 / Math.Max(rmx - rmn, 8)));
                                tmpbuf_byte[(x + y * picx) * 6 + 0] = bt;
                                tmpbuf_byte[(x + y * picx) * 6 + 1] = colortrim1(128.0 - ((double)ry * 64.0 / Math.Max(rmx - rmn, 8)));
                                tmpbuf_byte[(x + y * picx) * 6 + 2] = colortrim1(128.0 - ((double)gx * 64.0 / Math.Max(gmx - gmn, 8)));
                                tmpbuf_byte[(x + y * picx) * 6 + 3] = colortrim1(128.0 - ((double)gy * 64.0 / Math.Max(gmx - gmn, 8)));
                                tmpbuf_byte[(x + y * picx) * 6 + 4] = colortrim1(128.0 - ((double)bx * 64.0 / Math.Max(bmx - bmn, 8)));
                                tmpbuf_byte[(x + y * picx) * 6 + 5] = colortrim1(128.0 - ((double)by * 64.0 / Math.Max(bmx - bmn, 8)));
                            }

                        }
                        if (this.terminated)
                        {
                            tmpbuf_byte = null;
                            goto endH;
                        }
                        SetProgPos(BARDR * y / picy + BARDO);
                    }

                    {
                        double dr, dg, db;
                        int rxm, rym, gxm, gym, bxm, bym;
                        int mm;

                        int ta = 0, tb = 0;

                        for (y = 0; y < picy; y++)
                        {
                            for (x = 0; x < picx; x++)
                            {

                                if (x > 0 && x < picx - 1 && y > 0 && y < picy - 1)
                                {

                                    rxm = tmpbuf_byte[(x + y * picx) * 6 + 0];
                                    rym = tmpbuf_byte[(x + y * picx) * 6 + 1];
                                    gxm = tmpbuf_byte[(x + y * picx) * 6 + 2];
                                    gym = tmpbuf_byte[(x + y * picx) * 6 + 3];
                                    bxm = tmpbuf_byte[(x + y * picx) * 6 + 4];
                                    bym = tmpbuf_byte[(x + y * picx) * 6 + 5];

                                    dr = 0.0;
                                    dg = 0.0;
                                    db = 0.0;

                                    for (dy = -1; dy < 2; dy++)
                                    {
                                        for (dx = -1; dx < 2; dx++)
                                        {
                                            rx = tmpbuf_byte[((x + dx) + (y + dy) * picx) * 6 + 0];
                                            ry = tmpbuf_byte[((x + dx) + (y + dy) * picx) * 6 + 1];
                                            gx = tmpbuf_byte[((x + dx) + (y + dy) * picx) * 6 + 2];
                                            gy = tmpbuf_byte[((x + dx) + (y + dy) * picx) * 6 + 3];
                                            bx = tmpbuf_byte[((x + dx) + (y + dy) * picx) * 6 + 4];
                                            by = tmpbuf_byte[((x + dx) + (y + dy) * picx) * 6 + 5];

                                            dr += Math.Sqrt((double)((rx - rxm) * (rx - rxm) + (ry - rym) * (ry - rym)));
                                            dg += Math.Sqrt((double)((gx - gxm) * (gx - gxm) + (gy - gym) * (gy - gym)));
                                            db += Math.Sqrt((double)((bx - bxm) * (bx - bxm) + (by - bym) * (by - bym)));
                                        }
                                    }
                                    mm = colortrim1(255.0 - Math.Sqrt(dr * dr + dg * dg + db * db) / 8.0);
                                    mm = (mm < 160) ? 0 : 255;
                                }
                                else mm = 0;

                                if ((rgb = orgbuf[x + y * picx]) != trans)
                                {
                                    if (mm == 0)
                                    {
                                        picbuf[x + y * picx + adp] = t[rgb & 0xff] | (t[(rgb & 0xff00) >> 8] << 8) | (t[(rgb & 0xff0000) >> 16] << 16);
                                        ta++;
                                    }
                                    else
                                    {
                                        picbuf[x + y * picx + adp] = t[rgb & 0xff] | (t[(rgb & 0xff00) >> 8] << 8) | (t[(rgb & 0xff0000) >> 16] << 16) | 0x01000000;
                                        tb++;
                                    }
                                }
                                else
                                    picbuf[x + y * picx + adp] = 0x40000000;
                            }
                            if (terminated)
                            {
                                tmpbuf_byte = null;
                                goto endH;
                            }
                            SetProgPos(BARER * y / picy + BAREO);
                        }
                        if (tmpbuf_byte != null) tmpbuf_byte = null;
                    }
                }

                adp += picx * picy;
            }
            tmpbuf = new int[adp * 2];
            array_hsort(picbuf, 0, adp);


            c = picbuf[0] & 0xffffff;
            ct = 1;
            colorcount = 0;
            for (i = 1; i < adp; i++)
            {
                if (c != (k = picbuf[i]))
                {
                    tmpbuf[colorcount * 2] = ct;
                    tmpbuf[(colorcount++) * 2 + 1] = c;
                    c = k & 0xffffff;
                    ct = 0;
                }
                if ((k & 0x1000000) > 0) ct += 8; else ct += 1;
            }
            if (c != 0x40000000)
            {
                tmpbuf[colorcount * 2] = ct;
                tmpbuf[(colorcount++) * 2 + 1] = c;
            }
            if (terminated) goto endH;

            lSrcColorCount = tmpbuf.Length / 2;


            SetProgPos(BARFR / 2 + BARFO);

            medbuf = new MED[colorcount];
            smedbuf = new MED[colorcount];

            for (i = 0; i < medbuf.Length; i++)
            {
                medbuf[i] = new MED();
            }


            for (i = 0; i < colorcount; i++)
            {
                j = tmpbuf[i * 2 + 1];
                medbuf[i].cnt = tmpbuf[i * 2];
                cs.convRGBtoGCS(out (medbuf[i].gcs), (uint)j);

                smedbuf[i] = medbuf[i];
            }
            if (terminated) goto endH;
            if (tmpbuf != null) tmpbuf = null;
        }

        if (terminated) goto endH;
        SetProgPos(BARFR + BARFO);
        {
            bool f;
            int j, k, mxa, ct, cl, makepal;
            double mx;

            dv.makeMEDbox(medbox, smedbuf, 0, 0, colorcount, cs);


            if (terminated) goto endH;
            if (makepalettenum != 0) SetProgPos(BARAR * 1 / makepalettenum + BARAO);
            i = 1;
            makepal = 0;
            while (makepal < makepalettenum && i < 760)
            {
                mxa = 0;
                mx = medbox[mxa].strain;
                for (j = 1; j < i; j++)
                {
                    if (mx < medbox[j].strain)
                    {
                        mxa = j;
                        mx = medbox[mxa].strain;
                    }
                }

                if (mx > 0)
                {
                    if (dv.divideBox(medbox, smedbuf, mxa, i, cs) != true) continue;
                }
                else
                {
                    ct = 0;
                    for (j = 0; j < i; j++)
                    {
                        cl = medbox[j].rgb;
                        f = true;
                        for (k = 0; k < ct; k++) if (pal[k].c == cl) f = false;
                        if (f == true)
                        {
                            GCS a;
                            cs.convRGBtoGCS(out a, (uint)cl);
                            pal[ct].c = cl;
                            pal[ct].z = a.Z;
                            ct++;
                        }
                    }
                    makepal = ct;

                    break;
                }
                i++;

                ct = 0;
                for (j = 0; j < i; j++)
                {
                    cl = medbox[j].rgb;
                    f = true;
                    for (k = 0; k < ct; k++) if (pal[k].c == cl) f = false;
                    if (f == true)
                    {
                        GCS a;
                        cs.convRGBtoGCS(out a, (uint)cl);
                        pal[ct].c = cl;
                        pal[ct].z = a.Z;
                        ct++;
                    }
                }
                makepal = ct;

                if (makepal == 0x7b)
                {
                    if (terminated) goto endH;
                }

                SetProgPos((int)(BARAR * (((1 - 1.0 / Math.Sqrt((double)Math.Max(makepal, 1))) / 2) + ((double)makepal / makepalettenum / 2.0)) + BARAO));
            }
        }

        if (medbuf != null) medbuf = null;

        {

            int m = makepalettenum;

            if ((lSrcColorCount >= 10000) && (m >= 128))
            {
                makeCompressablePalette(pal, m);
            }

            for (i = 0; i < addpalettenum; i++)
            {
                pal[m + i].c = (int)addpalette[i];
                GCS a;
                cs.convRGBtoGCS(out a, addpalette[i]);
                pal[m + i].z = a.Z;
            }
            Array.Sort(pal, 0, m);
        }
        {
            int j, m, x, y, z;
            double tx = 0, ty = 0, tz = 0;
            double d;

            m = makepalettenum;
            m += addpalettenum;

            for (i = 0; i < m; i++)
            {
                {
                    j = pal[i].c;
                }
                cs.convRGBtoGCS(out gcstable[i], (uint)j);

                tx += x = gcstable[i].X;
                ty += y = gcstable[i].Y;
                tz += z = gcstable[i].Z;

                for (j = 0; j < 13; j++)
                {
                    d = x * wrapsign[j, 0] + y * wrapsign[j, 1] + z * wrapsign[j, 2];
                    if (i == 0) wrapmx[j] = wrapmn[j] = d;
                    else
                    {
                        if (d < wrapmn[j]) wrapmn[j] = d;
                        if (d > wrapmx[j]) wrapmx[j] = d;
                    }
                }

            }



            mxdx = (int)(wrapmx[0] - wrapmn[0]);
            mxdy = (int)(wrapmx[1] - wrapmn[1]);
            mxdz = (int)(wrapmx[2] - wrapmn[2]);
        }
        if (terminated) goto endH;

        makepalettenum = indexvolume;
        if (transparencycolor >= 0)
        {
            transpal = --makepalettenum;
            pal[makepalettenum].c = trans;
        }



        {
            int loop, lp;
            loop = 1;
            for (lp = 0; lp < loop; lp++)
            {
                if (dithermode == 0)
                {
                    GCS gcs1;
                    int x, y, k;
                    for (y = 0; y < picy; y++)
                    {
                        for (x = 0; x < picx; x++)
                        {
                            i = orgbuf[x + y * picx];
                            if (i == trans)
                            {
                                k = transpal;
                            }
                            else
                            {
                                cs.convRGBtoGCS(out gcs1, (uint)i);
                                k = findClosepalette(gcs1.X, gcs1.Y, gcs1.Z);
                            }
                            palbuf[x + y * picx] = (byte)k;
                        }
                        if (terminated) goto endH;
                        SetProgPos(BARBR * y / picy + BARBO);
                    }
                }
                else if (dithermode == 2 || dithermode == 3)
                {
                    GCS gcs1;
                    GCS[] dttmp;
                    int x, y, k, gx, gy, gz, dv, dvs, dx, dy, dz, sx, sy, m, kk;

                    for (i = 0; i < 3; i++)
                    {
                        dt[i] = new GCS[picx + 4];
                        for (int i2 = 0; i2 < dt[i].Length; i2++)
                        {
                            dt[i][i2].X = 0;
                            dt[i][i2].Y = 0;
                            dt[i][i2].Z = 0;
                        }
                    }
                    dv = 1;
                    dvs = -1;
                    for (y = 0; y < picy; y++)
                    {
                        for (x = ((dv == 1) ? 0 : picx - 1); x < picx && x >= 0; x += dv)
                        {
                            i = orgbuf[x + y * picx];
                            if (i == trans)
                            {
                                k = transpal;
                            }
                            else
                            {
                                cs.convRGBtoGCS(out gcs1, (uint)i);
                                dx = dy = dz = 0;
                                if (dithermode == 2)
                                {
                                    for (sx = -1; sx < 2; sx++) for (sy = 0; sy < 2; sy++)
                                        {
                                            if ((m = fbpattern[1 - sy, sx + 1]) > 0)
                                            {
                                                dx += dt[sy][kk = x + 2 + sx * dv].X * m;
                                                dy += dt[sy][kk].Y * m;
                                                dz += dt[sy][kk].Z * m;
                                            }
                                        }
                                    dx /= 160;
                                    dy /= 160;
                                    dz /= 160;
                                }
                                else
                                {
                                    for (sx = -2; sx < 3; sx++) for (sy = 0; sy < 3; sy++)
                                        {
                                            if ((m = maempattern[2 - sy, sx + 2]) > 0)
                                            {
                                                dx += dt[sy][kk = x + 2 + sx * dv].X * m;
                                                dy += dt[sy][kk].Y * m;
                                                dz += dt[sy][kk].Z * m;
                                            }
                                        }
                                    dx /= 480;
                                    dy /= 480;
                                    dz /= 480;
                                }

                                if (dx > mxdx) dx = mxdx; else if (dx < -mxdx) dx = -mxdx;
                                if (dy > mxdy) dy = mxdy; else if (dy < -mxdy) dy = -mxdy;
                                if (dz > mxdz) dz = mxdz; else if (dz < -mxdz) dz = -mxdz;

                                gx = gcs1.X + dx;
                                gy = gcs1.Y + dy;
                                gz = gcs1.Z + dz;
                                k = findClosepalette(gx, gy, gz);
                                dt[0][x + 2].X = (gx - gcstable[k].X) * ditherlevel;
                                dt[0][x + 2].Y = (gy - gcstable[k].Y) * ditherlevel;
                                dt[0][x + 2].Z = (gz - gcstable[k].Z) * ditherlevel;
                            }

                            palbuf[x + y * picx] = (byte)k;
                        }
                        dttmp = dt[2]; dt[2] = dt[1]; dt[1] = dt[0]; dt[0] = dttmp;
                        for (int i2 = 0; i2 < dt[0].Length; i2++)
                        {
                            dt[0][i2].X = 0;
                            dt[0][i2].Y = 0;
                            dt[0][i2].Z = 0;
                        }

                        if (terminated) goto endH;
                        SetProgPos(BARBR * y / picy + BARBO);
                        dv = dv * dvs;
                    }
                }
                else if (dithermode == 1)
                {
                    Biscan(picx, picy);
                }
                else if (dithermode == 4)
                {
                    Mtscan(picx, picy);
                }
                {
                    for (i = 0; i < indexvolume; i++) m_GraphArray.m_palette[i] = pal[i].c;

                    m_GraphArray.PaletteVolume = indexvolume;
                }
            }//loop

        }

        pictureconvertfinish = true;

    endH:
        if (pictureconvertfinish)
        {
            int x, y;
            for (y = 0; y < picy; y++)
            {
                for (x = 0; x < picx; x++)
                {
                    orgbuf[x + y * picx] = palbuf[x + y * picx];
                }
            }
            {
                MemoryStream lDestMS = new MemoryStream();
                CSaveBitmap csb = new CSaveBitmap();
                if (csb.setData(cg.Width, cg.Height, m_GraphArray.m_palette, orgbuf))
                {
                    csb.saveToStream(lDestMS);
                }
                else
                {
                    lDestMS = null;
                    return false;
                }

                cg.DestBMP = new Bitmap(lDestMS);
                cg.ReverseArray();
            }
        }
        else
        {
            cg.DestBMP = null;
        }


        cg.ClearArray();
        if (tmpbuf != null) tmpbuf = null;
        if (medbuf != null) medbuf = null;
        if (picbuf != null) picbuf = null;
        if (palbuf != null) palbuf = null;
        for (i = 0; i < CUBEALL; i++)
        {
            if (qcube[i] != null)
            {
                qcube[i] = null;
            }
        }
        for (i = 0; i < 3; i++)
        {
            if (dt[i] != null)
            {
                dt[i] = null;
            }
        }

        this.DestBitmap = cg.DestBMP;
        return pictureconvertfinish;

    }



    public CQuant()
    {

    }

    ~CQuant()
    {

    }

    protected bool even(int n)
    {
        return (n & 1) == 0;
    }

    protected bool terminated = false;
    protected void Bic(int m, int n, int a, int b, int c, int d)
    {
        int m1, m2;

        m1 = m2 = m / 2;
        if (even(n) && even(m1)) { m1++; m2--; }
        Bit(m1, n, a, b, c, d);
        link(a, b);
        Bit(m2, n, a, b, -c, -d);
    }

    protected void Biscan(int width, int height)
    {
        gpsx = gpsy = gpsc = 0;

        gpsdx = gpsdy = gpsdz = 0;

        link(0, 0);
        if (even(width) && even(height))
        {
            if (height > width) Bic(height, width, 0, 1, 1, 0);
            else Bic(width, height, 1, 0, 0, 1);
        }
        else Bit(width, height, 1, 0, 0, 1);
    }


    protected void Bit(int m, int n, int a, int b, int c, int d)
    {
        int i;

        if (1 == m) for (i = 1; i < n; link(c, d), i++) ;
        else if (1 == n) for (i = 1; i < m; link(a, b), i++) ;
        else
        {
            int m1, m2;

            if (m < n) Transpose(ref m, ref n, ref a, ref c, ref b, ref d);
            m1 = m2 = m / 2;
            switch (m % 4)
            {
                case 1: m2++; break;
                case 2: m1++; m2--; break;
                case 3: m1++; break;
            }
            Bic(m1, n, a, b, c, d);
            link(a, b);
            Bit(m2, n, a, b, c, d);
        }
    }

    protected void Transpose
(
    ref int a,
    ref int b,
    ref int c,
    ref int d,
    ref int e,
    ref int f)
    {
        int t;

        t = a; a = b; b = t;
        t = c; c = d; d = t;
        t = e; e = f; f = t;
    }

    protected void link(int a, int b)
    {

    }



    protected void Mtscan(int width, int height)
    {
        int x, y;
        int j, i, k, kk;
        int dx, dy, dz, gx, gy, gz;
        GCS gcs1;

        Anime2line[] a2l = new Anime2line[32];
        Anime2line[] a2l2 = new Anime2line[16];

        Anime2lineComparer a2lcomp = new Anime2lineComparer();

        for (i = 0; i < 32; i++)
            a2l[i] = new Anime2line();

        for (i = 0; i < 16; i++)
            a2l2[i] = new Anime2line();

        Byte[] dit = {
            0, 8, 2,10,
            12, 4,14, 6,
            3,11, 1, 9,
            15, 7,13, 5
        };

        for (y = 0; y < height; y++)
        {
            for (x = 0; x < width; x++)
            {
                i = orgbuf[x + y * picx];
                if (i == trans) k = transpal;
                else
                {

                    dx = dy = dz = 0;
                    cs.convRGBtoGCS(out gcs1, (uint)i);
                    for (j = 0; j < 18; j++)
                    {
                        gx = gcs1.X + dx;
                        gy = gcs1.Y + dy;
                        gz = gcs1.Z + dz;
                        a2l[j].pal = kk = findClosepalette(gx, gy, gz);
                        a2l[j].d = gcstable[kk].Z;

                        dx = (gx - gcstable[kk].X) * ditherlevel / 10;
                        dy = (gy - gcstable[kk].Y) * ditherlevel / 10;
                        dz = (gz - gcstable[kk].Z) * ditherlevel / 10;
                    }

                    // Array.Sort(a2l, 0, 16);
                    Array.Sort(a2l, a2lcomp);

                    k = a2l[dit[(y & 3) * 4 + (x & 3)]].pal;
                }
                palbuf[x + y * picx] = (byte)k;

            }
            if (terminated) return;
            SetProgPos(BARBR * y / height + BARBO);
        }
    }

    protected int findClosepaletteSub(Int16[] p, int x, int y, int z)
    {
        double mn, mt;
        int k = 0, kk;
        GCS gcs2;
        int n = 0;
        Int16[] pt = p;

        gcs2.X = x;
        gcs2.Y = y;
        gcs2.Z = z;

        mn = double.MaxValue;
        while ((kk = pt[n]) >= 0)
        {
            mt = cs.distanceGCS(ref gcs2, ref gcstable[kk]);
            if (mt < mn)
            {
                mn = mt;
                k = kk;
            }
            n++;
        }
        return k;
    }

    protected int findClosepalette(int x, int y, int z)
    {
        int k, j, i;
        GCS gcs2;
        double mn, mt;

        i = getCubeadr(x, y, z);
        if (i < 0)
        {
            gcs2.X = x;
            gcs2.Y = y;
            gcs2.Z = z;
            mn = cs.distanceGCS(ref gcs2, ref gcstable[k = 0]);
            for (j = 1; j < makepalettenum; j++)
            {
                if (mn > (mt = cs.distanceGCS(ref gcs2, ref gcstable[j])))
                {
                    mn = mt;
                    k = j;
                }
            }
        }
        else
        {
            if (qcube[i] == null) makecube(x, y, z);
            k = findClosepaletteSub(qcube[i], x, y, z);
        }
        return k;
    }

    protected void makecube(int x, int y, int z)
    {
        int c, a;
        Int16[] pt;
        int i;
        double[] dt = new double[256];
        double d, mn;
        GCS gcs1;
        Int16[] ap = new Int16[256];
        int cx, cy, cz;

        a = getCubeadr(x, y, z);

        cx = (x + CUBEH) / CUBESTEP;
        cy = (y + CUBEH) / CUBESTEP;
        cz = (z + CUBEZO) / CUBESTEP;
        d = Math.Sqrt((double)CUBESTEP * CUBESTEP * 3);
        gcs1.X = (int)(CUBESTEP * cx + CUBESTEPH - CUBEH);
        gcs1.Y = (int)(CUBESTEP * cy + CUBESTEPH - CUBEH);
        gcs1.Z = (int)(CUBESTEP * cz + CUBESTEPH - CUBEZO);

        mn = dt[0] = Math.Sqrt((double)cs.distanceGCS(ref gcs1, ref gcstable[0]));
        for (i = 1; i < makepalettenum; i++)
        {
            if (mn > (dt[i] = Math.Sqrt((double)cs.distanceGCS(ref gcs1, ref gcstable[i]))))
            {
                mn = dt[i];
            }
        }
        c = 0;
        for (i = 0; i < makepalettenum; i++)
        {
            if (dt[i] < mn + d)
            {
                ap[c++] = (Int16)i;
            }
        }

        qcube[a] = new Int16[c + 1];
        pt = qcube[a];
        for (i = 0; i < c; i++) pt[i] = ap[i];
        pt[c] = -1;
    }

    protected int getCubeadr(int x, int y, int z)
    {
        int cx, cy, cz;

        cx = (x + CUBEH) / CUBESTEP;
        cy = (y + CUBEH) / CUBESTEP;
        cz = (z + CUBEZO) / CUBESTEP;

        if (cx < 0 || cx >= CUBEX || cy < 0 || cy >= CUBEY || cz < 0 || cz >= CUBEZ) return -1;
        return (cz * CUBEY + cy) * CUBEX + cx;
    }



}
