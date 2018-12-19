using System;
using System.Collections.Generic;

namespace Waher.Networking.UPnP.Services
{
//#pragma warning disable
	/// <summary>
	/// Generated from SCPD
	/// </summary>
	public class X_MS_MediaReceiverRegistrar
	{
		private ServiceDescriptionDocument service;
		private UPnPAction actionIsAuthorized = null;
		private UPnPAction actionRegisterDevice = null;
		private UPnPAction actionIsValidated = null;

		/// <summary>
		/// Generated from SCPD
		/// </summary>
		public X_MS_MediaReceiverRegistrar(ServiceDescriptionDocument Service)
		{
			this.service = Service;
		}

		/// <summary>
		/// Generated from SCPD
		/// </summary>
		public void IsAuthorized(string DeviceID, out long Result)
		{
			Dictionary<string, object> OutputValues = new Dictionary<string, object>();
			
			if (actionIsAuthorized is null)
				actionIsAuthorized = this.service.GetAction("IsAuthorized");

			this.actionIsAuthorized.Invoke(out OutputValues,
				new KeyValuePair<string, object>("DeviceID", DeviceID));

			Result = (long)OutputValues["Result"];
		}

		/// <summary>
		/// Generated from SCPD
		/// </summary>
		public void RegisterDevice(byte[] RegistrationReqMsg, out byte[] RegistrationRespMsg)
		{
			Dictionary<string, object> OutputValues = new Dictionary<string, object>();
			
			if (actionRegisterDevice is null)
				actionRegisterDevice = this.service.GetAction("RegisterDevice");

			this.actionRegisterDevice.Invoke(out OutputValues,
				new KeyValuePair<string, object>("RegistrationReqMsg", RegistrationReqMsg));

			RegistrationRespMsg = (byte[])OutputValues["RegistrationRespMsg"];
		}

		/// <summary>
		/// Generated from SCPD
		/// </summary>
		public void IsValidated(string DeviceID, out long Result)
		{
			Dictionary<string, object> OutputValues = new Dictionary<string, object>();
			
			if (actionIsValidated is null)
				actionIsValidated = this.service.GetAction("IsValidated");

			this.actionIsValidated.Invoke(out OutputValues,
				new KeyValuePair<string, object>("DeviceID", DeviceID));

			Result = (long)OutputValues["Result"];
		}
	}
//#pragma warning restore
}