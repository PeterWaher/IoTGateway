using System;
using System.Collections.Generic;
using System.Text;

namespace Waher.Networking.HTTP
{
	/// <summary>
	/// The server is refusing to service the request because the entity of the request is in a format not supported by the requested resource 
	/// for the requested method. 
	/// </summary>
	public class UnsupportedMediaTypeException : HttpException
	{
		/// <summary>
		/// The server is refusing to service the request because the entity of the request is in a format not supported by the requested resource 
		/// for the requested method. 
		/// </summary>
		public UnsupportedMediaTypeException()
			: base(415, "Unsupported Media Type")
		{
		}
	}
}
