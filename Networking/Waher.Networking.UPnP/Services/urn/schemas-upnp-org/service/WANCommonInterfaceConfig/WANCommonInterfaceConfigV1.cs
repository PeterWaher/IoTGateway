using System;
using System.Collections.Generic;

namespace Waher.Networking.UPnP.Services
{
#pragma warning disable
	public class WANCommonInterfaceConfig
	{
		private ServiceDescriptionDocument service;
		private UPnPAction actionGetCommonLinkProperties = null;
		private UPnPAction actionGetTotalBytesSent = null;
		private UPnPAction actionGetTotalBytesReceived = null;
		private UPnPAction actionGetTotalPacketsSent = null;
		private UPnPAction actionGetTotalPacketsReceived = null;

		public WANCommonInterfaceConfig(ServiceDescriptionDocument Service)
		{
			this.service = Service;
		}

		public void GetCommonLinkProperties(out string NewWANAccessType, out uint NewLayer1UpstreamMaxBitRate, out uint NewLayer1DownstreamMaxBitRate, out string NewPhysicalLinkStatus)
		{
			Dictionary<string, object> OutputValues = new Dictionary<string, object>();
			
			if (actionGetCommonLinkProperties == null)
				actionGetCommonLinkProperties = this.service.GetAction("GetCommonLinkProperties");

			this.actionGetCommonLinkProperties.Invoke(out OutputValues);

			NewWANAccessType = (string)OutputValues["NewWANAccessType"];
			NewLayer1UpstreamMaxBitRate = (uint)OutputValues["NewLayer1UpstreamMaxBitRate"];
			NewLayer1DownstreamMaxBitRate = (uint)OutputValues["NewLayer1DownstreamMaxBitRate"];
			NewPhysicalLinkStatus = (string)OutputValues["NewPhysicalLinkStatus"];
		}

		public void GetTotalBytesSent(out uint NewTotalBytesSent)
		{
			Dictionary<string, object> OutputValues = new Dictionary<string, object>();
			
			if (actionGetTotalBytesSent == null)
				actionGetTotalBytesSent = this.service.GetAction("GetTotalBytesSent");

			this.actionGetTotalBytesSent.Invoke(out OutputValues);

			NewTotalBytesSent = (uint)OutputValues["NewTotalBytesSent"];
		}

		public void GetTotalBytesReceived(out uint NewTotalBytesReceived)
		{
			Dictionary<string, object> OutputValues = new Dictionary<string, object>();
			
			if (actionGetTotalBytesReceived == null)
				actionGetTotalBytesReceived = this.service.GetAction("GetTotalBytesReceived");

			this.actionGetTotalBytesReceived.Invoke(out OutputValues);

			NewTotalBytesReceived = (uint)OutputValues["NewTotalBytesReceived"];
		}

		public void GetTotalPacketsSent(out uint NewTotalPacketsSent)
		{
			Dictionary<string, object> OutputValues = new Dictionary<string, object>();
			
			if (actionGetTotalPacketsSent == null)
				actionGetTotalPacketsSent = this.service.GetAction("GetTotalPacketsSent");

			this.actionGetTotalPacketsSent.Invoke(out OutputValues);

			NewTotalPacketsSent = (uint)OutputValues["NewTotalPacketsSent"];
		}

		public void GetTotalPacketsReceived(out uint NewTotalPacketsReceived)
		{
			Dictionary<string, object> OutputValues = new Dictionary<string, object>();
			
			if (actionGetTotalPacketsReceived == null)
				actionGetTotalPacketsReceived = this.service.GetAction("GetTotalPacketsReceived");

			this.actionGetTotalPacketsReceived.Invoke(out OutputValues);

			NewTotalPacketsReceived = (uint)OutputValues["NewTotalPacketsReceived"];
		}
	}
#pragma warning enable
}