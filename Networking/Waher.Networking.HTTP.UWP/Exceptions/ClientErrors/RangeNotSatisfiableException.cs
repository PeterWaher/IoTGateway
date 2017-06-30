using System;
using System.Collections.Generic;
using System.Text;

namespace Waher.Networking.HTTP
{
	/// <summary>
	/// A server SHOULD return a response with this status code if a request included a Range request-header field (section 14.35), and none of the 
	/// range-specifier values in this field overlap the current extent of the selected resource, and the request did not include an If-Range 
	/// request-header field. (For byte-ranges, this means that the first- byte-pos of all of the byte-range-spec values were greater than the 
	/// current length of the selected resource.) 
	/// </summary>
	public class RangeNotSatisfiableException : HttpException
	{
		/// <summary>
		/// A server SHOULD return a response with this status code if a request included a Range request-header field (section 14.35), and none of the 
		/// range-specifier values in this field overlap the current extent of the selected resource, and the request did not include an If-Range 
		/// request-header field. (For byte-ranges, this means that the first- byte-pos of all of the byte-range-spec values were greater than the 
		/// current length of the selected resource.) 
		/// </summary>
		public RangeNotSatisfiableException()
			: base(416, "Range Not Satisfiable")
		{
		}
	}
}
