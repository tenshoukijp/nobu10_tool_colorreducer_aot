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


public struct RMA
{
    public double x, y, z, c;
};

public class MED
{
    public GCS gcs;
    public int cnt;
    public double flag;
};

public struct MEDBOX
{
    public double strain;
    public int bufstart, bufnum;
    public GCS mid;
    public int rgb;

    public RMA prin, std, avg;
    public double firstd;
};

public class CDivideBox
{
    protected double[,] jca = new double[3, 3];
    protected double[,] jcw = new double[3, 3];



    static private void div_hsort(MED[] ab, int idx, int n)
    {
        int a = idx - 1;
        int i, j, k;
        MED x;

        for (k = n / 2; k >= 1; k--)
        {
            i = k;
            x = ab[a + i];
            while ((j = 2 * i) <= n)
            {
                if ((j < n) && (ab[a + j].flag < (ab[a + j + 1].flag))) j++;
                if (x.flag >= ab[a + j].flag) break;
                ab[a + i] = ab[a + j]; i = j;
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
                if ((j < n) && (ab[a + j].flag < ab[a + j + 1].flag)) j++;
                if (x.flag >= ab[a + j].flag) break;
                ab[a + i] = ab[a + j];
                i = j;
            }
            ab[a + i] = x;
        }
    }

    public CDivideBox()
    {
        for (int y = 0; y < 3; y++)
        {
            for (int x = 0; x < 3; x++)
            {
                jca[x, y] = 0.0;
                jcw[x, y] = 0.0;
            }
        }
    }

    ~CDivideBox()
    {

    }

    private double dsqr(double a)
    {
        return a * a;
    }

    protected struct GCSd
    {
        public double X, Y, Z;
    };

    protected struct STC
    {
        public GCSd total;
        public double count;
        public GCSd sqr;
    };

    protected int FindSquareDiv(MED[] ix, int st, int n)
    {
        int idx;
        STC L, R;
        int i, mk = 0;
        double mn = double.MaxValue, m;
        GCSd a, b, d, e;
        double x, y, z, c;
        double xc, yc, zc;
        double mc;

        L.count = 0;
        L.total.X = 0;
        L.sqr.X = 0;
        L.total.Y = 0;
        L.sqr.Y = 0;
        L.total.Z = 0;
        L.sqr.Z = 0;
        R.count = 0;
        R.total.X = 0;
        R.sqr.X = 0;
        R.total.Y = 0;
        R.sqr.Y = 0;
        R.total.Z = 0;
        R.sqr.Z = 0;
        idx = st;
        for (i = 0; i < n; i++)
        {
            x = ix[idx].gcs.X;
            y = ix[idx].gcs.Y;
            z = ix[idx].gcs.Z;
            c = ix[idx].cnt;

            xc = x * c;
            yc = y * c;
            zc = z * c;

            R.count += c;
            R.total.X += xc;
            R.total.Y += yc;
            R.total.Z += zc;

            R.sqr.X += xc * x;
            R.sqr.Y += yc * y;
            R.sqr.Z += zc * z;

            idx++;
        }
        a.X = R.total.X / R.count;
        a.Y = R.total.Y / R.count;
        a.Z = R.total.Z / R.count;

        R.sqr.X += (double)a.X * a.X * R.count - 2.0 * a.X * R.total.X;
        R.sqr.Y += (double)a.Y * a.Y * R.count - 2.0 * a.Y * R.total.Y;
        R.sqr.Z += (double)a.Z * a.Z * R.count - 2.0 * a.Z * R.total.Z;

        idx = st;
        for (i = 1; i < n; i++)
        {

            x = ix[idx].gcs.X;
            y = ix[idx].gcs.Y;
            z = ix[idx].gcs.Z;
            c = ix[idx].cnt;
            xc = x * c;
            yc = y * c;
            zc = z * c;

            mc = Math.Max(L.count, 1.0);
            a.X = L.total.X / mc;
            a.Y = L.total.Y / mc;
            a.Z = L.total.Z / mc;
            mc = Math.Max(L.count + c, 1.0);
            d.X = (b.X = ((e.X = (L.total.X + xc)) / mc)) - a.X;
            d.Y = (b.Y = ((e.Y = (L.total.Y + yc)) / mc)) - a.Y;
            d.Z = (b.Z = ((e.Z = (L.total.Z + zc)) / mc)) - a.Z;
            L.sqr.X += d.X * d.X * L.count;
            L.sqr.Y += d.Y * d.Y * L.count;
            L.sqr.Z += d.Z * d.Z * L.count;
            L.total.X = e.X;
            L.total.Y = e.Y;
            L.total.Z = e.Z;
            L.count = mc;
            L.sqr.X += dsqr(x - b.X) * c;
            L.sqr.Y += dsqr(y - b.Y) * c;
            L.sqr.Z += dsqr(z - b.Z) * c;

            mc = Math.Max(R.count, 1.0);
            a.X = R.total.X / mc;
            a.Y = R.total.Y / mc;
            a.Z = R.total.Z / mc;
            mc = Math.Max(R.count - c, 1.0);
            d.X = (b.X = ((e.X = (R.total.X - x * c)) / mc)) - a.X;
            d.Y = (b.Y = ((e.Y = (R.total.Y - y * c)) / mc)) - a.Y;
            d.Z = (b.Z = ((e.Z = (R.total.Z - z * c)) / mc)) - a.Z;
            R.sqr.X += d.X * d.X * R.count;
            R.sqr.Y += d.Y * d.Y * R.count;
            R.sqr.Z += d.Z * d.Z * R.count;
            R.total.X = e.X;
            R.total.Y = e.Y;
            R.total.Z = e.Z;
            R.count = mc;
            R.sqr.X -= dsqr(x - b.X) * c;
            R.sqr.Y -= dsqr(y - b.Y) * c;
            R.sqr.Z -= dsqr(z - b.Z) * c;

            m = (L.sqr.X + L.sqr.Y + L.sqr.Z) + (R.sqr.X + R.sqr.Y + R.sqr.Z);
            if (mn > m)
            {
                mn = m;
                mk = i;
            }
            idx++;

        }

        return mk;
    }

