using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using Waher.Content;
using Waher.Content.Xml;
using Waher.Networking.XMPP.P2P.E2E;
using Waher.Security;
using Waher.Security.EllipticCurves;
using Waher.Networking.XMPP.Contracts.HumanReadable;

namespace Waher.Networking.XMPP.Contracts
{
	/// <summary>
	/// Contains the definition of a contract
	/// </summary>
	public class Contract
	{
		private string contractId = null;
		private string templateId = string.Empty;
		private string provider = null;
		private string forMachinesLocalName = null;
		private string forMachinesNamespace = null;
		private byte[] contentSchemaHash = null;
		private XmlElement forMachines = null;
		private Role[] roles = null;
		private Part[] parts = null;
		private Parameter[] parameters = null;
		private HumanReadableText[] forHumans = null;
		private ClientSignature[] clientSignatures = null;
		private ServerSignature serverSignature = null;
		private HashFunction contentSchemaHashFunction = HashFunction.SHA256;
		private ContractState state = ContractState.Proposed;
		private ContractVisibility visibility = ContractVisibility.CreatorAndParts;
		private ContractParts partsMode = ContractParts.ExplicitlyDefined;
		private DateTime created = DateTime.MinValue;
		private DateTime updated = DateTime.MinValue;
		private DateTime from = DateTime.MinValue;
		private DateTime to = DateTime.MaxValue;
		private DateTime? signAfter = null;
		private DateTime? signBefore = null;
		private Duration duration = null;
		private Duration archiveReq = null;
		private Duration archiveOpt = null;
		private bool canActAsTemplate = false;

		/// <summary>
		/// Contains the definition of a contract
		/// </summary>
		public Contract()
		{
		}

		/// <summary>
		/// Contract identity
		/// </summary>
		public string ContractId
		{
			get { return this.contractId; }
			set { this.contractId = value; }
		}

		/// <summary>
		/// JID of the Trust Provider hosting the contract
		/// </summary>
		public string Provider
		{
			get { return this.provider; }
			set { this.provider = value; }
		}

		/// <summary>
		/// Contract identity of template, if one was used to create the contract.
		/// </summary>
		public string TemplateId
		{
			get { return this.templateId; }
			set { this.templateId = value; }
		}

		/// <summary>
		/// Contract state
		/// </summary>
		public ContractState State
		{
			get { return this.state; }
			set { this.state = value; }
		}

		/// <summary>
		/// When the contract was created
		/// </summary>
		public DateTime Created
		{
			get { return this.created; }
			set { this.created = value; }
		}

		/// <summary>
		/// When the contract was last updated
		/// </summary>
		public DateTime Updated
		{
			get { return this.updated; }
			set { this.updated = value; }
		}

		/// <summary>
		/// From when the contract is valid (if signed)
		/// </summary>
		public DateTime From
		{
			get { return this.from; }
			set { this.from = value; }
		}

		/// <summary>
		/// Until when the contract is valid (if signed)
		/// </summary>
		public DateTime To
		{
			get { return this.to; }
			set { this.to = value; }
		}

		/// <summary>
		/// Signatures will only be accepted after this point in time.
		/// </summary>
		public DateTime? SignAfter
		{
			get { return this.signAfter; }
			set { this.signAfter = value; }
		}

		/// <summary>
		/// Signatures will only be accepted until this point in time.
		/// </summary>
		public DateTime? SignBefore
		{
			get { return this.signBefore; }
			set { this.signBefore = value; }
		}

		/// <summary>
		/// Contrat Visibility
		/// </summary>
		public ContractVisibility Visibility
		{
			get { return this.visibility; }
			set { this.visibility = value; }
		}

		/// <summary>
		/// Duration of the contract. Is counted from the time it is signed by the required parties.
		/// </summary>
		public Duration Duration
		{
			get { return this.duration; }
			set { this.duration = value; }
		}

		/// <summary>
		/// Requied time to archive a signed smart contract, after it becomes obsolete.
		/// </summary>
		public Duration ArchiveRequired
		{
			get { return this.archiveReq; }
			set { this.archiveReq = value; }
		}

		/// <summary>
		/// Optional time to archive a signed smart contract, after it becomes obsolete, and after its required archivation period.
		/// </summary>
		public Duration ArchiveOptional
		{
			get { return this.archiveOpt; }
			set { this.archiveOpt = value; }
		}

