using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using Waher.Content.Xml;
using Waher.Networking.XMPP.Contracts.EventArguments;
using Waher.Networking.XMPP.Contracts.HumanReadable;
using Waher.Persistence;
using Waher.Script;
using Waher.Script.Constants;

namespace Waher.Networking.XMPP.Contracts
{
	/// <summary>
	/// Contract-reference parameter
	/// </summary>
	public class ContractReferenceParameter : Parameter
	{
		private Label[] labels = null;
		private CaseInsensitiveString value = CaseInsensitiveString.Empty;
		private Contract reference = null;
		private ContractStatus? referenceStatus = null;
		private KeyValuePair<string, object>[] referenceStatusTags = null;
		private string localName = string.Empty;
		private string @namespace = string.Empty;
		private string templateId = string.Empty;
		private string provider = string.Empty;
		private string creatorRole = string.Empty;
		private bool required = false;

		/// <summary>
		/// Parameter value.
		/// </summary>
		public override object ObjectValue => this.@value;

		/// <summary>
		/// String representation of value.
		/// </summary>
		public override string StringValue
		{
			get => this.Value?.Value ?? string.Empty;
			set => this.Value = value;
		}

		/// <summary>
		/// Human-readable label to be shown in stead of a reference, in contract text, in different languages.
		/// </summary>
		public Label[] Labels
		{
			get => this.labels;
			set => this.labels = value;
		}

		/// <summary>
		/// The value of the parameter, containing the ID of the contract that is being referenced.
		/// </summary>
		public CaseInsensitiveString Value
		{
			get => this.@value;
			set
			{
				if (this.@value != value)
				{
					this.@value = value;
					this.ProtectedValue = null;
					this.reference = null;
					this.referenceStatus = null;
					this.referenceStatusTags = null;
				}
			}
		}

		/// <summary>
		/// Restriction on the local name of the machine-readable part of the referenced contract.
		/// </summary>
		public string LocalName
		{
			get => this.localName;
			set => this.localName = value;
		}

		/// <summary>
		/// Restriction on the namespace of the machine-readable part of the referenced contract.
		/// </summary>
		public string Namespace
		{
			get => this.@namespace;
			set => this.@namespace = value;
		}

		/// <summary>
		/// Restriction on the Template ID of the referenced contract.
		/// </summary>
		public string TemplateId
		{
			get => this.templateId;
			set => this.templateId = value;
		}

		/// <summary>
		/// Restriction on the provider of the referenced contract.
		/// </summary>
		public string Provider
		{
			get => this.provider;
			set => this.provider = value;
		}

		/// <summary>
		/// Restriction on the role the creator must have in the referenced contract.
		/// </summary>
		public string CreatorRole
		{
			get => this.creatorRole;
			set => this.creatorRole = value;
		}

		/// <summary>
		/// If the reference parameter is required or not.
		/// </summary>
		public bool Required
		{
			get => this.required;
			set => this.required = value;
		}

		/// <summary>
		/// Loaded reference contract. The contract is loaded during parameter validation.
		/// </summary>
		public Contract Reference => this.reference;

		/// <summary>
		/// Parameter type name, corresponding to the local name of the parameter element in XML.
		/// </summary>
		public override string ParameterType => "contractReferenceParameter";

