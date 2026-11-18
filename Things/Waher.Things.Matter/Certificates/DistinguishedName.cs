using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using Waher.Things.Matter.Asn1;
using Waher.Things.Matter.Tlv;

namespace Waher.Things.Matter.Certificates
{
	/// <summary>
	/// Attribute of a distinguished name, as encoded in Matter TLV certificates.
	/// </summary>
	public class DnAttribute
	{
		/// <summary>
		/// Attribute of a distinguished name.
		/// </summary>
		/// <param name="Tag">Context tag (Matter Core Specification, 6.5.6).</param>
		/// <param name="Value">Value (string, or unsigned integer for Matter-specific attributes).</param>
		public DnAttribute(byte Tag, object Value)
		{
			this.Tag = Tag;
			this.Value = Value;
		}

		/// <summary>
		/// Context tag of attribute.
		/// </summary>
		public byte Tag { get; }

		/// <summary>
		/// Attribute value: string, or ulong for Matter-specific attributes.
		/// </summary>
		public object Value { get; }

		/// <summary>
		/// If the attribute is encoded as a PrintableString in X.509.
		/// </summary>
		public bool IsPrintableString => (this.Tag & 0x80) != 0;

		/// <summary>
		/// Base tag (without the PrintableString flag).
		/// </summary>
		public byte BaseTag => (byte)(this.Tag & 0x7f);

		/// <inheritdoc/>
		public override string ToString()
		{
			string Name = DistinguishedName.GetAttributeName(this.BaseTag);

			if (this.Value is ulong ul)
				return Name + "=" + ul.ToString(this.BaseTag == DistinguishedName.TagNocCat ? "X8" : "X16");
			else
				return Name + "=" + this.Value?.ToString();
		}
	}

	/// <summary>
	/// Distinguished name of a Matter certificate (issuer or subject), preserving attribute order.
	/// </summary>
	public class DistinguishedName
	{
		/// <summary>
		/// Common name.
		/// </summary>
		public const byte TagCommonName = 1;

		/// <summary>
		/// Matter node ID.
		/// </summary>
		public const byte TagNodeId = 17;

		/// <summary>
		/// Matter firmware signing ID.
		/// </summary>
		public const byte TagFirmwareSigningId = 18;

		/// <summary>
		/// Matter ICAC ID.
		/// </summary>
		public const byte TagIcacId = 19;

		/// <summary>
		/// Matter RCAC ID.
		/// </summary>
		public const byte TagRcacId = 20;

		/// <summary>
		/// Matter fabric ID.
		/// </summary>
		public const byte TagFabricId = 21;

		/// <summary>
		/// Matter CASE Authenticated Tag (NOC CAT).
		/// </summary>
		public const byte TagNocCat = 22;

		/// <summary>
		/// Matter VID verification signer ID.
		/// </summary>
		public const byte TagVvsId = 23;

		private static readonly Dictionary<byte, string> oids = new Dictionary<byte, string>()
		{
			{ 1, "2.5.4.3" },
			{ 2, "2.5.4.4" },
			{ 3, "2.5.4.5" },
			{ 4, "2.5.4.6" },
			{ 5, "2.5.4.7" },
			{ 6, "2.5.4.8" },
			{ 7, "2.5.4.10" },
			{ 8, "2.5.4.11" },
			{ 9, "2.5.4.12" },
			{ 10, "2.5.4.41" },
			{ 11, "2.5.4.42" },
			{ 12, "2.5.4.43" },
			{ 13, "2.5.4.44" },
			{ 14, "2.5.4.46" },
			{ 15, "2.5.4.65" },
			{ 16, "0.9.2342.19200300.100.1.25" },
			{ 17, "1.3.6.1.4.1.37244.1.1" },
			{ 18, "1.3.6.1.4.1.37244.1.2" },
			{ 19, "1.3.6.1.4.1.37244.1.3" },
			{ 20, "1.3.6.1.4.1.37244.1.4" },
			{ 21, "1.3.6.1.4.1.37244.1.5" },
			{ 22, "1.3.6.1.4.1.37244.1.6" },
			{ 23, "1.3.6.1.4.1.37244.1.7" }
		};

