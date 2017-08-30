using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Waher.Networking.CoAP;
using Waher.Security.DTLS;

namespace Waher.Networking.LWM2M.ContentFormats
{
	/// <summary>
	/// Class used to serialize data into the Plain Text format. Cannot be used with nesting.
	/// </summary>
	public class TextWriter : ILwm2mWriter
	{
		private StringBuilder sb;

		/// <summary>
		/// Content format of generated payload.
		/// </summary>
		public virtual ushort ContentFormat => Tlv.ContentFormatCode;

		/// <summary>
		/// Class used to serialize data into the Plain Text format. Cannot be used with nesting.
		/// </summary>
		public TextWriter()
		{
			this.sb = new StringBuilder();
		}

		/// <summary>
		/// Writes a TLV.
		/// </summary>
		/// <param name="IdentifierType">Type of identifier.</param>
		/// <param name="Identifier">Identifier.</param>
		/// <param name="Value">Value</param>
		public virtual void Write(IdentifierType IdentifierType, ushort Identifier, byte[] Value)
		{
			this.sb.Append(Convert.ToBase64String(Value));
		}

		/// <summary>
		/// Binary serialization of what has been written.
		/// </summary>
		/// <returns></returns>
		public byte[] ToArray()
		{
			return Encoding.UTF8.GetBytes(this.sb.ToString());
		}

		/// <summary>
		/// Begins a new nested TLV
		/// </summary>
		/// <param name="IdentifierType">Type of identifier.</param>
		/// <param name="Identifier">Identifier.</param>
		public virtual void Begin(IdentifierType IdentifierType, ushort Identifier)
		{
			throw new CoapException(CoapCode.NotAcceptable);
		}

		/// <summary>
		/// Ends a nested TLV
		/// </summary>
		public virtual void End()
		{
			throw new CoapException(CoapCode.NotAcceptable);
		}

		/// <summary>
		/// Writes a string-valued TLV.
		/// </summary>
		/// <param name="IdentifierType">Type of identifier.</param>
		/// <param name="Identifier">Identifier.</param>
		/// <param name="Value">Value</param>
		public void Write(IdentifierType IdentifierType, ushort Identifier, string Value)
		{
			sb.Append(Value);
		}

		/// <summary>
		/// Writes an Integer-valued TLV.
		/// </summary>
		/// <param name="IdentifierType">Type of identifier.</param>
		/// <param name="Identifier">Identifier.</param>
		/// <param name="Value">Value</param>
		public void Write(IdentifierType IdentifierType, ushort Identifier, sbyte Value)
		{
			sb.Append(Value.ToString());
		}

		/// <summary>
		/// Writes an Integer-valued TLV.
		/// </summary>
		/// <param name="IdentifierType">Type of identifier.</param>
		/// <param name="Identifier">Identifier.</param>
		/// <param name="Value">Value</param>
		public void Write(IdentifierType IdentifierType, ushort Identifier, short Value)
		{
			sb.Append(Value.ToString());
		}

		/// <summary>
		/// Writes an Integer-valued TLV.
		/// </summary>
		/// <param name="IdentifierType">Type of identifier.</param>
		/// <param name="Identifier">Identifier.</param>
		/// <param name="Value">Value</param>
		public void Write(IdentifierType IdentifierType, ushort Identifier, int Value)
		{
			sb.Append(Value.ToString());
		}

		/// <summary>
		/// Writes an Integer-valued TLV.
		/// </summary>
		/// <param name="IdentifierType">Type of identifier.</param>
		/// <param name="Identifier">Identifier.</param>
		/// <param name="Value">Value</param>
		public void Write(IdentifierType IdentifierType, ushort Identifier, long Value)
		{
			sb.Append(Value.ToString());
		}

		/// <summary>
		/// Writes a floating point-valued TLV.
		/// </summary>
		/// <param name="IdentifierType">Type of identifier.</param>
		/// <param name="Identifier">Identifier.</param>
		/// <param name="Value">Value</param>
		public void Write(IdentifierType IdentifierType, ushort Identifier, float Value)
		{
			sb.Append(Value.ToString("F"));
		}

		/// <summary>
		/// Writes a floating point-valued TLV.
		/// </summary>
		/// <param name="IdentifierType">Type of identifier.</param>
		/// <param name="Identifier">Identifier.</param>
		/// <param name="Value">Value</param>
		public void Write(IdentifierType IdentifierType, ushort Identifier, double Value)
		{
			sb.Append(Value.ToString("F"));
		}

		/// <summary>
		/// Writes a Boolean valued TLV.
		/// </summary>
		/// <param name="IdentifierType">Type of identifier.</param>
		/// <param name="Identifier">Identifier.</param>
		/// <param name="Value">Value</param>
		public void Write(IdentifierType IdentifierType, ushort Identifier, bool Value)
		{
			if (Value)
				sb.Append('1');
			else
				sb.Append('0');
		}

		/// <summary>
		/// Writes a DateTime-valued TLV.
		/// </summary>
		/// <param name="IdentifierType">Type of identifier.</param>
		/// <param name="Identifier">Identifier.</param>
		/// <param name="Value">Value</param>
		public void Write(IdentifierType IdentifierType, ushort Identifier, DateTime Value)
		{
			int UnixTime = (int)((Value.ToUniversalTime() - DtlsEndpoint.UnixEpoch).TotalSeconds + 0.5);

			sb.Append(UnixTime.ToString());
		}

		/// <summary>
		/// Writes an object link TLV.
		/// </summary>
		/// <param name="IdentifierType">Type of identifier.</param>
		/// <param name="Identifier">Identifier.</param>
		/// <param name="ObjectId">Object ID</param>
		/// <param name="ObjectInstanceId">Object Instance ID</param>
		public void Write(IdentifierType IdentifierType, ushort Identifier, ushort ObjectId,
			ushort ObjectInstanceId)
		{
			sb.Append(ObjectId.ToString() + ":" + ObjectInstanceId.ToString());
		}

		/// <summary>
		/// Writes a none-valued (or void-valued) TLV.
		/// </summary>
		/// <param name="IdentifierType">Type of identifier.</param>
		/// <param name="Identifier">Identifier.</param>
		public void Write(IdentifierType IdentifierType, ushort Identifier)
		{
		}

	}
}