    protected void jacobi(int n, double[,] a, double[,] u, double eps)
    {

        bool finish;
        int m, i, j, k;
        double p, q, t, s2, c2, c, s, r;

        finish = false;

        for (m = 0; m < 10 && finish == false; m++)
        {
            finish = true;

            for (i = 0; i < n - 1; i++)
            {
                for (j = i + 1; j < n; j++)
                {
                    if (Math.Abs(a[i, j]) < eps) continue;
                    finish = false;

                    p = (a[i, i] - a[j, j]) / 2;
                    q = a[i, j];
                    t = p / q;
                    s2 = 1.0 / Math.Sqrt(1.0 + t * t);
                    if (q < 0) s2 = -s2;
                    c2 = t * s2;
                    if (c2 > 0)
                    {
                        c = Math.Sqrt((1.0 + c2) / 2.0);
                        s = s2 / c / 2.0;
                    }
                    else
                    {
                        s = Math.Sqrt((1.0 - c2) / 2.0);
                        c = s2 / s / 2.0;
                    }
                    r = a[i, i] + a[j, j];


                    a[i, i] = r / 2.0 + p * c2 + q * s2;


                    a[j, j] = r - a[i, i];
                    a[i, j] = a[j, i] = 0;

                    for (k = 0; k < n; k++)
                    {
                        if ((k != i) && (k != j))
                        {
                            p = a[k, i];
                            q = a[k, j];
                            a[k, i] = a[i, k] = p * c + q * s;
                            a[k, j] = a[j, k] = -p * s + q * c;
                        }
                    }
                    for (k = 0; k < n; k++)
                    {
                        p = u[k, i];
                        q = u[k, j];
                        u[k, i] = p * c + q * s;
                        u[k, j] = -p * s + q * c;
                    }
                }
            }
        }

    }

