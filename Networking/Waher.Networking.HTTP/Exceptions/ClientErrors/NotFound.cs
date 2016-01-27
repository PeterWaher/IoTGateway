using System;
using System.Collections.Generic;
using System.Text;

namespace Waher.Networking.HTTP
{
	/// <summary>
	/// The server has not found anything matching the Request-URI. No indication is given of whether the condition is temporary or permanent.
	/// </summary>
	public class NotFound : HttpException
	{
		/// <summary>
		/// The server has not found anything matching the Request-URI. No indication is given of whether the condition is temporary or permanent.
		/// </summary>
		public NotFound()
			: base(404, "Not Found")
		{
		}
	}
}
