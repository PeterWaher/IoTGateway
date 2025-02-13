﻿using System;
using System.Text;
using System.Threading.Tasks;
using Waher.Networking.LWM2M.Events;

namespace Waher.Networking.LWM2M
{
	/// <summary>
	/// LWM2M Device object instance.
	/// </summary>
	public class Lwm2mDeviceObjectInstance : Lwm2mObjectInstance
	{
		private readonly Lwm2mResourceString manufacturer;
		private readonly Lwm2mResourceString modelNr;
		private readonly Lwm2mResourceString serialNr;
		private readonly Lwm2mResourceString firmwareVersion;
		private readonly Lwm2mResourceCommand reboot;
		private readonly Lwm2mResourceInteger errorCodes;    // TODO: Implement
		private readonly Lwm2mResourceTime currentTime;
		private readonly Lwm2mResourceString timeZone;
		private readonly Lwm2mResourceString supportedBindings;
		private readonly Lwm2mResourceString deviceType;
		private readonly Lwm2mResourceString hardwareVersion;
		private readonly Lwm2mResourceString softwareVersion;

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

			this.manufacturer = new Lwm2mResourceString("Manufacturer", 3, 0, 0, false, false, Manufacturer);
			this.modelNr = new Lwm2mResourceString("Model Number", 3, 0, 1, false, false, ModelNr);
			this.serialNr = new Lwm2mResourceString("Serial Number", 3, 0, 2, false, false, SerialNr);
			this.firmwareVersion = new Lwm2mResourceString("Firmware Version", 3, 0, 3, false, false, FirmwareVersion);
			this.reboot = new Lwm2mResourceCommand("Reboot", 3, 0, 4);
			this.errorCodes = new Lwm2mResourceInteger("Error Code", 3, 0, 11, false, false, 0, true);
			this.currentTime = new Lwm2mResourceTime("Current Time", 3, 0, 13, true, false, Now);
			this.timeZone = new Lwm2mResourceString("Time Zone", 3, 0, 14, true, true, sb.ToString());
			this.supportedBindings = new Lwm2mResourceString("Supported Bindings", 3, 0, 16, false, false, "U");
			this.deviceType = new Lwm2mResourceString("Device Type", 3, 0, 17, false, false, DeviceType);
			this.hardwareVersion = new Lwm2mResourceString("Hardware Version", 3, 0, 18, false, false, HardwareVersion);
			this.softwareVersion = new Lwm2mResourceString("Software Version", 3, 0, 19, false, false, SoftwareVersion);

			this.currentTime.OnAfterRegister += this.CurrentTime_OnAfterRegister;
			this.currentTime.OnBeforeGet += this.CurrentTime_OnBeforeGet;

			this.reboot.OnExecute += this.Reboot_OnExecute;

			this.Add(this.manufacturer);
			this.Add(this.modelNr);
			this.Add(this.serialNr);
			this.Add(this.firmwareVersion);
			this.Add(this.reboot);
			this.Add(new Lwm2mResourceNotSupported(3, this.InstanceId, 5));  // Factory Reset 
			this.Add(new Lwm2mResourceNotSupported(3, this.InstanceId, 6));  // Available Power Sources 
			this.Add(new Lwm2mResourceNotSupported(3, this.InstanceId, 7));  // Power Source Voltage
			this.Add(new Lwm2mResourceNotSupported(3, this.InstanceId, 8));  // Power Source Current  
			this.Add(new Lwm2mResourceNotSupported(3, this.InstanceId, 9));  // Battery Level 
			this.Add(new Lwm2mResourceNotSupported(3, this.InstanceId, 10));  // Battery Level 
			this.Add(this.errorCodes);
			this.Add(new Lwm2mResourceNotSupported(3, this.InstanceId, 12));  // Reset Error Code 
			this.Add(this.currentTime);
			this.Add(this.timeZone);
			this.Add(new Lwm2mResourceNotSupported(3, this.InstanceId, 15));  // Timezone 
			this.Add(this.supportedBindings);
			this.Add(this.deviceType);
			this.Add(this.hardwareVersion);
			this.Add(this.softwareVersion);
			this.Add(new Lwm2mResourceNotSupported(3, this.InstanceId, 20));  // Battery Status 
			this.Add(new Lwm2mResourceNotSupported(3, this.InstanceId, 21));  // Memory Total 
			this.Add(new Lwm2mResourceNotSupported(3, this.InstanceId, 22));  // ExtDevInfo 
		}

		private Task Reboot_OnExecute(object Sender, EventArgs e)
		{
			return this.Object.Client.Reboot();
		}

		private Task CurrentTime_OnAfterRegister(object Sender, EventArgs e)
		{
			return this.currentTime.TriggerAll(new TimeSpan(0, 0, 1));
		}

		private Task CurrentTime_OnBeforeGet(object Sender, CoapRequestEventArgs e)
		{
			this.currentTime.TimeValue = DateTime.Now;
			return Task.CompletedTask;
		}

		/// <summary>
		/// Called after the resource has been registered on a CoAP Endpoint.
		/// </summary>
		/// <param name="Client">LWM2M Client</param>
		public override async Task AfterRegister(Lwm2mClient Client)
		{
			await base.AfterRegister(Client);
			await this.TriggerAll(new TimeSpan(0, 0, 1));
		}

	}
}
