using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CK.Mailer
{
    /// <summary>
    /// Provides tools to obtain physical path from virtual path.
    /// </summary>
    public interface IPhysicalPathProvider
    {
        /// <summary>
        /// Gets the application root path
        /// </summary>
        string ApplicationRootPath { get; }

        /// <summary>
        /// Maps a virtual path to a physical path on the server.
        /// </summary>
        /// <param name="virtualPath">The virtual path (absolute or relative).</param>
        /// <returns>The physical path on the server specified by virtualPath.</returns>
        string MapPath( string virtualPath );
    }
}