using System;
using System.Collections.Generic;
using System.Text;

namespace Waher.Networking.HTTP
{
	/// <summary>
	/// The request could not be understood by the server due to malformed syntax. The client SHOULD NOT repeat the request without modifications. 
	/// </summary>
	public class BadRequestException : HttpException
	{
		/// <summary>
		/// The request could not be understood by the server due to malformed syntax. The client SHOULD NOT repeat the request without modifications. 
		/// </summary>
		public BadRequestException()
			: base(400, "Bad Request")
		{
		}
	}
}
