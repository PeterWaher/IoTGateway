using System;
using System.Collections.Generic;

namespace Waher.Networking.UPnP.Services
{
#pragma warning disable
	public class WANIPConnectionV1
	{
		protected ServiceDescriptionDocument service;
		private UPnPAction actionAddPortMapping = null;
		private UPnPAction actionGetExternalIPAddress = null;
		private UPnPAction actionDeletePortMapping = null;
		private UPnPAction actionSetConnectionType = null;
		private UPnPAction actionGetConnectionTypeInfo = null;
		private UPnPAction actionRequestConnection = null;
		private UPnPAction actionForceTermination = null;
		private UPnPAction actionGetStatusInfo = null;
		private UPnPAction actionGetNATRSIPStatus = null;
		private UPnPAction actionGetGenericPortMappingEntry = null;
		private UPnPAction actionGetSpecificPortMappingEntry = null;

		public WANIPConnectionV1(ServiceDescriptionDocument Service)
		{
			this.service = Service;
		}

		public void AddPortMapping(string NewRemoteHost, ushort NewExternalPort, string NewProtocol, ushort NewInternalPort, string NewInternalClient, bool NewEnabled, string NewPortMappingDescription, uint NewLeaseDuration)
		{
			Dictionary<string, object> OutputValues = new Dictionary<string, object>();
			
			if (actionAddPortMapping == null)
				actionAddPortMapping = this.service.GetAction("AddPortMapping");

			this.actionAddPortMapping.Invoke(out OutputValues,
				new KeyValuePair<string, object>("NewRemoteHost", NewRemoteHost),
				new KeyValuePair<string, object>("NewExternalPort", NewExternalPort),
				new KeyValuePair<string, object>("NewProtocol", NewProtocol),
				new KeyValuePair<string, object>("NewInternalPort", NewInternalPort),
				new KeyValuePair<string, object>("NewInternalClient", NewInternalClient),
				new KeyValuePair<string, object>("NewEnabled", NewEnabled),
				new KeyValuePair<string, object>("NewPortMappingDescription", NewPortMappingDescription),
				new KeyValuePair<string, object>("NewLeaseDuration", NewLeaseDuration));
		}

		public void GetExternalIPAddress(out string NewExternalIPAddress)
		{
			Dictionary<string, object> OutputValues = new Dictionary<string, object>();
			
			if (actionGetExternalIPAddress == null)
				actionGetExternalIPAddress = this.service.GetAction("GetExternalIPAddress");

			this.actionGetExternalIPAddress.Invoke(out OutputValues);

			NewExternalIPAddress = (string)OutputValues["NewExternalIPAddress"];
		}

		public void DeletePortMapping(string NewRemoteHost, ushort NewExternalPort, string NewProtocol)
		{
			Dictionary<string, object> OutputValues = new Dictionary<string, object>();
			
			if (actionDeletePortMapping == null)
				actionDeletePortMapping = this.service.GetAction("DeletePortMapping");

			this.actionDeletePortMapping.Invoke(out OutputValues,
				new KeyValuePair<string, object>("NewRemoteHost", NewRemoteHost),
				new KeyValuePair<string, object>("NewExternalPort", NewExternalPort),
				new KeyValuePair<string, object>("NewProtocol", NewProtocol));
		}

		public void SetConnectionType(string NewConnectionType)
		{
			Dictionary<string, object> OutputValues = new Dictionary<string, object>();
			
			if (actionSetConnectionType == null)
				actionSetConnectionType = this.service.GetAction("SetConnectionType");

			this.actionSetConnectionType.Invoke(out OutputValues,
				new KeyValuePair<string, object>("NewConnectionType", NewConnectionType));
		}

		public void GetConnectionTypeInfo(out string NewConnectionType, out string NewPossibleConnectionTypes)
		{
			Dictionary<string, object> OutputValues = new Dictionary<string, object>();
			
			if (actionGetConnectionTypeInfo == null)
				actionGetConnectionTypeInfo = this.service.GetAction("GetConnectionTypeInfo");

			this.actionGetConnectionTypeInfo.Invoke(out OutputValues);

			NewConnectionType = (string)OutputValues["NewConnectionType"];
			NewPossibleConnectionTypes = (string)OutputValues["NewPossibleConnectionTypes"];
		}

