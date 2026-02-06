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
		Allow,

		/// <summary>
		/// Forbid request (response is returned to client)
		/// </summary>
		Forbid,

		/// <summary>
		/// Return a Not Found error to client.
		/// </summary>
		NotFound,

		/// <summary>
		/// Return a Rate Limit error to client.
		/// </summary>
		RateLimited,

		/// <summary>
		/// Ignore request (no response is returned to client)
		/// </summary>
		Ignore,

		/// <summary>
		/// Close connection
		/// </summary>
		Close
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
	}
}