		/// <summary>
		/// The hash digest of the schema used to validate the machine-readable contents (<see cref="ForMachines"/>) of the smart contract,
		/// if such a schema was used.
		/// </summary>
		public byte[] ContentSchemaHash
		{
			get { return this.contentSchemaHash; }
			set { this.contentSchemaHash = value; }
		}

		/// <summary>
		/// Hash function of the schema used to validate the machine-readable contents (<see cref="ForMachines"/>) of the smart contract,
		/// if such a schema was used.
		/// </summary>
		public HashFunction ContentSchemaHashFunction
		{
			get { return this.contentSchemaHashFunction; }
			set { this.contentSchemaHashFunction = value; }
		}

		/// <summary>
		/// Roles defined in the smart contract.
		/// </summary>
		public Role[] Roles
		{
			get { return this.roles; }
			set { this.roles = value; }
		}

		/// <summary>
		/// How parts are defined in the smart contract.
		/// </summary>
		public ContractParts PartsMode
		{
			get { return this.partsMode; }
			set { this.partsMode = value; }
		}

		/// <summary>
		/// Defined parts for the smart contract.
		/// </summary>
		public Part[] Parts
		{
			get { return this.parts; }
			set { this.parts = value; }
		}

		/// <summary>
		/// Defined parameters for the smart contract.
		/// </summary>
		public Parameter[] Parameters
		{
			get { return this.parameters; }
			set { this.parameters = value; }
		}

		/// <summary>
		/// Machine-readable contents of the contract.
		/// </summary>
		public XmlElement ForMachines
		{
			get { return this.forMachines; }
			set
			{
				this.forMachines = value;
				this.forMachinesLocalName = value?.LocalName;
				this.forMachinesNamespace = value?.NamespaceURI;
			}
		}

		/// <summary>
		/// Namespace used by the root node of the machine-readable contents of the contract (<see cref="ForMachines"/>).
		/// </summary>
		public string ForMachinesNamespace
		{
			get { return this.forMachinesNamespace; }
		}

		/// <summary>
		/// Local name used by the root node of the machine-readable contents of the contract (<see cref="ForMachines"/>).
		/// </summary>
		public string ForMachinesLocalName
		{
			get { return this.forMachinesLocalName; }
		}

		/// <summary>
		/// Human-readable contents of the contract.
		/// </summary>
		public HumanReadableText[] ForHumans
		{
			get { return this.forHumans; }
			set { this.forHumans = value; }
		}

		/// <summary>
		/// Client signatures of the contract.
		/// </summary>
		public ClientSignature[] ClientSignatures
		{
			get { return this.clientSignatures; }
			set { this.clientSignatures = value; }
		}

		/// <summary>
		/// Server signature attesting to the validity of the contents of the contract.
		/// </summary>
		public ServerSignature ServerSignature
		{
			get { return this.serverSignature; }
			set { this.serverSignature = value; }
		}

		/// <summary>
		/// If the contract can act as a template for other contracts.
		/// </summary>
		public bool CanActAsTemplate
		{
			get { return this.canActAsTemplate; }
			set { this.canActAsTemplate = value; }
		}

