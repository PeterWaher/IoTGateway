using System;
using System.Collections.Generic;
using System.Text;

namespace Waher.Networking.HTTP
{
	/// <summary>
	/// Further extensions to the request are required for the server to fulfil it.
	/// </summary>
	public class NotExtendedException : HttpException
	{
		/// <summary>
		/// Further extensions to the request are required for the server to fulfil it.
		/// </summary>
		public NotExtendedException()
			: base(510, "Not Extended")
		{
		}
	}
}
