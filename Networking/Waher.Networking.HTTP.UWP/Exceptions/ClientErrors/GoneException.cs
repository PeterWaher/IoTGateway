using System;
using System.Collections.Generic;
using System.Text;

namespace Waher.Networking.HTTP
{
	/// <summary>
	/// The requested resource is no longer available at the server and no forwarding address is known. This condition is expected to be considered 
	/// permanent. Clients with link editing capabilities SHOULD delete references to the Request-URI after user approval. If the server does not 
	/// know, or has no facility to determine, whether or not the condition is permanent, the status code 404 (Not Found) SHOULD be used instead. 
	/// This response is cacheable unless indicated otherwise.
	/// </summary>
	public class GoneException : HttpException
	{
		/// <summary>
		/// The requested resource is no longer available at the server and no forwarding address is known. This condition is expected to be considered 
		/// permanent. Clients with link editing capabilities SHOULD delete references to the Request-URI after user approval. If the server does not 
		/// know, or has no facility to determine, whether or not the condition is permanent, the status code 404 (Not Found) SHOULD be used instead. 
		/// This response is cacheable unless indicated otherwise.
		/// </summary>
		public GoneException()
			: base(410, "Gone")
		{
		}
	}
}
