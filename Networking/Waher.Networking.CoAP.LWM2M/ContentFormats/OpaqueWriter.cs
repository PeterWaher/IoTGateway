using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Waher.Security.DTLS;

namespace Waher.Networking.CoAP.LWM2M.ContentFormats
{
	/// <summary>
	/// Class used to serialize data in binary format. Cannot be used with nesting.
	/// </summary>
	public class OpaqueWriter : TlvWriter
	{
		private MemoryStream ms;

		/// <summary>
		/// Content format of generated payload.
		/// </summary>
		public override ushort ContentFormat => CoAP.ContentFormats.Binary.ContentFormatCode;

		/// <summary>
		/// Class used to serialize data into the TLV (Type-Length-Value) binary format.
		/// </summary>
		public OpaqueWriter()
		{
			this.ms = new MemoryStream();
		}

		/// <summary>
		/// Writes a TLV.
		/// </summary>
		/// <param name="IdentifierType">Type of identifier.</param>
		/// <param name="Identifier">Identifier.</param>
		/// <param name="Value">Value</param>
		public override void Write(IdentifierType IdentifierType, ushort Identifier, byte[] Value)
		{
			this.ms.Write(Value, 0, Value.Length);
		}

		/// <summary>
		/// Begins a new nested TLV
		/// </summary>
		/// <param name="IdentifierType">Type of identifier.</param>
		/// <param name="Identifier">Identifier.</param>
		public override void Begin(IdentifierType IdentifierType, ushort Identifier)
		{
			throw new CoapException(CoapCode.NotAcceptable);
		}

		/// <summary>
		/// Ends a nested TLV
		/// </summary>
		public override void End()
		{
			throw new CoapException(CoapCode.NotAcceptable);
		}

	}
}
