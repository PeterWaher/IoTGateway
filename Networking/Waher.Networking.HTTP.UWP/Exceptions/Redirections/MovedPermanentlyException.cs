using System;
using System.Collections.Generic;
using System.Text;

namespace Waher.Networking.HTTP
{
	/// <summary>
	/// The requested resource has been assigned a new permanent URI and any future references to this resource SHOULD use one of the returned URIs. 
	/// Clients with link editing capabilities ought to automatically re-link references to the Request-URI to one or more of the new references 
	/// returned by the server, where possible. This response is cacheable unless indicated otherwise. 
	/// </summary>
	public class MovedPermanentlyException : HttpException
	{
		/// <summary>
		/// The requested resource has been assigned a new permanent URI and any future references to this resource SHOULD use one of the returned URIs. 
		/// Clients with link editing capabilities ought to automatically re-link references to the Request-URI to one or more of the new references 
		/// returned by the server, where possible. This response is cacheable unless indicated otherwise. 
		/// </summary>
		/// <param name="Location">Location.</param>
		public MovedPermanentlyException(string Location)
			: base(301, "Moved Permanently", new KeyValuePair<string, string>("Location", Location))
		{
		}
	}
}