		public void RequestConnection()
		{
			Dictionary<string, object> OutputValues = new Dictionary<string, object>();
			
			if (actionRequestConnection == null)
				actionRequestConnection = this.service.GetAction("RequestConnection");

			this.actionRequestConnection.Invoke(out OutputValues);
		}

		public void ForceTermination()
		{
			Dictionary<string, object> OutputValues = new Dictionary<string, object>();
			
			if (actionForceTermination == null)
				actionForceTermination = this.service.GetAction("ForceTermination");

			this.actionForceTermination.Invoke(out OutputValues);
		}

		public void GetStatusInfo(out string NewConnectionStatus, out string NewLastConnectionError, out uint NewUptime)
		{
			Dictionary<string, object> OutputValues = new Dictionary<string, object>();
			
			if (actionGetStatusInfo == null)
				actionGetStatusInfo = this.service.GetAction("GetStatusInfo");

			this.actionGetStatusInfo.Invoke(out OutputValues);

			NewConnectionStatus = (string)OutputValues["NewConnectionStatus"];
			NewLastConnectionError = (string)OutputValues["NewLastConnectionError"];
			NewUptime = (uint)OutputValues["NewUptime"];
		}

		public void GetNATRSIPStatus(out bool NewRSIPAvailable, out bool NewNATEnabled)
		{
			Dictionary<string, object> OutputValues = new Dictionary<string, object>();
			
			if (actionGetNATRSIPStatus == null)
				actionGetNATRSIPStatus = this.service.GetAction("GetNATRSIPStatus");

			this.actionGetNATRSIPStatus.Invoke(out OutputValues);

			NewRSIPAvailable = (bool)OutputValues["NewRSIPAvailable"];
			NewNATEnabled = (bool)OutputValues["NewNATEnabled"];
		}

		public void GetGenericPortMappingEntry(ushort NewPortMappingIndex, out string NewRemoteHost, out ushort NewExternalPort, out string NewProtocol, out ushort NewInternalPort, out string NewInternalClient, out bool NewEnabled, out string NewPortMappingDescription, out uint NewLeaseDuration)
		{
			Dictionary<string, object> OutputValues = new Dictionary<string, object>();
			
			if (actionGetGenericPortMappingEntry == null)
				actionGetGenericPortMappingEntry = this.service.GetAction("GetGenericPortMappingEntry");

			this.actionGetGenericPortMappingEntry.Invoke(out OutputValues,
				new KeyValuePair<string, object>("NewPortMappingIndex", NewPortMappingIndex));

			NewRemoteHost = (string)OutputValues["NewRemoteHost"];
			NewExternalPort = (ushort)OutputValues["NewExternalPort"];
			NewProtocol = (string)OutputValues["NewProtocol"];
			NewInternalPort = (ushort)OutputValues["NewInternalPort"];
			NewInternalClient = (string)OutputValues["NewInternalClient"];
			NewEnabled = (bool)OutputValues["NewEnabled"];
			NewPortMappingDescription = (string)OutputValues["NewPortMappingDescription"];
			NewLeaseDuration = (uint)OutputValues["NewLeaseDuration"];
		}

		public void GetSpecificPortMappingEntry(string NewRemoteHost, ushort NewExternalPort, string NewProtocol, out ushort NewInternalPort, out string NewInternalClient, out bool NewEnabled, out string NewPortMappingDescription, out uint NewLeaseDuration)
		{
			Dictionary<string, object> OutputValues = new Dictionary<string, object>();
			
			if (actionGetSpecificPortMappingEntry == null)
				actionGetSpecificPortMappingEntry = this.service.GetAction("GetSpecificPortMappingEntry");

			this.actionGetSpecificPortMappingEntry.Invoke(out OutputValues,
				new KeyValuePair<string, object>("NewRemoteHost", NewRemoteHost),
				new KeyValuePair<string, object>("NewExternalPort", NewExternalPort),
				new KeyValuePair<string, object>("NewProtocol", NewProtocol));

			NewInternalPort = (ushort)OutputValues["NewInternalPort"];
			NewInternalClient = (string)OutputValues["NewInternalClient"];
			NewEnabled = (bool)OutputValues["NewEnabled"];
			NewPortMappingDescription = (string)OutputValues["NewPortMappingDescription"];
			NewLeaseDuration = (uint)OutputValues["NewLeaseDuration"];
		}
	}
#pragma warning enable
}