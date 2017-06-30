using System;
using System.Collections.Generic;
using System.Text;

namespace Waher.Networking.HTTP
{
	/// <summary>
	/// The resource identified by the request is only capable of generating response entities which have content characteristics not acceptable 
	/// according to the accept headers sent in the request. 
	/// </summary>
	public class NotAcceptableException : HttpException
	{
		/// <summary>
		/// The resource identified by the request is only capable of generating response entities which have content characteristics not acceptable 
		/// according to the accept headers sent in the request. 
		/// </summary>
		public NotAcceptableException()
			: base(404, "Not Acceptable")
		{
		}
	}
}
