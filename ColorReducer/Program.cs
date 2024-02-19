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
using System.Drawing;
using System.Drawing.Imaging;
using System.Diagnostics;
using System.Reflection;
using System.Windows.Forms;

namespace iZYINS;

partial class ColorReducer
{
    // 自分自身のバージョンを保持
    static string IZYINSVER = Assembly.GetExecutingAssembly().GetName().Version.ToString();

    protected static int changeOptionInt(string s)
    {
        if (s.Length < 3) return -1;
        string ls = s.Substring(2, s.Length - 2);
        int n = int.Parse(ls);
        return n;
    }

    static int mProgress = 0;

    static bool writeProgress(int n)
    {
        /*
        if (n >= mProgress + 50)
        {
            mProgress = n;
            Console.Write(" - " + (n / 10).ToString() + " % ");
        }
        */
        return false;
    }


    static void SubMain(string[] args)
    {

        string iZYINSVersion = "ColorREducer " + IZYINSVER;

        string s = iZYINSVersion + " : Color Reducer on .NET Framework" + "\n\r" +
            "Copyright (C) 2005-2008 Y.Nomura all rights reserved." + "\n\r" +
            "このプログラムはフリー・ソフトウェアです。\nあなたは、Free Software Foundationによって発行されたGNU一般公衆利用許諾契約書(GNU General Public License)のバージョン2、または(あなたの選択により)それ以降のバージョンのいずれかに従い、このプログラムを再配布または変更することができます。\nこのプログラムは有用であることを期待して配布されていますが、いかなる保証もありません。\n市場性または特定の目的への適合性の暗黙の保証さえもありません。\n詳細はGNU一般公衆利用許諾書をご覧ください。\nもしそうでなければ、Free Software Foundation, Inc., 59 Temple Place - Suite 330, Boston, MA 02111-1307, USA までご連絡ください。";

        int n;
        if (args.Length < 1)
        {
            if (Environment.UserInteractive)
            {
                Console.WriteLine(s);
            }
            return;
        }

        TiZYINSoption op = new TiZYINSoption();
        op.palletnum = 256;
        op.dithermode = 2;
        op.ditherlevel = 8;
        op.wblevel = 10;//unused
        op.yblevel = 10;
        op.crlevel = 10;
        op.gmlevel = 10;
        op.outlevel = 8;
        op.inlevel = 3;
        op.addpalettealpha = -1;
        op.BenchMark = false;

        string lSrcBitmap = "";
        string lDestBitmap = "";
        bool lOverwritable = false;

        uint[] lFixedPalette = null;
        uint[] lAddPalette = null;
        bool lBenchmark = false;


        op.addfix256 = new uint[256];

        for (int i = 0; i < args.Length; i++)
        {
            if (i == 0)
            {
                if (args[i][0] == '-')
                {
                    s = "error : option " + i.ToString();
                    Console.WriteLine(s);
                    return;
                }
                if (File.Exists(args[i]) == false)
                {
                    Console.WriteLine("error : There is no source file.");
                    return;
                }
                else
                {
                    lSrcBitmap = args[i];
                }
                continue;
            }

            if ((i == 1) && (args[i][0] != '-'))
            {
                if (args[i][0] == '-')
                {
                    s = "error : option " + i.ToString();
                    Console.WriteLine(s);
                    return;
                }
                else
                {
                    lDestBitmap = args[i];
                }
                continue;
            }

            if (args[i].Length < 3) continue;

            if (args[i][0] == '-')
            {
                switch (args[i][1])
                {
                    case 'Q':
                        n = changeOptionInt(args[i]);
                        if (n == 1)
                        {
                            s = "about License";
                            Console.WriteLine(s);
                            return;
                        }
                        break;
                    case 'T':
                        n = changeOptionInt(args[i]);
                        if (n == 1)
                        {
                            lBenchmark = true;
                        }
                        break;
                    case 'P':
                        n = changeOptionInt(args[i]);
                        if ((n >= 2) && (n <= 256))
                        {
                            op.palletnum = n;
                        }
                        break;
                    case 'D':
                        n = changeOptionInt(args[i]);
                        if ((n >= 0) && (n <= 4)) op.dithermode = n;
                        break;
                    case 'L':
                        n = changeOptionInt(args[i]);
                        if ((n >= 0) && (n <= 10)) op.ditherlevel = n;
                        break;
                    case 'Y':
                        n = changeOptionInt(args[i]);
                        if ((n >= 0) && (n <= 20)) op.yblevel = n;
                        break;
                    case 'C':
                        n = changeOptionInt(args[i]);
                        if ((n >= 0) && (n <= 20)) op.crlevel = n;
                        break;
                    case 'G':
                        n = changeOptionInt(args[i]);
                        if ((n >= 0) && (n <= 20)) op.gmlevel = n;
                        break;
                    case 'O':
                        n = changeOptionInt(args[i]);
                        if ((n >= 0) && (n <= 8)) op.outlevel = n;
                        break;
                    case 'I':
                        n = changeOptionInt(args[i]);
                        if ((n >= 5) && (n <= 8)) op.inlevel = n - 5;
                        break;
                    case 'A':
                        n = changeOptionInt(args[i]);
                        if ((n >= 0) && (n <= 4)) op.addpalettealpha = n - 1;
                        break;
                    case 'R':
                        n = changeOptionInt(args[i]);
                        if ((n >= 0) && (n <= 1))
                        {
                            if (n == 0)
                            {
                                lOverwritable = false;
                            }
                            else
                            {
                                lOverwritable = true;
                            }
                        }
                        break;
                    case 'F':
                        s = args[i].Substring(2, args[i].Length - 2);
                        if (File.Exists(s))
                        {
                            Bitmap bmp = new Bitmap(s);
                            if ((bmp.PixelFormat == PixelFormat.Format1bppIndexed) || (bmp.PixelFormat == PixelFormat.Format4bppIndexed) || (bmp.PixelFormat == PixelFormat.Format8bppIndexed))
                            {
                                lFixedPalette = new uint[bmp.Palette.Entries.Length];
                                for (int j = 0; j < bmp.Palette.Entries.Length; j++)
                                {
                                    lFixedPalette[j] = (uint)bmp.Palette.Entries[j].ToArgb();
                                }
                            }
                        }
                        break;
                    case 'H':
                        s = args[i].Substring(2, args[i].Length - 2);
                        if (File.Exists(s))
                        {
                            Bitmap bmp = new Bitmap(s);
                            if ((bmp.PixelFormat == PixelFormat.Format1bppIndexed) || (bmp.PixelFormat == PixelFormat.Format4bppIndexed) || (bmp.PixelFormat == PixelFormat.Format8bppIndexed))
                            {
                                lAddPalette = new uint[bmp.Palette.Entries.Length];
                                for (int j = 0; j < bmp.Palette.Entries.Length; j++)
                                {
                                    lAddPalette[j] = (uint)bmp.Palette.Entries[j].ToArgb();
                                }
                            }
                        }
                        break;
                    default:
                        continue;
                }
            }
        }


        if (lDestBitmap == "")
        {
            lDestBitmap = ZGUtils.CZGUtils.makeNewFileName(lSrcBitmap);
        }

        if (lOverwritable == false)
        {
            if (File.Exists(lDestBitmap))
            {
                Console.WriteLine("cannot overwrite a file.");
                return;
            }
        }

        if (File.Exists(lSrcBitmap) == false)
        {
            return;
        }

        Stopwatch sw = null;
        if (lBenchmark)
        {
            sw = new Stopwatch();
            sw.Reset();
            sw.Start();
        }

        Bitmap lSrcBMP = new Bitmap(lSrcBitmap);
        Bitmap lDestBMP;


        if (lFixedPalette != null)
        {
            op.addfix256num = lFixedPalette.Length;
            for (int j = 0; j < lFixedPalette.Length; j++)
            {
                op.addfix256[j] = lFixedPalette[j];
            }
        }
        else if (lAddPalette != null)
        {
            op.addfix256num = lAddPalette.Length;
            for (int j = 0; j < lAddPalette.Length; j++)
            {
                op.addfix256[j] = lAddPalette[j];
            }
        }
        else
        {
            op.addfix256 = null;
            op.addfix256num = 0;
        }

        CWrapper wrap = new CWrapper();
        byte[] extra = null;
        wrap.gdwrap(lSrcBMP, out lDestBMP, ref op, writeProgress, extra);

        if ((lDestBitmap != null) && (lDestBMP != null))
        {
            string ext = Path.GetExtension(lDestBitmap).ToLower();
            if (ext == ".png")
            {
                lDestBMP.Save(lDestBitmap, ImageFormat.Png);
            }
            else if (ext == ".gif")
            {
                lDestBMP.Save(lDestBitmap, ImageFormat.Gif);
            }
            else if ((ext == ".tif") || (ext == ".tiff"))
            {
                lDestBMP.Save(lDestBitmap, ImageFormat.Tiff);
            }
            else
            {
                lDestBMP.Save(lDestBitmap);
            }
        }

        if (lBenchmark)
        {
            sw.Stop();
            long li = sw.ElapsedMilliseconds;

            string logpass = System.IO.Path.GetDirectoryName(lDestBitmap);
            logpass = logpass + Path.DirectorySeparatorChar + "iZYINS.log";

            if (File.Exists(logpass))
            {
                File.Delete(logpass);
            }

            string textFile = logpass;
            System.Text.Encoding enc = System.Text.Encoding.GetEncoding("utf-8");
            string str = lDestBitmap + "\r\n";

            System.IO.File.AppendAllText(textFile, str, enc);

            str = li.ToString();
            System.IO.File.AppendAllText(textFile, str, enc);

        }

    }
}
