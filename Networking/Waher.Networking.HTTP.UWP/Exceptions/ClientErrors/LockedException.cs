using System.Collections.Generic;

namespace Waher.Networking.HTTP
{
	/// <summary>
	/// The resource that is being accessed is locked.
	/// </summary>
	public class LockedException : HttpException
	{
		/// <summary>
		/// 423
		/// </summary>
		public const int Code = 423;

		/// <summary>
		/// Locked
		/// </summary>
		public const string StatusMessage = "Locked";

		/// <summary>
		/// The resource that is being accessed is locked.
		/// </summary>
		/// <param name="HeaderFields">HTTP Header fields to include in the response.</param>
		public LockedException(params KeyValuePair<string, string>[] HeaderFields)
			: base(Code, StatusMessage, HeaderFields)
		{
		}

		/// <summary>
		/// The resource that is being accessed is locked.
		/// </summary>
		/// <param name="ContentObject">Any content object to return. The object will be encoded before being sent.</param>
		/// <param name="HeaderFields">HTTP Header fields to include in the response.</param>
		public LockedException(object ContentObject, params KeyValuePair<string, string>[] HeaderFields)
			: base(Code, StatusMessage, ContentObject, HeaderFields)
		{
		}

		/// <summary>
		/// The resource that is being accessed is locked.
		/// </summary>
		/// <param name="Content">Any encoded content to return.</param>
		/// <param name="ContentType">The content type of <paramref name="Content"/>, if provided.</param>
		/// <param name="HeaderFields">HTTP Header fields to include in the response.</param>
		public LockedException(byte[] Content, string ContentType, params KeyValuePair<string, string>[] HeaderFields)
			: base(Code, StatusMessage, Content, ContentType, HeaderFields)
		{
		}
	}
}
