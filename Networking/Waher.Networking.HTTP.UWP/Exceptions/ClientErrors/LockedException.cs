using System;

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
		public LockedException()
			: base(Code, StatusMessage)
		{
		}

		/// <summary>
		/// The resource that is being accessed is locked.
		/// </summary>
		/// <param name="ContentObject">Any content object to return. The object will be encoded before being sent.</param>
		public LockedException(object ContentObject)
			: base(Code, StatusMessage, ContentObject)
		{
		}

		/// <summary>
		/// The resource that is being accessed is locked.
		/// </summary>
		/// <param name="Content">Any encoded content to return.</param>
		/// <param name="ContentType">The content type of <paramref name="Content"/>, if provided.</param>
		public LockedException(byte[] Content, string ContentType)
			: base(Code, StatusMessage, Content, ContentType)
		{
		}
	}
}