		/// <summary>
		/// Parses a contract from is XML representation.
		/// </summary>
		/// <param name="Xml">XML representation</param>
		/// <param name="HasStatus">If a status element was found.</param>
		/// <returns>Parsed contract, or null if it contains errors.</returns>
		public static Contract Parse(XmlElement Xml, out bool HasStatus)
		{
			Contract Result = new Contract();
			bool HasVisibility = false;

			HasStatus = false;

			foreach (XmlAttribute Attr in Xml.Attributes)
			{
				switch (Attr.Name)
				{
					case "id":
						Result.contractId = Attr.Value;
						break;

					case "visibility":
						if (Enum.TryParse<ContractVisibility>(Attr.Value, out ContractVisibility Visibility))
						{
							Result.visibility = Visibility;
							HasVisibility = true;
						}
						else
							return null;
						break;

					case "duration":
						if (Duration.TryParse(Attr.Value, out Duration D))
							Result.duration = D;
						else
							return null;
						break;

					case "archiveReq":
						if (Duration.TryParse(Attr.Value, out D))
							Result.archiveReq = D;
						else
							return null;
						break;

					case "archiveOpt":
						if (Duration.TryParse(Attr.Value, out D))
							Result.archiveOpt = D;
						else
							return null;
						break;

					case "signAfter":
						if (DateTime.TryParse(Attr.Value, out DateTime TP))
							Result.signAfter = TP;
						else
							return null;
						break;

					case "signBefore":
						if (DateTime.TryParse(Attr.Value, out TP))
							Result.signBefore = TP;
						else
							return null;
						break;

					case "canActAsTemplate":
						if (CommonTypes.TryParse(Attr.Value, out bool b))
							Result.canActAsTemplate = b;
						else
							return null;
						break;

					case "xmlns":
						break;

					default:
						if (Attr.Prefix == "xmlns")
							break;
						else
							return null;
				}
			}

			if (!HasVisibility ||
				Result.duration == null ||
				Result.archiveReq == null ||
				Result.archiveOpt == null ||
				Result.signBefore <= Result.signAfter)
			{
				return null;
			}

			List<HumanReadableText> ForHumans = new List<HumanReadableText>();
			List<Role> Roles = new List<Role>();
			List<Parameter> Parameters = new List<Parameter>();
			List<ClientSignature> Signatures = new List<ClientSignature>();
			XmlElement Content = null;
			HumanReadableText Text;
			bool First = true;
			bool PartsDefined = false;

			foreach (XmlNode N in Xml.ChildNodes)
			{
				if (!(N is XmlElement E))
					continue;

				if (First)
				{
					Content = E;
					First = false;
					continue;
				}

				switch (E.LocalName)
				{
					case "role":
						List<HumanReadableText> Descriptions = new List<HumanReadableText>();
						Role Role = new Role()
						{
							MinCount = -1,
							MaxCount = -1
						};

						foreach (XmlAttribute Attr in E.Attributes)
						{
							switch (Attr.Name)
							{
								case "name":
									Role.Name = Attr.Value;
									if (string.IsNullOrEmpty(Role.Name))
										return null;
									break;

								case "minCount":
									if (int.TryParse(Attr.Value, out int i) && i >= 0)
										Role.MinCount = i;
									else
										return null;
									break;

								case "maxCount":
									if (int.TryParse(Attr.Value, out i) && i >= 0)
										Role.MaxCount = i;
									else
										return null;
									break;

								case "xmlns":
									break;

								default:
									if (Attr.Prefix == "xmlns")
										break;
									else
										return null;
							}
						}

						if (string.IsNullOrEmpty(Role.Name) ||
							Role.MinCount < 0 ||
							Role.MaxCount < 0)
						{
							return null;
						}

						foreach (XmlNode N2 in E.ChildNodes)
						{
							if (N2 is XmlElement E2)
							{
								if (E2.LocalName == "description")
								{
									Text = HumanReadableText.Parse(E2);
									if (Text == null || !Text.IsWellDefined)
										return null;

									Descriptions.Add(Text);
								}
								else
									return null;
							}
						}

						if (Descriptions.Count == 0)
							return null;

						Role.Descriptions = Descriptions.ToArray();

						Roles.Add(Role);
						break;

					case "parts":
						List<Part> Parts = null;
						ContractParts? Mode = null;

						foreach (XmlNode N2 in E.ChildNodes)
						{
							if (N2 is XmlElement E2)
							{
								switch (E2.LocalName)
								{
									case "open":
										if (Mode.HasValue)
											return null;

										Mode = ContractParts.Open;
										break;

									case "templateOnly":
										if (Mode.HasValue)
											return null;

										Mode = ContractParts.TemplateOnly;
										break;

									case "part":
										if (Mode.HasValue)
										{
											if (Mode.Value != ContractParts.ExplicitlyDefined)
												return null;
										}
										else
											Mode = ContractParts.ExplicitlyDefined;

										string LegalId = null;
										string RoleRef = null;

										foreach (XmlAttribute Attr in E2.Attributes)
										{
											switch (Attr.Name)
											{
												case "legalId":
													LegalId = Attr.Value;
													break;

												case "role":
													RoleRef = Attr.Value;
													break;

												case "xmlns":
													break;

												default:
													if (Attr.Prefix == "xmlns")
														break;
													else
														return null;
											}
										}

										if (LegalId == null || RoleRef == null || string.IsNullOrEmpty(LegalId) || string.IsNullOrEmpty(RoleRef))
											return null;

										bool RoleFound = false;

										foreach (Role Role2 in Roles)
										{
											if (Role2.Name == RoleRef)
											{
												RoleFound = true;
												break;
											}
										}

										if (!RoleFound)
											return null;

										if (Parts == null)
											Parts = new List<Part>();

										Parts.Add(new Part()
										{
											LegalId = LegalId,
											Role = RoleRef
										});

										break;

									default:
										return null;
								}
							}
						}

						if (!Mode.HasValue)
							return null;

						Result.partsMode = Mode.Value;
						Result.parts = Parts?.ToArray();

						PartsDefined = true;
						break;

					case "parameters":
						foreach (XmlNode N2 in E.ChildNodes)
						{
							if (N2 is XmlElement E2)
							{
								string Name = XML.Attribute(E2, "name");
								if (string.IsNullOrEmpty(Name))
									return null;

								Descriptions = new List<HumanReadableText>();

								foreach (XmlNode N3 in E2.ChildNodes)
								{
									if (N3 is XmlElement E3)
									{
										if (E3.LocalName == "description")
										{
											Text = HumanReadableText.Parse(E3);
											if (Text == null || !Text.IsWellDefined)
												return null;

											Descriptions.Add(Text);
										}
										else
											return null;
									}
								}

								switch (E2.LocalName)
								{
									case "stringParameter":
										Parameters.Add(new StringParameter()
										{
											Name = Name,
											Value = XML.Attribute(E2, "value"),
											Descriptions = Descriptions.ToArray()
										});
										break;

									case "numericalParameter":
										Parameters.Add(new NumericalParameter()
										{
											Name = Name,
											Value = XML.Attribute(E2, "value", 0.0),
											Descriptions = Descriptions.ToArray()
										});
										break;

									default:
										return null;
								}
							}
						}

						if (Parameters.Count == 0)
							return null;
						break;

					case "humanReadableText":
						Text = HumanReadableText.Parse(E);
						if (Text == null || !Text.IsWellDefined)
							return null;

						ForHumans.Add(Text);
						break;

					case "signature":
						ClientSignature Signature = new ClientSignature();

						foreach (XmlAttribute Attr in E.Attributes)
						{
							switch (Attr.Name)
							{
								case "legalId":
									Signature.LegalId = Attr.Value;
									break;

								case "bareJid":
									Signature.BareJid = Attr.Value;
									break;

								case "role":
									Signature.Role = Attr.Value;
									break;

								case "timestamp":
									if (XML.TryParse(Attr.Value, out DateTime TP))
										Signature.Timestamp = TP;
									else
										return null;
									break;

								case "transferable":
									if (CommonTypes.TryParse(Attr.Value, out bool b))
										Signature.Transferable = b;
									else
										return null;
									break;

								case "s1":
									Signature.S1 = Convert.FromBase64String(Attr.Value);
									break;

								case "s2":
									Signature.S2 = Convert.FromBase64String(Attr.Value);
									break;

								case "xmlns":
									break;

								default:
									if (Attr.Prefix == "xmlns")
										break;
									else
										return null;
							}
						}

						if (string.IsNullOrEmpty(Signature.LegalId) ||
							string.IsNullOrEmpty(Signature.BareJid) ||
							string.IsNullOrEmpty(Signature.Role) ||
							Signature.Timestamp == DateTime.MinValue ||
							Signature.S1 == null)
						{
							return null;
						}

						Signatures.Add(Signature);
						break;

					case "status":
						HasStatus = true;
						foreach (XmlAttribute Attr in E.Attributes)
						{
							switch (Attr.Name)
							{
								case "provider":
									Result.provider = Attr.Value;
									break;

								case "state":
									if (Enum.TryParse<ContractState>(Attr.Value, out ContractState ContractState))
										Result.state = ContractState;
									break;

								case "created":
									if (XML.TryParse(Attr.Value, out DateTime TP))
										Result.created = TP;
									break;

								case "updated":
									if (XML.TryParse(Attr.Value, out TP))
										Result.updated = TP;
									break;

								case "from":
									if (XML.TryParse(Attr.Value, out TP))
										Result.from = TP;
									break;

								case "to":
									if (XML.TryParse(Attr.Value, out TP))
										Result.to = TP;
									break;

								case "templateId":
									Result.templateId = Attr.Value;
									break;

								case "schemaHash":
									Result.contentSchemaHash = Convert.FromBase64String(Attr.Value);
									break;

								case "schemaHashFunction":
									if (Enum.TryParse<HashFunction>(Attr.Value, out HashFunction H))
										Result.contentSchemaHashFunction = H;
									else
										return null;
									break;

								case "xmlns":
									break;

								default:
									if (Attr.Prefix == "xmlns")
										break;
									else
										return null;
							}
						}
						break;

					case "serverSignature":
						ServerSignature ServerSignature = new ServerSignature();

						foreach (XmlAttribute Attr in E.Attributes)
						{
							switch (Attr.Name)
							{
								case "timestamp":
									if (XML.TryParse(Attr.Value, out DateTime TP))
										ServerSignature.Timestamp = TP;
									else
										return null;
									break;

								case "s1":
									ServerSignature.S1 = Convert.FromBase64String(Attr.Value);
									break;

								case "s2":
									ServerSignature.S2 = Convert.FromBase64String(Attr.Value);
									break;

								case "xmlns":
									break;

								default:
									if (Attr.Prefix == "xmlns")
										break;
									else
										return null;
							}
						}

						if (ServerSignature.Timestamp == DateTime.MinValue ||
							ServerSignature.S1 == null)
						{
							return null;
						}

						Result.serverSignature = ServerSignature;
						break;

					default:
						return null;
				}
			}

			if (Content == null || ForHumans.Count == 0 || !PartsDefined)
				return null;

			Result.roles = Roles.ToArray();
			Result.parameters = Parameters.ToArray();
			Result.forMachines = Content;
			Result.forMachinesLocalName = Content.LocalName;
			Result.forMachinesNamespace = Content.NamespaceURI;
			Result.forHumans = ForHumans.ToArray();
			Result.clientSignatures = Signatures.ToArray();

			return Result;
		}

