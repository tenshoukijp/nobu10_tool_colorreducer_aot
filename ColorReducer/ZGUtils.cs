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

namespace ZGUtils;

class CZGUtils
{
    public static string makeNewFileName(string aSrcFileName)
    {
        string lSrcFolder = Path.GetDirectoryName(aSrcFileName);
        string lSrcFileName = Path.GetFileNameWithoutExtension(aSrcFileName);
        string lSrcExt = Path.GetExtension(aSrcFileName);
        int n = 2;

        string lTestFileName = lSrcFolder + Path.DirectorySeparatorChar +
                                lSrcFileName + "_(" + n.ToString() + ")" +
                                lSrcExt;
        while (File.Exists(lTestFileName))
        {
            n++;
            lTestFileName = lSrcFolder + Path.DirectorySeparatorChar +
                                    lSrcFileName + "_(" + n.ToString() + ")" +
                                    lSrcExt;
        }
        return lTestFileName;
    }
}
