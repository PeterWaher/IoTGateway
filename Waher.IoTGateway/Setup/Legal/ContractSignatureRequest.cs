using System;
using System.Text;
using System.Xml;
using Waher.Networking.XMPP.Contracts;
using Waher.Persistence.Attributes;

namespace Waher.IoTGateway.Setup.Legal
{
	/// <summary>
	/// Contains information about a contract signature request.
	/// </summary>
	[CollectionName("SignatureRequests")]
	[TypeName(TypeNameSerialization.FullName)]
	[Index("Signed", "Received")]
	[Index("ContractId", "Role", "Received")]
	public class ContractSignatureRequest
	{
		private string objectId = null;
		private Contract contract = null;
		private DateTime received = DateTime.MinValue;
		private DateTime? signed = null;
		private string contractXml = string.Empty;
		private string contractId = string.Empty;
		private string role = string.Empty;
		private string module = string.Empty;
		private string provider = string.Empty;
		private string purpose = string.Empty;

		/// <summary>
		/// Contains information about a contract signature request.
		/// </summary>
		public ContractSignatureRequest()
		{
		}

		/// <summary>
		/// Object ID
		/// </summary>
		[ObjectId]
		public string ObjectId
		{
			get => this.objectId;
			set => this.objectId = value;
		}

		/// <summary>
		/// Contract to sign
		/// </summary>
		[IgnoreMember]
		public Contract Contract
		{
			get
			{
				if (this.contract is null)
				{
					XmlDocument Doc = new XmlDocument()
					{
						PreserveWhitespace = true
					};
					Doc.LoadXml(this.contractXml);
					this.contract = Contract.Parse(Doc.DocumentElement, out bool _, out bool _);
				}

				return this.contract;
			}

			set
			{
				this.contract = value;
				StringBuilder Xml = new StringBuilder();
				this.contract.Serialize(Xml, true, true, true, true, true, true, true);
				this.contractXml = Xml.ToString();
			}
		}

		/// <summary>
		/// XML of contract to sign.
		/// </summary>
		[DefaultValueStringEmpty]
		public string ContractXml
		{
			get => this.contractXml;
			set
			{
				this.contractXml = value;
				this.contract = null;
			}
		}

		/// <summary>
		/// When request was received.
		/// </summary>
		public DateTime Received
		{
			get => this.received;
			set => this.received = value;
		}

		/// <summary>
		/// When contract was signed.
		/// </summary>
		public DateTime? Signed
		{
			get => this.signed;
			set => this.signed = value;
		}

		/// <summary>
		/// Contract ID
		/// </summary>
		public string ContractId
		{
			get => this.contractId;
			set => this.contractId = value;
		}

		/// <summary>
		/// Requested role.
		/// </summary>
		public string Role
		{
			get => this.role;
			set => this.role = value;
		}

		/// <summary>
		/// Module making the request.
		/// </summary>
		[DefaultValueStringEmpty]
		public string Module
		{
			get => this.module;
			set => this.module = value;
		}

		/// <summary>
		/// Provider of the contract.
		/// </summary>
		[DefaultValueStringEmpty]
		public string Provider
		{
			get => this.provider;
			set => this.provider = value;
		}

		/// <summary>
		/// Purpose for signing the contract.
		/// </summary>
		[DefaultValueStringEmpty]
		public string Purpose
		{
			get => this.purpose;
			set => this.purpose = value;
		}

	}
}