		private static readonly Dictionary<string, byte> tagsByOid = CreateReverse();

		private static Dictionary<string, byte> CreateReverse()
		{
			Dictionary<string, byte> Result = new Dictionary<string, byte>();

			foreach (KeyValuePair<byte, string> P in oids)
				Result[P.Value] = P.Key;

			return Result;
		}

		private readonly List<DnAttribute> attributes = new List<DnAttribute>();

		/// <summary>
		/// Distinguished name of a Matter certificate.
		/// </summary>
		public DistinguishedName()
		{
		}

		/// <summary>
		/// Attributes, in order.
		/// </summary>
		public IReadOnlyList<DnAttribute> Attributes => this.attributes;

		/// <summary>
		/// Adds an attribute.
		/// </summary>
		/// <param name="Tag">Tag.</param>
		/// <param name="Value">Value.</param>
		/// <returns>The distinguished name object.</returns>
		public DistinguishedName Add(byte Tag, object Value)
		{
			this.attributes.Add(new DnAttribute(Tag, Value));
			return this;
		}

		/// <summary>
		/// Gets the first unsigned integer value of a given attribute.
		/// </summary>
		/// <param name="Tag">Tag.</param>
		/// <returns>Value, or null if not found.</returns>
		public ulong? GetUInt64(byte Tag)
		{
			foreach (DnAttribute A in this.attributes)
			{
				if (A.Tag == Tag && A.Value is ulong ul)
					return ul;
			}

			return null;
		}

		/// <summary>
		/// Gets all unsigned integer values of a given attribute.
		/// </summary>
		/// <param name="Tag">Tag.</param>
		/// <returns>Values.</returns>
		public ulong[] GetUInt64s(byte Tag)
		{
			List<ulong> Result = new List<ulong>();

			foreach (DnAttribute A in this.attributes)
			{
				if (A.Tag == Tag && A.Value is ulong ul)
					Result.Add(ul);
			}

			return Result.ToArray();
		}

		/// <summary>
		/// Gets the first string value of a given attribute (including PrintableString variant).
		/// </summary>
		/// <param name="Tag">Base tag.</param>
		/// <returns>Value, or null if not found.</returns>
		public string GetString(byte Tag)
		{
			foreach (DnAttribute A in this.attributes)
			{
				if (A.BaseTag == Tag && A.Value is string s)
					return s;
			}

			return null;
		}

		/// <summary>
		/// Matter node ID, if present.
		/// </summary>
		public ulong? NodeId => this.GetUInt64(TagNodeId);

		/// <summary>
		/// Matter fabric ID, if present.
		/// </summary>
		public ulong? FabricId => this.GetUInt64(TagFabricId);

		/// <summary>
		/// Matter RCAC ID, if present.
		/// </summary>
		public ulong? RcacId => this.GetUInt64(TagRcacId);

		/// <summary>
		/// Matter ICAC ID, if present.
		/// </summary>
		public ulong? IcacId => this.GetUInt64(TagIcacId);

		/// <summary>
		/// CASE Authenticated Tags.
		/// </summary>
		public uint[] CaseAuthenticatedTags
		{
			get
			{
				ulong[] Values = this.GetUInt64s(TagNocCat);
				uint[] Result = new uint[Values.Length];
				int i;

				for (i = 0; i < Values.Length; i++)
					Result[i] = (uint)Values[i];

				return Result;
			}
		}

