using System.Threading.Tasks;

namespace Waher.Networking.HTTP.Interfaces
{
	/// <summary>
	/// Actions that a Web Server can take after reviewing a request.
	/// </summary>
	public enum WafResult
	{
		/// <summary>
		/// Allow request
		/// </summary>
		Allow = 0,

		/// <summary>
		/// Forbid request (response is returned to client)
		/// </summary>
		Forbid = 1,

		/// <summary>
		/// Return a Not Found error to client.
		/// </summary>
		NotFound = 2,

		/// <summary>
		/// Return a Rate Limit error to client.
		/// </summary>
		RateLimited = 3,

		/// <summary>
		/// Ignore request (no response is returned to client)
		/// </summary>
		Ignore = 4,

		/// <summary>
		/// Close connection
		/// </summary>
		Close = 5
	}

	/// <summary>
	/// Interface for Web Application Firewalls (WAF).
	/// </summary>
	public interface IWebApplicationFirewall
	{
		/// <summary>
		/// Reviews an incoming request.
		/// </summary>
		/// <param name="Request">Current HTTP Request</param>
		/// <param name="Resource">Corresponding HTTP Resource, if found.</param>
		/// <returns>Action to take.</returns>
		Task<WafResult> Review(HttpRequest Request, HttpResource Resource);

		/// <summary>
		/// Reloads the firewall configuration from its original source.
		/// </summary>
		Task Reload();

		/// <summary>
		/// Tries to get a redirection, if the result is a redirection result.
		/// </summary>
		/// <param name="Result">Result</param>
		/// <param name="Redirection">Associated redirection, if found.</param>
		/// <returns>If result represents a redirection.</returns>
		bool TryGetRedirection(WafResult Result, out HttpException Redirection);
	}
}
