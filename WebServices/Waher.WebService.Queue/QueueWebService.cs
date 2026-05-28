using System.Text;
using System.Threading.Tasks;
using Waher.Content.Html;
using Waher.Content.Markdown;
using Waher.IoTGateway;
using Waher.Networking.HTTP;
using Waher.Runtime.IO;
using Waher.Script;

namespace Waher.WebService.Queue
{
	/// <summary>
	/// Provides a REST web service interface for access to local queues.
	/// </summary>
	public class QueueWebService : HttpAsynchronousResource, 
		IHttpGetMethod, IHttpPostMethod, IHttpPutMethod, IHttpDeleteMethod
	{
		private readonly HttpAuthenticationScheme[] authenticationSchemes;

		/// <summary>
		/// Provides a REST web service interface for access to local queues.
		/// </summary>
		/// <param name="ResourceName">Resource name.</param>
		/// <param name="AuthenticationSchemes">Authentication schemes.</param>
		public QueueWebService(string ResourceName, 
			params HttpAuthenticationScheme[] AuthenticationSchemes)
			: base(ResourceName)
		{
			this.authenticationSchemes = AuthenticationSchemes;
		}

		/// <summary>
		/// If the resource handles sub-paths.
		/// </summary>
		public override bool HandlesSubPaths => true;

		/// <summary>
		/// If the resource uses user sessions.
		/// </summary>
		public override bool UserSessions => false;

		/// <summary>
		/// Any authentication schemes used to authenticate users before access is granted to the corresponding resource.
		/// </summary>
		/// <param name="Request">Current request</param>
		public override HttpAuthenticationScheme[] GetAuthenticationSchemes(HttpRequest Request)
		{
			if (Request.Header.Method.ToUpper() == "GET")
				return null;
			else
				return this.authenticationSchemes;
		}

		/// <summary>
		/// If the GET method is allowed.
		/// </summary>
		public bool AllowsGET => true;

		/// <summary>
		/// If the POST method is allowed.
		/// </summary>
		public bool AllowsPOST => true;

		/// <summary>
		/// If the PUT method is allowed.
		/// </summary>
		public bool AllowsPUT => true;

		/// <summary>
		/// If the DELETE method is allowed.
		/// </summary>
		public bool AllowsDELETE => true;

		/// <summary>
		/// Executes the GET method on the resource.
		/// </summary>
		/// <param name="Request">HTTP Request</param>
		/// <param name="Response">HTTP Response</param>
		/// <exception cref="HttpException">If an error occurred when processing the method.</exception>
		public async Task GET(HttpRequest Request, HttpResponse Response)
		{
			byte[] Data = Resources.LoadResource(
				typeof(QueueWebService).Namespace + ".Data.ApiDocumentation.md",
				typeof(QueueWebService).Assembly);

			string Markdown = Strings.GetString(Data, Encoding.UTF8);
			MarkdownSettings Settings = new MarkdownSettings(null, true, new Variables())
			{
				RootFolder = Gateway.RootFolder,
				ResourceMap = Gateway.HttpServer
			};
			MarkdownDocument Doc = await MarkdownDocument.CreateAsync(Markdown, Settings,
				null, Request.Resource.ResourceName, Request.Header.GetURL());
			string Html = await Doc.GenerateHTML();

			Response.ContentType = HtmlCodec.DefaultContentType;

			await Response.Write(Html);
			await Response.SendResponse();
		}

		/// <summary>
		/// Executes the POST method on the resource.
		/// </summary>
		/// <param name="Request">HTTP Request</param>
		/// <param name="Response">HTTP Response</param>
		/// <exception cref="HttpException">If an error occurred when processing the method.</exception>
		public async Task POST(HttpRequest Request, HttpResponse Response)
		{
			// TODO
			await this.GET(Request, Response);
		}

		/// <summary>
		/// Executes the PUT method on the resource.
		/// </summary>
		/// <param name="Request">HTTP Request</param>
		/// <param name="Response">HTTP Response</param>
		/// <exception cref="HttpException">If an error occurred when processing the method.</exception>
		public async Task PUT(HttpRequest Request, HttpResponse Response)
		{
			// TODO
			await this.GET(Request, Response);
		}

		/// <summary>
		/// Executes the DELETE method on the resource.
		/// </summary>
		/// <param name="Request">HTTP Request</param>
		/// <param name="Response">HTTP Response</param>
		/// <exception cref="HttpException">If an error occurred when processing the method.</exception>
		public async Task DELETE(HttpRequest Request, HttpResponse Response)
		{
			// TODO
			await this.GET(Request, Response);
		}

	}
}
