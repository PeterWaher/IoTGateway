using System;
using System.Collections.Generic;
using System.Text;

namespace Waher.Networking.HTTP
{
	/// <summary>
	/// Further extensions to the request are required for the server to fulfil it.
	/// </summary>
	public class NotExtendedException : HttpException
	{
		private const int Code = 510;
		private const string Msg = "Not Extended";

		/// <summary>
		/// Further extensions to the request are required for the server to fulfil it.
		/// </summary>
		public NotExtendedException()
			: base(Code, Msg)
		{
		}

		/// <summary>
		/// Further extensions to the request are required for the server to fulfil it.
		/// </summary>
		/// <param name="ContentObject">Any content object to return. The object will be encoded before being sent.</param>
		public NotExtendedException(object ContentObject)
			: base(Code, Msg, ContentObject)
		{
		}

		/// <summary>
		/// Further extensions to the request are required for the server to fulfil it.
		/// </summary>
		/// <param name="Content">Any encoded content to return.</param>
		/// <param name="ContentType">The content type of <paramref name="Content"/>, if provided.</param>
		public NotExtendedException(byte[] Content, string ContentType)
			: base(Code, Msg, Content, ContentType)
		{
		}
	}
}