		/// <summary>
		/// Gets the human-readable name of an attribute tag.
		/// </summary>
		/// <param name="Tag">Base tag.</param>
		/// <returns>Name.</returns>
		public static string GetAttributeName(byte Tag)
		{
			switch (Tag)
			{
				case 1: return "CN";
				case 2: return "SN";
				case 3: return "SerialNumber";
				case 4: return "C";
				case 5: return "L";
				case 6: return "ST";
				case 7: return "O";
				case 8: return "OU";
				case 9: return "Title";
				case 10: return "Name";
				case 11: return "GivenName";
				case 12: return "Initials";
				case 13: return "GenQualifier";
				case 14: return "DnQualifier";
				case 15: return "Pseudonym";
				case 16: return "DC";
				case 17: return "NodeId";
				case 18: return "FirmwareSigningId";
				case 19: return "IcacId";
				case 20: return "RcacId";
				case 21: return "FabricId";
				case 22: return "CAT";
				case 23: return "VvsId";
				default: return "Tag" + Tag.ToString();
			}
		}

		private static bool IsMatterIdTag(byte Tag)
		{
			return Tag >= TagNodeId && Tag <= TagVvsId;
		}

		/// <summary>
		/// Encodes the distinguished name as a TLV list.
		/// </summary>
		/// <param name="Output">TLV writer.</param>
		/// <param name="Tag">Tag of list.</param>
		public void EncodeTlv(TlvWriter Output, TlvTag Tag)
		{
			Output.StartList(Tag);

			foreach (DnAttribute A in this.attributes)
			{
				if (A.Value is ulong ul)
					Output.WriteUInt(A.Tag, ul);
				else
					Output.WriteString(A.Tag, A.Value?.ToString() ?? string.Empty);
			}

			Output.EndContainer();
		}

		/// <summary>
		/// Decodes a distinguished name from a TLV list.
		/// </summary>
		/// <param name="List">TLV list.</param>
		/// <returns>Distinguished name.</returns>
		public static DistinguishedName DecodeTlv(TlvElement List)
		{
			DistinguishedName Result = new DistinguishedName();

			foreach (TlvElement E in List.Children)
			{
				if (!E.Tag.IsContextSpecific)
					throw new MatterDecodingException("Invalid distinguished name attribute tag.");

				byte Tag = (byte)E.Tag.Number;

				if (E.IsInteger)
					Result.Add(Tag, E.AsUInt64());
				else
					Result.Add(Tag, E.AsString());
			}

			return Result;
		}

		/// <summary>
		/// Encodes the distinguished name in X.509 (DER) form.
		/// </summary>
		/// <param name="Output">DER writer.</param>
		public void EncodeDer(DerWriter Output)
		{
			Output.StartSequence();

			foreach (DnAttribute A in this.attributes)
			{
				if (!oids.TryGetValue(A.BaseTag, out string Oid))
					throw new MatterException("Unsupported distinguished name attribute: " + A.Tag.ToString());

				Output.StartSet();
				Output.StartSequence();
				Output.WriteOid(Oid);

				if (A.Value is ulong ul)
				{
					if (A.BaseTag == TagNocCat)
						Output.WriteUtf8String(((uint)ul).ToString("X8"));
					else
						Output.WriteUtf8String(ul.ToString("X16"));
				}
				else
				{
					string s = A.Value?.ToString() ?? string.Empty;

					if (A.BaseTag == 16)
						Output.WriteIa5String(s);
					else if (A.IsPrintableString)
						Output.WritePrintableString(s);
					else
						Output.WriteUtf8String(s);
				}

				Output.EndSequence();
				Output.EndSet();
			}

			Output.EndSequence();
		}

		/// <summary>
		/// Decodes a distinguished name from X.509 (DER) form.
		/// </summary>
		/// <param name="Reader">Reader positioned at the Name SEQUENCE.</param>
		/// <returns>Distinguished name.</returns>
		public static DistinguishedName DecodeDer(DerReader Reader)
		{
			DistinguishedName Result = new DistinguishedName();
			DerReader Seq = Reader.ReadSequence();

			while (Seq.HasMore)
			{
				DerReader Set = Seq.ReadSet();

				while (Set.HasMore)
				{
					DerReader Atv = Set.ReadSequence();
					string Oid = Atv.ReadOid();
					string Value = Atv.ReadString(out byte StringTag);

					if (!tagsByOid.TryGetValue(Oid, out byte Tag))
					{
						Result.otherAttributes.Add(new KeyValuePair<string, string>(Oid, Value));
						continue;
					}

					if (IsMatterIdTag(Tag))
					{
						if (!ulong.TryParse(Value, NumberStyles.HexNumber, CultureInfo.InvariantCulture, out ulong ul))
							throw new MatterDecodingException("Invalid Matter identifier in distinguished name: " + Value);

						Result.Add(Tag, ul);
					}
					else if (StringTag == DerWriter.TagPrintableString)
						Result.Add((byte)(Tag | 0x80), Value);
					else
						Result.Add(Tag, Value);
				}
			}

			return Result;
		}

