using System;
using System.Collections.Generic;

namespace Waher.Networking.UPnP.Services
{
//#pragma warning disable
	/// <summary>
	/// Generated from SCPD
	/// </summary>
	public class Layer3Forwarding
	{
		private ServiceDescriptionDocument service;
		private UPnPAction actionSetDefaultConnectionService = null;
		private UPnPAction actionGetDefaultConnectionService = null;

		/// <summary>
		/// Generated from SCPD
		/// </summary>
		public Layer3Forwarding(ServiceDescriptionDocument Service)
		{
			this.service = Service;
		}

		/// <summary>
		/// Generated from SCPD
		/// </summary>
		public void SetDefaultConnectionService(string NewDefaultConnectionService)
		{
			Dictionary<string, object> OutputValues = new Dictionary<string, object>();
			
			if (actionSetDefaultConnectionService is null)
				actionSetDefaultConnectionService = this.service.GetAction("SetDefaultConnectionService");

			this.actionSetDefaultConnectionService.Invoke(out OutputValues,
				new KeyValuePair<string, object>("NewDefaultConnectionService", NewDefaultConnectionService));
		}

		/// <summary>
		/// Generated from SCPD
		/// </summary>
		public void GetDefaultConnectionService(out string NewDefaultConnectionService)
		{
			Dictionary<string, object> OutputValues = new Dictionary<string, object>();
			
			if (actionGetDefaultConnectionService is null)
				actionGetDefaultConnectionService = this.service.GetAction("GetDefaultConnectionService");

			this.actionGetDefaultConnectionService.Invoke(out OutputValues);

			NewDefaultConnectionService = (string)OutputValues["NewDefaultConnectionService"];
		}
	}
//#pragma warning restore
}