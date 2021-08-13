using System;

namespace Waher.Networking.HTTP
{
	/// <summary>
	/// Defined in the internet draft "A New HTTP Status Code for Legally-restricted Resources". Intended to be used when resource access is 
	/// denied for legal reasons, e.g. censorship or government-mandated blocked access.
	/// </summary>
	public class UnavailableForLegalReasonsException : HttpException
	{
		/// <summary>
		/// 451
		/// </summary>
		public const int Code = 451;

		/// <summary>
		/// Unavailable For Legal Reasons
		/// </summary>
		public const string StatusMessage = "Unavailable For Legal Reasons";

		/// <summary>
		/// Defined in the internet draft "A New HTTP Status Code for Legally-restricted Resources". Intended to be used when resource access is 
		/// denied for legal reasons, e.g. censorship or government-mandated blocked access.
		/// </summary>
		public UnavailableForLegalReasonsException()
			: base(Code, StatusMessage)
		{
		}

		/// <summary>
		/// Defined in the internet draft "A New HTTP Status Code for Legally-restricted Resources". Intended to be used when resource access is 
		/// denied for legal reasons, e.g. censorship or government-mandated blocked access.
		/// </summary>
		/// <param name="ContentObject">Any content object to return. The object will be encoded before being sent.</param>
		public UnavailableForLegalReasonsException(object ContentObject)
			: base(Code, StatusMessage, ContentObject)
		{
		}

		/// <summary>
		/// Defined in the internet draft "A New HTTP Status Code for Legally-restricted Resources". Intended to be used when resource access is 
		/// denied for legal reasons, e.g. censorship or government-mandated blocked access.
		/// </summary>
		/// <param name="Content">Any encoded content to return.</param>
		/// <param name="ContentType">The content type of <paramref name="Content"/>, if provided.</param>
		public UnavailableForLegalReasonsException(byte[] Content, string ContentType)
			: base(Code, StatusMessage, Content, ContentType)
		{
		}
	}
}
