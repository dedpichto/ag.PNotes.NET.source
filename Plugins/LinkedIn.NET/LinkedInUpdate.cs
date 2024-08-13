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

namespace LinkedIn.NET
{
    /// <summary>
    /// Represents LimkedIn update
    /// </summary>
    public class LinkedInUpdate
    {
        /// <summary>
        /// Gets update date
        /// </summary>
        public DateTime UpdateDate { get; }
        /// <summary>
        /// Gets update key
        /// </summary>
        public string UpdateKey { get; }
        /// <summary>
        /// Gets update type
        /// </summary>
        public string UpdateType { get; }
        /// <summary>
        /// Gets update status text
        /// </summary>
        public string UpdateStatus { get; }
        /// <summary>
        /// Gets person first name
        /// </summary>
        public string PersonFirstName { get; }
        /// <summary>
        /// Gets person last name
        /// </summary>
        public string PersonLastName { get; }
        /// <summary>
        /// Gets person headline
        /// </summary>
        public string PersonHeadLine { get; }
        /// <summary>
        /// Gets person id
        /// </summary>
        public string PersonId { get; }
        /// <summary>
        /// Gets value specified whether update is commentable
        /// </summary>
        public bool IsCommentable { get; }
        /// <summary>
        /// Gets value specified whether update is likable
        /// </summary>
        public bool IsLikable { get; }
        /// <summary>
        /// Gets value specified whether update is liked
        /// </summary>
        public bool IsLiked { get; }
        /// <summary>
        /// Gets number of comments
        /// </summary>
        public int NumberOfComments { get; }
        /// <summary>
        /// Gets number of likes
        /// </summary>
        public int NumberOfLikes { get; }

        internal LinkedInUpdate(DateTime updDate, string updKey, string updType, string status, string firstName,
                                string lastName, string headLine, string id, bool isCommentable, bool isLikable,
                                bool isLiked, int numOfComments, int numOfLikes)
        {
            UpdateDate = updDate;
            UpdateKey = updKey;
            UpdateType = updType;
            UpdateStatus = status;
            PersonFirstName = firstName;
            PersonLastName = lastName;
            PersonHeadLine = headLine;
            PersonId = id;
            IsCommentable = isCommentable;
            IsLikable = isLikable;
            IsLiked = isLiked;
            NumberOfComments = numOfComments;
            NumberOfLikes = numOfLikes;
        }
    }
}