		/// <summary>
		/// Serializes the parameter, in normalized form.
		/// </summary>
		/// <param name="Xml">XML Output</param>
		/// <param name="UsingTemplate">If the XML is for creating a contract using a template.</param>
		public override void Serialize(StringBuilder Xml, bool UsingTemplate)
		{
			Xml.Append("<contractReferenceParameter");

			if (!UsingTemplate)
			{
				if (!string.IsNullOrEmpty(this.creatorRole))
				{
					Xml.Append(" creatorRole=\"");
					Xml.Append(XML.Encode(this.creatorRole.Normalize(NormalizationForm.FormC)));
					Xml.Append('"');
				}

				if (!string.IsNullOrEmpty(this.Expression))
				{
					Xml.Append(" exp=\"");
					Xml.Append(XML.Encode(this.Expression.Normalize(NormalizationForm.FormC)));
					Xml.Append('"');
				}

				if (!string.IsNullOrEmpty(this.Guide))
				{
					Xml.Append(" guide=\"");
					Xml.Append(XML.Encode(this.Guide.Normalize(NormalizationForm.FormC)));
					Xml.Append('"');
				}

				if (!string.IsNullOrEmpty(this.localName))
				{
					Xml.Append(" localName=\"");
					Xml.Append(XML.Encode(this.localName.Normalize(NormalizationForm.FormC)));
					Xml.Append('"');
				}
			}

			Xml.Append(" name=\"");
			Xml.Append(XML.Encode(this.Name));
			Xml.Append('"');

			if (!UsingTemplate && !string.IsNullOrEmpty(this.@namespace))
			{
				Xml.Append(" namespace=\"");
				Xml.Append(XML.Encode(this.@namespace.Normalize(NormalizationForm.FormC)));
				Xml.Append('"');
			}

			if (this.CanSerializeProtectedValue)
			{
				Xml.Append(" protected=\"");
				Xml.Append(Convert.ToBase64String(this.ProtectedValue));
				Xml.Append('"');
			}

			if (!UsingTemplate)
			{
				if (this.Protection != ProtectionLevel.Normal)
				{
					Xml.Append(" protection=\"");
					Xml.Append(this.Protection.ToString());
					Xml.Append('"');
				}

				if (!string.IsNullOrEmpty(this.provider))
				{
					Xml.Append(" provider=\"");
					Xml.Append(XML.Encode(this.provider.Normalize(NormalizationForm.FormC)));
					Xml.Append('"');
				}

				if (this.Required)
					Xml.Append(" required=\"true\"");

				if (!string.IsNullOrEmpty(this.templateId))
				{
					Xml.Append(" templateId=\"");
					Xml.Append(XML.Encode(this.templateId.Normalize(NormalizationForm.FormC)));
					Xml.Append('"');
				}
			}

			if (!CaseInsensitiveString.IsNullOrEmpty(this.@value) && this.CanSerializeValue)
			{
				Xml.Append(" value=\"");
				Xml.Append(XML.Encode(this.@value.Value));
				Xml.Append('"');
			}

			if (!UsingTemplate &&
				((!(this.Descriptions is null) && this.Descriptions.Length > 0) ||
				(!(this.labels is null) && this.labels.Length > 0)))
			{
				Xml.Append('>');

				if (!(this.Descriptions is null))
				{
					foreach (HumanReadableText Description in this.Descriptions)
						Description.Serialize(Xml, "description", null);
				}

				if (!(this.labels is null))
				{
					foreach (Label Label in this.labels)
						Label.Serialize(Xml);
				}

				Xml.Append("</contractReferenceParameter>");
			}
			else
				Xml.Append("/>");
		}