		/// <summary>
		/// Normalizes an XML element.
		/// </summary>
		/// <param name="Xml">XML element to normalize</param>
		/// <param name="Output">Normalized XML will be output here</param>
		/// <param name="CurrentNamespace">Namespace at the encapsulating entity.</param>
		public static void NormalizeXml(XmlElement Xml, StringBuilder Output, string CurrentNamespace)
		{
			Output.Append('<');

			SortedDictionary<string, string> Attributes = null;
			string TagName = Xml.LocalName;

			if (!string.IsNullOrEmpty(Xml.Prefix))
				TagName = Xml.Prefix + ":" + TagName;

			Output.Append(TagName);

			foreach (XmlAttribute Attr in Xml.Attributes)
			{
				if (Attributes == null)
					Attributes = new SortedDictionary<string, string>();

				Attributes[Attr.Name] = Attr.Value;
			}

			if (Xml.NamespaceURI != CurrentNamespace && string.IsNullOrEmpty(Xml.Prefix))
			{
				if (Attributes == null)
					Attributes = new SortedDictionary<string, string>();

				Attributes["xmlns"] = Xml.NamespaceURI;
				CurrentNamespace = Xml.NamespaceURI;
			}
			else
				Attributes?.Remove("xmlns");

			if (Attributes != null)
			{
				foreach (KeyValuePair<string, string> Attr in Attributes)
				{
					Output.Append(' ');
					Output.Append(Attr.Key);
					Output.Append("=\"");
					Output.Append(XML.Encode(Attr.Value));
					Output.Append('"');
				}
			}

			bool HasElements = false;

			foreach (XmlNode N in Xml.ChildNodes)
			{
				if (N is XmlElement E)
				{
					if (!HasElements)
					{
						HasElements = true;
						Output.Append('>');
					}

					NormalizeXml(E, Output, CurrentNamespace);
				}
			}

			if (HasElements)
			{
				Output.Append("</");
				Output.Append(TagName);
				Output.Append('>');
			}
			else
				Output.Append("/>");
		}

