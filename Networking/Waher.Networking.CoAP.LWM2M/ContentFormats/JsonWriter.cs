using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Waher.Security.DTLS;

namespace Waher.Networking.CoAP.LWM2M.ContentFormats
{
	/// <summary>
	/// Class used to serialize data into the SenML JSON format.
	/// </summary>
	public class JsonWriter : ILwm2mWriter
	{
		private StringBuilder sb = new StringBuilder();
		private bool first = true;
		private bool ended = false;

		/// <summary>
		/// Content format of generated payload.
		/// </summary>
		public virtual ushort ContentFormat => Json.ContentFormatCode;

		/// <summary>
		/// Class used to serialize data into the SenML JSON format.
		/// </summary>
		/// <param name="BaseName">Base name.</param>
		public JsonWriter(string BaseName)
		{
			this.sb = new StringBuilder();

			sb.Append("{\"bn\":\"");
			sb.Append(Content.JSON.Encode(BaseName));
			sb.Append("\",\"e\":[");
		}

		private void WriteName(ushort Identifier)
		{
			if (this.ended)
				throw new InvalidOperationException("Output ended.");

			if (this.first)
				this.first = false;
			else
				sb.Append(',');

			sb.Append("{\"n\":\"");
			sb.Append(Identifier.ToString());
			sb.Append("\"");
		}

		/// <summary>
		/// Writes a TLV.
		/// </summary>
		/// <param name="IdentifierType">Type of identifier.</param>
		/// <param name="Identifier">Identifier.</param>
		/// <param name="Value">Value</param>
		public virtual void Write(IdentifierType IdentifierType, ushort Identifier, byte[] Value)
		{
			this.Write(IdentifierType, Identifier, Convert.ToBase64String(Value));
		}

		/// <summary>
		/// Binary serialization of what has been written.
		/// </summary>
		/// <returns></returns>
		public byte[] ToArray()
		{
			if (!this.ended)
			{
				this.sb.Append('}');
				this.ended = true;
			}

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
			this.WriteName(Identifier);

			sb.Append(",\"sv\":\"");
			sb.Append(Value);
			sb.Append("\"}");
		}

		/// <summary>
		/// Writes an Integer-valued TLV.
		/// </summary>
		/// <param name="IdentifierType">Type of identifier.</param>
		/// <param name="Identifier">Identifier.</param>
		/// <param name="Value">Value</param>
		public void Write(IdentifierType IdentifierType, ushort Identifier, sbyte Value)
		{
			this.Write(IdentifierType, Identifier, (long)Value);
		}

		/// <summary>
		/// Writes an Integer-valued TLV.
		/// </summary>
		/// <param name="IdentifierType">Type of identifier.</param>
		/// <param name="Identifier">Identifier.</param>
		/// <param name="Value">Value</param>
		public void Write(IdentifierType IdentifierType, ushort Identifier, short Value)
		{
			this.Write(IdentifierType, Identifier, (long)Value);
		}

		/// <summary>
		/// Writes an Integer-valued TLV.
		/// </summary>
		/// <param name="IdentifierType">Type of identifier.</param>
		/// <param name="Identifier">Identifier.</param>
		/// <param name="Value">Value</param>
		public void Write(IdentifierType IdentifierType, ushort Identifier, int Value)
		{
			this.Write(IdentifierType, Identifier, (long)Value);
		}

		/// <summary>
		/// Writes an Integer-valued TLV.
		/// </summary>
		/// <param name="IdentifierType">Type of identifier.</param>
		/// <param name="Identifier">Identifier.</param>
		/// <param name="Value">Value</param>
		public void Write(IdentifierType IdentifierType, ushort Identifier, long Value)
		{
			this.WriteName(Identifier);

			sb.Append(",\"v\":");
			sb.Append(Value.ToString());
			sb.Append('}');
		}

		/// <summary>
		/// Writes a floating point-valued TLV.
		/// </summary>
		/// <param name="IdentifierType">Type of identifier.</param>
		/// <param name="Identifier">Identifier.</param>
		/// <param name="Value">Value</param>
		public void Write(IdentifierType IdentifierType, ushort Identifier, float Value)
		{
			this.Write(IdentifierType, Identifier, (double)Value);
		}

		/// <summary>
		/// Writes a floating point-valued TLV.
		/// </summary>
		/// <param name="IdentifierType">Type of identifier.</param>
		/// <param name="Identifier">Identifier.</param>
		/// <param name="Value">Value</param>
		public void Write(IdentifierType IdentifierType, ushort Identifier, double Value)
		{
			this.WriteName(Identifier);

			sb.Append(",\"v\":");
			sb.Append(Value.ToString("F"));
			sb.Append('}');
		}

		/// <summary>
		/// Writes a Boolean valued TLV.
		/// </summary>
		/// <param name="IdentifierType">Type of identifier.</param>
		/// <param name="Identifier">Identifier.</param>
		/// <param name="Value">Value</param>
		public void Write(IdentifierType IdentifierType, ushort Identifier, bool Value)
		{
			this.WriteName(Identifier);

			sb.Append(",\"bv\":");
			sb.Append(Value ? "true" : "false");
			sb.Append('}');
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
			this.Write(IdentifierType, Identifier, UnixTime);
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
			this.WriteName(Identifier);

			sb.Append(",\"ov\":\"");
			sb.Append(ObjectId.ToString());
			sb.Append(':');
			sb.Append(ObjectInstanceId.ToString());
			sb.Append("\"}");
		}

		/// <summary>
		/// Writes a none-valued (or void-valued) TLV.
		/// </summary>
		/// <param name="IdentifierType">Type of identifier.</param>
		/// <param name="Identifier">Identifier.</param>
		public void Write(IdentifierType IdentifierType, ushort Identifier)
		{
			this.WriteName(Identifier);
			sb.Append('}');
		}

	}
}