		/// <summary>
		/// Checks if the parameter value is valid.
		/// </summary>
		/// <param name="Variables">Collection of parameter values.</param>
		/// <param name="Client">Connected contracts client. If offline or null, partial validation in certain cases will be performed.</param>
		/// <returns>If parameter value is valid.</returns>
		public override async Task<bool> IsParameterValid(Variables Variables, ContractsClient Client)
		{
			this.ErrorReason = null;
			this.ErrorText = null;

			if (CaseInsensitiveString.IsNullOrEmpty(this.@value))
			{
				if (this.required)
				{
					this.ErrorReason = ParameterErrorReason.LacksValue;
					return false;
				}
				else
					return true;
			}

			if (!XmppClient.BareJidRegEx.IsMatch(this.@value))
			{
				this.ErrorReason = ParameterErrorReason.InvalidReference;
				return false;
			}

			if (!(Client is null) && Client.Client.State == XmppState.Connected)
			{
				try
				{
					if (this.reference is null || this.@value != this.reference.ContractId)
					{
						this.reference = null;
						this.referenceStatus = null;
						this.referenceStatusTags = null;
						this.reference = await Client.GetContractAsync(this.@value);
					}

					if (this.reference is null)
						return false;

					if (!string.IsNullOrEmpty(this.localName) &&
						this.localName != this.reference.ForMachinesLocalName)
					{
						this.ErrorReason = ParameterErrorReason.UnableToGetContract;
						return false;
					}

					if (!string.IsNullOrEmpty(this.@namespace) &&
						this.@namespace != this.reference.ForMachinesNamespace)
					{
						this.ErrorReason = ParameterErrorReason.InvalidContractNamespace;
						return false;
					}

					if (!string.IsNullOrEmpty(this.templateId) &&
						this.templateId != this.reference.TemplateId)
					{
						this.ErrorReason = ParameterErrorReason.InvalidTemplateId;
						return false;
					}

					if (!string.IsNullOrEmpty(this.provider) &&
						this.provider != this.reference.Provider)
					{
						this.ErrorReason = ParameterErrorReason.InvalidProvider;
						return false;
					}

					if (!string.IsNullOrEmpty(this.creatorRole))
					{
						// TODO
					}

					if (!this.referenceStatus.HasValue)
					{
						ContractValidationEventArgs e = await Client.ValidateAsync(this.reference, true, false, false, false);
						this.referenceStatus = e.Status;
						this.referenceStatusTags = e.Tags;
					}

					if (this.referenceStatus != ContractStatus.Valid)
					{
						StringBuilder sb = new StringBuilder();

						switch (this.referenceStatus)
						{
							case ContractStatus.ContractUndefined:
								sb.Append("Client Contract is not defined.");
								break;

							case ContractStatus.NotApproved:
								sb.Append("Contract is not approved (yet) by trust provider.");
								break;

							case ContractStatus.NotValidYet:
								sb.Append("Contract is not valid yet.");
								break;

							case ContractStatus.NotValidAnymore:
								sb.Append("Contract is not valid anymore.");
								break;

							case ContractStatus.TemplateOnly:
								sb.Append("Contract is a template, not a valid legal document.");
								break;

							case ContractStatus.NotLegallyBinding:
								sb.Append("The contract is not in a legally binding state. The prerequisites of the contracts are not met.");
								break;

							case ContractStatus.HumanReadableNotWellDefined:
								sb.Append("Human-readable section not properly defined.");
								break;

							case ContractStatus.ParameterValuesNotValid:
								sb.Append("Parameter Values not valid");
								break;

							case ContractStatus.MachineReadableNotWellDefined:
								sb.Append("Machine-readable section not properly defined.");
								break;

							case ContractStatus.NoSchemaAccess:
								sb.Append("Unable to get access to schema used to validate the machine-readable section.");
								break;

							case ContractStatus.CorruptSchema:
								sb.Append("Corrupt schema used for validating machine-readable part.");
								break;

							case ContractStatus.FraudulentSchema:
								sb.Append("Fraudulent schema used for validating machine-readable part.");
								break;

							case ContractStatus.FraudulentMachineReadable:
								sb.Append("Fraudulent machine-readable claim. Does not validate against schema.");
								break;

							case ContractStatus.NoClientSignatures:
								sb.Append("No client signature found.");
								break;

							case ContractStatus.ClientSignatureInvalid:
								sb.Append("One or more of the client signatures are invalid.");
								break;

							case ContractStatus.ClientSignatureNotValidated:
								sb.Append("Unable to validate a client signature.");
								break;

							case ContractStatus.ClientIdentityInvalid:
								sb.Append("One or more of the legal identities used to sign the contract are invalid.");
								break;

							case ContractStatus.AttachmentLacksUrl:
								sb.Append("Attachment is missing download URL.");
								break;

							case ContractStatus.AttachmentUnavailable:
								sb.Append("Unable to download attachment.");
								break;

							case ContractStatus.AttachmentInconsistency:
								sb.Append("Information about attachment is inconsistent.");
								break;

							case ContractStatus.AttachmentSignatureInvalid:
								sb.Append("Attachment signature is invalid.");
								break;

							case ContractStatus.NoTrustProvider:
								sb.Append("No Trust Provider attesting to the validity of the identity.");
								break;

							case ContractStatus.NoProviderPublicKey:
								sb.Append("No provider public key found.");
								break;

							case ContractStatus.NoProviderSignature:
								sb.Append("No Trust Provider signature found.");
								break;

							case ContractStatus.ProviderSignatureInvalid:
								sb.Append("Provider signature invalid.");
								break;

							case ContractStatus.ProviderKeyNotRecognized:
								sb.Append("Provider key not recognized.");
								break;

							case ContractStatus.NoResponse:
								sb.Append("Response to query was not returned in a timely fashion.");
								break;

							default:
								sb.Append("Client contract validation failed.");
								break;
						}

						if (!(this.referenceStatusTags is null))
						{
							bool First = true;

							foreach (KeyValuePair<string, object> Tag in this.referenceStatusTags)
							{
								if (First)
								{
									sb.AppendLine();
									First = false;
								}

								sb.AppendLine();

								switch (Tag.Key)
								{
									case "State":
										sb.Append("State");
										break;

									case "From":
										sb.Append("From");
										break;

									case "To":
										sb.Append("To");
										break;

									case "DataBase64":
										sb.Append("Data (BASE64)");
										break;

									case "SignatureBase64":
										sb.Append("Signature (BASE64)");
										break;

									case "KeyName":
										sb.Append("Client Key Name");
										break;

									case "AttachmentId":
										sb.Append("Attachment ID");
										break;

									case "AttachmentUrl":
										sb.Append("Attachment URL");
										break;

									case "ExpectedContentType":
										sb.Append("Expected Content-Type");
										break;

									case "ContentType":
										sb.Append("Content-Type returned");
										break;

									case "AttachmentSignatureBase64":
										sb.Append("Attachment Signature (BASE64)");
										break;

									case "Error":
										sb.Append("Error");
										break;

									case "Provider":
										sb.Append("Provider");
										break;

									case "LocalName":
										sb.Append("Local Name");
										break;

									case "Namespace":
										sb.Append("Namespace");
										break;

									case "PublicKeyBase64":
										sb.Append("Public Key (BASE64)");
										break;

									case "Status":
										sb.Append("Validation Status");
										break;

									case "LegalId":
										sb.Append("Legal ID");
										break;

									case "IdentityStatus":
										sb.Append("Identity Validation Status");
										break;

									default:
										sb.Append(Tag.Key);
										break;
								}

								sb.Append(": ");
								sb.Append(Tag.Value.ToString());
							}
						}

						this.ErrorReason = ParameterErrorReason.ContractNotValid;
						this.ErrorText = sb.ToString();

						return false;
					}
				}
				catch (Exception ex)
				{
					this.ErrorReason = ParameterErrorReason.Exception;
					this.ErrorText = ex.Message;

					return false;
				}

				Dictionary<string, object> ContractParameters = new Dictionary<string, object>();

				foreach (Parameter P in this.reference.Parameters)
					ContractParameters[P.Name] = P.ObjectValue;

				Variables[this.Name] = ContractParameters;
			}

			return await base.IsParameterValid(Variables, Client);
		}

