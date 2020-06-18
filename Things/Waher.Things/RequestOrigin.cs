using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Waher.Things
{
	/// <summary>
	/// Tokens available in request.
	/// </summary>
	public class RequestOrigin
	{
		/// <summary>
		/// Empty request origin.
		/// </summary>
		public static readonly RequestOrigin Empty = new RequestOrigin(string.Empty, new string[0], new string[0], new string[0]);

		private readonly string[] deviceTokens;
		private readonly string[] serviceTokens;
		private readonly string[] userTokens;
		private readonly string from;

		/// <summary>
		/// Tokens available in request.
		/// </summary>
		/// <param name="From">Address of caller.</param>
		/// <param name="DeviceTokens">Device tokens, or null.</param>
		/// <param name="ServiceTokens">Service tokens, or null.</param>
		/// <param name="UserTokens">User tokens, or null.</param>
		public RequestOrigin(string From, string[] DeviceTokens, string[] ServiceTokens, string[] UserTokens)
		{
			this.from = From;
			this.deviceTokens = DeviceTokens;
			this.serviceTokens = ServiceTokens;
			this.userTokens = UserTokens;
		}

		/// <summary>
		/// Address of caller.
		/// </summary>
		public string From
		{
			get { return this.from; }
		}

		/// <summary>
		/// Device tokens, or null.
		/// </summary>
		public string[] DeviceTokens
		{
			get { return this.deviceTokens; }
		}

		/// <summary>
		/// Service tokens, or null.
		/// </summary>
		public string[] ServiceTokens
		{
			get { return this.serviceTokens; }
		}

		/// <summary>
		/// User tokens, or null.
		/// </summary>
		public string[] UserTokens
		{
			get { return this.userTokens; }
		}
	}
}
