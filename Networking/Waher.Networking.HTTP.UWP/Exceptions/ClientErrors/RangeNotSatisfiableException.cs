using System;

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
		/// 416
		/// </summary>
		public const int Code = 416;

		/// <summary>
		/// Range Not Satisfiable
		/// </summary>
		public const string StatusMessage = "Range Not Satisfiable";

		/// <summary>
		/// A server SHOULD return a response with this status code if a request included a Range request-header field (section 14.35), and none of the 
		/// range-specifier values in this field overlap the current extent of the selected resource, and the request did not include an If-Range 
		/// request-header field. (For byte-ranges, this means that the first- byte-pos of all of the byte-range-spec values were greater than the 
		/// current length of the selected resource.) 
		/// </summary>
		public RangeNotSatisfiableException()
			: base(Code, StatusMessage)
		{
		}

		/// <summary>
		/// A server SHOULD return a response with this status code if a request included a Range request-header field (section 14.35), and none of the 
		/// range-specifier values in this field overlap the current extent of the selected resource, and the request did not include an If-Range 
		/// request-header field. (For byte-ranges, this means that the first- byte-pos of all of the byte-range-spec values were greater than the 
		/// current length of the selected resource.) 
		/// </summary>
		/// <param name="ContentObject">Any content object to return. The object will be encoded before being sent.</param>
		public RangeNotSatisfiableException(object ContentObject)
			: base(Code, StatusMessage, ContentObject)
		{
		}

		/// <summary>
		/// A server SHOULD return a response with this status code if a request included a Range request-header field (section 14.35), and none of the 
		/// range-specifier values in this field overlap the current extent of the selected resource, and the request did not include an If-Range 
		/// request-header field. (For byte-ranges, this means that the first- byte-pos of all of the byte-range-spec values were greater than the 
		/// current length of the selected resource.) 
		/// </summary>
		/// <param name="Content">Any encoded content to return.</param>
		/// <param name="ContentType">The content type of <paramref name="Content"/>, if provided.</param>
		public RangeNotSatisfiableException(byte[] Content, string ContentType)
			: base(Code, StatusMessage, Content, ContentType)
		{
		}
	}
}
