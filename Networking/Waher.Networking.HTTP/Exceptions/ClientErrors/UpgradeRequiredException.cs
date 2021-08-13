using System;
using System.Collections.Generic;
using System.Text;

namespace Waher.Networking.HTTP
{
	/// <summary>
	/// The client should switch to a different protocol such as TLS/1.0, given in the Upgrade header field.
	/// </summary>
	public class UpgradeRequiredException : HttpException
	{
		/// <summary>
		/// 426
		/// </summary>
		public const int Code = 426;

		/// <summary>
		/// Upgrade Required
		/// </summary>
		public const string StatusMessage = "Upgrade Required";

		/// <summary>
		/// The client should switch to a different protocol such as TLS/1.0, given in the Upgrade header field.
		/// </summary>
		/// <param name="Protocol">Protocol to upgrade to.</param>
		public UpgradeRequiredException(string Protocol)
			: base(Code, StatusMessage, new KeyValuePair<string, string>("Upgrade", Protocol))
		{
		}

		/// <summary>
		/// The client should switch to a different protocol such as TLS/1.0, given in the Upgrade header field.
		/// </summary>
		/// <param name="Protocol">Protocol to upgrade to.</param>
		/// <param name="ContentObject">Any content object to return. The object will be encoded before being sent.</param>
		public UpgradeRequiredException(string Protocol, object ContentObject)
			: base(Code, StatusMessage, ContentObject, new KeyValuePair<string, string>("Upgrade", Protocol))
		{
		}

		/// <summary>
		/// The client should switch to a different protocol such as TLS/1.0, given in the Upgrade header field.
		/// </summary>
		/// <param name="Protocol">Protocol to upgrade to.</param>
		/// <param name="Content">Any encoded content to return.</param>
		/// <param name="ContentType">The content type of <paramref name="Content"/>, if provided.</param>
		public UpgradeRequiredException(string Protocol, byte[] Content, string ContentType)
			: base(Code, StatusMessage, Content, ContentType, new KeyValuePair<string, string>("Upgrade", Protocol))
		{
		}
	}
}