    protected double setrma(MED[] medbuf, int bs, int bn, out RMA prin, out RMA std, out RMA avg)
    {
        double c;
        double d;
        double cnt = 0;
        int i, j;

        avg.x = 0;
        avg.y = 0;
        avg.z = 0;
        avg.c = 0;

        if (bn <= 0)
        {
            prin = new RMA();
            std = new RMA();
            avg = new RMA();
            return 0;
        }

        for (i = 0; i < bn; i++)
        {
            cnt += c = (medbuf[bs + i]).cnt;
            avg.x += (double)(medbuf[bs + i]).gcs.X * c;
            avg.y += (double)(medbuf[bs + i]).gcs.Y * c;
            avg.z += (double)(medbuf[bs + i]).gcs.Z * c;
        }
        avg.x /= cnt;
        avg.y /= cnt;
        avg.z /= cnt;
        avg.c = std.c = prin.c = cnt;
        {
            double[] da = new double[3];
            int dx, dy;
            for (i = 0; i < bn; i++)
            {
                c = (medbuf[bs + i]).cnt;
                da[0] = (medbuf[bs + i]).gcs.X - avg.x;
                da[1] = (medbuf[bs + i]).gcs.Y - avg.y;
                da[2] = (medbuf[bs + i]).gcs.Z - avg.z;

                for (dx = 0; dx < 3; dx++)
                    for (dy = 0; dy < 3; dy++)
                    {
                        jca[dx, dy] += da[dx] * da[dy] * c;
                    };
            }
            for (dx = 0; dx < 3; dx++)
                for (dy = 0; dy < 3; dy++)
                    jca[dx, dy] /= avg.c;

            for (dx = 0; dx < 3; dx++)
            {
                for (dy = 0; dy < 3; dy++) jcw[dx, dy] = 0;
                jcw[dx, dx] = 1;
            }
        }


        if (jca[0, 0] >= 0)
        {
            std.x = Math.Sqrt(jca[0, 0]);
        }
        else
        {
            std.x = 0;
        }
        if (jca[1, 1] >= 0)
        {
            std.y = Math.Sqrt(jca[1, 1]);
        }
        else
        {
            std.y = 0;
        }
        if (jca[2, 2] >= 0)
        {
            std.z = Math.Sqrt(jca[2, 2]);

        }
        else
        {
            std.z = 0;

        }

        jacobi(3, jca, jcw, Math.Pow(10.0, -16.0));

        j = 2; d = jca[j, j];
        for (i = 0; i < 3; i++)
        {
            if (d < jca[i, i])
            {
                j = i;
                d = jca[i, i];
            }
        }

        prin.x = jcw[0, j];
        prin.y = jcw[1, j];
        prin.z = jcw[2, j];

        return d;
    }

    public void makeMEDbox(MEDBOX[] medbox, MED[] medbuf, int ad, int bs, int bn, CColorSpace cs)
    {
        int i;
        int mx, my, mz;
        double st;
        RMA prin, std, avg;

        medbox[ad].bufstart = bs;
        medbox[ad].bufnum = bn;

        medbox[ad].firstd = setrma(medbuf, bs, bn, out prin, out std, out avg);

        medbox[ad].prin = prin;
        medbox[ad].std = std;
        medbox[ad].avg = avg;


        mx = (int)Math.Floor(avg.x + 0.5);
        my = (int)Math.Floor(avg.y + 0.5);
        mz = (int)Math.Floor(avg.z + 0.5);

        medbox[ad].rgb = cs.maskRGB(cs.convGCStoRGB(mx, my, mz));

        medbox[ad].mid.X = mx;
        medbox[ad].mid.Y = my;
        medbox[ad].mid.Z = mz;


        st = 0;
        for (i = bs; i < bs + bn; i++)
            st += (cs.distanceGCSd(ref medbuf[i].gcs, mx, my, mz)) * (double)medbuf[i].cnt;
        medbox[ad].strain = st;

    }

    public bool divideBox(MEDBOX[] medbox, MED[] medbuf, int mxa, int oi, CColorSpace cs)
    {

        int bs, bn, j;
        RMA prin, std, avg;

        bs = medbox[mxa].bufstart;
        bn = medbox[mxa].bufnum;
        if (bn == 1) goto oneerr;

        prin = medbox[mxa].prin;
        std = medbox[mxa].std;
        avg = medbox[mxa].avg;

        {
            int mm = bs + bn;
            for (j = bs; j < mm; j++)
                (medbuf[j]).flag = (medbuf[j]).gcs.X * prin.x +
                                    (medbuf[j]).gcs.Y * prin.y +
                                    (medbuf[j]).gcs.Z * prin.z;
            div_hsort(medbuf, bs, bn);
            j = FindSquareDiv(medbuf, bs, bn);


        }
        if (j < 1 || bn - j < 1)
        {
            if (bn == 2) j = 1;
            else goto oneerr;
        }



        makeMEDbox(medbox, medbuf, mxa, bs, j, cs);
        makeMEDbox(medbox, medbuf, oi, bs + j, bn - j, cs);
        return true;
    oneerr:
        makeMEDbox(medbox, medbuf, mxa, bs, bn, cs);
        medbox[mxa].strain = 0;
        return false;
    }

}//CDivideBox


//Namespace
