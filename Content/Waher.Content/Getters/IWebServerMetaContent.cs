using System.Net.Http;
using System.Threading.Tasks;

namespace Waher.Content.Getters
{
	/// <summary>
	/// Interface for content classes, that process information available in
	/// HTTP headers in the response.
	/// </summary>
	public interface IWebServerMetaContent
	{
		/// <summary>
		/// Decodes meta-information available in the HTTP Response.
		/// </summary>
		/// <param name="HttpResponse">HTTP Response.</param>
		Task DecodeMetaInformation(HttpResponseMessage HttpResponse);
	}
}
