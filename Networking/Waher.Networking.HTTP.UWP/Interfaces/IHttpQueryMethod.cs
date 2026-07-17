using System.Threading.Tasks;

namespace Waher.Networking.HTTP
{
	/// <summary>
	/// QUERY Interface for HTTP resources.
	/// </summary>
	public interface IHttpQueryMethod
	{
		/// <summary>
		/// Executes the QUERY method on the resource.
		/// </summary>
		/// <param name="Request">HTTP Request</param>
		/// <param name="Response">HTTP Response</param>
		/// <exception cref="HttpException">If an error occurred when processing the method.</exception>
		Task QUERY(HttpRequest Request, HttpResponse Response);

		/// <summary>
		/// If the QUERY method is allowed.
		/// </summary>
		bool AllowsQUERY { get; }

		/// <summary>
		/// Acceptable Content-Types of the body of the QUERY request.
		/// </summary>
		string[] AcceptableQueryTypes { get; }
	}
}
