using System.Collections.Generic;
using Waher.Events;

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
		/// <param name="HeaderFields">HTTP Header fields to include in the response.</param>
		public UpgradeRequiredException(string Protocol, params KeyValuePair<string, string>[] HeaderFields)
			: base(Code, StatusMessage, HeaderFields.Join(new KeyValuePair<string, string>("Upgrade", Protocol)))
		{
		}

		/// <summary>
		/// The client should switch to a different protocol such as TLS/1.0, given in the Upgrade header field.
		/// </summary>
		/// <param name="Protocol">Protocol to upgrade to.</param>
		/// <param name="ContentObject">Any content object to return. The object will be encoded before being sent.</param>
		/// <param name="HeaderFields">HTTP Header fields to include in the response.</param>
		public UpgradeRequiredException(string Protocol, object ContentObject, params KeyValuePair<string, string>[] HeaderFields)
			: base(Code, StatusMessage, ContentObject, HeaderFields.Join(new KeyValuePair<string, string>("Upgrade", Protocol)))
		{
		}

		/// <summary>
		/// The client should switch to a different protocol such as TLS/1.0, given in the Upgrade header field.
		/// </summary>
		/// <param name="Protocol">Protocol to upgrade to.</param>
		/// <param name="Content">Any encoded content to return.</param>
		/// <param name="ContentType">The content type of <paramref name="Content"/>, if provided.</param>
		/// <param name="HeaderFields">HTTP Header fields to include in the response.</param>
		public UpgradeRequiredException(string Protocol, byte[] Content, string ContentType, params KeyValuePair<string, string>[] HeaderFields)
			: base(Code, StatusMessage, Content, ContentType, HeaderFields.Join(new KeyValuePair<string, string>("Upgrade", Protocol)))
		{
		}
	}
}
