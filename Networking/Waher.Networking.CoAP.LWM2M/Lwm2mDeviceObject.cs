using System;
using System.Collections.Generic;
using System.Text;

namespace Waher.Networking.CoAP.LWM2M
{
	/// <summary>
	/// LWM2M Device object.
	/// </summary>
    public class Lwm2mDeviceObject : Lwm2mObject
    {
		/// <summary>
		/// LWM2M Device object.
		/// </summary>
		public Lwm2mDeviceObject(string Manufacturer, string ModelNr, string SerialNr,
			string FirmwareVersion, string DeviceType, string HardwareVersion, 
			string SoftwareVersion)
			: base(3, new Lwm2mDeviceObjectInstance(Manufacturer, ModelNr, SerialNr, 
				FirmwareVersion, DeviceType, HardwareVersion, SoftwareVersion))
		{
		}

    }
}
