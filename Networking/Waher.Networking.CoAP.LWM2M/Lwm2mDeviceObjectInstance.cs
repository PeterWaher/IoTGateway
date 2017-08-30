using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Waher.Events;
using Waher.Networking.CoAP.LWM2M.ContentFormats;
using Waher.Networking.CoAP.LWM2M.Events;
using Waher.Persistence;
using Waher.Persistence.Attributes;

namespace Waher.Networking.CoAP.LWM2M
{
	/// <summary>
	/// LWM2M Device object instance.
	/// </summary>
	public class Lwm2mDeviceObjectInstance : Lwm2mObjectInstance
	{
		private Lwm2mResourceString manufacturer;
		private Lwm2mResourceString modelNr;
		private Lwm2mResourceString serialNr;
		private Lwm2mResourceString firmwareVersion;
		private Lwm2mResourceCommand reboot;
		private Lwm2mResourceInteger errorCodes;    // TODO: Implement
		private Lwm2mResourceTime currentTime;
		private Lwm2mResourceString timeZone;
		private Lwm2mResourceString supportedBindings;
		private Lwm2mResourceString deviceType;
		private Lwm2mResourceString hardwareVersion;
		private Lwm2mResourceString softwareVersion;

		/// <summary>
		/// LWM2M Device object instance.
		/// </summary>
		public Lwm2mDeviceObjectInstance(string Manufacturer, string ModelNr, string SerialNr,
			string FirmwareVersion, string DeviceType, string HardwareVersion,
			string SoftwareVersion)
			: base(3, 0)
		{
			DateTime Now = DateTime.Now;
			TimeSpan TimeZone = Now - Now.ToUniversalTime();
			StringBuilder sb = new StringBuilder();

			if (TimeZone < TimeSpan.Zero)
			{
				sb.Append('-');
				TimeZone = -TimeZone;
			}

			sb.Append(TimeZone.Hours.ToString("D2"));
			sb.Append(':');
			sb.Append(TimeZone.Minutes.ToString("D2"));

			this.manufacturer = new Lwm2mResourceString(3, 0, 0, Manufacturer);
			this.modelNr = new Lwm2mResourceString(3, 0, 1, ModelNr);
			this.serialNr = new Lwm2mResourceString(3, 0, 2, SerialNr);
			this.firmwareVersion = new Lwm2mResourceString(3, 0, 3, FirmwareVersion);
			this.reboot = new Lwm2mResourceCommand(3, 0, 4);
			this.errorCodes = new Lwm2mResourceInteger(3, 0, 11, 0, true);
			this.currentTime = new Lwm2mResourceTime(3, 0, 13, Now);
			this.timeZone = new Lwm2mResourceString(3, 0, 14, sb.ToString());
			this.supportedBindings = new Lwm2mResourceString(3, 0, 16, "U");
			this.deviceType = new Lwm2mResourceString(3, 0, 17, DeviceType);
			this.hardwareVersion = new Lwm2mResourceString(3, 0, 18, HardwareVersion);
			this.softwareVersion = new Lwm2mResourceString(3, 0, 19, SoftwareVersion);

			this.currentTime.OnAfterRegister += CurrentTime_OnAfterRegister;
			this.currentTime.OnBeforeGet += CurrentTime_OnBeforeGet;

			this.reboot.OnExecute += Reboot_OnExecute;

			this.Add(this.manufacturer);
			this.Add(this.modelNr);
			this.Add(this.serialNr);
			this.Add(this.firmwareVersion);
			this.Add(this.reboot);
			this.Add(this.errorCodes);
			this.Add(this.currentTime);
			this.Add(this.timeZone);
			this.Add(this.supportedBindings);
			this.Add(this.deviceType);
			this.Add(this.hardwareVersion);
			this.Add(this.softwareVersion);
		}

		private void Reboot_OnExecute(object sender, EventArgs e)
		{
			this.Object.Client.Reboot();
		}

		private void CurrentTime_OnAfterRegister(object sender, EventArgs e)
		{
			this.currentTime.TriggerAll(new TimeSpan(0, 0, 1));
		}

		private void CurrentTime_OnBeforeGet(object sender, CoapRequestEventArgs e)
		{
			this.currentTime.TimeValue = DateTime.Now;
		}

		internal override void AfterRegister(Lwm2mClient Client)
		{
			base.AfterRegister(Client);
			this.TriggerAll(new TimeSpan(0, 0, 1));
		}

	}
}