		/// <summary>
		/// Checks if a contract is legally binding.
		/// </summary>
		/// <param name="CheckCurrentTime">If the current time should be checked as well.</param>
		/// <returns>If the contract is legally binding.</returns>
		public bool IsLegallyBinding(bool CheckCurrentTime)
		{
			if (this.clientSignatures == null || this.serverSignature == null)
				return false;

			switch (this.state)
			{
				case ContractState.Proposed:
				case ContractState.Obsoleted:
				case ContractState.Deleted:
				case ContractState.Rejected:
					return false;
			}

			switch (this.partsMode)
			{
				case ContractParts.TemplateOnly:
					return false;

				case ContractParts.ExplicitlyDefined:
					if (this.parts == null)
						return false;

					foreach (Part Part in this.parts)
					{
						bool Found = false;

						foreach (ClientSignature Signature in this.clientSignatures)
						{
							if (string.Compare(Signature.LegalId, Part.LegalId, true) == 0 &&
								string.Compare(Signature.Role, Part.Role, true) == 0)
							{
								Found = true;
								break;
							}
						}

						if (!Found)
							return false;
					}
					break;
			}

			if (this.roles != null)
			{
				foreach (Role Role in this.roles)
				{
					int Count = 0;

					foreach (ClientSignature Signature in this.clientSignatures)
					{
						if (string.Compare(Signature.Role, Role.Name, true) == 0)
							Count++;
					}

					if (Count < Role.MinCount || Count > Role.MaxCount)
						return false;
				}
			}

			if (CheckCurrentTime)
			{
				DateTime Now = DateTime.Now;

				if (Now < this.from || Now > this.to)
					return false;
			}

			return true;
		}

