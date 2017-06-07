using System;
using System.Collections.Generic;

namespace Waher.Networking.UPnP.Services
{
//#pragma warning disable
	public class X_MS_MediaReceiverRegistrar
	{
		private ServiceDescriptionDocument service;
		private UPnPAction actionIsAuthorized = null;
		private UPnPAction actionRegisterDevice = null;
		private UPnPAction actionIsValidated = null;

		public X_MS_MediaReceiverRegistrar(ServiceDescriptionDocument Service)
		{
			this.service = Service;
		}

		public void IsAuthorized(string DeviceID, out long Result)
		{
			Dictionary<string, object> OutputValues = new Dictionary<string, object>();
			
			if (actionIsAuthorized == null)
				actionIsAuthorized = this.service.GetAction("IsAuthorized");

			this.actionIsAuthorized.Invoke(out OutputValues,
				new KeyValuePair<string, object>("DeviceID", DeviceID));

			Result = (long)OutputValues["Result"];
		}

		public void RegisterDevice(byte[] RegistrationReqMsg, out byte[] RegistrationRespMsg)
		{
			Dictionary<string, object> OutputValues = new Dictionary<string, object>();
			
			if (actionRegisterDevice == null)
				actionRegisterDevice = this.service.GetAction("RegisterDevice");

			this.actionRegisterDevice.Invoke(out OutputValues,
				new KeyValuePair<string, object>("RegistrationReqMsg", RegistrationReqMsg));

			RegistrationRespMsg = (byte[])OutputValues["RegistrationRespMsg"];
		}

		public void IsValidated(string DeviceID, out long Result)
		{
			Dictionary<string, object> OutputValues = new Dictionary<string, object>();
			
			if (actionIsValidated == null)
				actionIsValidated = this.service.GetAction("IsValidated");

			this.actionIsValidated.Invoke(out OutputValues,
				new KeyValuePair<string, object>("DeviceID", DeviceID));

			Result = (long)OutputValues["Result"];
		}
	}
//#pragma warning restore
}