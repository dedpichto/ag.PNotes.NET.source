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
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace PluginsCore
{
    /// <summary>
    /// Represents synchronization result
    /// </summary>
    public enum SyncResult
    {
        /// <summary>
        /// Default value, no further interaction needed
        /// </summary>
        None,
        /// <summary>
        /// Host application should reload notes
        /// </summary>
        Reload,
        /// <summary>
        /// Different versions of databases on host and remote machines, synchronization aborted
        /// </summary>
        AbortVersion,
        /// <summary>
        /// An error occured during synchronization
        /// </summary>
        Error
    }

    /// <summary>
    /// Provides functionality for plugins interaction with host application
    /// </summary>
    public interface IPluginsHost
    {
        /// <summary>
        /// Active RichTextBox of host application
        /// </summary>
        RichTextBox ActiveTextBox { get; }
        /// <summary>
        /// Active culture of host application
        /// </summary>
        string ActiveCulture { get; }
        /// <summary>
        /// Active note name of hoast application
        /// </summary>
        string ActiveNoteName { get; }
        /// <summary>
        /// Max limit of posts to get from social network
        /// </summary>
        int LimitToGet { get; }
        /// <summary>
        /// Set of data needed for synchronization
        /// </summary>
        Dictionary<string, string> SyncParameters { get; }
    }

    /// <summary>
    /// Provides basic plugin functionality
    /// </summary>
    public interface IPlugin
    {
        /// <summary>
        /// Plugin name - must be unique
        /// </summary>
        string Name { get; }
        /// <summary>
        /// Author of plugin
        /// </summary>
        string Author { get; }
        /// <summary>
        /// Version of plugin
        /// </summary>
        string Version { get; }
        /// <summary>
        /// Product name of plugin
        /// </summary>
        string ProductName { get; }
        /// <summary>
        /// Provides any additional information about plugin
        /// </summary>
        string AdditionalInfo { get; }
        /// <summary>
        /// Initialize plugin
        /// </summary>
        /// <param name="host">Host application</param>
        void Init(IPluginsHost host);
    }

    /// <summary>
    /// Provides functionality for handling IPlugin classes
    /// </summary>
    public interface IPluginsFactory
    {
        /// <summary>
        /// Gets list of available plugins
        /// </summary>
        /// <returns>List of available plugins</returns>
        List<IPlugin> GetPlugins();
    }

    /// <summary>
    /// Provides functionality for synchronization
    /// </summary>
    public interface ISyncPlugin : IPlugin
    {
        /// <summary>
        /// Menu item for synchronization
        /// </summary>
        ToolStripMenuItem MenuSync { get; }
        /// <summary>
        /// Performs synchronization showing modal dialog box
        /// </summary>
        /// <returns>One of <see cref="SyncResult"/>constants</returns>
        SyncResult Synchronize();
        /// <summary>
        /// Fires when synchronization completes
        /// </summary>
        event EventHandler<SyncCompleteEventArgs> SyncComplete;
        /// <summary>
        /// Fires before synchronization, allowing to complete remained jobs or cancel synchronization
        /// </summary>
        event EventHandler<BeforeSyncEventArgs> BeforeSync;
    }

    /// <summary>
    /// Provides additional functionality for synchronization
    /// </summary>
    public interface ISyncEnhPlugin : ISyncPlugin
    {
        /// <summary>
        /// Current synchronization status indicator
        /// </summary>
        bool InProgress { get; set; }
        /// <summary>
        /// Performs silent synchronization
        /// </summary>
        void SynchronizeInBackground();
        /// <summary>
        /// Fires when synchronization completes
        /// </summary>
        event EventHandler<SyncCompleteEventArgs> SyncCompleteInBackground;
    }

    /// <summary>
    /// Adds async Synchronize method
    /// </summary>
    public interface ISyncAsyncPlugin : ISyncEnhPlugin
    {
        /// <summary>
        /// Performs asynchronous synchronization
        /// </summary>
        /// <returns>One of <see cref="SyncResult"/>constants</returns>
        Task<SyncResult> SynchronizeAsync();
    }

    /// <summary>
    /// Provides functionality for posting messages
    /// </summary>
    public interface IPostPlugin : IPlugin
    {
        /// <summary>
        /// Menu item for posting selected text
        /// </summary>
        ToolStripMenuItem MenuPostPartial { get; }
        /// <summary>
        /// Menu item for posting entire text
        /// </summary>
        ToolStripMenuItem MenuPostFull { get; }
        /// <summary>
        /// Menu item for getting posts and further replacing selected text
        /// </summary>
        ToolStripMenuItem MenuGetPartial { get; }
        /// <summary>
        /// Menu item for getting posts and further replacing entire text
        /// </summary>
        ToolStripMenuItem MenuGetFull { get; }
        /// <summary>
        /// Fires while receiving posts for further replacing selected text
        /// </summary>
        event EventHandler<GotPostsEventArgs> GotPostsPartial;
        /// <summary>
        /// Fires while receiving posts for further replacing entire text
        /// </summary>
        event EventHandler<GotPostsEventArgs> GotPostsFull;
        /// <summary>
        /// Fires after posting on social network
        /// </summary>
        event EventHandler<PostPerformedEventArgs> PostPerformed;
    }

    /// <summary>
    /// Represents object which stores details of social network post
    /// </summary>
    public class PostDetails
    {
        /// <summary>
        /// Creates new instance of PostDetails
        /// </summary>
        /// <param name="date">Post date</param>
        /// <param name="text">Post text</param>
        public PostDetails(DateTime date, string text)
        {
            PostDate = date;
            PostText = text;
        }

        /// <summary>
        /// Gets the post date
        /// </summary>
        public DateTime PostDate { get; private set; }
        /// <summary>
        /// Gets the post text
        /// </summary>
        public string PostText { get; private set; }
    }

    /// <summary>
    /// Represents object which stores information about received posts
    /// </summary>
    public class GotPostsEventArgs : EventArgs
    {
        /// <summary>
        /// List of received posts
        /// </summary>
        public List<PostDetails> Details { get; private set; }
        /// <summary>
        /// Creates new instance of GotPostsEventArgs
        /// </summary>
        /// <param name="details">List of received posts</param>
        public GotPostsEventArgs(List<PostDetails> details)
        {
            Details = details;
        }
    }

    /// <summary>
    /// Represents object which stores information about post attempt
    /// </summary>
    public class PostPerformedEventArgs : EventArgs
    {
        /// <summary>
        /// Result of post attempt
        /// </summary>
        public bool Result { get; private set; }
        /// <summary>
        /// Creates new instance of PostPerformedEventArgs
        /// </summary>
        /// <param name="result">Result of post attempt</param>
        public PostPerformedEventArgs(bool result)
        {
            Result = result;
        }
    }

    /// <summary>
    /// Represents object which stores information about synchronization result
    /// </summary>
    public class SyncCompleteEventArgs : EventArgs
    {
        /// <summary>
        /// Result of synchronization
        /// </summary>
        public SyncResult Result { get; private set; }
        /// <summary>
        /// Creates new instance of SyncCompleteEventArgs
        /// </summary>
        /// <param name="result">Result of synchronization</param>
        public SyncCompleteEventArgs(SyncResult result)
        {
            Result = result;
        }
    }

    /// <summary>
    /// Represents object which can receive host feedback before synchronization occurs and cancel it if needed
    /// </summary>
    public class BeforeSyncEventArgs : EventArgs
    {
        /// <summary>
        /// Specifies whether synchronization should be cancelled
        /// </summary>
        public bool Cancel { get; set; }
    }
}