		/// <summary>
		/// Serializes the Contract, in normalized form.
		/// </summary>
		/// <param name="Xml">XML Output</param>
		/// <param name="IncludeNamespace">If namespace attribute should be included.</param>
		/// <param name="IncludeIdAttribute">If id attribute should be included.</param>
		/// <param name="IncludeClientSignatures">If client signatures should be included.</param>
		/// <param name="IncludeStatus">If the status element should be included.</param>
		/// <param name="IncludeServerSignature">If the server signature should be included.</param>
		public void Serialize(StringBuilder Xml, bool IncludeNamespace, bool IncludeIdAttribute, bool IncludeClientSignatures,
			bool IncludeStatus, bool IncludeServerSignature)
		{
			Xml.Append("<contract archiveOpt=\"");
			Xml.Append(this.archiveOpt.ToString());
			Xml.Append("\" archiveReq=\"");
			Xml.Append(this.archiveReq.ToString());
			Xml.Append("\" canActAsTemplate=\"");
			Xml.Append(CommonTypes.Encode(this.canActAsTemplate));
			Xml.Append("\" duration=\"");
			Xml.Append(this.duration.ToString());
			Xml.Append('"');

			if (IncludeIdAttribute)
			{
				Xml.Append(" id=\"");
				Xml.Append(XML.Encode(this.contractId));
				Xml.Append('"');
			}

			if (this.signAfter.HasValue)
			{
				Xml.Append(" signAfter=\"");
				Xml.Append(XML.Encode(this.signAfter.Value));
				Xml.Append('"');
			}

			if (this.signBefore.HasValue)
			{
				Xml.Append(" signBefore=\"");
				Xml.Append(XML.Encode(this.signBefore.Value));
				Xml.Append('"');
			}

			Xml.Append(" visibility=\"");
			Xml.Append(this.visibility.ToString());
			Xml.Append('"');

			if (IncludeNamespace)
			{
				Xml.Append(" xmlns=\"");
				Xml.Append(ContractsClient.NamespaceSmartContracts);
				Xml.Append('"');
			}

			Xml.Append('>');

			NormalizeXml(this.forMachines, Xml, ContractsClient.NamespaceSmartContracts);

			if (this.roles != null)
			{
				foreach (Role Role in this.roles)
				{
					Xml.Append("<role maxCount=\"");
					Xml.Append(Role.MaxCount.ToString());
					Xml.Append("\" minCount=\"");
					Xml.Append(Role.MinCount.ToString());
					Xml.Append("\" name=\"");
					Xml.Append(XML.Encode(Role.Name));
					Xml.Append("\">");

					foreach (HumanReadableText Description in Role.Descriptions)
						Description.Serialize(Xml, "description", false);

					Xml.Append("</role>");
				}
			}

			Xml.Append("<parts>");

			switch (this.partsMode)
			{
				case ContractParts.Open:
					Xml.Append("<open/>");
					break;

				case ContractParts.TemplateOnly:
					Xml.Append("<templateOnly/>");
					break;

				case ContractParts.ExplicitlyDefined:
					if (this.parts != null)
					{
						foreach (Part Part in this.parts)
						{
							Xml.Append("<part legalId=\"");
							Xml.Append(XML.Encode(Part.LegalId));
							Xml.Append("\" role=\"");
							Xml.Append(XML.Encode(Part.Role));
							Xml.Append("\"/>");
						}
					}
					break;
			}

			Xml.Append("</parts>");

			if (this.parameters != null && this.parameters.Length > 0)
			{
				Xml.Append("<parameters>");

				foreach (Parameter Parameter in this.parameters)
					Parameter.Serialize(Xml);

				Xml.Append("</parameters>");
			}

			if (this.forHumans != null && this.forHumans.Length > 0)
			{
				foreach (HumanReadableText Text in this.forHumans)
					Text.Serialize(Xml);
			}

			if (IncludeClientSignatures && this.clientSignatures != null)
			{
				foreach (Signature Signature in this.clientSignatures)
					Signature.Serialize(Xml);
			}

			if (IncludeStatus)
			{
				Xml.Append("<status created=\"");
				Xml.Append(XML.Encode(this.created));

				if (this.from != DateTime.MinValue)
				{
					Xml.Append("\" from=\"");
					Xml.Append(XML.Encode(this.from));
				}

				Xml.Append("\" provider=\"");
				Xml.Append(XML.Encode(this.provider));

				if (this.contentSchemaHash != null && this.contentSchemaHash.Length > 0)
				{
					Xml.Append("\" schemaHash=\"");
					Xml.Append(Convert.ToBase64String(this.contentSchemaHash));

					Xml.Append("\" schemaHashFunction=\"");
					Xml.Append(this.contentSchemaHashFunction.ToString());
				}

				Xml.Append("\" state=\"");
				Xml.Append(this.state.ToString());

				if (!string.IsNullOrEmpty(this.templateId))
				{
					Xml.Append("\" templateId=\"");
					Xml.Append(XML.Encode(this.templateId));
				}

				if (this.to != DateTime.MaxValue)
				{
					Xml.Append("\" to=\"");
					Xml.Append(XML.Encode(this.to));
				}

				if (this.updated != DateTime.MinValue)
				{
					Xml.Append("\" updated=\"");
					Xml.Append(XML.Encode(this.updated));
				}

				Xml.Append("\"/>");
			}

			if (IncludeServerSignature)
				this.serverSignature?.Serialize(Xml);

			Xml.Append("</contract>");
		}

