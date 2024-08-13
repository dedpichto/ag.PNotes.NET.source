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

using System;

namespace VKontakte.NET
{
    /// <summary>
    /// Represents VKontakte post
    /// </summary>
    public class VKPost
    {
        /// <summary>
        /// Gets post ID
        /// </summary>
        public string Id { get; }
        /// <summary>
        /// Gets post date
        /// </summary>
        public DateTime Date { get; }
        /// <summary>
        /// Gets post text
        /// </summary>
        public string Text { get; }
        /// <summary>
        /// Creates new instance of VKPost
        /// </summary>
        /// <param name="id">Post ID</param>
        /// <param name="date">Post date</param>
        /// <param name="text">Post text</param>
        internal VKPost(string id, DateTime date, string text)
        {
            Id = id;
            Date = date;
            Text = text;
        }
    }
}
