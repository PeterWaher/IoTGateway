using System;
using System.Collections.Generic;

namespace Waher.Networking.UPnP.Services
{
#pragma warning disable
	public class WANEthernetLinkConfigV1
	{
		private ServiceDescriptionDocument service;
		private UPnPAction actionGetEthernetLinkStatus = null;

		public WANEthernetLinkConfigV1(ServiceDescriptionDocument Service)
		{
			this.service = Service;
		}

		public void GetEthernetLinkStatus(out string NewEthernetLinkStatus)
		{
			Dictionary<string, object> OutputValues = new Dictionary<string, object>();
			
			if (actionGetEthernetLinkStatus == null)
				actionGetEthernetLinkStatus = this.service.GetAction("GetEthernetLinkStatus");

			this.actionGetEthernetLinkStatus.Invoke(out OutputValues);

			NewEthernetLinkStatus = (string)OutputValues["NewEthernetLinkStatus"];
		}
	}
#pragma warning enable
}