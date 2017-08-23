using System;

namespace Waher.Networking.CoAP.Options
{
	/// <summary>
	/// ETag option
	/// 
	/// Defined in RFC 7252 §5.10.6: 
	/// https://tools.ietf.org/html/rfc7252#page-56
	/// </summary>
	public class CoapOptionETag : CoapOptionOpaque
	{
		/// <summary>
		/// ETag option
		/// </summary>
		public CoapOptionETag()
			: base()
		{
		}

		/// <summary>
		/// ETag option
		/// </summary>
		/// <param name="Value">Opaque value.</param>
		public CoapOptionETag(byte[] Value)
			: base(Value)
		{
		}

		/// <summary>
		/// If the option is critical or not. Messages containing critical options that are not processed, must be discarded.
		/// </summary>
		public override bool Critical
		{
			get { return false; }
		}

		/// <summary>
		/// Option number.
		/// </summary>
		public override int OptionNumber
		{
			get { return 4; }
		}

		/// <summary>
		/// Creates a new CoAP option.
		/// </summary>
		/// <param name="Value">Option value.</param>
		/// <returns>Newly created CoAP option.</returns>
		public override CoapOption Create(byte[] Value)
		{
			return new CoapOptionETag(Value);
		}

	}
}
