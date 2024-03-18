using System.Threading.Tasks;

namespace Waher.Networking.HTTP
{
	/// <summary>
	/// Ranged PATCH Interface for HTTP resources.
	/// </summary>
	public interface IHttpPatchRangesMethod
	{
		/// <summary>
		/// Executes the ranged PATCH method on the resource.
		/// </summary>
		/// <param name="Request">HTTP Request</param>
		/// <param name="Response">HTTP Response</param>
		/// <param name="Interval">Content byte range.</param>
		/// <exception cref="HttpException">If an error occurred when processing the method.</exception>
		Task PATCH(HttpRequest Request, HttpResponse Response, ContentByteRangeInterval Interval);

		/// <summary>
		/// If the PATCH method is allowed.
		/// </summary>
		bool AllowsPATCH { get; }
	}
}
