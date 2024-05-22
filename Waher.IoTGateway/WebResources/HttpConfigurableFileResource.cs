using System;
using System.IO;
using System.Threading.Tasks;
using Waher.Content.Text;
using Waher.Networking.HTTP;

namespace Waher.IoTGateway.WebResources
{
    /// <summary>
    /// Represents a file-based resource that can have custom values depending on what domain the resource is requested.
    /// </summary>
    public class HttpConfigurableFileResource : HttpSynchronousResource, IHttpGetMethod
    {
        private readonly string fileName;
        private readonly string contentType;

        /// <summary>
        /// Represents a file-based resource that can have custom values depending on what domain the resource is requested.
        /// </summary>
        /// <param name="ResourceName">Name of resource.</param>
        /// <param name="FileName"></param>
        public HttpConfigurableFileResource(string ResourceName, string FileName)
            : this(ResourceName, FileName, PlainTextCodec.DefaultContentType)
        {
        }

        /// <summary>
        /// Represents a file-based resource that can have custom values depending on what domain the resource is requested.
        /// </summary>
        /// <param name="ResourceName">Name of resource.</param>
        /// <param name="FileName"></param>
        public HttpConfigurableFileResource(string ResourceName, string FileName, string ContentType)
            : base(ResourceName)
        {
            this.fileName = Path.GetFullPath(FileName);
            this.contentType = ContentType;
        }

        /// <summary>
        /// If the resource handles sub-paths.
        /// </summary>
        public override bool HandlesSubPaths => false;

        /// <summary>
        /// If the resource uses user sessions.
        /// </summary>
        public override bool UserSessions => false;

        /// <summary>
        /// If the GET method is allowed.
        /// </summary>
        public bool AllowsGET => true;

        /// <summary>
        /// Executes the GET method on the resource.
        /// </summary>
        /// <param name="Request">HTTP Request</param>
        /// <param name="Response">HTTP Response</param>
        /// <exception cref="HttpException">If an error occurred when processing the method.</exception>
        public async Task GET(HttpRequest Request, HttpResponse Response)
        {
            string Content = await DomainSettings.GetFileSettingAsync(Request, this.fileName);
            DateTime TP = await DomainSettings.GetFileSettingTimestampAsync(Request, this.fileName);

            Response.ContentType = this.contentType;
            Response.Date = TP;
            
            await Response.Write(Content);
        }
    }
}