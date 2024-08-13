using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ODrive.NET
{
    /// <summary>
    /// Represents object with limited OneDrive item properties
    /// </summary>
    public class ODMetadata
    {
        /// <summary>
        /// Item ID
        /// </summary>
        public string Id { get; set; }
        /// <summary>
        /// Item download link
        /// </summary>
        public string DownloadLink { get; set; }
        /// <summary>
        /// Item name
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// Item last modified time
        /// </summary>
        public DateTime LastModified { get; set; }
        /// <summary>
        /// Item extension
        /// </summary>
        public string Extension { get; set; }
        /// <summary>
        /// Item type (file or folder)
        /// </summary>
        public bool IsFile { get; set; }
    }
}
