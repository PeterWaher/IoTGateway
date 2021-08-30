using System;
using System.Collections.Generic;
using System.Text;

namespace Waher.Networking.UPnP
{
	/// <summary>
	/// UPnP Exception
	/// </summary>
	public class UPnPException : Exception
	{
		private readonly string faultCode;
		private readonly string faultString;
		private readonly string upnpErrorCode;
		private readonly string upnpErrorDescription;

		internal UPnPException(string FaultCode, string FaultString, string UPnPErrorCode, string UPnPErrorDescription)
			: base(string.IsNullOrEmpty(UPnPErrorDescription) ? FaultString : UPnPErrorDescription)
		{
			this.faultCode = FaultCode;
			this.faultString = FaultString;
			this.upnpErrorCode = UPnPErrorCode;
			this.upnpErrorDescription = UPnPErrorDescription;
		}

		/// <summary>
		/// SOAP Fault Code
		/// </summary>
		public string FaultCode { get { return this.faultCode; } }

		/// <summary>
		/// SOAP Fault String
		/// </summary>
		public string FaultString { get { return this.faultString; } }

		/// <summary>
		/// UPnP Error Code
		/// </summary>
		public string UPnPErrorCode { get { return this.upnpErrorCode; } }

		/// <summary>
		/// UPnP Error Description
		/// </summary>
		public string UPnPErrorDescription { get { return this.upnpErrorDescription; } }
	}
}