		/// <summary>
		/// Populates a variable collection with the value of the parameter.
		/// </summary>
		/// <param name="Variables">Variable collection.</param>
		public override void Populate(Variables Variables)
		{
			// Populated in the validation phase.
		}

		/// <summary>
		/// Sets the parameter value.
		/// </summary>
		/// <param name="Value">Value</param>
		/// <exception cref="ArgumentException">If <paramref name="Value"/> is not of the correct type.</exception>
		public override void SetValue(object Value)
		{
			this.Value = Value.ToString();
		}

		/// <summary>
		/// Sets the minimum value allowed by the parameter.
		/// </summary>
		/// <param name="Value">Minimum value.</param>
		/// <param name="Inclusive">If the value is included in the range. If null, keeps the original value.</param>
		public override void SetMinValue(object Value, bool? Inclusive)
		{
			throw new InvalidOperationException("Minimum value for Contract Reference parameter types not supported.");
		}

		/// <summary>
		/// Sets the maximum value allowed by the parameter.
		/// </summary>
		/// <param name="Value">Maximum value.</param>
		/// <param name="Inclusive">If the value is included in the range. If null, keeps the original value.</param>
		public override void SetMaxValue(object Value, bool? Inclusive)
		{
			throw new InvalidOperationException("Maximum value for Contract Reference parameter types not supported.");
		}

		/// <summary>
		/// Imports parameter values from its XML definition.
		/// </summary>
		/// <param name="Xml">XML definition.</param>
		/// <returns>If import was successful.</returns>
		public override async Task<bool> Import(XmlElement Xml)
		{
			List<Label> Labels = new List<Label>();

			foreach (XmlNode N in Xml.ChildNodes)
			{
				if (N is XmlElement E)
				{
					switch (E.LocalName)
					{
						case "label": // Smart contract
							Label Label = Label.Parse(E);
							if (Label is null || !(await Label.IsWellDefined() is null))
								return false;

							Labels.Add(Label);
							break;

						case "Label": // Simplified (ex. state machine note command).
							Label = Label.ParseSimplified(E);
							if (Label is null || !(await Label.IsWellDefined() is null))
								return false;

							Labels.Add(Label);
							break;
					}
				}
			}

			this.labels = Labels.ToArray();

			this.@value = Xml.HasAttribute("value") ? XML.Attribute(Xml, "value") : null;
			this.required = XML.Attribute(Xml, "required", false);
			this.localName = Xml.HasAttribute("localName") ? XML.Attribute(Xml, "localName") : null;
			this.@namespace = Xml.HasAttribute("namespace") ? XML.Attribute(Xml, "namespace") : null;
			this.templateId = Xml.HasAttribute("templateId") ? XML.Attribute(Xml, "templateId") : null;
			this.provider = Xml.HasAttribute("provider") ? XML.Attribute(Xml, "provider") : null;
			this.creatorRole = Xml.HasAttribute("creatorRole") ? XML.Attribute(Xml, "creatorRole") : null;
			this.reference = null;
			this.referenceStatus = null;
			this.referenceStatusTags = null;

			return await base.Import(Xml);
		}

		/// <summary>
		/// If a local name of a child XML element is recognized.
		/// </summary>
		/// <param name="LocalName">Local name.</param>
		/// <returns>If recognized.</returns>
		protected override bool IsChildLocalNameRecognized(string LocalName)
		{
			switch (LocalName)
			{
				case "label": // Smart contract
				case "Label": // Simplified (ex. state machine note command).
					return true;

				default:
					return base.IsChildLocalNameRecognized(LocalName);
			}
		}
	}
}