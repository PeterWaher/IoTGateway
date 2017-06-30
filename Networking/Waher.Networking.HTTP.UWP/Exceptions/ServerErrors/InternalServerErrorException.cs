using System;
using System.Collections.Generic;
using System.Text;

namespace Waher.Networking.HTTP
{
	/// <summary>
	/// The server encountered an unexpected condition which prevented it from fulfilling the request. 
	/// </summary>
	public class InternalServerErrorException : HttpException
	{
		/// <summary>
		/// The server encountered an unexpected condition which prevented it from fulfilling the request. 
		/// </summary>
		public InternalServerErrorException()
			: base(500, "Internal Server Error")
		{
		}
	}
}
