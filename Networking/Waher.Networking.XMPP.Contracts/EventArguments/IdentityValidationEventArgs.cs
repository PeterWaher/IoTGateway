using System;
using System.Threading.Tasks;

namespace Waher.Networking.XMPP.Contracts
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
		/// No Trust Provider attesting to the validity of the identity
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
		/// Legal identity valid
		/// </summary>
		Valid
	}

	/// <summary>
	/// Delegate for identity validation callback methods.
	/// </summary>
	/// <param name="Sender">Sender</param>
	/// <param name="e">Event arguments</param>
	public delegate Task IdentityValidationEventHandler(object Sender, IdentityValidationEventArgs e);

	/// <summary>
	/// Event arguments for identity validation responses
	/// </summary>
	public class IdentityValidationEventArgs : EventArgs
	{
		private readonly IdentityStatus status;
		private readonly object state;

		/// <summary>
		/// Event arguments for identity validation responses
		/// </summary>
		/// <param name="Status">Validation status</param>
		/// <param name="State">State object</param>
		public IdentityValidationEventArgs(IdentityStatus Status, object State)
			: base()
		{
			this.status = Status;
			this.state = State;
		}

		/// <summary>
		/// Validation status of legal identity.
		/// </summary>
		public IdentityStatus Status => this.status;

		/// <summary>
		/// State object.
		/// </summary>
		public object State => this.state;
	}
}
