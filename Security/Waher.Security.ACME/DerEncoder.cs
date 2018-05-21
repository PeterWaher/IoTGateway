using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace Waher.Security.ACME
{
	/// <summary>
	/// Encodes data using the Distinguished Encoding Rules (DER), as defined in X.690
	/// </summary>
	public class DerEncoder
	{
		private List<byte> output = new List<byte>();
		private LinkedList<List<byte>> stack = null;

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
			this.EncodeInteger(Value);
		}

		private void EncodeInteger(long i)
		{
			int Pos = this.output.Count;
			int c = 8;
			byte b;

			this.output.Add(0);

			if (i < 0)
			{
				do
				{
					b = (byte)(i >> 56);
					i <<= 8;
					c--;
				}
				while (b == 0xff);

				if (b < 0x80)
					this.output.Add(0xff);

				this.output.Add(b);

				while (c-- > 0)
				{
					b = (byte)(i >> 56);
					this.output.Add(b);
					i <<= 8;
				}
			}
			else if (i == 0)
				this.output.Add(0);
			else
			{
				do
				{
					b = (byte)(i >> 56);
					i <<= 8;
					c--;
				}
				while (b == 0);

				if (b >= 0x80)
					this.output.Add(0);

				this.output.Add(b);

				while (c-- > 0)
				{
					b = (byte)(i >> 56);
					this.output.Add(b);
					i <<= 8;
				}
			}

			this.output[Pos] = (byte)(this.output.Count - Pos - 1);
		}

		/// <summary>
		/// Encodes an INTEGER value.
		/// </summary>
		/// <param name="Value">INTEGER value.</param>
		public void INTEGER(byte[] Value)
		{
			this.output.Add(2);
			this.EncodeBinary(Value);
		}

		private void EncodeBinary(byte[] Bin)
		{
			int Len = Bin.Length;

			if (Len <= 127)
			{
				this.output.Add((byte)Len);
				this.output.AddRange(Bin);
			}
			else
			{
				int Pos = this.output.Count;
				this.EncodeInteger(Len);
				this.output[Pos] |= 0x80;
			}
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
		/// Encodes an OCTET STRING value.
		/// </summary>
		/// <param name="Value">OCTET STRING value.</param>
		public void OCTET_STRING(byte[] Value)
		{
			this.output.Add(4);
			this.EncodeBinary(Value);
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

				if (j < 0x80)
					Bin.Add((byte)j);
				else if (j < 0x4000)
				{
					Bin.Add((byte)(128 + (j >> 7)));
					Bin.Add((byte)(j & 127));
				}
				else if (j < 0x200000)
				{
					Bin.Add((byte)(128 + (j >> 14)));
					Bin.Add((byte)(128 + ((j >> 7) & 127)));
					Bin.Add((byte)(j & 127));
				}
				else if (j < 0x10000000)
				{
					Bin.Add((byte)(128 + (j >> 21)));
					Bin.Add((byte)(128 + ((j >> 14) & 127)));
					Bin.Add((byte)(128 + ((j >> 7) & 127)));
					Bin.Add((byte)(j & 127));
				}
				else
				{
					Bin.Add((byte)(128 + (j >> 28)));
					Bin.Add((byte)(128 + ((j >> 21) & 127)));
					Bin.Add((byte)(128 + ((j >> 14) & 127)));
					Bin.Add((byte)(128 + ((j >> 7) & 127)));
					Bin.Add((byte)(j & 127));
				}
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
			if (!PrintableString.IsMatch(Value))
				throw new ArgumentException("Not printable string.", nameof(Value));

			this.output.Add(0x13);
			this.EncodeBinary(Encoding.ASCII.GetBytes(Value));
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
			this.Start();
		}

		private void Start()
		{
			if (this.stack == null)
				this.stack = new LinkedList<List<byte>>();

			this.stack.AddLast(this.output);

			this.output = new List<byte>();
		}

		/// <summary>
		/// Starts the current SEQUENCE.
		/// </summary>
		public void EndSEQUENCE()
		{
			this.End(0x30);
		}

		private void End(byte Type)
		{
			if (this.stack == null || this.stack.First == null)
				throw new Exception("Not properly started.");

			byte[] Bin = this.output.ToArray();

			this.output = this.stack.First.Value;
			this.stack.RemoveFirst();

			this.output.Add(Type);
			this.EncodeBinary(Bin);
		}

		/// <summary>
		/// Starts a SET.
		/// </summary>
		public void StartSET()
		{
			this.Start();
		}

		/// <summary>
		/// Starts the current SET.
		/// </summary>
		public void EndSET()
		{
			this.End(0x31);
		}

	}
}
