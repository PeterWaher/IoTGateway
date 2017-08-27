using System;
using System.Collections.Generic;
using System.Text;
using Waher.Security.DTLS;

namespace Waher.Networking.CoAP.LWM2M.ContentFormats
{
	/// <summary>
	/// Data representing one TLV record.
	/// </summary>
	public class TlvRecord
	{
		private IdentifierType type;
		private ushort identifier;
		private byte[] rawValue;

		/// <summary>
		/// Data representing one TLV record.
		/// </summary>
		/// <param name="Type">Identifier type.</param>
		/// <param name="Identifier">Identifier.</param>
		/// <param name="RawValue">Raw value.</param>
		public TlvRecord(IdentifierType Type, ushort Identifier, byte[] RawValue)
		{
			this.type = Type;
			this.identifier = Identifier;
			this.rawValue = RawValue;
		}

		/// <summary>
		/// Identifier type.
		/// </summary>
		public IdentifierType Type
		{
			get { return this.type; }
		}

		/// <summary>
		/// Identifier.
		/// </summary>
		public ushort Identifier
		{
			get { return this.identifier; }
		}

		/// <summary>
		/// Raw value.
		/// </summary>
		public byte[] RawValue
		{
			get { return this.rawValue; }
		}

		/// <summary>
		/// If the record is nested.
		/// </summary>
		public bool IsNested
		{
			get
			{
				return this.type == IdentifierType.MultipleResource ||
					this.type == IdentifierType.ObjectInstance;
			}
		}

		/// <summary>
		/// Returns the value of the record, as a nested set of records.
		/// </summary>
		/// <returns>Set of records.</returns>
		/// <exception cref="InvalidOperationException">If record is not nested.</exception>
		public TlvRecord[] AsNested()
		{
			if (!this.IsNested)
				throw new InvalidOperationException("Record is not nested.");

			TlvReader Reader = new TlvReader(this.rawValue);
			List<TlvRecord> Records = new List<TlvRecord>();

			while (!Reader.EOF)
				Records.Add(Reader.ReadRecord());

			return Records.ToArray();
		}

		/// <summary>
		/// If the value can be parsed as a string.
		/// </summary>
		public bool IsString
		{
			get { return true; }
		}

		/// <summary>
		/// Returns the value of the record, as a string.
		/// </summary>
		/// <returns>Decoded value.</returns>
		public string AsString()
		{
			return Encoding.UTF8.GetString(this.rawValue);
		}

		/// <summary>
		/// If the value can be parsed as a 8-bit signed integer.
		/// </summary>
		public bool IsInt8
		{
			get { return this.rawValue.Length == 1; }
		}

		/// <summary>
		/// Returns the value of the record, as a signed byte.
		/// </summary>
		/// <returns>Decoded value.</returns>
		/// <exception cref="InvalidOperationException">If record is not of correct type.</exception>
		public sbyte AsInt8()
		{
			if (this.rawValue.Length != 1)
				throw new InvalidOperationException("Value is not a byte.");

			return (sbyte)this.rawValue[0];
		}

		/// <summary>
		/// If the value can be parsed as a 16-bit signed integer.
		/// </summary>
		public bool IsInt16
		{
			get { return this.rawValue.Length == 2; }
		}

		/// <summary>
		/// Returns the value of the record, as a signed 16-bit integer.
		/// </summary>
		/// <returns>Decoded value.</returns>
		/// <exception cref="InvalidOperationException">If record is not of correct type.</exception>
		public short AsInt16()
		{
			if (this.rawValue.Length != 2)
				throw new InvalidOperationException("Value is not a signed 16-bit integer.");

			ushort Result = this.rawValue[0];
			Result <<= 8;
			Result |= this.rawValue[1];

			return (short)Result;
		}

		/// <summary>
		/// If the value can be parsed as a 32-bit signed integer.
		/// </summary>
		public bool IsInt32
		{
			get { return this.rawValue.Length == 4; }
		}

		/// <summary>
		/// Returns the value of the record, as a signed 32-bit integer.
		/// </summary>
		/// <returns>Decoded value.</returns>
		/// <exception cref="InvalidOperationException">If record is not of correct type.</exception>
		public int AsInt32()
		{
			if (this.rawValue.Length != 4)
				throw new InvalidOperationException("Value is not a signed 32-bit integer.");

			int Result = 0;
			int i;

			for (i = 0; i < 4; i++)
			{
				Result <<= 8;
				Result |= this.rawValue[i];
			}

			return Result;
		}

		/// <summary>
		/// If the value can be parsed as a 64-bit signed integer.
		/// </summary>
		public bool IsInt64
		{
			get { return this.rawValue.Length == 8; }
		}

		/// <summary>
		/// Returns the value of the record, as a signed 64-bit integer.
		/// </summary>
		/// <returns>Decoded value.</returns>
		/// <exception cref="InvalidOperationException">If record is not of correct type.</exception>
		public long AsInt64()
		{
			if (this.rawValue.Length != 8)
				throw new InvalidOperationException("Value is not a signed 64-bit integer.");

			long Result = 0;
			int i;

			for (i = 0; i < 8; i++)
			{
				Result <<= 8;
				Result |= this.rawValue[i];
			}

			return Result;
		}

		/// <summary>
		/// If the value can be parsed as a singe precision floating point number.
		/// </summary>
		public bool IsSingle
		{
			get { return this.rawValue.Length == 4; }
		}

