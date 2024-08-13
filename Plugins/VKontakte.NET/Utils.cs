// This program is free software; you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation; either version 2 of the License, or
// (at your option) any later version.

// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.

// You should have received a copy of the GNU General Public License
// along with this program; if not, write to the Free Software
// Foundation, Inc., 51 Franklin St, Fifth Floor, Boston, MA  02110-1301  USA

using System.Collections.Specialized;
using System.Linq;

namespace VKontakte.NET
{
    internal class Utils
    {
        internal static NameValueCollection ParseResponse(string response)
        {
            var nvc = new NameValueCollection();
            if (response.StartsWith("#")) response = response.Substring(1);
            var arr1 = response.Split('&');
            foreach (var arr2 in arr1.Select(s => s.Split('=')).Where(arr2 => arr2.Length == 2))
            {
                nvc.Add(arr2[0].Trim(), arr2[1].Trim());
            }
            return nvc;
        }
    }
}
