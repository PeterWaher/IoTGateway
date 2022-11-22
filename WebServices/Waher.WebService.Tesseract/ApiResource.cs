using System;
using System.Text;
using System.Threading.Tasks;
using Waher.Networking.HTTP;

namespace Waher.WebService.Tesseract
{
	/// <summary>
	/// Tesseract API Resource.
	/// </summary>
	public class ApiResource : HttpResource, IHttpPostMethod
	{
		private readonly TesseractApi api;
		private readonly HttpAuthenticationScheme[] authenticationSchemes;

		/// <summary>
		/// Tesseract API Resource.
		/// </summary>
		/// <param name="Api">API Class.</param>
		/// <param name="AuthenticationSchemes">Authentication schemes.</param>
		public ApiResource(TesseractApi Api, params HttpAuthenticationScheme[] AuthenticationSchemes)
			: base("/Tesseract/Api")
		{
			this.api = Api;
			this.authenticationSchemes = AuthenticationSchemes;
		}

		/// <summary>
		/// If the resource is synchronous (i.e. returns a response in the method handler), or if it is asynchronous
		/// (i.e. sends the response from another thread).
		/// </summary>
		public override bool Synchronous => true;

		/// <summary>
		/// If the resource handles sub-paths.
		/// </summary>
		public override bool HandlesSubPaths => false;

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
			return this.authenticationSchemes;
		}

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
			if (!this.api.ExeFound)
				throw new ServiceUnavailableException("Tesseract executable not found on system.");

			if (!Request.HasData)
				throw new BadRequestException("No content included.");

			string ContentType = Request.Header.ContentType?.Type;
			if (string.IsNullOrEmpty(ContentType) || !ContentType.StartsWith("image/", StringComparison.InvariantCultureIgnoreCase))
				throw new BadRequestException("Expected image content.");

			string Language = Request.Header["X-LANGUAGE"];
			string PageSegmentationMode = Request.Header["X-PSM"];
			byte[] ImageBin = await Request.ReadDataAsync();

			string Text = await this.api.PerformOcr(ImageBin, ContentType, PageSegmentationMode, Language);

			await Response.Return("text/plain; charset=utf-8", Encoding.UTF8.GetBytes(Text));
		}
	}
}
