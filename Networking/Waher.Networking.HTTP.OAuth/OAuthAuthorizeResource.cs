using System.Threading.Tasks;

namespace Waher.Networking.HTTP.OAuth
{
	/// <summary>
	/// OAUTH authorize resource.
	/// </summary>
	public class OAuthAuthorizeResource : HttpSynchronousResource, IHttpGetMethod
	{
		/// <summary>
		/// OAUTH authorize resource.
		/// </summary>
		public OAuthAuthorizeResource()
			: this("/oauth/authorize")
		{
		}

		/// <summary>
		/// OAUTH authorize resource.
		/// </summary>
		/// <param name="ResourceName">Resource name.</param>
		public OAuthAuthorizeResource(string ResourceName)
			: base(ResourceName)
		{
		}

		/// <summary>
		/// If the resource uses user sessions.
		/// </summary>
		public override bool UserSessions => false;

		/// <summary>
		/// If the resource handles sub-paths.
		/// </summary>
		public override bool HandlesSubPaths => false;

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
			if (!Request.Header.TryGetQueryParameter("response_type", out string ResponseType))
			{
				await Response.SendResponse(new BadRequestException("Missing response_type parameter."));
				return;
			}

			switch (ResponseType)
			{
				case "code":
					break;

				default:
					await Response.SendResponse(new BadRequestException("Unsupported response_type parameter: " + ResponseType));
					return;
			}
		}

	}
}
