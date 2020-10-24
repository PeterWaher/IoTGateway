using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Numerics;
using System.Text;
using System.Xml;
using Waher.Content.Xml;
using Waher.Networking.XMPP.P2P;
using Waher.Networking.XMPP.P2P.E2E;
using Waher.Security;
using Waher.Security.EllipticCurves;

namespace Waher.Networking.XMPP.Contracts
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
	/// Legal identity
	/// </summary>
	public class LegalIdentity
	{
		private string id = null;
		private string provider = null;
		private IdentityState state = IdentityState.Created;
		private DateTime created = DateTime.MinValue;
		private DateTime updated = DateTime.MinValue;
		private DateTime from = DateTime.MinValue;
		private DateTime to = DateTime.MaxValue;
		private Property[] properties = null;
		private Attachment[] attachments = null;
		private string clientKeyName = null;
		private byte[] clientPubKey = null;
		private byte[] clientSignature = null;
		private byte[] serverSignature = null;

		/// <summary>
		/// Legal identity
		/// </summary>
		public LegalIdentity()
		{
		}

		/// <summary>
		/// ID of the legal identity
		/// </summary>
		public string Id
		{
			get { return this.id; }
			set { this.id = value; }
		}

		/// <summary>
		/// ID of legal identity, as an URI.
		/// </summary>
		public Uri IdUri => ContractsClient.LegalIdUri(this.id);

		/// <summary>
		/// ID of legal identity, as an URI string.
		/// </summary>
		public string IdUriString => ContractsClient.LegalIdUriString(this.id);

		/// <summary>
		/// Provider where the identity is maintained.
		/// </summary>
		public string Provider
		{
			get { return this.provider; }
			set { this.provider = value; }
		}

		/// <summary>
		/// Current state of identity
		/// </summary>
		public IdentityState State
		{
			get { return this.state; }
			set { this.state = value; }
		}

		/// <summary>
		/// When the identity object was created
		/// </summary>
		public DateTime Created
		{
			get { return this.created; }
			set { this.created = value; }
		}

		/// <summary>
		/// When the identity object was last updated
		/// </summary>
		public DateTime Updated
		{
			get { return this.updated; }
			set { this.updated = value; }
		}

		/// <summary>
		/// From what point in time the legal identity is valid.
		/// </summary>
		public DateTime From
		{
			get { return this.from; }
			set { this.from = value; }
		}

		/// <summary>
		/// To what point in time the legal identity is valid.
		/// </summary>
		public DateTime To
		{
			get { return this.to; }
			set { this.to = value; }
		}

		/// <summary>
		/// Properties detailing the legal identity.
		/// </summary>
		public Property[] Properties
		{
			get { return this.properties; }
			set { this.properties = value; }
		}

		/// <summary>
		/// Attachments assigned to the legal identity.
		/// </summary>
		public Attachment[] Attachments
		{
			get { return this.attachments; }
			set { this.attachments = value; }
		}

		/// <summary>
		/// Type of key used for client signatures
		/// </summary>
		public string ClientKeyName
		{
			get { return this.clientKeyName; }
			set { this.clientKeyName = value; }
		}

		/// <summary>
		/// Client Public key
		/// </summary>
		public byte[] ClientPubKey
		{
			get { return this.clientPubKey; }
			set { this.clientPubKey = value; }
		}

		/// <summary>
		/// Client signature
		/// </summary>
		public byte[] ClientSignature
		{
			get { return this.clientSignature; }
			set { this.clientSignature = value; }
		}

		/// <summary>
		/// Server signature
		/// </summary>
		public byte[] ServerSignature
		{
			get { return this.serverSignature; }
			set { this.serverSignature = value; }
		}

		/// <summary>
		/// Parses an identity from its XML representation
		/// </summary>
		/// <param name="Xml">XML representation</param>
		/// <returns>Legal identity</returns>
		public static LegalIdentity Parse(XmlElement Xml)
		{
			List<Property> Properties = new List<Property>();
			List<Attachment> Attachments = new List<Attachment>();
			LegalIdentity Result = new LegalIdentity()
			{
				id = XML.Attribute(Xml, "id")
			};

			foreach (XmlNode N in Xml.ChildNodes)
			{
				if (N is XmlElement E)
				{
					switch (E.LocalName)
					{
						case "clientPublicKey":
							foreach (XmlNode N2 in E.ChildNodes)
							{
								if (N2 is XmlElement E2)
								{
									IE2eEndpoint Key = EndpointSecurity.ParseE2eKey(E2);
									if (Key != null && Key.Namespace == EndpointSecurity.IoTHarmonizationE2E)
									{
                                        Result.clientPubKey = Key.PublicKey;

                                        if (Key is RsaEndpoint RsaEndpoint)
											Result.clientKeyName = "RSA" + RsaEndpoint.KeySize.ToString();
										else 
											Result.clientKeyName = Key.LocalName;
									}
								}
							}
							break;

						case "property":
							string Name = XML.Attribute(E, "name");
							string Value = XML.Attribute(E, "value");

							Properties.Add(new Property(Name, Value));
							break;

						case "clientSignature":
                            Result.clientSignature = Convert.FromBase64String(E.InnerText);
							break;


						case "attachment":
							Attachments.Add(new Attachment()
							{
								Id = XML.Attribute(E, "id"),
								LegalId = Result.id,
								ContentType = XML.Attribute(E, "contentType"),
								FileName = XML.Attribute(E, "fileName"),
								Signature = Convert.FromBase64String(XML.Attribute(E, "s")),
								Timestamp = XML.Attribute(E, "timestamp", DateTime.MinValue)
							});
							break;

						case "status":
							foreach (XmlAttribute Attr in E.Attributes)
							{
								switch (Attr.Name)
								{
									case "provider":
										Result.provider = Attr.Value;
										break;

									case "state":
										if (Enum.TryParse<IdentityState>(Attr.Value, out IdentityState IdentityState))
											Result.state = IdentityState;
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
								}
							}
							break;

						case "serverSignature":
                            Result.serverSignature = Convert.FromBase64String(E.InnerText);
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
					}
				}
			}

			Result.properties = Properties.ToArray();
			Result.attachments = Attachments.ToArray();

			return Result;
		}

		/// <summary>
		/// If the identity has a client public key
		/// </summary>
		public bool HasClientPublicKey
		{
			get { return !string.IsNullOrEmpty(this.clientKeyName) && !(this.ClientPubKey is null); }
		}

		/// <summary>
		/// If the identity has a client signature
		/// </summary>
		public bool HasClientSignature
		{
			get
			{
				if (this.clientKeyName is null)
					return false;

                return !(this.clientSignature is null);
			}
		}

		/// <summary>
		/// Serializes the identity to XML
		/// </summary>
		/// <param name="Xml">XML output</param>
		/// <param name="IncludeNamespace">If namespace should be included in the identity element.</param>
		/// <param name="IncludeIdAttribute">If the id attribute should be included</param>
		/// <param name="IncludeClientSignature">If the client signature should be included</param>
		/// <param name="IncludeAttachments">If attachments should be included</param>
		/// <param name="IncludeStatus">If the status should be included</param>
		/// <param name="IncludeServerSignature">If the server signature should be included</param>
		/// <param name="IncludeAttachmentReferences">If URLs to attachment content is to be included</param>
		public void Serialize(StringBuilder Xml, bool IncludeNamespace, bool IncludeIdAttribute, bool IncludeClientSignature,
			bool IncludeAttachments, bool IncludeStatus, bool IncludeServerSignature, bool IncludeAttachmentReferences)
		{
			Xml.Append("<identity");

			if (IncludeIdAttribute)
			{
				Xml.Append(" id=\"");
				Xml.Append(XML.Encode(this.id));
				Xml.Append('"');
			}

			if (IncludeNamespace)
			{
				Xml.Append(" xmlns=\"");
				Xml.Append(ContractsClient.NamespaceLegalIdentities);
				Xml.Append('"');
			}

			Xml.Append('>');

			if (this.HasClientPublicKey)
			{
				Xml.Append("<clientPublicKey>");

				if (this.clientKeyName.StartsWith("RSA"))
				{
					Xml.Append("<rsa pub=\"");
					Xml.Append(Convert.ToBase64String(this.clientPubKey));
					Xml.Append("\" size=\"");
					Xml.Append(this.clientKeyName.Substring(3));
					Xml.Append("\" xmlns=\"");
					Xml.Append(EndpointSecurity.IoTHarmonizationE2E);
				}
				else
				{
					Xml.Append('<');
					Xml.Append(this.clientKeyName);
					Xml.Append(" pub=\"");
					Xml.Append(Convert.ToBase64String(this.clientPubKey));
					Xml.Append("\" xmlns=\"");
					Xml.Append(EndpointSecurity.IoTHarmonizationE2E);
				}

				Xml.Append("\"/></clientPublicKey>");
			}

			if (!(this.properties is null))
			{
				foreach (Property P in this.properties)
				{
					Xml.Append("<property name=\"");
					Xml.Append(P.Name);
					Xml.Append("\" value=\"");
					Xml.Append(P.Value);
					Xml.Append("\"/>");
				}
			}

			if (IncludeClientSignature)
			{
                Xml.Append("<clientSignature>");

                if (!(this.clientSignature is null))
                    Xml.Append(Convert.ToBase64String(this.clientSignature));

                Xml.Append("</clientSignature>");
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

				Xml.Append("\" state=\"");
				Xml.Append(this.state.ToString());

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
			{
				Xml.Append("<serverSignature>");

				if (!(this.serverSignature is null))
					Xml.Append(Convert.ToBase64String(this.serverSignature));

                Xml.Append("</serverSignature>");
            }

			if (IncludeAttachmentReferences && !(this.attachments is null))
			{
				foreach (Attachment A in this.attachments)
				{
					if (!string.IsNullOrEmpty(A.Url))
					{
						Xml.Append("<attachmentRef attachmentId=\"");
						Xml.Append(XML.Encode(A.Id));
						Xml.Append("\" url=\"");
						Xml.Append(XML.Encode(A.Url));
						Xml.Append("\"/>");
					}
				}
			}

			Xml.Append("</identity>");
		}

        /// <summary>
        /// Validates a client signature
        /// </summary>
        /// <param name="Data">Binary data being signed.</param>
        /// <param name="Signature">Digital signature</param>
        /// <returns>If the client signature is correct</returns>
        public bool ValidateSignature(byte[] Data, byte[] Signature)
		{
			if (!this.HasClientPublicKey)
				return false;

			if (this.clientKeyName.StartsWith("RSA"))
			{
				if (!int.TryParse(this.clientKeyName.Substring(3), out int KeySize))
					return false;

				return RsaEndpoint.Verify(Data, Signature, KeySize, this.clientPubKey);
			}
			else if (EndpointSecurity.TryCreateEndpoint(this.clientKeyName,
				EndpointSecurity.IoTHarmonizationE2E, out IE2eEndpoint Endpoint) &&
                Endpoint is EllipticCurveEndpoint LocalEc)
			{
				return LocalEc.Verify(Data, this.clientPubKey, Signature);
			}
			else
				return false;
		}

		/// <summary>
		/// Validates the client signature of the legal identity
		/// </summary>
		/// <returns>If the client signature of the legal identity is correct</returns>
		public bool ValidateClientSignature()
		{
			if (!this.HasClientSignature)
				return false;

			StringBuilder Xml = new StringBuilder();
			this.Serialize(Xml, false, false, false, false, false, false, false);
			byte[] Data = Encoding.UTF8.GetBytes(Xml.ToString());

			return this.ValidateSignature(Data, this.clientSignature);
		}

		/// <summary>
		/// Gets tags describing the legal identity.
		/// </summary>
		/// <returns>Set of tags</returns>
		public KeyValuePair<string, object>[] GetTags()
		{
			List<KeyValuePair<string, object>> Response = new List<KeyValuePair<string, object>>()
			{
				new KeyValuePair<string, object>("ID", this.id),
				new KeyValuePair<string, object>("Provider", this.provider),
				new KeyValuePair<string, object>("State", this.state),
				new KeyValuePair<string, object>("Created", this.created)
			};

			if (this.updated != DateTime.MinValue)
				Response.Add(new KeyValuePair<string, object>("Updated", this.updated));

			if (this.from != DateTime.MinValue)
				Response.Add(new KeyValuePair<string, object>("From", this.from));

			if (this.to != DateTime.MaxValue)
				Response.Add(new KeyValuePair<string, object>("To", this.to));

			if (!(this.properties is null))
			{
				foreach (Property P in this.properties)
					Response.Add(new KeyValuePair<string, object>(P.Name, P.Value));
			}

			return Response.ToArray();
		}

		/// <summary>
		/// Access to property values.
		/// </summary>
		/// <param name="Key">Property key</param>
		/// <returns>Corresponding property value, if one is found with the same key, or the empty string, if not.</returns>
		public string this[string Key]
		{
			get
			{
				if (!(this.properties is null))
				{
					foreach (Property P in this.properties)
					{
						if (P.Name == Key)
							return P.Value;
					}
				}

				return string.Empty;
			}
		}

		/// <summary>
		/// <see cref="Object.Equals(object)"/>
		/// </summary>
		public override bool Equals(object obj)
		{
			if (!(obj is LegalIdentity ID))
				return false;

			if (!this.id.Equals(ID.id) ||
				!this.provider.Equals(ID.provider) ||
				!this.state.Equals(ID.state) ||
				!this.created.Equals(ID.created) ||
				!this.updated.Equals(ID.updated) ||
				!this.from.Equals(ID.from) ||
				!this.to.Equals(ID.to) ||
				!this.clientKeyName.Equals(ID.clientKeyName) ||
				!AreEqual(this.clientPubKey, ID.clientPubKey) ||
				!AreEqual(this.clientSignature, ID.clientSignature) ||
				!AreEqual(this.serverSignature, ID.serverSignature))
			{
				return false;
			}

			if ((this.properties is null) ^ (ID.properties is null))
				return false;

			if (!(this.properties is null))
			{
				int i, c = this.properties.Length;

				if (c != ID.properties.Length)
					return false;

				for (i = 0; i < c; i++)
				{
					if (!this.properties[i].Equals(ID.properties[i]))
						return false;
				}
			}

			return true;
		}

		internal static bool AreEqual(byte[] A1, byte[] A2)
		{
			if ((A1 is null) ^ (A2 is null))
				return false;

			if (A1 is null)
				return true;

			int i, c = A1.Length;
			if (c != A2.Length)
				return false;

			for (i = 0; i < c; i++)
			{
				if (A1[i] != A2[i])
					return false;
			}

			return true;
		}

		/// <summary>
		/// <see cref="object.GetHashCode()"/>
		/// </summary>
		public override int GetHashCode()
		{
			int Result = this.id.GetHashCode();
			Result ^= Result << 5 ^ this.provider.GetHashCode();
			Result ^= Result << 5 ^ this.state.GetHashCode();
			Result ^= Result << 5 ^ this.created.GetHashCode();
			Result ^= Result << 5 ^ this.updated.GetHashCode();
			Result ^= Result << 5 ^ this.from.GetHashCode();
			Result ^= Result << 5 ^ this.to.GetHashCode();
			Result ^= Result << 5 ^ this.clientKeyName.GetHashCode();
			Result ^= Result << 5 ^ GetHashCode(this.clientPubKey);
			Result ^= Result << 5 ^ GetHashCode(this.clientSignature);
			Result ^= Result << 5 ^ GetHashCode(this.serverSignature);

			if (!(this.properties is null))
			{
				int i, c = this.properties.Length;

				for (i = 0; i < c; i++)
					Result ^= Result << 5 ^ this.properties[i].GetHashCode();
			}

			return Result;
		}

		internal static int GetHashCode(byte[] Bin)
		{
			int Result = 0;

			if (!(Bin is null))
			{
				int i, c = Bin.Length;

				for (i = 0; i < c; i++)
					Result ^= Result << 5 ^ Bin[i];
			}

			return Result;
		}

	}
}
