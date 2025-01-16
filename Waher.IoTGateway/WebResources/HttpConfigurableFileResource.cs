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
        private readonly bool isText;

        /// <summary>
        /// Represents a file-based resource that can have custom values depending on what domain the resource is requested.
        /// </summary>
        /// <param name="ResourceName">Name of resource.</param>
        /// <param name="FileName">Name of text file containing default contents.</param>
        public HttpConfigurableFileResource(string ResourceName, string FileName)
            : this(ResourceName, FileName, PlainTextCodec.DefaultContentType, true)
        {
        }

        /// <summary>
        /// Represents a file-based resource that can have custom values depending on what domain the resource is requested.
        /// </summary>
        /// <param name="ResourceName">Name of resource.</param>
        /// <param name="FileName">Name of text file containing default contents.</param>
        /// <param name="ContentType">Content-Type of text file.</param>
        public HttpConfigurableFileResource(string ResourceName, string FileName, string ContentType)
            : this(ResourceName, FileName, ContentType, true)
        {
        }

        /// <summary>
        /// Represents a file-based resource that can have custom values depending on what domain the resource is requested.
        /// </summary>
        /// <param name="ResourceName">Name of resource.</param>
        /// <param name="FileName">Name of file containing default contents.</param>
        /// <param name="ContentType">Content-Type of file.</param>
        /// <param name="IsText">If file is a text file (true) or a binary file (false)</param>
        public HttpConfigurableFileResource(string ResourceName, string FileName, string ContentType, bool IsText)
            : base(ResourceName)
        {
            this.fileName = Path.GetFullPath(FileName);
            this.contentType = ContentType;
            this.isText = IsText;
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
            DateTime TP = await DomainSettings.GetFileSettingTimestampAsync(Request, this.fileName);

            Response.ContentType = this.contentType;
            Response.Date = TP;

            if (this.isText)
            {
                string Content = await DomainSettings.GetTextFileSettingAsync(Request, this.fileName);
                await Response.Write(Content);
            }
            else
            {
                byte[] Content = await DomainSettings.GetBinaryFileSettingAsync(Request, this.fileName);

                Response.ContentLength = Content.Length;
                
                await Response.Write(true, Content);
            }
        }
    }
}