		/// <summary>
		/// Gets event tags describing the contract.
		/// </summary>
		/// <returns>Tags</returns>
		public KeyValuePair<string, object>[] GetTags()
		{
			List<KeyValuePair<string, object>> Response = new List<KeyValuePair<string, object>>()
			{
				new KeyValuePair<string, object>("ID", this.contractId),
				new KeyValuePair<string, object>("Provider", this.provider),
				new KeyValuePair<string, object>("State", this.state),
				new KeyValuePair<string, object>("Visibility", this.visibility),
				new KeyValuePair<string, object>("Contract Local Name", this.forMachinesLocalName),
				new KeyValuePair<string, object>("Contract Namespace", this.forMachinesNamespace),
				new KeyValuePair<string, object>("Created", this.created)
			};

			if (this.updated != DateTime.MinValue)
				Response.Add(new KeyValuePair<string, object>("Updated", this.updated));

			if (this.from != DateTime.MinValue)
				Response.Add(new KeyValuePair<string, object>("From", this.from));

			if (this.to != DateTime.MaxValue)
				Response.Add(new KeyValuePair<string, object>("To", this.to));

			if (!string.IsNullOrEmpty(this.templateId))
				Response.Add(new KeyValuePair<string, object>("Template ID", this.templateId));

			if (this.parameters != null)
			{
				foreach (Parameter P in this.parameters)
					Response.Add(new KeyValuePair<string, object>(P.Name, P.ObjectValue));
			}

			return Response.ToArray();
		}

		/// <summary>
		/// Access to contract paameters.
		/// </summary>
		/// <param name="Key"></param>
		/// <returns></returns>
		public object this[string Key]
		{
			get
			{
				if (this.parameters != null)
				{
					foreach (Parameter P in this.parameters)
					{
						if (P.Name == Key)
							return P.ObjectValue;
					}
				}

				return string.Empty;
			}
		}

	}
}
