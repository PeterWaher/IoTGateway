using System;
using System.Collections.Generic;
using System.Text;

namespace Waher.Networking.HTTP
{
	/// <summary>
	/// The request was directed at a server that is not able to produce a response (for example because a connection reuse).
	/// </summary>
	public class MisdirectedRequestException : HttpException
	{
		/// <summary>
		/// The request was directed at a server that is not able to produce a response (for example because a connection reuse).
		/// </summary>
		public MisdirectedRequestException()
			: base(421, "Misdirected Request")
		{
		}
	}
}
