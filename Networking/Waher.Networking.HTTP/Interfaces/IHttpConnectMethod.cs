using System.Threading.Tasks;

namespace Waher.Networking.HTTP
{
	/// <summary>
	/// CONNECT Interface for HTTP resources.
	/// </summary>
	public interface IHttpConnectMethod
	{
		/// <summary>
		/// Executes the CONNECT method on the resource.
		/// </summary>
		/// <param name="Request">HTTP Request</param>
		/// <param name="Response">HTTP Response</param>
		/// <exception cref="HttpException">If an error occurred when processing the method.</exception>
		Task CONNECT(HttpRequest Request, HttpResponse Response);

		/// <summary>
		/// If the CONNECT method is allowed.
		/// </summary>
		bool AllowsCONNECT { get; }
	}
}
