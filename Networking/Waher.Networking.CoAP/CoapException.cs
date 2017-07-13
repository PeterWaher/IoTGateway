using System;
using System.Collections.Generic;
using System.Text;

namespace Waher.Networking.CoAP
{
	/// <summary>
	/// CoAP Exception
	/// </summary>
    public class CoapException : Exception
    {
		private CoapCode errorCode;

		/// <summary>
		/// CoAP Exception
		/// </summary>
		/// <param name="ErrorCode">Error code.</param>
		public CoapException(CoapCode ErrorCode)
			: base("CoAP Exception: " + ErrorCode.ToString())
		{
			this.errorCode = ErrorCode;
		}

		/// <summary>
		/// CoAP error code.
		/// </summary>
		public CoapCode ErrorCode
		{
			get { return this.errorCode; }
		}
    }
}
