using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace Waher.Security.PKCS
{
	/// <summary>
	/// Type class
	/// </summary>
	public enum Asn1TypeClass
	{
		/// <summary>
		/// Native to ASN.1
		/// </summary>
		Universal = 0,

		/// <summary>
		/// Only valid for a specific application.
		/// </summary>
		Application = 1,

		/// <summary>
		/// Depends on the context.
		/// </summary>
		ContextSpecific = 2,

		/// <summary>
		/// Defined in private specifications.
		/// </summary>
		Private = 3
	}

	/// <summary>
	/// Encodes data using the Distinguished Encoding Rules (DER), as defined in X.690
	/// </summary>
	public class DerEncoder
	{
		private List<byte> output = new List<byte>();
		private LinkedList<KeyValuePair<byte, List<byte>>> stack = null;

		/// <summary>
		/// Encodes data using the Distinguished Encoding Rules (DER), as defined in X.690
		/// </summary>
		public DerEncoder()
		{
		}

		/// <summary>
		/// Clears the output buffer.
		/// </summary>
		public void Clear()
		{
			this.output.Clear();
		}

		/// <summary>
		/// Converts the generated output to a byte arary.
		/// </summary>
		/// <returns>Byte array.</returns>
		public byte[] ToArray()
		{
			if (this.stack != null && this.stack.First != null)
				throw new Exception("DER output not properly closed.");

			return this.output.ToArray();
		}

		/// <summary>
		/// Encodes an BOOLEAN value.
		/// </summary>
		/// <param name="Value">BOOLEAN value.</param>
		public void BOOLEAN(bool Value)
		{
			this.output.Add(1);
			this.output.Add(1);

			if (Value)
				this.output.Add(0xff);
			else
				this.output.Add(0x00);
		}

		/// <summary>
		/// Encodes an INTEGER value.
		/// </summary>
		/// <param name="Value">INTEGER value.</param>
		public void INTEGER(long Value)
		{
			this.output.Add(2);

			int Pos = this.output.Count;
			int c = 8;
			byte b;

			this.output.Add(0);

			if (Value < 0)
			{
				do
				{
					b = (byte)(Value >> 56);
					Value <<= 8;
					c--;
				}
				while (b == 0xff);

				if (b < 0x80)
					this.output.Add(0xff);

				this.output.Add(b);

				while (c-- > 0)
				{
					b = (byte)(Value >> 56);
					this.output.Add(b);
					Value <<= 8;
				}
			}
			else if (Value == 0)
				this.output.Add(0);
			else
			{
				do
				{
					b = (byte)(Value >> 56);
					Value <<= 8;
					c--;
				}
				while (b == 0);

				if (b >= 0x80)
					this.output.Add(0);

				this.output.Add(b);

				while (c-- > 0)
				{
					b = (byte)(Value >> 56);
					this.output.Add(b);
					Value <<= 8;
				}
			}

			this.output[Pos] = (byte)(this.output.Count - Pos - 1);
		}

		/// <summary>
		/// Encodes an INTEGER value.
		/// </summary>
		/// <param name="Value">INTEGER value.</param>
		/// <param name="Negative">If the value is negative.</param>
		public void INTEGER(byte[] Value, bool Negative)
		{
			this.output.Add(2);

			if (Negative)
			{
				if (Value == null || Value.Length == 0)
					Value = new byte[] { 0xff };
				else if (Value[0] < 0x80)
				{
					int c = Value.Length;
					byte[] Value2 = new byte[c + 1];
					Value2[0] = 0xff;
					Array.Copy(Value, 0, Value2, 1, Value.Length);
					Value = Value2;
				}
			}
			else
			{
				if (Value == null || Value.Length == 0)
					Value = new byte[] { 0 };
				else if (Value[0] >= 0x80)
				{
					int c = Value.Length;
					byte[] Value2 = new byte[c + 1];
					Value2[0] = 0;
					Array.Copy(Value, 0, Value2, 1, Value.Length);
					Value = Value2;
				}
			}

			this.EncodeBinary(Value);
		}

		private void EncodeBinary(byte[] Bin)
		{
			int Len = Bin.Length;

			if (Len >= 0x80)
			{
				if (Len <= 0xff)
					this.output.Add(0x81);
				else
				{
					if (Len <= 0xffff)
						this.output.Add(0x82);
					else
					{
						if (Len <= 0xffffff)
							this.output.Add(0x83);
						else
						{
							this.output.Add(0x84);
							this.output.Add((byte)(Len >> 24));
						}

						this.output.Add((byte)(Len >> 16));
					}

					this.output.Add((byte)(Len >> 8));
				}
			}

			this.output.Add((byte)Len);
			this.output.AddRange(Bin);
		}

		/// <summary>
		/// Encodes an BITSTRING value.
		/// </summary>
		/// <param name="Bits">BITSTRING value.</param>
		public void BITSTRING(BitArray Bits)
		{
			this.output.Add(3);

			int NrBits = Bits.Length;
			int Len = (NrBits + 7) / 8 + 1;
			int NrUnusedBits = 8 - (NrBits & 7);
			byte[] Bin = new byte[Len];
			byte b = 0;
			int i, j = 0;

			if (NrUnusedBits == 8)
				NrUnusedBits = 0;

			Bin[j++] = (byte)NrUnusedBits;

			for (i = 0; i < NrBits; i++)
			{
				b <<= 1;
				if (Bits[i])
					b |= 1;

				if ((i & 7) == 7)
					Bin[j++] = b;
			}

			if (NrUnusedBits > 0)
			{
				b <<= NrUnusedBits;
				Bin[j++] = b;
			}

			this.EncodeBinary(Bin);
		}


		/// <summary>
		/// Encodes an BITSTRING value.
		/// </summary>
		/// <param name="Bytes">Bytes containing BITSTRING value.</param>
		public void BITSTRING(byte[] Bytes)
		{
			this.output.Add(3);

			int c = Bytes.Length;
			byte[] Bytes2 = new byte[c + 1];
			Bytes2[0] = 0;  // Unused bits.
			Array.Copy(Bytes, 0, Bytes2, 1, c);

			this.EncodeBinary(Bytes2);
		}

		/// <summary>
		/// Starts a BITSTRING.
		/// </summary>
		public void StartBITSTRING()
		{
			this.Start(3);
			this.output.Add(0);
		}

		/// <summary>
		/// Ends the current BITSTRING.
		/// </summary>
		public void EndBITSTRING()
		{
			this.End(3);
		}

		/// <summary>
		/// Encodes an OCTET STRING value.
		/// </summary>
		/// <param name="Value">OCTET STRING value.</param>
		public void OCTET_STRING(byte[] Value)
		{
			this.output.Add(4);
			this.EncodeBinary(Value);
		}

		/// <summary>
		/// Starts a OCTET_STRING.
		/// </summary>
		public void StartOCTET_STRING()
		{
			this.Start(4);
		}

		/// <summary>
		/// Ends the current OCTET_STRING.
		/// </summary>
		public void EndOCTET_STRING()
		{
			this.End(4);
		}

		/// <summary>
		/// Encodes an NULL value.
		/// </summary>
		public void NULL()
		{
			this.output.Add(5);
			this.output.Add(0);
		}

		/// <summary>
		/// Encodes an OBJECT IDENTIFIER value.
		/// </summary>
		/// <param name="OID">OBJECT IDENTIFIER value.</param>
		public void OBJECT_IDENTIFIER(string OID)
		{
			string[] s = OID.Split('.');
			int i, c = s.Length;
			uint[] OID2 = new uint[c];

			for (i = 0; i < c; i++)
			{
				if (!uint.TryParse(s[i], out OID2[i]))
					throw new ArgumentException("Invalid object identifier.", nameof(OID));
			}

			this.OBJECT_IDENTIFIER(OID2);
		}

		/// <summary>
		/// Encodes an OBJECT IDENTIFIER value.
		/// </summary>
		/// <param name="OID">OBJECT IDENTIFIER value.</param>
		public void OBJECT_IDENTIFIER(uint[] OID)
		{
			int i, c = OID.Length;
			uint j;

			if (c < 2)
				throw new ArgumentException("Invalid length of OID.", nameof(OID));

			if (OID[1] >= 40)
				throw new ArgumentException("Invalid second integer in OID.", nameof(OID));

			j = OID[0] * 40;
			if (j > 255 || j + OID[1] > 255)
				throw new ArgumentException("Invalid first integer in OID.", nameof(OID));

			j += OID[1];

			List<byte> Bin = new List<byte>()
			{
				(byte)j
			};

			for (i = 2; i < c; i++)
			{
				j = OID[i];

				if (j >= 0x10000000)
					Bin.Add((byte)(128 + (j >> 28)));

				if (j >= 0x200000)
					Bin.Add((byte)(128 + (j >> 21)));

				if (j >= 0x4000)
					Bin.Add((byte)(128 + (j >> 14)));

				if (j >= 0x80)
					Bin.Add((byte)(128 + (j >> 7)));

				Bin.Add((byte)(j & 127));
			}

			this.output.Add(6);
			this.EncodeBinary(Bin.ToArray());
		}

		/// <summary>
		/// Encodes a UNICODE STRING (BMPString) value.
		/// </summary>
		/// <param name="Value">UNICODE STRING (BMPString) value.</param>
		public void UNICODE_STRING(string Value)
		{
			this.output.Add(0x1e);
			this.EncodeBinary(Encoding.BigEndianUnicode.GetBytes(Value));
		}

		/// <summary>
		/// Encodes an IA5 STRING value.
		/// </summary>
		/// <param name="Value">IA5 STRING value.</param>
		public void IA5_STRING(string Value)
		{
			this.output.Add(0x16);
			this.EncodeBinary(Encoding.ASCII.GetBytes(Value));
		}

		/// <summary>
		/// Encodes an PRINTABLE STRING value.
		/// </summary>
		/// <param name="Value">PRINTABLE STRING value.</param>
		public void PRINTABLE_STRING(string Value)
		{
			if (!IsPrintable(Value))
				throw new ArgumentException("Not a printable string.", nameof(Value));

			this.output.Add(0x13);
			this.EncodeBinary(Encoding.ASCII.GetBytes(Value));
		}

		/// <summary>
		/// Checks if a string is a printable string.
		/// </summary>
		/// <param name="Value">Value to check.</param>
		/// <returns>If the string is a printable string or not.</returns>
		public static bool IsPrintable(string Value)
		{
			return PrintableString.IsMatch(Value);
		}

		private static readonly Regex PrintableString = new Regex("^[A-Za-z0-9 '()+,-./:=?]*$", RegexOptions.Singleline | RegexOptions.Compiled);

		/// <summary>
		/// Encodes an UTF-8 STRING value.
		/// </summary>
		/// <param name="Value">UTF-8 STRING value.</param>
		public void UTF8_STRING(string Value)
		{
			this.output.Add(0x0c);
			this.EncodeBinary(Encoding.UTF8.GetBytes(Value));
		}

		/// <summary>
		/// Starts a SEQUENCE.
		/// </summary>
		public void StartSEQUENCE()
		{
			this.Start(0x30);
		}

		private void Start(byte Expected)
		{
			if (this.stack == null)
				this.stack = new LinkedList<KeyValuePair<byte, List<byte>>>();

			this.stack.AddLast(new KeyValuePair<byte, List<byte>>(Expected, this.output));

			this.output = new List<byte>();
		}

		/// <summary>
		/// Ends the current SEQUENCE.
		/// </summary>
		public void EndSEQUENCE()
		{
			this.End(0x30);
		}

		private void End(byte Type)
		{
			if (this.stack == null || this.stack.Last == null)
				throw new Exception("Not properly started.");

			if (Type != this.stack.Last.Value.Key)
				throw new Exception("Start/End type mismatch.");

			byte[] Bin = this.output.ToArray();

			this.output = this.stack.Last.Value.Value;
			this.stack.RemoveLast();

			this.output.Add(Type);
			this.EncodeBinary(Bin);
		}

		/// <summary>
		/// Starts a SET.
		/// </summary>
		public void StartSET()
		{
			this.Start(0x31);
		}

		/// <summary>
		/// Ends the current SET.
		/// </summary>
		public void EndSET()
		{
			this.End(0x31);
		}

		/// <summary>
		/// Encodes content to the output.
		/// </summary>
		/// <param name="Class">Class</param>
		public void Content(Asn1TypeClass Class)
		{
			this.output.Add((byte)((((int)Class) << 6) | 0x20));
			this.output.Add(0);
		}

		/// <summary>
		/// Starts a content section.
		/// </summary>
		/// <param name="Class">Class</param>
		public void StartContent(Asn1TypeClass Class)
		{
			this.Start((byte)((((int)Class) << 6) | 0x20));
		}

		/// <summary>
		/// Ends the current Content section.
		/// </summary>
		/// <param name="Class">Class</param>
		public void EndContent(Asn1TypeClass Class)
		{
			this.End((byte)((((int)Class) << 6) | 0x20));
		}

		/// <summary>
		/// Adds DER-encoded bytes to the output.
		/// </summary>
		/// <param name="DerEncodedBytes">DER encoded bytes.</param>
		public void Raw(byte[] DerEncodedBytes)
		{
			this.output.AddRange(DerEncodedBytes);
		}

		/// <summary>
		/// Current output position.
		/// </summary>
		public int Position
		{
			get { return this.output.Count; }
		}

		/// <summary>
		/// Access to binary output.
		/// </summary>
		/// <param name="Index">Zero-based index into generated output.</param>
		/// <returns>Binary byte at position.</returns>
		public byte this[int Index]
		{
			get { return this.output[Index]; }
			set { this.output[Index] = value; }
		}

	}
}
