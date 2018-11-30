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
		/// Submitted keys are being challenged.
		/// </summary>
		ChallengingKeys,

		/// <summary>
		/// The legal identity has been rejected.
		/// </summary>
		Rejected,

		/// <summary>
		/// The legal identity is pending confirmation out-of-band.
		/// </summary>
		PendingIdentification,

		/// <summary>
		/// The legal identity is authenticated and approved by the Trust Anchor.
		/// </summary>
		Approved
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
		private string clientKeyName = null;
		private byte[] clientPubKey1 = null;
		private byte[] clientPubKey2 = null;
		private byte[] clientSignature1 = null;
		private byte[] clientSignature2 = null;
		private byte[] serverSignature1 = null;
		private byte[] serverSignature2 = null;

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
		/// Type of key used for client signatures
		/// </summary>
		public string ClientKeyName
		{
			get { return this.clientKeyName; }
			set { this.clientKeyName = value; }
		}

		/// <summary>
		/// Public key 1
		/// </summary>
		public byte[] ClientPubKey1
		{
			get { return this.clientPubKey1; }
			set { this.clientPubKey1 = value; }
		}

		/// <summary>
		/// Public key 2
		/// </summary>
		public byte[] ClientPubKey2
		{
			get { return this.clientPubKey2; }
			set { this.clientPubKey2 = value; }
		}

		/// <summary>
		/// Client signature 1
		/// </summary>
		public byte[] ClientSignature1
		{
			get { return this.clientSignature1; }
			set { this.clientSignature1 = value; }
		}

		/// <summary>
		/// Client signature 2
		/// </summary>
		public byte[] ClientSignature2
		{
			get { return this.clientSignature2; }
			set { this.clientSignature2 = value; }
		}

		/// <summary>
		/// Server signature 1
		/// </summary>
		public byte[] ServerSignature1
		{
			get { return this.serverSignature1; }
			set { this.serverSignature1 = value; }
		}

		/// <summary>
		/// Server signature 2
		/// </summary>
		public byte[] ServerSignature2
		{
			get { return this.serverSignature2; }
			set { this.serverSignature2 = value; }
		}

		/// <summary>
		/// Parses an identity from its XML representation
		/// </summary>
		/// <param name="Xml">XML representation</param>
		/// <returns>Legal identity</returns>
		public static LegalIdentity Parse(XmlElement Xml)
		{
			List<Property> Properties = new List<Property>();
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
										if (Key is RsaAes RsaAes)
										{
											Result.clientKeyName = "RSA" + RsaAes.KeySize.ToString();
											Result.clientPubKey1 = RsaAes.Modulus;
											Result.clientPubKey2 = RsaAes.Exponent;
										}
										else if (Key is EcAes256 EcAes256)
										{
											Result.clientKeyName = Key.LocalName;
											Result.ClientPubKey1 = EcAes256.ToNetwork(EcAes256.PublicKey.X);
											Result.ClientPubKey2 = EcAes256.ToNetwork(EcAes256.PublicKey.Y);
										}
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
							foreach (XmlAttribute Attr in E.Attributes)
							{
								switch (Attr.Name)
								{
									case "s1":
										Result.clientSignature1 = Convert.FromBase64String(Attr.Value);
										break;

									case "s2":
										Result.clientSignature2 = Convert.FromBase64String(Attr.Value);
										break;
								}
							}
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
							foreach (XmlAttribute Attr in E.Attributes)
							{
								switch (Attr.Name)
								{
									case "s1":
										Result.serverSignature1 = Convert.FromBase64String(Attr.Value);
										break;

									case "s2":
										Result.serverSignature2 = Convert.FromBase64String(Attr.Value);
										break;
								}
							}
							break;
					}
				}
			}

			Result.properties = Properties.ToArray();

			return Result;
		}

		/// <summary>
		/// If the identity has a client public key
		/// </summary>
		public bool HasClientPublicKey
		{
			get { return !string.IsNullOrEmpty(this.clientKeyName) && this.ClientPubKey1 != null && this.clientPubKey2 != null; }
		}

		/// <summary>
		/// If the identity has a client signature
		/// </summary>
		public bool HasClientSignature
		{
			get
			{
				if (this.clientKeyName == null)
					return false;

				if (this.clientKeyName.StartsWith("RSA"))
					return this.clientSignature1 != null;
				else
					return this.clientSignature1 != null && this.clientSignature2 != null;
			}
		}

		/// <summary>
		/// Serializes the identity to XML
		/// </summary>
		/// <param name="Xml">XML output</param>
		/// <param name="IncludeNamespace">If namespace should be included in the identity element.</param>
		/// <param name="IncludeIdAttribute">If the id attribute should be included</param>
		/// <param name="IncludeClientSignature">If the client signature should be included</param>
		/// <param name="IncludeStatus">If the status should be included</param>
		/// <param name="IncludeServerSignature">If the server signature should be included</param>
		public void Serialize(StringBuilder Xml, bool IncludeNamespace, bool IncludeIdAttribute, bool IncludeClientSignature,
			bool IncludeStatus, bool IncludeServerSignature)
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
					Xml.Append("<rsa exp=\"");
					Xml.Append(Convert.ToBase64String(this.clientPubKey1));
					Xml.Append("\" mod=\"");
					Xml.Append(Convert.ToBase64String(this.clientPubKey2));
					Xml.Append("\" size=\"");
					Xml.Append(this.clientKeyName.Substring(3));
					Xml.Append("\" xmlns=\"");
					Xml.Append(EndpointSecurity.IoTHarmonizationE2E);
				}
				else
				{
					Xml.Append('<');
					Xml.Append(this.clientKeyName);
					Xml.Append(" x=\"");
					Xml.Append(Convert.ToBase64String(this.clientPubKey1));
					Xml.Append("\" xmlns=\"");
					Xml.Append(EndpointSecurity.IoTHarmonizationE2E);
					Xml.Append("\" y=\"");
					Xml.Append(Convert.ToBase64String(this.clientPubKey2));
				}

				Xml.Append("\"/></clientPublicKey>");
			}

			if (this.properties != null)
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
				Xml.Append("<clientSignature");

				if (this.clientSignature1 != null)
				{
					Xml.Append(" s1=\"");
					Xml.Append(Convert.ToBase64String(this.clientSignature1));
					Xml.Append("\"");
				}

				if (this.clientSignature2 != null)
				{
					Xml.Append(" s2=\"");
					Xml.Append(Convert.ToBase64String(this.clientSignature2));
					Xml.Append("\"");
				}

				Xml.Append("/>");
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
				Xml.Append("<serverSignature");

				if (this.serverSignature1 != null)
				{
					Xml.Append(" s1=\"");
					Xml.Append(Convert.ToBase64String(this.serverSignature1));
					Xml.Append("\"");
				}

				if (this.serverSignature2 != null)
				{
					Xml.Append(" s2=\"");
					Xml.Append(Convert.ToBase64String(this.serverSignature2));
					Xml.Append("\"");
				}

				Xml.Append("/>");
			}

			Xml.Append("</identity>");
		}

		/// <summary>
		/// Validates the client signature
		/// </summary>
		/// <returns>If the client signature is correct</returns>
		public bool ValidateClientSignature()
		{
			if (!this.HasClientPublicKey)
				return false;

			if (!this.HasClientSignature)
				return false;

			StringBuilder Xml = new StringBuilder();
			this.Serialize(Xml, false, false, false, false, false);
			byte[] Data = Encoding.UTF8.GetBytes(Xml.ToString());

			if (this.clientKeyName.StartsWith("RSA"))
			{
				if (!int.TryParse(this.clientKeyName.Substring(3), out int KeySize))
					return false;

				return RsaAes.Verify(Data, this.clientSignature1, KeySize, this.clientPubKey1, this.clientPubKey2);
			}
			else if (EndpointSecurity.TryCreateEndpoint(this.clientKeyName,
				EndpointSecurity.IoTHarmonizationE2E, out IE2eEndpoint Endpoint) &&
				Endpoint is EcAes256 EcAes256)
			{
				return EcAes256.Verify(Data, this.clientPubKey1, this.clientPubKey2,
					this.clientSignature1, this.clientSignature2, HashFunction.SHA256);
			}
			else
				return false;
		}


	}
}
