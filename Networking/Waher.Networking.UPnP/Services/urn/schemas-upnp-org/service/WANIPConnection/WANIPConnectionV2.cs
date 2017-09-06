using System;
using System.Collections.Generic;

namespace Waher.Networking.UPnP.Services
{
//#pragma warning disable
	/// <summary>
	/// Generated from SCPD
	/// </summary>
	public class WANIPConnectionV2 : WANIPConnectionV1
	{
		private UPnPAction actionRequestTermination = null;
		private UPnPAction actionSetAutoDisconnectTime = null;
		private UPnPAction actionSetIdleDisconnectTime = null;
		private UPnPAction actionSetWarnDisconnectDelay = null;
		private UPnPAction actionGetAutoDisconnectTime = null;
		private UPnPAction actionGetIdleDisconnectTime = null;
		private UPnPAction actionGetWarnDisconnectDelay = null;
		private UPnPAction actionDeletePortMappingRange = null;
		private UPnPAction actionGetListOfPortMappings = null;
		private UPnPAction actionAddAnyPortMapping = null;

		/// <summary>
		/// Generated from SCPD
		/// </summary>
		public WANIPConnectionV2(ServiceDescriptionDocument Service)
			: base(Service)
		{
			this.service = Service;
		}

		/// <summary>
		/// Generated from SCPD
		/// </summary>
		public void RequestTermination()
		{
			Dictionary<string, object> OutputValues = new Dictionary<string, object>();
			
			if (actionRequestTermination == null)
				actionRequestTermination = this.service.GetAction("RequestTermination");

			this.actionRequestTermination.Invoke(out OutputValues);
		}

		/// <summary>
		/// Generated from SCPD
		/// </summary>
		public void SetAutoDisconnectTime(string NewAutoDisconnectTime)
		{
			Dictionary<string, object> OutputValues = new Dictionary<string, object>();
			
			if (actionSetAutoDisconnectTime == null)
				actionSetAutoDisconnectTime = this.service.GetAction("SetAutoDisconnectTime");

			this.actionSetAutoDisconnectTime.Invoke(out OutputValues,
				new KeyValuePair<string, object>("NewAutoDisconnectTime", NewAutoDisconnectTime));
		}

		/// <summary>
		/// Generated from SCPD
		/// </summary>
		public void SetIdleDisconnectTime(string NewIdleDisconnectTime)
		{
			Dictionary<string, object> OutputValues = new Dictionary<string, object>();
			
			if (actionSetIdleDisconnectTime == null)
				actionSetIdleDisconnectTime = this.service.GetAction("SetIdleDisconnectTime");

			this.actionSetIdleDisconnectTime.Invoke(out OutputValues,
				new KeyValuePair<string, object>("NewIdleDisconnectTime", NewIdleDisconnectTime));
		}

		/// <summary>
		/// Generated from SCPD
		/// </summary>
		public void SetWarnDisconnectDelay(string NewWarnDisconnectDelay)
		{
			Dictionary<string, object> OutputValues = new Dictionary<string, object>();
			
			if (actionSetWarnDisconnectDelay == null)
				actionSetWarnDisconnectDelay = this.service.GetAction("SetWarnDisconnectDelay");

			this.actionSetWarnDisconnectDelay.Invoke(out OutputValues,
				new KeyValuePair<string, object>("NewWarnDisconnectDelay", NewWarnDisconnectDelay));
		}

		/// <summary>
		/// Generated from SCPD
		/// </summary>
		public void GetAutoDisconnectTime(out string NewAutoDisconnectTime)
		{
			Dictionary<string, object> OutputValues = new Dictionary<string, object>();
			
			if (actionGetAutoDisconnectTime == null)
				actionGetAutoDisconnectTime = this.service.GetAction("GetAutoDisconnectTime");

			this.actionGetAutoDisconnectTime.Invoke(out OutputValues);

			NewAutoDisconnectTime = (string)OutputValues["NewAutoDisconnectTime"];
		}

