using System;
using System.Text;
using Waher.Networking.XMPP.P2P.SymmetricCiphers;
using Waher.Persistence;
using Waher.Persistence.Attributes;
using Waher.Runtime.Collections;
using Waher.Runtime.Inventory;
using Waher.Security.CallStack;

namespace Waher.Networking.XMPP.Contracts
{
	/// <summary>
	/// Contains a persisted shared secret associated with a smart contract.
	/// </summary>
	[CollectionName("ContractSharedSecretStates")]
	[TypeName(TypeNameSerialization.None)]
	[Index("BareJid", "ContractId")]
	public class ContractSharedSecretState : IEncryptedProperties
	{
		private static ICallStackCheck[] approvedSources = null;

		private string objectId = null;
		private CaseInsensitiveString bareJid = CaseInsensitiveString.Empty;
		private string contractId = string.Empty;
		private string creatorJid = string.Empty;
		private SymmetricCipherAlgorithms keyAlgorithm = SymmetricCipherAlgorithms.Aes256;
		private byte[] sharedSecret = null;

		/// <summary>
		/// Contains a persisted shared secret associated with a smart contract.
		/// </summary>
		public ContractSharedSecretState()
		{
		}

		/// <summary>
		/// Contains a persisted shared secret associated with a smart contract.
		/// </summary>
		/// <param name="ContractId">Contract ID.</param>
		public ContractSharedSecretState(string ContractId)
		{
			this.contractId = ContractId;
		}

		/// <summary>
		/// Object ID.
		/// </summary>
		[ObjectId]
		public string ObjectId
		{
			get => this.objectId;
			set => this.objectId = value;
		}

		/// <summary>
		/// Bare JID of the client owning the contract state.
		/// </summary>
		public CaseInsensitiveString BareJid
		{
			get => this.bareJid;
			set => this.bareJid = value;
		}

		/// <summary>
		/// Contract ID.
		/// </summary>
		public string ContractId
		{
			get => this.contractId;
			set => this.contractId = value;
		}

		/// <summary>
		/// Bare JID of the contract creator.
		/// </summary>
		[DefaultValueStringEmpty]
		public string CreatorJid
		{
			get => this.creatorJid;
			set => this.creatorJid = value;
		}

		/// <summary>
		/// Symmetric encryption algorithm used with the shared secret.
		/// </summary>
		public SymmetricCipherAlgorithms KeyAlgorithm
		{
			get => this.keyAlgorithm;
			set => this.keyAlgorithm = value;
		}

		/// <summary>
		/// If a shared secret snapshot is available for the contract.
		/// </summary>
		public bool HasSharedSecret => !(this.sharedSecret is null);

		/// <summary>
		/// Shared secret snapshot stored with the contract state.
		/// </summary>
		[DefaultValueNull]
		[Encrypted(32)]
		public byte[] SharedSecret
		{
			get
			{
				AssertAllowed();
				return this.sharedSecret;
			}

			set
			{
				if (!(this.sharedSecret is null))
					AssertAllowed();

				this.sharedSecret = value;
			}
		}

		/// <summary>
		/// Array of properties that are encrypted.
		/// </summary>
		public string[] EncryptedProperties => new string[]
		{
			nameof(this.SharedSecret)
		};

		/// <summary>
		/// If access to sensitive properties is only accessible from a set of approved sources.
		/// </summary>
		/// <param name="ApprovedSources">Approved sources.</param>
		/// <exception cref="NotSupportedException">If trying to change previously set sources.</exception>
		public static void SetAllowedSources(ICallStackCheck[] ApprovedSources)
		{
			if (!(approvedSources is null))
				throw new NotSupportedException("Changing approved sources not permitted.");

			approvedSources = ApprovedSources;
		}

		private static void AssertAllowed()
		{
			if (!(approvedSources is null))
				Assert.CallFromSource(approvedSources);
		}

		/// <inheritdoc/>
		public override string ToString()
		{
			StringBuilder sb = new StringBuilder();

			sb.Append(this.bareJid);
			sb.Append(", ");
			sb.Append(this.contractId);
			sb.Append(", ");
			sb.Append(this.creatorJid);
			sb.Append(", ");
			sb.Append(this.keyAlgorithm.ToString());

			if (!(this.sharedSecret is null))
				sb.Append(", SharedSecret");

			return sb.ToString();
		}
	}
}