		private readonly List<KeyValuePair<string, string>> otherAttributes = new List<KeyValuePair<string, string>>();

		/// <summary>
		/// Attributes found in X.509 names that have no Matter TLV representation (OID, value).
		/// </summary>
		public IReadOnlyList<KeyValuePair<string, string>> OtherAttributes => this.otherAttributes;

		/// <summary>
		/// Gets the value of an attribute by OID (including non-Matter attributes in X.509 names).
		/// </summary>
		/// <param name="Oid">Object identifier.</param>
		/// <returns>Value, or null if not found.</returns>
		public string GetByOid(string Oid)
		{
			if (tagsByOid.TryGetValue(Oid, out byte Tag))
			{
				foreach (DnAttribute A in this.attributes)
				{
					if (A.BaseTag == Tag)
					{
						if (A.Value is ulong ul)
							return ul.ToString("X16");
						else
							return A.Value?.ToString();
					}
				}
			}

			foreach (KeyValuePair<string, string> P in this.otherAttributes)
			{
				if (P.Key == Oid)
					return P.Value;
			}

			return null;
		}

		/// <summary>
		/// Checks if two distinguished names are equal (same attributes in the same order).
		/// </summary>
		/// <param name="obj">Other object.</param>
		/// <returns>If equal.</returns>
		public override bool Equals(object obj)
		{
			if (!(obj is DistinguishedName Other) || Other.attributes.Count != this.attributes.Count)
				return false;

			int i, c = this.attributes.Count;

			for (i = 0; i < c; i++)
			{
				DnAttribute A = this.attributes[i];
				DnAttribute B = Other.attributes[i];

				if (A.BaseTag != B.BaseTag || !Equals(A.Value, B.Value))
					return false;
			}

			return true;
		}

		/// <inheritdoc/>
		public override int GetHashCode()
		{
			int Result = 0;

			foreach (DnAttribute A in this.attributes)
				Result = (Result * 31) ^ A.BaseTag ^ (A.Value?.GetHashCode() ?? 0);

			return Result;
		}

		/// <inheritdoc/>
		public override string ToString()
		{
			StringBuilder sb = new StringBuilder();
			bool First = true;

			foreach (DnAttribute A in this.attributes)
			{
				if (First)
					First = false;
				else
					sb.Append(", ");

				sb.Append(A.ToString());
			}

			foreach (KeyValuePair<string, string> P in this.otherAttributes)
			{
				if (First)
					First = false;
				else
					sb.Append(", ");

				sb.Append(P.Key);
				sb.Append('=');
				sb.Append(P.Value);
			}

			return sb.ToString();
		}

		/// <summary>
		/// Converts an integer used in certificates to a string (for logging purposes).
		/// </summary>
		/// <param name="Value">Value.</param>
		/// <returns>Hexadecimal string.</returns>
		public static string ToHex(ulong Value) => Value.ToString("X16");

		/// <summary>
		/// Checks if a value is a valid operational node ID (Matter Core Specification, 2.5.5.1).
		/// </summary>
		/// <param name="NodeId">Node ID.</param>
		/// <returns>If operational.</returns>
		public static bool IsOperationalNodeId(ulong NodeId)
		{
			return NodeId >= 1 && NodeId <= 0xFFFFFFEFFFFFFFFF;
		}

		/// <summary>
		/// Exception helper for invalid formats.
		/// </summary>
		internal static Exception Invalid(string Message) => new MatterDecodingException(Message);
	}
}
