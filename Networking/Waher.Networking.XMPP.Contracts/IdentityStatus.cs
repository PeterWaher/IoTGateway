﻿namespace Waher.Networking.XMPP.Contracts
{
	/// <summary>
	/// Validation Status of legal identity
	/// </summary>
	public enum IdentityStatus
	{
		/// <summary>
		/// Identity is not defined.
		/// </summary>
		IdentityUndefined,

		/// <summary>
		/// Identity is not approved (yet) by trust provider.
		/// </summary>
		NotApproved,

		/// <summary>
		/// Legal identity is not valid yet. 
		/// <see cref="LegalIdentity.From"/>
		/// </summary>
		NotValidYet,

		/// <summary>
		/// Legal identity is not valid anymore.
		/// <see cref="LegalIdentity.To"/>
		/// </summary>
		NotValidAnymore,

		/// <summary>
		/// No client public key found.
		/// </summary>
		NoClientPublicKey,

		/// <summary>
		/// Client key not recognized.
		/// </summary>
		ClientKeyNotRecognized,

		/// <summary>
		/// No client signature found.
		/// </summary>
		NoClientSignature,

		/// <summary>
		/// Client signature invalid.
		/// </summary>
		ClientSignatureInvalid,

		/// <summary>
		/// Attachment is missing download URL.
		/// </summary>
		AttachmentLacksUrl,

		/// <summary>
		/// Unable to download attachment.
		/// </summary>
		AttachmentUnavailable,

		/// <summary>
		/// Information about attachment is inconsistent.
		/// </summary>
		AttachmentInconsistency,

		/// <summary>
		/// Attachment signature is invalid.
		/// </summary>
		AttachmentSignatureInvalid,

		/// <summary>
		/// No Trust Provider attesting to the validity of the identity.
		/// </summary>
		NoTrustProvider,

		/// <summary>
		/// No provider public key found.
		/// </summary>
		NoProviderPublicKey,

		/// <summary>
		/// No Trust Provider signature found.
		/// </summary>
		NoProviderSignature,

		/// <summary>
		/// Provider signature invalid.
		/// </summary>
		ProviderSignatureInvalid,

		/// <summary>
		/// Provider key not recognized.
		/// </summary>
		ProviderKeyNotRecognized,

		/// <summary>
		/// Response to a query was not received in a timely fashion.
		/// </summary>
		NoResponse,

		/// <summary>
		/// Legal identity valid
		/// </summary>
		Valid
	}
}
