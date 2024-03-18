using System.Threading.Tasks;

namespace Waher.Networking.HTTP
{
	/// <summary>
	/// PATCH Interface for HTTP resources.
	/// </summary>
	public interface IHttpPatchMethod
	{
		/// <summary>
		/// Executes the PATCH method on the resource.
		/// </summary>
		/// <param name="Request">HTTP Request</param>
		/// <param name="Response">HTTP Response</param>
		/// <exception cref="HttpException">If an error occurred when processing the method.</exception>
		Task PATCH(HttpRequest Request, HttpResponse Response);

		/// <summary>
		/// If the PATCH method is allowed.
		/// </summary>
		bool AllowsPATCH { get; }
	}
}
