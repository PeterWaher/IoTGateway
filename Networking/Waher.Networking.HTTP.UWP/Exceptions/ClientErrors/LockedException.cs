using System;
using System.Collections.Generic;
using System.Text;

namespace Waher.Networking.HTTP
{
	/// <summary>
	/// The resource that is being accessed is locked.
	/// </summary>
	public class LockedException : HttpException
	{
		private const int Code = 423;
		private const string Msg = "Locked";

		/// <summary>
		/// The resource that is being accessed is locked.
		/// </summary>
		public LockedException()
			: base(Code, Msg)
		{
		}

		/// <summary>
		/// The resource that is being accessed is locked.
		/// </summary>
		/// <param name="ContentObject">Any content object to return. The object will be encoded before being sent.</param>
		public LockedException(object ContentObject)
			: base(Code, Msg, ContentObject)
		{
		}

		/// <summary>
		/// The resource that is being accessed is locked.
		/// </summary>
		/// <param name="Content">Any encoded content to return.</param>
		/// <param name="ContentType">The content type of <paramref name="Content"/>, if provided.</param>
		public LockedException(byte[] Content, string ContentType)
			: base(Code, Msg, Content, ContentType)
		{
		}
	}
}