		/// <summary>
		/// Returns the value of the record, as a single precision floating point number.
		/// </summary>
		/// <returns>Decoded value.</returns>
		/// <exception cref="InvalidOperationException">If record is not of correct type.</exception>
		public float AsSingle()
		{
			if (this.rawValue.Length != 4)
				throw new InvalidOperationException("Value is not a single precision floating point number.");

			byte[] Bin = this.rawValue;
			if (BitConverter.IsLittleEndian)
			{
				Bin = new byte[4];
				Array.Copy(this.rawValue, 0, Bin, 0, 4);
				Array.Reverse(Bin);
			}
			else
				Bin = this.rawValue;

			return BitConverter.ToSingle(Bin, 0);
		}

		/// <summary>
		/// If the value can be parsed as a double precision floating point number.
		/// </summary>
		public bool IsDouble
		{
			get { return this.rawValue.Length == 8; }
		}

		/// <summary>
		/// Returns the value of the record, as a double precision floating point number.
		/// </summary>
		/// <returns>Decoded value.</returns>
		/// <exception cref="InvalidOperationException">If record is not of correct type.</exception>
		public double AsDouble()
		{
			if (this.rawValue.Length != 8)
				throw new InvalidOperationException("Value is not a double precision floating point number.");

			byte[] Bin = this.rawValue;
			if (BitConverter.IsLittleEndian)
			{
				Bin = new byte[8];
				Array.Copy(this.rawValue, 0, Bin, 0, 8);
				Array.Reverse(Bin);
			}
			else
				Bin = this.rawValue;

			return BitConverter.ToDouble(Bin, 0);
		}

		/// <summary>
		/// If the value can be parsed as a boolean value.
		/// </summary>
		public bool IsBoolean
		{
			get { return this.rawValue.Length == 1; }
		}

		/// <summary>
		/// Returns the value of the record, as a boolean value.
		/// </summary>
		/// <returns>Decoded value.</returns>
		/// <exception cref="InvalidOperationException">If record is not of correct type.</exception>
		public bool AsBoolean()
		{
			if (this.rawValue.Length != 1)
				throw new InvalidOperationException("Value is not a boolean value.");

			return this.rawValue[0] != 0;
		}

		/// <summary>
		/// If the value can be parsed as a <see cref="DateTime"/> value.
		/// </summary>
		public bool IsDateTime
		{
			get { return this.rawValue.Length == 4; }
		}

		/// <summary>
		/// Returns the value of the record, as a <see cref="DateTime"/>.
		/// </summary>
		/// <returns>Decoded value.</returns>
		/// <exception cref="InvalidOperationException">If record is not of correct type.</exception>
		public DateTime AsDateTime()
		{
			return DtlsEndpoint.UnixEpoch.AddSeconds(this.AsInt32());
		}

		/// <summary>
		/// If the value can be parsed as an object link.
		/// </summary>
		public bool IsObjectLink
		{
			get { return this.rawValue.Length == 4; }
		}

		/// <summary>
		/// Returns the value of the record, as an object link.
		/// </summary>
		/// <returns>Decoded value.</returns>
		/// <exception cref="InvalidOperationException">If record is not of correct type.</exception>
		public KeyValuePair<ushort, ushort> AsObjectLink()
		{
			if (this.rawValue.Length != 4)
				throw new InvalidOperationException("Value is not an object link value.");

			ushort ObjectId = this.rawValue[0];
			ObjectId <<= 8;
			ObjectId |= this.rawValue[1];

			ushort ObjectInstanceId = this.rawValue[0];
			ObjectInstanceId <<= 8;
			ObjectInstanceId |= this.rawValue[1];

			return new KeyValuePair<ushort, ushort>(ObjectId, ObjectInstanceId);
		}

		/// <summary>
		/// If the value contains a none (void) value.
		/// </summary>
		public bool IsNone
		{
			get { return this.rawValue.Length == 0; }
		}

		/// <summary>
		/// If the value can be parsed as an integer value (8, 16, 32 or 64 bit).
		/// </summary>
		public bool IsInteger
		{
			get
			{
				switch (this.rawValue.Length)
				{
					case 1: return true;
					case 2: return true;
					case 4: return true;
					case 8: return true;
					default: return false;
				}
			}
		}

		/// <summary>
		/// Returns the value of the record, as an integer value (8, 16, 32 or 64 bit).
		/// </summary>
		/// <returns>Decoded value.</returns>
		/// <exception cref="InvalidOperationException">If record is not of correct type.</exception>
		public long AsInteger()
		{
			switch (this.rawValue.Length)
			{
				case 1:return this.AsInt8();
				case 2:return this.AsInt16();
				case 4:return this.AsInt32();
				case 8:return this.AsInt64();
				default: throw new InvalidOperationException("Value is not an integer value.");
			}
		}

		/// <summary>
		/// <see cref="Object.ToString()"/>
		/// </summary>
		public override string ToString()
		{
			StringBuilder sb = new StringBuilder();

			sb.Append(this.type.ToString());
			sb.Append(", ");
			sb.Append(this.identifier.ToString());
			sb.Append(", ");
			sb.Append(Waher.Security.Hashes.BinaryToString(this.rawValue));

			return sb.ToString();
		}

	}
}
