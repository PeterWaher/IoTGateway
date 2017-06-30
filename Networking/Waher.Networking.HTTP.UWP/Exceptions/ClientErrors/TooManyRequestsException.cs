using System;
using System.Collections.Generic;
using System.Text;

namespace Waher.Networking.HTTP
{
	/// <summary>
	/// The user has sent too many requests in a given amount of time. Intended for use with rate limiting schemes.
	/// </summary>
	public class TooManyRequestsException : HttpException
	{
		/// <summary>
		/// The user has sent too many requests in a given amount of time. Intended for use with rate limiting schemes.
		/// </summary>
		public TooManyRequestsException()
			: base(429, "Too Many Requests")
		{
		}
	}
}
