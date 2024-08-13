using System;

namespace GDrive.NET
{
    /// <summary>
    /// Represents partion metadata of Google Drive object
    /// </summary>
    public class GDMetaData
    {
        /// <summary>
        /// Gets object id
        /// </summary>
        public string Id { get; internal set; }
        /// <summary>
        /// Gets object name
        /// </summary>
        public string Name { get; internal set; }
        /// <summary>
        /// Gets object parent id
        /// </summary>
        public string ParentId { get; internal set; }
        /// <summary>
        /// Gets object last modified date
        /// </summary>
        public DateTime LastModified { get; internal set; }
        /// <summary>
        /// Gets value specifying whther object is folder
        /// </summary>
        public bool IsFolder { get; internal set; }
        /// <summary>
        /// Gets object extension
        /// </summary>
        public string FileExtension { get; internal set; }
        /// <summary>
        /// Object's download URL
        /// </summary>
        public string DownloadUrl { get; internal set; }
    }
}
