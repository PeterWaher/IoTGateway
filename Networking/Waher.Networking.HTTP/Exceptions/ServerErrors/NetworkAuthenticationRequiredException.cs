using System.Collections.Generic;

namespace Waher.Networking.HTTP
{
	/// <summary>
	/// The client needs to authenticate to gain network access. Intended for use by intercepting proxies used to control access to the network.
	/// </summary>
	public class NetworkAuthenticationRequiredException : HttpException
	{
		/// <summary>
		/// 511
		/// </summary>
		public const int Code = 511;

		/// <summary>
		/// Network Authentication Required
		/// </summary>
		public const string StatusMessage = "Network Authentication Required";

		/// <summary>
		/// The client needs to authenticate to gain network access. Intended for use by intercepting proxies used to control access to the network.
		/// </summary>
		/// <param name="HeaderFields">HTTP Header fields to include in the response.</param>
		public NetworkAuthenticationRequiredException(params KeyValuePair<string, string>[] HeaderFields)
			: base(Code, StatusMessage, HeaderFields)
		{
		}

		/// <summary>
		/// The client needs to authenticate to gain network access. Intended for use by intercepting proxies used to control access to the network.
		/// </summary>
		/// <param name="ContentObject">Any content object to return. The object will be encoded before being sent.</param>
		/// <param name="HeaderFields">HTTP Header fields to include in the response.</param>
		public NetworkAuthenticationRequiredException(object ContentObject, params KeyValuePair<string, string>[] HeaderFields)
			: base(Code, StatusMessage, ContentObject, HeaderFields)
		{
		}

		/// <summary>
		/// The client needs to authenticate to gain network access. Intended for use by intercepting proxies used to control access to the network.
		/// </summary>
		/// <param name="Content">Any encoded content to return.</param>
		/// <param name="ContentType">The content type of <paramref name="Content"/>, if provided.</param>
		/// <param name="HeaderFields">HTTP Header fields to include in the response.</param>
		public NetworkAuthenticationRequiredException(byte[] Content, string ContentType, params KeyValuePair<string, string>[] HeaderFields)
			: base(Code, StatusMessage, Content, ContentType, HeaderFields)
		{
		}
	}
}
