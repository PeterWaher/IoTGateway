namespace Waher.Security
{
	/// <summary>
	/// Interface for legal identity attachments.
	/// </summary>
	public interface ILegalIdentityAttachmentUrl : ILegalIdentityAttachment
	{
		/// <summary>
		/// URL to retrieve attachment, if provided.
		/// </summary>
		string Url { get; }
	}
}
