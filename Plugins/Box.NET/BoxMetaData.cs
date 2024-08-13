using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Box.NET
{
    /// <summary>
    /// Represents metadata of file or folder object
    /// </summary>
    public class BoxMetaData
    {
        /// <summary>
        /// Specifies whether object is folder
        /// </summary>
        public bool IsFolder { get; internal set; }
        /// <summary>
        /// Specifies whether object is file
        /// </summary>
        public bool IsFile { get; internal set; }
        /// <summary>
        /// Returns object id
        /// </summary>
        public string Id { get; internal set; }
        /// <summary>
        /// Returns object name
        /// </summary>
        public string Name { get; internal set; }
        /// <summary>
        /// Returns object creation date/time (on Box server)
        /// </summary>
        public DateTime CreatedAt { get; internal set; }
        /// <summary>
        /// Returns object modification date/time (on Box server)
        /// </summary>
        public DateTime ModifiedAt { get; internal set; }
        //public DateTime ContentCreatedAt { get; internal set; }
        //public DateTime ContentModifiedAt { get; internal set; }
        /// <summary>
        /// Returns object description
        /// </summary>
        public string Description { get; internal set; }
        /// <summary>
        /// Returns object size
        /// </summary>
        public int Size { get; internal set; }
        /// <summary>
        /// Returns parent object of current object
        /// </summary>
        public BoxMetaData Parent { get; internal set; }
        /// <summary>
        /// Specifies whether object is deleted
        /// </summary>
        public bool IsDeleted { get; internal set; }
        /// <summary>
        /// Returns array of child objects of current object
        /// </summary>
        public BoxMetaData[] Items { get; internal set; }
    }
}