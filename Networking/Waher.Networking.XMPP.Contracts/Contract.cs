﻿using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Schema;
using Waher.Content;
using Waher.Content.Xml;
using Waher.Content.Xsl;
using Waher.Events;
using Waher.Networking.XMPP.Contracts.EventArguments;
using Waher.Networking.XMPP.Contracts.HumanReadable;
using Waher.Persistence;
using Waher.Script;

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
		private string @namespace = ContractsClient.NamespaceSmartContractsCurrent;
		private byte[] contentSchemaDigest = null;
		private byte[] nonce = null;
		private XmlElement forMachines = null;
		private Role[] roles = null;
		private Part[] parts = null;
		private Parameter[] parameters = null;
		private HumanReadableText[] forHumans = null;
		private ClientSignature[] clientSignatures = null;
		private Attachment[] attachments = null;
		private ServerSignature serverSignature = null;
		private Security.HashFunction contentSchemaHashFunction = Security.HashFunction.SHA256;
		private ContractState state = ContractState.Proposed;
		private ContractVisibility visibility = ContractVisibility.CreatorAndParts;
		private ContractParts partsMode = ContractParts.ExplicitlyDefined;
		private DateTime created = DateTime.MinValue;
		private DateTime updated = DateTime.MinValue;
		private DateTime from = DateTime.MinValue;
		private DateTime to = DateTime.MaxValue;
		private DateTime? signAfter = null;
		private DateTime? signBefore = null;
		private Duration? duration = null;
		private Duration? archiveReq = null;
		private Duration? archiveOpt = null;
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
			get => this.contractId;
			set => this.contractId = value;
		}

		/// <summary>
		/// Contract identity, as an URI.
		/// </summary>
		public Uri ContractIdUri => ContractsClient.ContractIdUri(this.contractId);

		/// <summary>
		/// Contract identity, as an URI string.
		/// </summary>
		public string ContractIdUriString => ContractsClient.ContractIdUriString(this.contractId);

		/// <summary>
		/// JID of the Trust Provider hosting the contract
		/// </summary>
		public string Provider
		{
			get => this.provider;
			set => this.provider = value;
		}

		/// <summary>
		/// Contract identity of template, if one was used to create the contract.
		/// </summary>
		public string TemplateId
		{
			get => this.templateId;
			set => this.templateId = value;
		}

		/// <summary>
		/// Namespace used when serializing the identity for signatures.
		/// </summary>
		public string Namespace
		{
			get => this.@namespace;
			set => this.@namespace = value;
		}

		/// <summary>
		/// Contract identity of template, if one was used to create the contract, as an URI.
		/// </summary>
		public Uri TemplateIdUri => string.IsNullOrEmpty(this.templateId) ? null : ContractsClient.ContractIdUri(this.templateId);

		/// <summary>
		/// Contract identity of template, if one was used to create the contract, as an URI string.
		/// </summary>
		public string TemplateIdUriString => string.IsNullOrEmpty(this.templateId) ? null : ContractsClient.ContractIdUriString(this.templateId);

		/// <summary>
		/// Contract state
		/// </summary>
		public ContractState State
		{
			get => this.state;
			set => this.state = value;
		}

		/// <summary>
		/// When the contract was created
		/// </summary>
		public DateTime Created
		{
			get => this.created;
			set => this.created = value;
		}

		/// <summary>
		/// When the contract was last updated
		/// </summary>
		public DateTime Updated
		{
			get => this.updated;
			set => this.updated = value;
		}

		/// <summary>
		/// From when the contract is valid (if signed)
		/// </summary>
		public DateTime From
		{
			get => this.from;
			set => this.from = value;
		}

		/// <summary>
		/// Until when the contract is valid (if signed)
		/// </summary>
		public DateTime To
		{
			get => this.to;
			set => this.to = value;
		}

		/// <summary>
		/// Signatures will only be accepted after this point in time.
		/// </summary>
		public DateTime? SignAfter
		{
			get => this.signAfter;
			set => this.signAfter = value;
		}

		/// <summary>
		/// Signatures will only be accepted until this point in time.
		/// </summary>
		public DateTime? SignBefore
		{
			get => this.signBefore;
			set => this.signBefore = value;
		}

		/// <summary>
		/// Contrat Visibility
		/// </summary>
		public ContractVisibility Visibility
		{
			get => this.visibility;
			set => this.visibility = value;
		}

		/// <summary>
		/// Duration of the contract. Is counted from the time it is signed by the required parties.
		/// </summary>
		public Duration? Duration
		{
			get => this.duration;
			set => this.duration = value;
		}

		/// <summary>
		/// Requied time to archive a signed smart contract, after it becomes obsolete.
		/// </summary>
		public Duration? ArchiveRequired
		{
			get => this.archiveReq;
			set => this.archiveReq = value;
		}

		/// <summary>
		/// Optional time to archive a signed smart contract, after it becomes obsolete, and after its required archivation period.
		/// </summary>
		public Duration? ArchiveOptional
		{
			get => this.archiveOpt;
			set => this.archiveOpt = value;
		}

		/// <summary>
		/// The hash digest of the schema used to validate the machine-readable contents (<see cref="ForMachines"/>) of the smart contract,
		/// if such a schema was used.
		/// </summary>
		public byte[] ContentSchemaDigest
		{
			get => this.contentSchemaDigest;
			set => this.contentSchemaDigest = value;
		}

		/// <summary>
		/// Hash function of the schema used to validate the machine-readable contents (<see cref="ForMachines"/>) of the smart contract,
		/// if such a schema was used.
		/// </summary>
		public Security.HashFunction ContentSchemaHashFunction
		{
			get => this.contentSchemaHashFunction;
			set => this.contentSchemaHashFunction = value;
		}

		/// <summary>
		/// Roles defined in the smart contract.
		/// </summary>
		public Role[] Roles
		{
			get => this.roles;
			set => this.roles = value;
		}

		/// <summary>
		/// How parts are defined in the smart contract.
		/// </summary>
		public ContractParts PartsMode
		{
			get => this.partsMode;
			set => this.partsMode = value;
		}

		/// <summary>
		/// Defined parts for the smart contract.
		/// </summary>
		public Part[] Parts
		{
			get => this.parts;
			set => this.parts = value;
		}

		/// <summary>
		/// Defined parameters for the smart contract.
		/// </summary>
		public Parameter[] Parameters
		{
			get => this.parameters;
			set => this.parameters = value;
		}

		/// <summary>
		/// Machine-readable contents of the contract.
		/// </summary>
		public XmlElement ForMachines
		{
			get => this.forMachines;
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
		public string ForMachinesNamespace => this.forMachinesNamespace;

		/// <summary>
		/// Local name used by the root node of the machine-readable contents of the contract (<see cref="ForMachines"/>).
		/// </summary>
		public string ForMachinesLocalName => this.forMachinesLocalName;

		/// <summary>
		/// Human-readable contents of the contract.
		/// </summary>
		public HumanReadableText[] ForHumans
		{
			get => this.forHumans;
			set => this.forHumans = value;
		}

		/// <summary>
		/// Client signatures of the contract.
		/// </summary>
		public ClientSignature[] ClientSignatures
		{
			get => this.clientSignatures;
			set => this.clientSignatures = value;
		}

		/// <summary>
		/// Attachments assigned to the legal identity.
		/// </summary>
		public Attachment[] Attachments
		{
			get => this.attachments;
			set => this.attachments = value;
		}

		/// <summary>
		/// Server signature attesting to the validity of the contents of the contract.
		/// </summary>
		public ServerSignature ServerSignature
		{
			get => this.serverSignature;
			set => this.serverSignature = value;
		}

		/// <summary>
		/// If the contract can act as a template for other contracts.
		/// </summary>
		public bool CanActAsTemplate
		{
			get => this.canActAsTemplate;
			set => this.canActAsTemplate = value;
		}

		/// <summary>
		/// An optional nonce value that is used when encrypting protected parameter values.
		/// </summary>
		public byte[] Nonce
		{
			get => this.nonce;
			set => this.nonce = value;
		}

		/// <summary>
		/// Timestamp of first client signature, if one exists.
		/// </summary>
		public DateTime? FirstSignatureAt
		{
			get
			{
				DateTime? Result = null;

				if (!(this.clientSignatures is null))
				{
					foreach (ClientSignature Signature in this.clientSignatures)
					{
						if (!Result.HasValue || Result.Value > Signature.Timestamp)
							Result = Signature.Timestamp;
					}
				}

				return Result;
			}
		}

		/// <summary>
		/// Timestamp of first client signature, if one exists.
		/// </summary>
		public DateTime? LastSignatureAt
		{
			get
			{
				DateTime? Result = null;

				if (!(this.clientSignatures is null))
				{
					foreach (ClientSignature Signature in this.clientSignatures)
					{
						if (!Result.HasValue || Result.Value < Signature.Timestamp)
							Result = Signature.Timestamp;
					}
				}

				return Result;
			}
		}

		/// <summary>
		/// If contract has parameters that require encryption and decryption.
		/// </summary>
		public bool HasEncryptedParameters
		{
			get
			{
				if (this.parameters is null)
					return false;

				foreach (Parameter P in this.parameters)
				{
					if (P.Protection == ProtectionLevel.Encrypted)
						return true;
				}

				return false;
			}
		}

		/// <summary>
		/// If contract has parameters that are transient.
		/// </summary>
		public bool HasTransientParameters
		{
			get
			{
				if (this.parameters is null)
					return false;

				foreach (Parameter P in this.parameters)
				{
					if (P.Protection == ProtectionLevel.Transient)
						return true;
				}

				return false;
			}
		}

		/// <summary>
		/// Validates a contract XML Document, and returns the contract definition in it.
		/// </summary>
		/// <param name="Xml">XML representation</param>
		/// <returns>Parsed contract.</returns>
		/// <exception cref="Exception">If XML is invalid or contains errors.</exception>
		[Obsolete("Use the Parse(XmlDocument Xml, ContractsClient Client) overload instead.")]
		public static Task<ParsedContract> Parse(XmlDocument Xml)
		{
			return Parse(Xml, null);
		}

		/// <summary>
		/// Validates a contract XML Document, and returns the contract definition in it.
		/// </summary>
		/// <param name="Xml">XML representation</param>
		/// <param name="Client">Connected contracts client. If offline or null, partial validation in certain cases will be performed.</param>
		/// <returns>Parsed contract.</returns>
		/// <exception cref="Exception">If XML is invalid or contains errors.</exception>
		public static Task<ParsedContract> Parse(XmlDocument Xml, ContractsClient Client)
		{
			XSL.Validate(string.Empty, Xml, "contract", ContractsClient.NamespaceSmartContractsCurrent,
				contractSchema, identitiesSchema, e2eSchema, p2pSchema, xmlSchema);

			return Parse(Xml.DocumentElement, Client, true);
		}

		private static readonly XmlSchema identitiesSchema = XSL.LoadSchema(typeof(Contract).Namespace + ".Schema.LegalIdentities.xsd");
		private static readonly XmlSchema contractSchema = XSL.LoadSchema(typeof(Contract).Namespace + ".Schema.SmartContracts.xsd");
		private static readonly XmlSchema e2eSchema = XSL.LoadSchema(typeof(Contract).Namespace + ".Schema.E2E.xsd");
		private static readonly XmlSchema p2pSchema = XSL.LoadSchema(typeof(Contract).Namespace + ".Schema.P2P.xsd");
		private static readonly XmlSchema xmlSchema = XSL.LoadSchema(typeof(Contract).Namespace + ".Schema.Xml.xsd");

		/// <summary>
		/// Parses a contract from is XML representation.
		/// </summary>
		/// <param name="Xml">XML representation</param>
		/// <returns>Parsed contract, or null if it contains errors.</returns>
		[Obsolete("Use the Parse(XmlElement Xml, ContractsClient Client, bool) overload instead.")]
		public static Task<ParsedContract> Parse(XmlElement Xml)
		{
			return Parse(Xml, null, false);
		}

		/// <summary>
		/// Parses a contract from is XML representation.
		/// </summary>
		/// <param name="Xml">XML representation</param>
		/// <param name="Client">Connected contracts client. If offline or null, partial validation in certain cases will be performed.</param>
		/// <returns>Parsed contract, or null if it contains errors.</returns>
		[Obsolete("Use the Parse(XmlElement, Client, bool) overload instead")]
		public static Task<ParsedContract> Parse(XmlElement Xml, ContractsClient Client)
		{
			return Parse(Xml, Client, false);
		}

		/// <summary>
		/// Parses a contract from is XML representation.
		/// </summary>
		/// <param name="Xml">XML representation</param>
		/// <param name="Client">Connected contracts client. If offline or null, partial validation in certain cases will be performed.</param>
		/// <param name="ExceptionIfError">If an exception should be thrown if unale to parse contract (true), or if null should be returned (false)</param>
		/// <returns>Parsed contract (or null if it contains errors and <paramref name="ExceptionIfError"/> is false).</returns>
		public static async Task<ParsedContract> Parse(XmlElement Xml, ContractsClient Client, bool ExceptionIfError)
		{
			bool HasVisibility = false;
			Contract Result = new Contract()
			{
				Namespace = Xml.NamespaceURI
			};
			ParsedContract ParsedContract = new ParsedContract()
			{
				Contract = Result,
				HasStatus = false,
				ParametersValid = true
			};

			foreach (XmlAttribute Attr in Xml.Attributes)
			{
				switch (Attr.Name)
				{
					case "id":
						Result.contractId = Attr.Value;
						break;

					case "nonce":
						if (ExceptionIfError)
							Result.nonce = Convert.FromBase64String(Attr.Value);
						else
						{
							try
							{
								Result.nonce = Convert.FromBase64String(Attr.Value);
							}
							catch (Exception)
							{
								return null;
							}
						}
						break;

					case "visibility":
						if (Enum.TryParse(Attr.Value, out ContractVisibility Visibility))
						{
							Result.visibility = Visibility;
							HasVisibility = true;
						}
						else if (ExceptionIfError)
							throw new Exception("Invalid visibility: " + Attr.Value);
						else
							return null;
						break;

					case "duration":
						if (Waher.Content.Duration.TryParse(Attr.Value, out Duration D))
							Result.duration = D;
						else if (ExceptionIfError)
							throw new Exception("Invalid duration: " + Attr.Value);
						else
							return null;
						break;

					case "archiveReq":
						if (Waher.Content.Duration.TryParse(Attr.Value, out D))
							Result.archiveReq = D;
						else if (ExceptionIfError)
							throw new Exception("Invalid required archiving time: " + Attr.Value);
						else
							return null;
						break;

					case "archiveOpt":
						if (Waher.Content.Duration.TryParse(Attr.Value, out D))
							Result.archiveOpt = D;
						else if (ExceptionIfError)
							throw new Exception("Invalid optional archiving time: " + Attr.Value);
						else
							return null;
						break;

					case "signAfter":
						if (XML.TryParse(Attr.Value, out DateTime TP))
							Result.signAfter = TP;
						else if (ExceptionIfError)
							throw new Exception("Invalid sign after time: " + Attr.Value);
						else
							return null;
						break;

					case "signBefore":
						if (XML.TryParse(Attr.Value, out TP))
							Result.signBefore = TP;
						else if (ExceptionIfError)
							throw new Exception("Invalid sign before time: " + Attr.Value);
						else
							return null;
						break;

					case "canActAsTemplate":
						if (CommonTypes.TryParse(Attr.Value, out bool b))
							Result.canActAsTemplate = b;
						else if (ExceptionIfError)
							throw new Exception("Invalid Boolean value: " + Attr.Value);
						else
							return null;
						break;

					case "xmlns":
						break;

					default:
						if (Attr.Prefix == "xmlns" || Attr.NamespaceURI == "http://www.w3.org/2001/XMLSchema-instance")
							break;
						else if (ExceptionIfError)
							throw new Exception("Invalid attribute: " + Attr.Name);
						else
							return null;
				}
			}

			if (!HasVisibility)
			{
				if (ExceptionIfError)
					throw new Exception("Visibility required.");
				else
					return null;
			}

			if (Result.duration is null)
			{
				if (ExceptionIfError)
					throw new Exception("Duration required.");
				else
					return null;
			}

			if (Result.archiveReq is null)
			{
				if (ExceptionIfError)
					throw new Exception("Required archiving time required.");
				else
					return null;
			}

			if (Result.archiveOpt is null)
			{
				if (ExceptionIfError)
					throw new Exception("Optional archiving time required.");
				else
					return null;
			}

			if (Result.signBefore <= Result.signAfter)
			{
				if (ExceptionIfError)
					throw new Exception("Before signature time must not occur after the after signature time.");
				else
					return null;
			}

			List<HumanReadableText> ForHumans = new List<HumanReadableText>();
			List<Role> Roles = new List<Role>();
			List<Parameter> Parameters = new List<Parameter>();
			List<ClientSignature> Signatures = new List<ClientSignature>();
			List<Attachment> Attachments = new List<Attachment>();
			XmlElement Content = null;
			HumanReadableText Text;
			HumanReadableElement ErrorElement;
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
							MaxCount = -1,
							CanRevoke = false
						};

						foreach (XmlAttribute Attr in E.Attributes)
						{
							switch (Attr.Name)
							{
								case "name":
									Role.Name = Attr.Value;
									if (string.IsNullOrEmpty(Role.Name))
									{
										if (ExceptionIfError)
											throw new Exception("Role name cannot be empty.");
										else
											return null;
									}
									break;

								case "minCount":
									if (int.TryParse(Attr.Value, out int i) && i >= 0)
										Role.MinCount = i;
									else if (ExceptionIfError)
										throw new Exception("Invalid minimum count.");
									else
										return null;
									break;

								case "maxCount":
									if (int.TryParse(Attr.Value, out i) && i >= 0)
										Role.MaxCount = i;
									else if (ExceptionIfError)
										throw new Exception("Invalid maximum count.");
									else
										return null;
									break;

								case "canRevoke":
									if (CommonTypes.TryParse(Attr.Value, out bool b))
										Role.CanRevoke = b;
									else if (ExceptionIfError)
										throw new Exception("Invalid can Revoke value.");
									else
										return null;
									break;

								case "xmlns":
									break;

								default:
									if (Attr.Prefix == "xmlns")
										break;
									else if (ExceptionIfError)
										throw new Exception("Invalid role attribute: " + Attr.Name);
									else
										return null;
							}
						}

						if (string.IsNullOrEmpty(Role.Name) ||
							Role.MinCount < 0 ||
							Role.MaxCount < 0)
						{
							if (ExceptionIfError)
								throw new Exception("Role name missing, or invalid role counts.");
							else
								return null;
						}

						foreach (XmlNode N2 in E.ChildNodes)
						{
							if (N2 is XmlElement E2)
							{
								if (E2.LocalName == "description")
								{
									Text = HumanReadableText.Parse(E2);

									if (Text is null)
									{
										if (ExceptionIfError)
											throw new Exception("Unable to parse human-readable text for role.");
										else
											return null;
									}

									ErrorElement = await Text.IsWellDefined();
									if (!(ErrorElement is null))
									{
										if (ExceptionIfError)
										{
											StringBuilder sb = new StringBuilder();
											ErrorElement.Serialize(sb);
											throw new Exception("Human-readable text not well-defined for role: " + sb.ToString());
										}
										else
											return null;
									}

									Descriptions.Add(Text);
								}
								else if (ExceptionIfError)
									throw new Exception("Unrecognized element: " + E2.Name);
								else
									return null;
							}
						}

						if (Descriptions.Count == 0)
						{
							if (ExceptionIfError)
								throw new Exception("Missing description.");
							else
								return null;
						}

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
										{
											if (ExceptionIfError)
												throw new Exception("Duplicate mode.");
											else
												return null;
										}

										Mode = ContractParts.Open;
										break;

									case "templateOnly":
										if (Mode.HasValue)
										{
											if (ExceptionIfError)
												throw new Exception("Duplicate mode.");
											else
												return null;
										}

										Mode = ContractParts.TemplateOnly;
										break;

									case "part":
										if (Mode.HasValue)
										{
											if (Mode.Value != ContractParts.ExplicitlyDefined)
											{
												if (ExceptionIfError)
													throw new Exception("Duplicate mode.");
												else
													return null;
											}
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
													else if (ExceptionIfError)
														throw new Exception("Unrecognized attribute: " + Attr.Name);
													else
														return null;
											}
										}

										if (string.IsNullOrEmpty(LegalId) || string.IsNullOrEmpty(RoleRef))
										{
											if (ExceptionIfError)
												throw new Exception("Missing legal ID or role reference for part.");
											else
												return null;
										}

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
										{
											if (ExceptionIfError)
												throw new Exception("Role not found: " + RoleRef);
											else
												return null;
										}

										Parts ??= new List<Part>();
										Parts.Add(new Part()
										{
											LegalId = LegalId,
											Role = RoleRef
										});

										break;

									default:
										if (ExceptionIfError)
											throw new Exception("Unrecognized element: " + E2.Name);
										else
											return null;
								}
							}
						}

						if (!Mode.HasValue)
						{
							if (ExceptionIfError)
								throw new Exception("Missing mode.");
							else
								return null;
						}

						Result.partsMode = Mode.Value;
						Result.parts = Parts?.ToArray();

						PartsDefined = true;
						break;

					case "parameters":
						foreach (XmlNode N2 in E.ChildNodes)
						{
							if (N2 is XmlElement E2)
							{
								Parameter P2;

								switch (E2.LocalName)
								{
									case "stringParameter":
										P2 = new StringParameter();
										break;

									case "numericalParameter":
										P2 = new NumericalParameter();
										break;

									case "booleanParameter":
										P2 = new BooleanParameter();
										break;

									case "dateParameter":
										P2 = new DateParameter();
										break;

									case "dateTimeParameter":
										P2 = new DateTimeParameter();
										break;

									case "timeParameter":
										P2 = new TimeParameter();
										break;

									case "durationParameter":
										P2 = new DurationParameter();
										break;

									case "geoParameter":
										P2 = new GeoParameter();
										break;

									case "calcParameter":
										P2 = new CalcParameter();
										break;

									case "roleParameter":
										P2 = new RoleParameter();
										break;

									case "contractReferenceParameter":
										P2 = new ContractReferenceParameter();
										break;

									case "attachmentParameter":
										P2 = new AttachmentParameter();
										break;

									default:
										if (ExceptionIfError)
											throw new Exception("Unrecognized parameter type: " + E2.Name);
										else
											return null;
								}

								if (!await P2.Import(E2))
								{
									if (ExceptionIfError)
										throw new Exception("Unable to parse parameter.");
									else
										return null;
								}

								Parameters.Add(P2);
							}
						}

						if (Parameters.Count == 0)
						{
							if (ExceptionIfError)
								throw new Exception("No parameters in defined.");
							else
								return null;
						}
						break;

					case "humanReadableText":
						Text = HumanReadableText.Parse(E);
						if (Text is null)
						{
							if (ExceptionIfError)
								throw new Exception("Unable to parse human-readable text for contract.");
							else
								return null;
						}

						ErrorElement = await Text.IsWellDefined();
						if (!(ErrorElement is null))
						{
							if (ExceptionIfError)
							{
								StringBuilder sb = new StringBuilder();
								ErrorElement.Serialize(sb);
								throw new Exception("Human-readable text not well-defined for contract: " + sb.ToString());
							}
							else
								return null;
						}

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
									else if (ExceptionIfError)
										throw new Exception("Invalid signature timestamp.");
									else
										return null;
									break;

								case "transferable":
									if (CommonTypes.TryParse(Attr.Value, out bool b))
										Signature.Transferable = b;
									else if (ExceptionIfError)
										throw new Exception("Invalid signature transferable status.");
									else
										return null;
									break;

								case "xmlns":
									break;

								default:
									if (Attr.Prefix == "xmlns")
										break;
									else if (ExceptionIfError)
										throw new Exception("Unrecognized signature attribute: " + Attr.Name);
									else
										return null;
							}
						}

						Signature.DigitalSignature = Convert.FromBase64String(E.InnerText);

						if (string.IsNullOrEmpty(Signature.LegalId) ||
							string.IsNullOrEmpty(Signature.BareJid) ||
							string.IsNullOrEmpty(Signature.Role) ||
							Signature.Timestamp == DateTime.MinValue)
						{
							if (ExceptionIfError)
								throw new Exception("Incomplete Signature.");
							else
								return null;
						}

						Signatures.Add(Signature);
						break;

					case "attachment":
						Attachments.Add(new Attachment()
						{
							Id = XML.Attribute(E, "id"),
							LegalId = XML.Attribute(E, "legalId"),
							ContentType = XML.Attribute(E, "contentType"),
							FileName = XML.Attribute(E, "fileName"),
							Signature = Convert.FromBase64String(XML.Attribute(E, "s")),
							Timestamp = XML.Attribute(E, "timestamp", DateTime.MinValue)
						});
						break;

					case "status":
						ParsedContract.HasStatus = true;
						foreach (XmlAttribute Attr in E.Attributes)
						{
							switch (Attr.Name)
							{
								case "provider":
									Result.provider = Attr.Value;
									break;

								case "state":
									if (Enum.TryParse(Attr.Value, out ContractState ContractState))
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

								case "schemaDigest":
									Result.contentSchemaDigest = Convert.FromBase64String(Attr.Value);
									break;

								case "schemaHashFunction":
									if (Enum.TryParse(Attr.Value, out Security.HashFunction H))
										Result.contentSchemaHashFunction = H;
									else if (ExceptionIfError)
										throw new Exception("Unrecognized schema hash function.");
									else
										return null;
									break;

								case "xmlns":
									break;

								default:
									if (Attr.Prefix == "xmlns")
										break;
									else if (ExceptionIfError)
										throw new Exception("Unrecognized status attribute: " + Attr.Name);
									else
										return null;
							}
						}

						foreach (XmlNode N2 in E.ChildNodes)
						{
							if (N2 is XmlElement E2 &&
								E2.LocalName == "roleParameters" &&
								E2.NamespaceURI == E.NamespaceURI)
							{
								Dictionary<string, RoleParameter> RoleParameters = new Dictionary<string, RoleParameter>();

								foreach (Parameter P in Parameters)
								{
									if (P is RoleParameter RoleParameter)
										RoleParameters[RoleParameter.Name] = RoleParameter;
								}

								foreach (XmlNode N3 in E2.ChildNodes)
								{
									if (N3 is XmlElement E3 &&
										E3.LocalName == "parameter" &&
										E3.NamespaceURI == E2.NamespaceURI)
									{
										string Name = XML.Attribute(E3, "name");
										if (!RoleParameters.TryGetValue(Name, out RoleParameter RoleParameter))
										{
											if (ExceptionIfError)
												throw new Exception("Invalid role parameter reference: " + Name);
											else
												return null;
										}

										RoleParameter.SetValue(XML.Attribute(E3, "value"));
									}
								}
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
									else if (ExceptionIfError)
										throw new Exception("Invalid server signature timestamp.");
									else
										return null;
									break;

								case "xmlns":
									break;

								default:
									if (Attr.Prefix == "xmlns")
										break;
									else if (ExceptionIfError)
										throw new Exception("Unrecognized server signature attribute: " + Attr.Name);
									else
										return null;
							}
						}

						ServerSignature.DigitalSignature = Convert.FromBase64String(E.InnerText);

						if (ServerSignature.Timestamp == DateTime.MinValue)
						{
							if (ExceptionIfError)
								throw new Exception("Invalid server signature timestamp.");
							else
								return null;
						}

						Result.serverSignature = ServerSignature;
						break;

					case "attachmentRef":
						string AttachmentId = XML.Attribute(E, "attachmentId");
						string Url = XML.Attribute(E, "url");

						foreach (Attachment Attachment in Attachments)
						{
							if (Attachment.Id == AttachmentId)
							{
								Attachment.Url = Url;
								break;
							}
						}
						break;

					default:
						if (ExceptionIfError)
							throw new Exception("Unrecognized element: " + E.Name);
						else
							return null;
				}
			}

			if (Content is null)
			{
				if (ExceptionIfError)
					throw new Exception("Missing machine-readable content.");
				else
					return null;
			}

			if (ForHumans.Count == 0)
			{
				if (ExceptionIfError)
					throw new Exception("Missing human-readable content.");
				else
					return null;
			}

			if (!PartsDefined)
			{
				if (ExceptionIfError)
					throw new Exception("Missing part definitions.");
				else
					return null;
			}

			Variables Variables = new Variables
			{
				["Duration"] = Result.duration
			};

			DateTime? FirstSignature = Result.FirstSignatureAt;
			if (FirstSignature.HasValue)
			{
				Variables["Now"] = FirstSignature.Value.ToLocalTime();
				Variables["NowUtc"] = FirstSignature.Value.ToUniversalTime();
			}

			foreach (Parameter Parameter in Parameters)
				Parameter.Populate(Variables);

			foreach (Parameter Parameter in Parameters)
			{
				if (!await Parameter.IsParameterValid(Variables, Client))
				{
					ParsedContract.ParametersValid = false;
					break;
				}
			}

			Result.roles = Roles.ToArray();
			Result.parameters = Parameters.ToArray();
			Result.forMachines = Content;
			Result.forMachinesLocalName = Content.LocalName;
			Result.forMachinesNamespace = Content.NamespaceURI;
			Result.forHumans = ForHumans.ToArray();
			Result.clientSignatures = Signatures.ToArray();
			Result.attachments = Attachments.ToArray();

			return ParsedContract;
		}

		/// <summary>
		/// Normalizes XML.
		/// </summary>
		/// <param name="Xml">XML string</param>
		/// <returns>Normalized XML</returns>
		public static string NormalizeXml(string Xml)
		{
			XmlDocument Doc = new XmlDocument()
			{
				PreserveWhitespace = true
			};
			Doc.LoadXml(Xml);

			StringBuilder sb = new StringBuilder();

			NormalizeXml(Doc.DocumentElement, sb, string.Empty);

			return sb.ToString();
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
			string TagName = Xml.LocalName.Normalize(NormalizationForm.FormC);

			if (!string.IsNullOrEmpty(Xml.Prefix))
				TagName = Xml.Prefix.Normalize(NormalizationForm.FormC) + ":" + TagName;

			Output.Append(TagName);

			foreach (XmlAttribute Attr in Xml.Attributes)
			{
				Attributes ??= new SortedDictionary<string, string>();
				Attributes[Attr.Name.Normalize(NormalizationForm.FormC)] = Attr.Value.Normalize(NormalizationForm.FormC);
			}

			if (Xml.NamespaceURI != CurrentNamespace && string.IsNullOrEmpty(Xml.Prefix))
			{
				Attributes ??= new SortedDictionary<string, string>();
				Attributes["xmlns"] = Xml.NamespaceURI;
				CurrentNamespace = Xml.NamespaceURI;
			}
			else
				Attributes?.Remove("xmlns");

			if (!(Attributes is null))
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

			bool HasContent = false;
			XmlWhitespace SpaceContent = null;

			foreach (XmlNode N in Xml.ChildNodes)
			{
				if (N is XmlElement E)
				{
					if (!HasContent)
					{
						HasContent = true;
						Output.Append('>');
					}

					NormalizeXml(E, Output, CurrentNamespace);
				}
				else if (N is XmlText || N is XmlCDataSection || N is XmlSignificantWhitespace)
				{
					if (!HasContent)
					{
						HasContent = true;
						Output.Append('>');
					}

					Output.Append(XML.Encode(N.InnerText.Normalize(NormalizationForm.FormC)));
				}
				else if (N is XmlWhitespace Space)
					SpaceContent = Space;
			}

			if (HasContent)
			{
				Output.Append("</");
				Output.Append(TagName);
				Output.Append('>');
			}
			else if (!(SpaceContent is null))
			{
				Output.Append('>');

				foreach (char _ in SpaceContent.InnerText)
					Output.Append(' ');

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
		/// <param name="Client">Contracts Client performing validation of signatures.</param>
		/// <returns>If the contract is legally binding.</returns>
		public async Task<bool> IsLegallyBinding(bool CheckCurrentTime, ContractsClient Client)
		{
			if (this.clientSignatures is null || this.serverSignature is null)
				return false;

			if (this.state != ContractState.Signed)
				return false;

			if (!(this.roles is null))
			{
				foreach (Role Role in this.roles)
				{
					int Count = 0;

					if (!(this.clientSignatures is null))
					{
						foreach (ClientSignature Signature in this.clientSignatures)
						{
							if (string.Compare(Signature.Role, Role.Name, true) == 0)
								Count++;
						}
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

			switch (this.partsMode)
			{
				case ContractParts.TemplateOnly:
					return false;

				case ContractParts.ExplicitlyDefined:
					if (this.parts is null)
						return false;

					LinkedList<Part> MissingParts = null;
					Dictionary<CaseInsensitiveString, ClientSignature> UnmatchedSignatures = new Dictionary<CaseInsensitiveString, ClientSignature>();

					foreach (ClientSignature Signature in this.clientSignatures)
						UnmatchedSignatures[Signature.Role + "|" + Signature.LegalId] = Signature;

					foreach (Part Part in this.parts)
					{
						bool Found = false;

						foreach (ClientSignature Signature in UnmatchedSignatures.Values)
						{
							if (string.Compare(Signature.LegalId, Part.LegalId, true) == 0 &&
								string.Compare(Signature.Role, Part.Role, true) == 0)
							{
								UnmatchedSignatures.Remove(Signature.Role + "|" + Signature.LegalId);
								Found = true;
								break;
							}
						}

						if (!Found)
						{
							MissingParts ??= new LinkedList<Part>();
							MissingParts.AddLast(Part);
						}
					}

					if (!(MissingParts is null))
					{
						foreach (Part Part in MissingParts)
						{
							bool Found = false;

							foreach (ClientSignature Signature in UnmatchedSignatures.Values)
							{
								if (Signature.Role != Part.Role)
									continue;

								if (Client is null)
									return false;

								if (await Client.CanSignAs(Part.LegalId, Signature.LegalId))
								{
									Found = true;
									UnmatchedSignatures.Remove(Signature.Role + "|" + Signature.LegalId);
									break;
								}
							}

							if (!Found)
								return false;
						}

						if (UnmatchedSignatures.Count > 0)
							return false;
					}
					break;
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
		/// <param name="IncludeAttachments">If attachments are to be included.</param>
		/// <param name="IncludeStatus">If the status element should be included.</param>
		/// <param name="IncludeServerSignature">If the server signature should be included.</param>
		/// <param name="IncludeAttachmentReferences">If attachment references (URLs) are to be included.</param>
		public void Serialize(StringBuilder Xml, bool IncludeNamespace, bool IncludeIdAttribute, bool IncludeClientSignatures,
			bool IncludeAttachments, bool IncludeStatus, bool IncludeServerSignature, bool IncludeAttachmentReferences)
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

			if (!(this.nonce is null) && this.nonce.Length > 0)
			{
				Xml.Append(" nonce=\"");
				Xml.Append(Convert.ToBase64String(this.nonce));
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
				Xml.Append(this.@namespace);
				Xml.Append('"');
			}

			Xml.Append('>');

			if (this.forMachines is null)
				throw new InvalidOperationException("No Machine-readable XML provided.");

			NormalizeXml(this.forMachines, Xml, this.@namespace);

			if (!(this.roles is null))
			{
				foreach (Role Role in this.roles)
				{
					Xml.Append("<role");

					if (Role.CanRevoke)
						Xml.Append(" canRevoke=\"true\"");

					Xml.Append(" maxCount=\"");
					Xml.Append(Role.MaxCount.ToString());
					Xml.Append("\" minCount=\"");
					Xml.Append(Role.MinCount.ToString());
					Xml.Append("\" name=\"");
					Xml.Append(XML.Encode(Role.Name));
					Xml.Append("\">");

					foreach (HumanReadableText Description in Role.Descriptions)
						Description.Serialize(Xml, "description", null);

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
					if (!(this.parts is null))
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

			LinkedList<RoleParameter> RoleParameters = null;

			if (!(this.parameters is null) && this.parameters.Length > 0)
			{
				Xml.Append("<parameters>");

				foreach (Parameter Parameter in this.parameters)
				{
					Parameter.Serialize(Xml, false);

					if (Parameter is RoleParameter RoleParameter &&
						RoleParameter.ObjectValue is string RoleParameterValue &&
						!string.IsNullOrEmpty(RoleParameterValue))
					{
						RoleParameters ??= new LinkedList<RoleParameter>();
						RoleParameters.AddLast(RoleParameter);
					}
				}

				Xml.Append("</parameters>");
			}

			if (!(this.forHumans is null) && this.forHumans.Length > 0)
			{
				foreach (HumanReadableText Text in this.forHumans)
					Text.Serialize(Xml);
			}

			if (IncludeClientSignatures && !(this.clientSignatures is null))
			{
				foreach (Signature Signature in this.clientSignatures)
					Signature.Serialize(Xml);
			}

			if (IncludeAttachments && !(this.attachments is null))
			{
				foreach (Attachment A in this.attachments)
				{
					Xml.Append("<attachment contentType=\"");
					Xml.Append(A.ContentType.Normalize(NormalizationForm.FormC));
					Xml.Append("\" fileName=\"");
					Xml.Append(A.FileName.Normalize(NormalizationForm.FormC));
					Xml.Append("\" id=\"");
					Xml.Append(A.Id.Normalize(NormalizationForm.FormC));
					Xml.Append("\" legalId=\"");
					Xml.Append(A.LegalId.Normalize(NormalizationForm.FormC));
					Xml.Append("\" s=\"");
					Xml.Append(Convert.ToBase64String(A.Signature));
					Xml.Append("\" timestamp=\"");
					Xml.Append(XML.Encode(A.Timestamp));
					Xml.Append("\"/>");
				}
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

				if (!(this.contentSchemaDigest is null) && this.contentSchemaDigest.Length > 0)
				{
					Xml.Append("\" schemaDigest=\"");
					Xml.Append(Convert.ToBase64String(this.contentSchemaDigest));

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

				if (RoleParameters is null)
					Xml.Append("\"/>");
				else
				{
					Xml.Append("\"><roleParameters>");

					foreach (RoleParameter RoleParameter in RoleParameters)
					{
						Xml.Append("<parameter name=\"");
						Xml.Append(XML.Encode(RoleParameter.Name));
						Xml.Append("\" value=\"");
						Xml.Append(XML.Encode(RoleParameter.ObjectValue?.ToString()));
						Xml.Append("\"/>");
					}

					Xml.Append("</roleParameters></status>");
				}
			}

			if (IncludeServerSignature)
				this.serverSignature?.Serialize(Xml);

			if (IncludeAttachmentReferences && !(this.attachments is null))
			{
				foreach (Attachment A in this.attachments)
				{
					Xml.Append("<attachmentRef attachmentId=\"");
					Xml.Append(XML.Encode(A.Id));
					Xml.Append("\" url=\"");
					Xml.Append(XML.Encode(A.Url));
					Xml.Append("\"/>");
				}
			}

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

			if (!(this.parameters is null))
			{
				foreach (Parameter P in this.parameters)
				{
					if (P.Protection == ProtectionLevel.Normal)
						Response.Add(new KeyValuePair<string, object>(P.Name, P.ObjectValue ?? string.Empty));
				}
			}

			return Response.ToArray();
		}

		/// <summary>
		/// Tries to get a parameter object, given its name.
		/// </summary>
		/// <param name="Name">Parameter name.</param>
		/// <param name="Parameter">Parameter object, if found.</param>
		/// <returns>If a parameter with the given name was found.</returns>
		public bool TryGetParameter(string Name, out Parameter Parameter)
		{
			if (!(this.parameters is null))
			{
				foreach (Parameter P in this.parameters)
				{
					if (P.Name == Name)
					{
						Parameter = P;
						return true;
					}
				}
			}

			Parameter = null;
			return false;
		}

		/// <summary>
		/// Access to contract parameters.
		/// </summary>
		/// <param name="Key"></param>
		/// <returns>Parameter value.</returns>
		public object this[string Key]
		{
			get
			{
				if (this.TryGetParameter(Key, out Parameter Parameter))
					return Parameter.ObjectValue;
				else
					return null;
			}

			set
			{
				if (this.TryGetParameter(Key, out Parameter Parameter))
					Parameter.SetValue(value);
				else
					throw new IndexOutOfRangeException("A parameter named " + Key + " not found.");
			}
		}

		/// <summary>
		/// Default language for contract.
		/// </summary>
		public string DefaultLanguage
		{
			get
			{
				if (!(this.forHumans is null))
				{
					foreach (HumanReadableText Text in this.forHumans)
						return Text.Language;
				}

				return string.Empty;
			}
		}

		/// <summary>
		/// Gets available languages encoded in contract.
		/// </summary>
		/// <returns>Array of languages.</returns>
		public string[] GetLanguages()
		{
			SortedDictionary<string, bool> Languages = new SortedDictionary<string, bool>(StringComparer.CurrentCultureIgnoreCase);

			this.Add(Languages, this.forHumans);

			if (!(this.roles is null))
			{
				foreach (Role Role in this.roles)
					this.Add(Languages, Role.Descriptions);
			}

			if (!(this.parameters is null))
			{
				foreach (Parameter Parameter in this.parameters)
					this.Add(Languages, Parameter.Descriptions);
			}

			string[] Result = new string[Languages.Count];
			Languages.Keys.CopyTo(Result, 0);

			return Result;
		}

		private void Add(SortedDictionary<string, bool> Languages, HumanReadableText[] Texts)
		{
			if (Texts is null)
				return;

			foreach (HumanReadableText Text in Texts)
			{
				if (!string.IsNullOrEmpty(Text.Language))
					Languages[Text.Language] = true;
			}
		}

		/// <summary>
		/// Creates a human-readable Markdown document for the contract.
		/// </summary>
		/// <param name="Language">Desired language</param>
		/// <returns>Markdown</returns>
		public Task<string> ToMarkdown(string Language)
		{
			return this.ToMarkdown(Language, MarkdownType.ForRendering);
		}

		/// <summary>
		/// Creates a human-readable Markdown document for the contract.
		/// </summary>
		/// <param name="Language">Desired language</param>
		/// <param name="Type">Type of Markdown being generated.</param>
		/// <returns>Markdown</returns>
		public Task<string> ToMarkdown(string Language, MarkdownType Type)
		{
			return this.ToMarkdown(this.forHumans, Language, Type);
		}

		/// <summary>
		/// Creates a human-readable HTML document for the contract.
		/// </summary>
		/// <param name="Language">Desired language</param>
		/// <returns>Markdown</returns>
		public Task<string> ToHTML(string Language)
		{
			return this.ToHTML(this.forHumans, Language);
		}

		/// <summary>
		/// Creates a human-readable Plain Trext document for the contract.
		/// </summary>
		/// <param name="Language">Desired language</param>
		/// <returns>Markdown</returns>
		public Task<string> ToPlainText(string Language)
		{
			return this.ToPlainText(this.forHumans, Language);
		}

		/// <summary>
		/// Selects a human-readable text, and generates a Markdown document from it.
		/// </summary>
		/// <param name="Text">Collection of texts in different languages.</param>
		/// <param name="Language">Language</param>
		/// <param name="Type">Type of markdown to generate.</param>
		/// <returns>Markdown document.</returns>
		public async Task<string> ToMarkdown(HumanReadableText[] Text, string Language, MarkdownType Type)
		{
			HumanReadableText SelectedText = this.Select(Text, Language);
            if (SelectedText is null)
				return string.Empty;
			else
				return await SelectedText.GenerateMarkdown(this, Type);
		}

		/// <summary>
		/// Selects a human-readable text, and generates a plain-text document from it.
		/// </summary>
		/// <param name="Text">Collection of texts in different languages.</param>
		/// <param name="Language">Language</param>
		/// <returns>Plain-text document.</returns>
		public Task<string> ToPlainText(HumanReadableText[] Text, string Language)
		{
			return this.Select(Text, Language)?.GeneratePlainText(this) ?? Task.FromResult<string>(null);
		}

		/// <summary>
		/// Selects a human-readable text, and generates a HTML document from it.
		/// </summary>
		/// <param name="Text">Collection of texts in different languages.</param>
		/// <param name="Language">Language</param>
		/// <returns>HTML document.</returns>
		public Task<string> ToHTML(HumanReadableText[] Text, string Language)
		{
			return this.Select(Text, Language)?.GenerateHTML(this) ?? Task.FromResult<string>(null);
		}

		/// <summary>
		/// Selects a label, and generates a Markdown document from it.
		/// </summary>
		/// <param name="Label">Collection of texts in different languages.</param>
		/// <param name="Language">Language</param>
		/// <param name="Type">Type of markdown to generate.</param>
		/// <returns>Markdown document.</returns>
		public string ToMarkdown(Label[] Label, string Language, MarkdownType Type)
		{
			return this.Select(Label, Language)?.GenerateMarkdown(this, Type);
		}

		/// <summary>
		/// Selects a label, and generates a plain-text document from it.
		/// </summary>
		/// <param name="Label">Collection of texts in different languages.</param>
		/// <param name="Language">Language</param>
		/// <returns>Plain-text document.</returns>
		public Task<string> ToPlainText(Label[] Label, string Language)
		{
			return this.Select(Label, Language)?.GeneratePlainText(this) ?? Task.FromResult<string>(null);
		}

		/// <summary>
		/// Selects a label, and generates a HTML document from it.
		/// </summary>
		/// <param name="Label">Collection of texts in different languages.</param>
		/// <param name="Language">Language</param>
		/// <returns>HTML document.</returns>
		public Task<string> ToHTML(Label[] Label, string Language)
		{
			return this.Select(Label, Language)?.GenerateHTML(this) ?? Task.FromResult<string>(null);
		}

		/// <summary>
		/// Selects a human-readable text, from a collection of texts, based on currently selected language.
		/// </summary>
		/// <param name="Text">Collection of texts in different languages.</param>
		/// <param name="Language">Language</param>
		/// <returns>Selected text..</returns>
		public HumanReadableText Select(HumanReadableText[] Text, string Language)
		{
			if (Text is null)
				return null;

			HumanReadableText First = null;

			foreach (HumanReadableText T in Text)
			{
				if (string.Compare(T.Language, Language, StringComparison.CurrentCultureIgnoreCase) == 0)
					return T;

				First ??= T;
			}

			return First;
		}

		/// <summary>
		/// Selects a human-readable text, from a collection of texts, based on currently selected language.
		/// </summary>
		/// <param name="Label">Collection of labels in different languages.</param>
		/// <param name="Language">Language</param>
		/// <returns>Selected text..</returns>
		public Label Select(Label[] Label, string Language)
		{
			if (Label is null)
				return null;

			Label First = null;

			foreach (Label T in Label)
			{
				if (string.Compare(T.Language, Language, StringComparison.CurrentCultureIgnoreCase) == 0)
					return T;

				First ??= T;
			}

			return First;
		}

		/// <summary>
		/// Tries to set role parameters.
		/// </summary>
		/// <param name="Role">Role</param>
		/// <param name="RoleIndex">Role index.</param>
		/// <param name="Identity">Corresponding identity of signatory.</param>
		/// <returns>null if OK, name of role parameter that could not be set if failed.</returns>
		public string TrySetRoleParameters(string Role, int RoleIndex, LegalIdentity Identity)
		{
			if (this.parameters is null)
				return null;

			// Check all first, without setting role reference values.
			foreach (Parameter P2 in this.parameters)
			{
				if (P2 is RoleParameter RoleParameter &&
					RoleParameter.Role == Role &&
					RoleParameter.Index == RoleIndex)
				{
					string s = Identity[RoleParameter.Property];
					if (RoleParameter.Required && string.IsNullOrEmpty(s))
						return RoleParameter.Property;
				}
			}

			// If all role reference parameters OK, setting role reference values in contract.
			foreach (Parameter P2 in this.parameters)
			{
				if (P2 is RoleParameter RoleParameter &&
					RoleParameter.Role == Role &&
					RoleParameter.Index == RoleIndex)
				{
					string s = Identity[RoleParameter.Property];
					RoleParameter.SetValue(s);
				}
			}

			return null;
		}

		/// <summary>
		/// Clears role-reference parameter values.
		/// </summary>
		/// <param name="Role">Role</param>
		/// <param name="RoleIndex">Role index.</param>
		public void ClearRoleParameters(string Role, int RoleIndex)
		{
			if (this.parameters is null)
				return;

			// Check all first, without setting role reference values.
			foreach (Parameter P2 in this.parameters)
			{
				if (P2 is RoleParameter RoleParameter &&
					RoleParameter.Role == Role &&
					RoleParameter.Index == RoleIndex)
				{
					RoleParameter.SetValue(string.Empty);
				}
			}
		}

		/// <summary>
		/// Event raised when a parameter value needs to be formatted for display to a human user.
		/// </summary>
		public event EventHandlerAsync<ParameterValueFormattingEventArgs> FormatParameterDisplay;

		internal async Task<object> FormatParameterValue(string Name, object Value)
		{
			ParameterValueFormattingEventArgs e = new ParameterValueFormattingEventArgs(Name, Value);
			await this.FormatParameterDisplay.Raise(this, e, false);
			return e.Value;
		}

		/// <summary>
		/// Protects transient values, by generating GUIDs for those that lack GUIDs.
		/// </summary>
		public void ProtectTransientParameters()
		{
			if (this.parameters is null)
				return;

			foreach (Parameter P in this.parameters)
			{
				if (P.Protection == ProtectionLevel.Transient && P.ProtectedValue is null)
					P.ProtectedValue = Guid.NewGuid().ToByteArray();
			}
		}

		/// <summary>
		/// Protects encrypted values, by encrypting the clear text string representations for those that lack encrypted counterparts.
		/// </summary>
		/// <param name="CreatorJid">Bare JID of creator of contract.</param>
		/// <param name="Algorithm">Algorithm to use for protecting values.</param>
		public void EncryptEncryptedParameters(string CreatorJid, IParameterEncryptionAlgorithm Algorithm)
		{
			if (this.parameters is null)
				return;

			this.nonce ??= Guid.NewGuid().ToByteArray();

			uint i, c = (uint)this.parameters.Length;

			for (i = 0; i < c; i++)
			{
				Parameter P = this.parameters[i];

				if (P.Protection == ProtectionLevel.Encrypted && P.ProtectedValue is null)
					P.ProtectedValue = Algorithm.Encrypt(P.Name, P.ParameterType, i, CreatorJid, this.nonce, P.ObjectValue is null ? null : P.StringValue);
			}
		}

		/// <summary>
		/// Protects encrypted values, by encrypting the clear text string representations for those that lack encrypted counterparts.
		/// </summary>
		/// <param name="CreatorJid">Bare JID of creator of contract.</param>
		/// <param name="Algorithm">Algorithm to use for unprotecting values.</param>
		/// <returns>If protected values where unprotected successfully.</returns>
		public bool DecryptEncryptedParameters(string CreatorJid, IParameterEncryptionAlgorithm Algorithm)
		{
			if (this.parameters is null)
				return true;

			if (this.nonce is null)
				return false;

			try
			{
				uint i, c = (uint)this.parameters.Length;

				for (i = 0; i < c; i++)
				{
					Parameter P = this.parameters[i];

					if (P.Protection == ProtectionLevel.Encrypted && !(P.ProtectedValue is null))
						P.StringValue = Algorithm.Decrypt(P.Name, P.ParameterType, i, CreatorJid, this.nonce, P.ProtectedValue);
				}

				return true;
			}
			catch (Exception)
			{
				return false;
			}
		}
	}
}