		/// <summary>
		/// Generated from SCPD
		/// </summary>
		public void GetIdleDisconnectTime(out string NewIdleDisconnectTime)
		{
			Dictionary<string, object> OutputValues = new Dictionary<string, object>();
			
			if (actionGetIdleDisconnectTime == null)
				actionGetIdleDisconnectTime = this.service.GetAction("GetIdleDisconnectTime");

			this.actionGetIdleDisconnectTime.Invoke(out OutputValues);

			NewIdleDisconnectTime = (string)OutputValues["NewIdleDisconnectTime"];
		}

		/// <summary>
		/// Generated from SCPD
		/// </summary>
		public void GetWarnDisconnectDelay(out string NewWarnDisconnectDelay)
		{
			Dictionary<string, object> OutputValues = new Dictionary<string, object>();
			
			if (actionGetWarnDisconnectDelay == null)
				actionGetWarnDisconnectDelay = this.service.GetAction("GetWarnDisconnectDelay");

			this.actionGetWarnDisconnectDelay.Invoke(out OutputValues);

			NewWarnDisconnectDelay = (string)OutputValues["NewWarnDisconnectDelay"];
		}

		/// <summary>
		/// Generated from SCPD
		/// </summary>
		public void DeletePortMappingRange(string NewStartPort, string NewEndPort, string NewProtocol, string NewManage)
		{
			Dictionary<string, object> OutputValues = new Dictionary<string, object>();
			
			if (actionDeletePortMappingRange == null)
				actionDeletePortMappingRange = this.service.GetAction("DeletePortMappingRange");

			this.actionDeletePortMappingRange.Invoke(out OutputValues,
				new KeyValuePair<string, object>("NewStartPort", NewStartPort),
				new KeyValuePair<string, object>("NewEndPort", NewEndPort),
				new KeyValuePair<string, object>("NewProtocol", NewProtocol),
				new KeyValuePair<string, object>("NewManage", NewManage));
		}

		/// <summary>
		/// Generated from SCPD
		/// </summary>
		public void GetListOfPortMappings(string NewStartPort, string NewEndPort, string NewProtocol, string NewManage, string NewNumberOfPorts, out string NewPortListing)
		{
			Dictionary<string, object> OutputValues = new Dictionary<string, object>();
			
			if (actionGetListOfPortMappings == null)
				actionGetListOfPortMappings = this.service.GetAction("GetListOfPortMappings");

			this.actionGetListOfPortMappings.Invoke(out OutputValues,
				new KeyValuePair<string, object>("NewStartPort", NewStartPort),
				new KeyValuePair<string, object>("NewEndPort", NewEndPort),
				new KeyValuePair<string, object>("NewProtocol", NewProtocol),
				new KeyValuePair<string, object>("NewManage", NewManage),
				new KeyValuePair<string, object>("NewNumberOfPorts", NewNumberOfPorts));

			NewPortListing = (string)OutputValues["NewPortListing"];
		}

		/// <summary>
		/// Generated from SCPD
		/// </summary>
		public void AddAnyPortMapping(string NewRemoteHost, string NewExternalPort, string NewProtocol, string NewInternalPort, string NewInternalClient, bool NewEnabled, string NewPortMappingDescription, string NewLeaseDuration, out string NewReservedPort)
		{
			Dictionary<string, object> OutputValues = new Dictionary<string, object>();
			
			if (actionAddAnyPortMapping == null)
				actionAddAnyPortMapping = this.service.GetAction("AddAnyPortMapping");

			this.actionAddAnyPortMapping.Invoke(out OutputValues,
				new KeyValuePair<string, object>("NewRemoteHost", NewRemoteHost),
				new KeyValuePair<string, object>("NewExternalPort", NewExternalPort),
				new KeyValuePair<string, object>("NewProtocol", NewProtocol),
				new KeyValuePair<string, object>("NewInternalPort", NewInternalPort),
				new KeyValuePair<string, object>("NewInternalClient", NewInternalClient),
				new KeyValuePair<string, object>("NewEnabled", NewEnabled),
				new KeyValuePair<string, object>("NewPortMappingDescription", NewPortMappingDescription),
				new KeyValuePair<string, object>("NewLeaseDuration", NewLeaseDuration));

			NewReservedPort = (string)OutputValues["NewReservedPort"];
		}
	}
//#pragma warning restore
}