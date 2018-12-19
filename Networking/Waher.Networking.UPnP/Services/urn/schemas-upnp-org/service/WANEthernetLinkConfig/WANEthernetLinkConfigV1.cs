using System;
using System.Collections.Generic;

namespace Waher.Networking.UPnP.Services
{
//#pragma warning disable
	/// <summary>
	/// Generated from SCPD
	/// </summary>
	public class WANEthernetLinkConfigV1
	{
		private ServiceDescriptionDocument service;
		private UPnPAction actionGetEthernetLinkStatus = null;

		/// <summary>
		/// Generated from SCPD
		/// </summary>
		public WANEthernetLinkConfigV1(ServiceDescriptionDocument Service)
		{
			this.service = Service;
		}

		/// <summary>
		/// Generated from SCPD
		/// </summary>
		public void GetEthernetLinkStatus(out string NewEthernetLinkStatus)
		{
			Dictionary<string, object> OutputValues = new Dictionary<string, object>();
			
			if (actionGetEthernetLinkStatus is null)
				actionGetEthernetLinkStatus = this.service.GetAction("GetEthernetLinkStatus");

			this.actionGetEthernetLinkStatus.Invoke(out OutputValues);

			NewEthernetLinkStatus = (string)OutputValues["NewEthernetLinkStatus"];
		}
	}
//#pragma warning restore
}