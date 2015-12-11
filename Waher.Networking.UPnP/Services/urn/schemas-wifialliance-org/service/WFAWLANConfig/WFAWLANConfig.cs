using System;
using System.Collections.Generic;

namespace Waher.Networking.UPnP.Services
{
#pragma warning disable
	public class WFAWLANConfig
	{
		private ServiceDescriptionDocument service;
		private UPnPAction actionDelAPSettings = null;
		private UPnPAction actionDelSTASettings = null;
		private UPnPAction actionGetAPSettings = null;
		private UPnPAction actionGetDeviceInfo = null;
		private UPnPAction actionGetSTASettings = null;
		private UPnPAction actionPutMessage = null;
		private UPnPAction actionPutWLANResponse = null;
		private UPnPAction actionRebootAP = null;
		private UPnPAction actionRebootSTA = null;
		private UPnPAction actionResetAP = null;
		private UPnPAction actionResetSTA = null;
		private UPnPAction actionSetAPSettings = null;
		private UPnPAction actionSetSelectedRegistrar = null;
		private UPnPAction actionSetSTASettings = null;

		public WFAWLANConfig(ServiceDescriptionDocument Service)
		{
			this.service = Service;
		}

		public void DelAPSettings(byte[] NewAPSettings)
		{
			Dictionary<string, object> OutputValues = new Dictionary<string, object>();
			
			if (actionDelAPSettings == null)
				actionDelAPSettings = this.service.GetAction("DelAPSettings");

			this.actionDelAPSettings.Invoke(out OutputValues,
				new KeyValuePair<string, object>("NewAPSettings", NewAPSettings));
		}

		public void DelSTASettings(byte[] NewSTASettings)
		{
			Dictionary<string, object> OutputValues = new Dictionary<string, object>();
			
			if (actionDelSTASettings == null)
				actionDelSTASettings = this.service.GetAction("DelSTASettings");

			this.actionDelSTASettings.Invoke(out OutputValues,
				new KeyValuePair<string, object>("NewSTASettings", NewSTASettings));
		}

		public void GetAPSettings(byte[] NewMessage, out byte[] NewAPSettings)
		{
			Dictionary<string, object> OutputValues = new Dictionary<string, object>();
			
			if (actionGetAPSettings == null)
				actionGetAPSettings = this.service.GetAction("GetAPSettings");

			this.actionGetAPSettings.Invoke(out OutputValues,
				new KeyValuePair<string, object>("NewMessage", NewMessage));

			NewAPSettings = (byte[])OutputValues["NewAPSettings"];
		}

		public void GetDeviceInfo(out byte[] NewDeviceInfo)
		{
			Dictionary<string, object> OutputValues = new Dictionary<string, object>();
			
			if (actionGetDeviceInfo == null)
				actionGetDeviceInfo = this.service.GetAction("GetDeviceInfo");

			this.actionGetDeviceInfo.Invoke(out OutputValues);

			NewDeviceInfo = (byte[])OutputValues["NewDeviceInfo"];
		}

		public void GetSTASettings(byte[] NewMessage, out byte[] NewSTASettings)
		{
			Dictionary<string, object> OutputValues = new Dictionary<string, object>();
			
			if (actionGetSTASettings == null)
				actionGetSTASettings = this.service.GetAction("GetSTASettings");

			this.actionGetSTASettings.Invoke(out OutputValues,
				new KeyValuePair<string, object>("NewMessage", NewMessage));

			NewSTASettings = (byte[])OutputValues["NewSTASettings"];
		}

		public void PutMessage(byte[] NewInMessage, out byte[] NewOutMessage)
		{
			Dictionary<string, object> OutputValues = new Dictionary<string, object>();
			
			if (actionPutMessage == null)
				actionPutMessage = this.service.GetAction("PutMessage");

			this.actionPutMessage.Invoke(out OutputValues,
				new KeyValuePair<string, object>("NewInMessage", NewInMessage));

			NewOutMessage = (byte[])OutputValues["NewOutMessage"];
		}

		public void PutWLANResponse(byte[] NewMessage, byte NewWLANEventType, string NewWLANEventMAC)
		{
			Dictionary<string, object> OutputValues = new Dictionary<string, object>();
			
			if (actionPutWLANResponse == null)
				actionPutWLANResponse = this.service.GetAction("PutWLANResponse");

			this.actionPutWLANResponse.Invoke(out OutputValues,
				new KeyValuePair<string, object>("NewMessage", NewMessage),
				new KeyValuePair<string, object>("NewWLANEventType", NewWLANEventType),
				new KeyValuePair<string, object>("NewWLANEventMAC", NewWLANEventMAC));
		}

		public void RebootAP(byte[] NewAPSettings)
		{
			Dictionary<string, object> OutputValues = new Dictionary<string, object>();
			
			if (actionRebootAP == null)
				actionRebootAP = this.service.GetAction("RebootAP");

			this.actionRebootAP.Invoke(out OutputValues,
				new KeyValuePair<string, object>("NewAPSettings", NewAPSettings));
		}

		public void RebootSTA(byte[] NewSTASettings)
		{
			Dictionary<string, object> OutputValues = new Dictionary<string, object>();
			
			if (actionRebootSTA == null)
				actionRebootSTA = this.service.GetAction("RebootSTA");

			this.actionRebootSTA.Invoke(out OutputValues,
				new KeyValuePair<string, object>("NewSTASettings", NewSTASettings));
		}

		public void ResetAP(byte[] NewMessage)
		{
			Dictionary<string, object> OutputValues = new Dictionary<string, object>();
			
			if (actionResetAP == null)
				actionResetAP = this.service.GetAction("ResetAP");

			this.actionResetAP.Invoke(out OutputValues,
				new KeyValuePair<string, object>("NewMessage", NewMessage));
		}

		public void ResetSTA(byte[] NewMessage)
		{
			Dictionary<string, object> OutputValues = new Dictionary<string, object>();
			
			if (actionResetSTA == null)
				actionResetSTA = this.service.GetAction("ResetSTA");

			this.actionResetSTA.Invoke(out OutputValues,
				new KeyValuePair<string, object>("NewMessage", NewMessage));
		}

		public void SetAPSettings(byte[] NewAPSettings)
		{
			Dictionary<string, object> OutputValues = new Dictionary<string, object>();
			
			if (actionSetAPSettings == null)
				actionSetAPSettings = this.service.GetAction("SetAPSettings");

			this.actionSetAPSettings.Invoke(out OutputValues,
				new KeyValuePair<string, object>("NewAPSettings", NewAPSettings));
		}

		public void SetSelectedRegistrar(byte[] NewMessage)
		{
			Dictionary<string, object> OutputValues = new Dictionary<string, object>();
			
			if (actionSetSelectedRegistrar == null)
				actionSetSelectedRegistrar = this.service.GetAction("SetSelectedRegistrar");

			this.actionSetSelectedRegistrar.Invoke(out OutputValues,
				new KeyValuePair<string, object>("NewMessage", NewMessage));
		}

		public void SetSTASettings(out byte[] NewSTASettings)
		{
			Dictionary<string, object> OutputValues = new Dictionary<string, object>();
			
			if (actionSetSTASettings == null)
				actionSetSTASettings = this.service.GetAction("SetSTASettings");

			this.actionSetSTASettings.Invoke(out OutputValues);

			NewSTASettings = (byte[])OutputValues["NewSTASettings"];
		}
	}
#pragma warning enable
}