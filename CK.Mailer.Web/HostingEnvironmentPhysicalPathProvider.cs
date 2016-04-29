using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Hosting;

namespace CK.Mailer
{
    class HostingEnvironmentPhysicalPathProvider : IPhysicalPathProvider
    {
        public string MapPath( string virtualPath )
        {
            return HostingEnvironment.MapPath( virtualPath );
        }

        public string ApplicationRootPath
        {
            get { return HostingEnvironment.ApplicationPhysicalPath; }
        }
    }
}
