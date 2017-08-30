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

			this.manufacturer = new Lwm2mResourceString("Manufacturer", 3, 0, 0, false, Manufacturer);
			this.modelNr = new Lwm2mResourceString("Model Number", 3, 0, 1, false, ModelNr);
			this.serialNr = new Lwm2mResourceString("Serial Number", 3, 0, 2, false, SerialNr);
			this.firmwareVersion = new Lwm2mResourceString("Firmware Version", 3, 0, 3, false, FirmwareVersion);
			this.reboot = new Lwm2mResourceCommand("Reboot", 3, 0, 4);
			this.errorCodes = new Lwm2mResourceInteger("Error Code", 3, 0, 11, false, 0, true);
			this.currentTime = new Lwm2mResourceTime("Current Time", 3, 0, 13, true, Now);
			this.timeZone = new Lwm2mResourceString("Time Zone", 3, 0, 14, true, sb.ToString());
			this.supportedBindings = new Lwm2mResourceString("Supported Bindings", 3, 0, 16, false, "U");
			this.deviceType = new Lwm2mResourceString("Device Type", 3, 0, 17, false, DeviceType);
			this.hardwareVersion = new Lwm2mResourceString("Hardware Version", 3, 0, 18, false, HardwareVersion);
			this.softwareVersion = new Lwm2mResourceString("Software Version", 3, 0, 19, false, SoftwareVersion);

			this.currentTime.OnAfterRegister += CurrentTime_OnAfterRegister;
			this.currentTime.OnBeforeGet += CurrentTime_OnBeforeGet;

			this.reboot.OnExecute += Reboot_OnExecute;

			this.Add(this.manufacturer);
			this.Add(this.modelNr);
			this.Add(this.serialNr);
			this.Add(this.firmwareVersion);
			this.Add(this.reboot);
			this.Add(new Lwm2mResourceNotSupported(3, InstanceId, 5));  // Factory Reset 
			this.Add(new Lwm2mResourceNotSupported(3, InstanceId, 6));  // Available Power Sources 
			this.Add(new Lwm2mResourceNotSupported(3, InstanceId, 7));  // Power Source Voltage
			this.Add(new Lwm2mResourceNotSupported(3, InstanceId, 8));  // Power Source Current  
			this.Add(new Lwm2mResourceNotSupported(3, InstanceId, 9));  // Battery Level 
			this.Add(new Lwm2mResourceNotSupported(3, InstanceId, 10));  // Battery Level 
			this.Add(this.errorCodes);
			this.Add(new Lwm2mResourceNotSupported(3, InstanceId, 12));  // Reset Error Code 
			this.Add(this.currentTime);
			this.Add(this.timeZone);
			this.Add(new Lwm2mResourceNotSupported(3, InstanceId, 15));  // Timezone 
			this.Add(this.supportedBindings);
			this.Add(this.deviceType);
			this.Add(this.hardwareVersion);
			this.Add(this.softwareVersion);
			this.Add(new Lwm2mResourceNotSupported(3, InstanceId, 20));  // Battery Status 
			this.Add(new Lwm2mResourceNotSupported(3, InstanceId, 21));  // Memory Total 
			this.Add(new Lwm2mResourceNotSupported(3, InstanceId, 22));  // ExtDevInfo 
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
