using System;
using System.Net.Mail;

namespace Waher.Security
{
	/// <summary>
	/// Lists recognized legal identity states.
	/// </summary>
	public enum IdentityState
	{
		/// <summary>
		/// An application has been received.
		/// </summary>
		Created,

		/// <summary>
		/// The legal identity has been rejected.
		/// </summary>
		Rejected,

		/// <summary>
		/// The legal identity is authenticated and approved by the Trust Provider.
		/// </summary>
		Approved,

		/// <summary>
		/// The legal identity has been explicitly obsoleted by its owner, or by the Trust Provider.
		/// </summary>
		Obsoleted,

		/// <summary>
		/// The legal identity has been reported compromised by its owner, or by the Trust Provider.
		/// </summary>
		Compromised
	}

	/// <summary>
	/// Interfaces for legal identities.
	/// </summary>
	public interface ILegalIdentity
	{
		/// <summary>
		/// ID of the legal identity
		/// </summary>
		string Id { get; }

		/// <summary>
		/// Provider where the identity is maintained.
		/// </summary>
		string Provider { get; }

		/// <summary>
		/// Current state of identity
		/// </summary>
		IdentityState State { get; }

		/// <summary>
		/// When the identity object was created
		/// </summary>
		DateTime Created { get; }

		/// <summary>
		/// When the identity object was last updated
		/// </summary>
		DateTime Updated { get; }

		/// <summary>
		/// From what point in time the legal identity is valid.
		/// </summary>
		DateTime From { get; }

		/// <summary>
		/// To what point in time the legal identity is valid.
		/// </summary>
		DateTime To { get; }

		/// <summary>
		/// Properties detailing the legal identity.
		/// </summary>
		ILegalIdentityProperty[] Properties { get; }

		/// <summary>
		/// Attachments assigned to the legal identity.
		/// </summary>
		ILegalIdentityAttachment[] Attachments { get; }

		/// <summary>
		/// Type of key used for client signatures
		/// </summary>
		string ClientKeyName { get; }

		/// <summary>
		/// Client Public key
		/// </summary>
		byte[] ClientPubKey { get; }

		/// <summary>
		/// Client signature
		/// </summary>
		byte[] ClientSignature { get; }

		/// <summary>
		/// Server signature
		/// </summary>
		byte[] ServerSignature { get; }

		/// <summary>
		/// If the identity has a client public key
		/// </summary>
		bool HasClientPublicKey { get; }

		/// <summary>
		/// If the identity has a client signature
		/// </summary>
		bool HasClientSignature { get; }

		/// <summary>
		/// Access to property values.
		/// </summary>
		/// <param name="Key">Property key</param>
		/// <returns>Corresponding property value, if one is found with the same key, or the empty string, if not.</returns>
		string this[string Key] { get; }
	}
}
