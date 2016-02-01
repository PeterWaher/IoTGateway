using System;
using System.Collections.Generic;
using System.Text;

namespace Waher.Networking.HTTP
{
	/// <summary>
	/// The server is unable to store the representation needed to complete the request.
	/// </summary>
	public class InsufficientStorageException : HttpException
	{
		/// <summary>
		/// The server is unable to store the representation needed to complete the request.
		/// </summary>
		public InsufficientStorageException()
			: base(507, "Insufficient Storage")
		{
		}
	}
}
