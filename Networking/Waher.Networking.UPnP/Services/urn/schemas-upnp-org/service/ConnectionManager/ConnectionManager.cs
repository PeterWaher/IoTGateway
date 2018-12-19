using System;
using System.Collections.Generic;

namespace Waher.Networking.UPnP.Services
{
//#pragma warning disable
	/// <summary>
	/// Generated from SCPD
	/// </summary>
	public class ConnectionManager
	{
		private ServiceDescriptionDocument service;
		private UPnPAction actionGetProtocolInfo = null;
		private UPnPAction actionGetCurrentConnectionIDs = null;
		private UPnPAction actionGetCurrentConnectionInfo = null;

		/// <summary>
		/// Generated from SCPD
		/// </summary>
		public ConnectionManager(ServiceDescriptionDocument Service)
		{
			this.service = Service;
		}

		/// <summary>
		/// Generated from SCPD
		/// </summary>
		public void GetProtocolInfo(out string Source, out string Sink)
		{
			Dictionary<string, object> OutputValues = new Dictionary<string, object>();
			
			if (actionGetProtocolInfo is null)
				actionGetProtocolInfo = this.service.GetAction("GetProtocolInfo");

			this.actionGetProtocolInfo.Invoke(out OutputValues);

			Source = (string)OutputValues["Source"];
			Sink = (string)OutputValues["Sink"];
		}

		/// <summary>
		/// Generated from SCPD
		/// </summary>
		public void GetCurrentConnectionIDs(out string ConnectionIDs)
		{
			Dictionary<string, object> OutputValues = new Dictionary<string, object>();
			
			if (actionGetCurrentConnectionIDs is null)
				actionGetCurrentConnectionIDs = this.service.GetAction("GetCurrentConnectionIDs");

			this.actionGetCurrentConnectionIDs.Invoke(out OutputValues);

			ConnectionIDs = (string)OutputValues["ConnectionIDs"];
		}

		/// <summary>
		/// Generated from SCPD
		/// </summary>
		public void GetCurrentConnectionInfo(int ConnectionID, out int RcsID, out int AVTransportID, out string ProtocolInfo, out string PeerConnectionManager, out int PeerConnectionID, out string Direction, out string Status)
		{
			Dictionary<string, object> OutputValues = new Dictionary<string, object>();
			
			if (actionGetCurrentConnectionInfo is null)
				actionGetCurrentConnectionInfo = this.service.GetAction("GetCurrentConnectionInfo");

			this.actionGetCurrentConnectionInfo.Invoke(out OutputValues,
				new KeyValuePair<string, object>("ConnectionID", ConnectionID));

			RcsID = (int)OutputValues["RcsID"];
			AVTransportID = (int)OutputValues["AVTransportID"];
			ProtocolInfo = (string)OutputValues["ProtocolInfo"];
			PeerConnectionManager = (string)OutputValues["PeerConnectionManager"];
			PeerConnectionID = (int)OutputValues["PeerConnectionID"];
			Direction = (string)OutputValues["Direction"];
			Status = (string)OutputValues["Status"];
		}
	}
//#pragma warning restore
}