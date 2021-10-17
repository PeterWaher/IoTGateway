using System;
using System.Threading.Tasks;

namespace Waher.Networking.XMPP.Contracts
{
	/// <summary>
	/// Validation Status of smart contract
	/// </summary>
	public enum ContractStatus
	{
		/// <summary>
		/// Contract is not defined.
		/// </summary>
		ContractUndefined,

		/// <summary>
		/// Contract is not approved (yet) by trust provider.
		/// </summary>
		NotApproved,

		/// <summary>
		/// Contract is not valid yet. 
		/// <see cref="Contract.From"/>
		/// </summary>
		NotValidYet,

		/// <summary>
		/// Contract is not valid anymore.
		/// <see cref="Contract.To"/>
		/// </summary>
		NotValidAnymore,

		/// <summary>
		/// Contract is a template, not a valid legal document.
		/// </summary>
		TemplateOnly,

		/// <summary>
		/// The contract is not in a legally binding state. The prerequisites of the contracts are not met.
		/// </summary>
		NotLegallyBinding,

		/// <summary>
		/// Human-readable section not properly defined.
		/// </summary>
		HumanReadableNotWellDefined,

		/// <summary>
		/// Parameter Values not valid
		/// </summary>
		ParameterValuesNotValid,

		/// <summary>
		/// Machine-readable section not properly defined.
		/// </summary>
		MachineReadableNotWellDefined,

		/// <summary>
		/// Unable to get access to schema used to validate the machine-readable section.
		/// </summary>
		NoSchemaAccess,

		/// <summary>
		/// Corrupt schema used for validating machine-readable part.
		/// </summary>
		CorruptSchema,

		/// <summary>
		/// Fraudulent schema used for validating machine-readable part.
		/// </summary>
		FraudulentSchema,

		/// <summary>
		/// Fraudulent machine-readable claim. Does not validate against schema.
		/// </summary>
		FraudulentMachineReadable,

		/// <summary>
		/// No client signature found.
		/// </summary>
		NoClientSignatures,

		/// <summary>
		/// One or more of the client signatures are invalid.
		/// </summary>
		ClientSignatureInvalid,

		/// <summary>
		/// One or more of the legal identities used to sign the contract are invalid.
		/// </summary>
		ClientIdentityInvalid,

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
		/// Contract valid.
		/// </summary>
		Valid
	}

	/// <summary>
	/// Delegate for identity validation callback methods.
	/// </summary>
	/// <param name="Sender">Sender</param>
	/// <param name="e">Event arguments</param>
	public delegate Task ContractValidationEventHandler(object Sender, ContractValidationEventArgs e);

	/// <summary>
	/// Event arguments for identity validation responses
	/// </summary>
	public class ContractValidationEventArgs : EventArgs
	{
		private readonly ContractStatus status;
		private readonly object state;

		/// <summary>
		/// Event arguments for identity validation responses
		/// </summary>
		/// <param name="Status">Validation status</param>
		/// <param name="State">State object</param>
		public ContractValidationEventArgs(ContractStatus Status, object State)
			: base()
		{
			this.status = Status;
			this.state = State;
		}

		/// <summary>
		/// Validation status of smart contract.
		/// </summary>
		public ContractStatus Status => this.status;

		/// <summary>
		/// State object.
		/// </summary>
		public object State => this.state;
	}
}
