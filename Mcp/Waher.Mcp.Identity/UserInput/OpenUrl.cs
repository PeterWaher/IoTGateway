using System;
using Waher.Networking.HTTP.Mcp.Model.Attributes;

namespace Waher.Mcp.Identity.UserInput
{
	/// <summary>
	/// Requests the user to open a URL.
	/// </summary>
	internal class OpenUrl
	{
		/// <summary>
		/// Requests the user to open a URL.
		/// </summary>
		/// <param name="Url">URL to open</param>
		public OpenUrl(string Url)
		{
			this.Url = new Uri(Url);
		}

		/// <summary>
		/// URI to open
		/// </summary>
		[McpUriParameter("URI", "You need to open this URI to continue")]
		public Uri Url;
	}
}
