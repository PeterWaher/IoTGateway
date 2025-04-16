using System.Threading.Tasks;
using Waher.Networking.HTTP;
using Waher.Runtime.IO;

namespace Waher.IoTGateway.WebResources
{
	/// <summary>
	/// Echoes what the client sends in.
	/// </summary>
	public class Echo : HttpSynchronousResource, IHttpPostMethod
	{
		/// <summary>
		/// Echoes what the client sends in.
		/// </summary>
		public Echo()
			: base("/Echo")
		{
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
		/// If the POST method is allowed.
		/// </summary>
		public bool AllowsPOST => true;

		/// <summary>
		/// Executes the POST method on the resource.
		/// </summary>
		/// <param name="Request">HTTP Request</param>
		/// <param name="Response">HTTP Response</param>
		/// <exception cref="HttpException">If an error occurred when processing the method.</exception>
		public async Task POST(HttpRequest Request, HttpResponse Response)
		{
			if (string.IsNullOrEmpty(Request.Header.ContentType?.Value))
			{
				await Response.SendResponse(new BadRequestException("Content-Type not defined."));
				return;
			}

			Response.ContentType = Request.Header.ContentType.Value;

			if (Request.HasData)
			{
				if (Request.DataStream.Length >= 65536)
				{
					await Response.SendResponse(new ForbiddenException(Request, "Content to echo may not exceed 64 kB."));
					return;
				}

				byte[] Data = await Request.DataStream.ReadAllAsync();
				await Response.Write(true, Data);
			}
		}

	}
}
