using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Waher.Networking.CoAP.LWM2M.ContentFormats
{
	/// <summary>
	/// OMA LWM2M TLV (Type-Length-Value)
	/// </summary>
	public class Tlv : ICoapContentFormat
	{
		/// <summary>
		/// 11542
		/// </summary>
		public const int ContentFormatCode = 11542;

		/// <summary>
		/// OMA LWM2M TLV (Type-Length-Value)
		/// </summary>
		public Tlv()
		{
		}

		/// <summary>
		/// Content format number
		/// </summary>
		public int ContentFormat
		{
			get { return ContentFormatCode; }
		}

		/// <summary>
		/// Internet content type.
		/// </summary>
		public string ContentType
		{
			get { return TlvDecoder.ContentType; }
		}
	}
}
