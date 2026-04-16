using System.Threading.Tasks;

namespace Waher.Networking.HTTP.JsonRpc
{
	/// <summary>
	/// Web Service interface based on JSON-RPC v2.0.
	/// 
	/// Ref:
	/// https://www.jsonrpc.org/specification
	/// https://www.jsonrpc.org/historical/json-rpc-over-http.html
	/// </summary>
	public class JsonRpcWebService : HttpSynchronousResource, IHttpGetMethod, IHttpPostMethod
	{
		private readonly bool userSessions;

		/// <summary>
		/// Web Service interface based on JSON-RPC v2.0.
		/// </summary>
		/// <param name="ResourceName">Name of resource.</param>
		/// <param name="UserSessions">If the resource uses user sessions.</param>
		public JsonRpcWebService(string ResourceName, bool UserSessions)
			: base(ResourceName)
		{
			this.userSessions = UserSessions;
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
		/// If the resource handles sub-paths.
		/// </summary>
		public override bool HandlesSubPaths => false;

		/// <summary>
		/// If the resource uses user sessions.
		/// </summary>
		public override bool UserSessions => this.userSessions;

		/// <summary>
		/// Executes the GET method on the resource.
		/// </summary>
		/// <param name="Request">HTTP Request</param>
		/// <param name="Response">HTTP Response</param>
		/// <exception cref="HttpException">If an error occurred when processing the method.</exception>
		public async Task GET(HttpRequest Request, HttpResponse Response)
		{
			// TODO
			await Response.SendResponse(new NotImplementedException());
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
			await Response.SendResponse(new NotImplementedException());
		}
	}
}
