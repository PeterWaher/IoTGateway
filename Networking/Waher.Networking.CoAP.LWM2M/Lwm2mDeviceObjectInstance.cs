using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Waher.Events;
using Waher.Networking.CoAP.LWM2M.ContentFormats;
using Waher.Persistence;
using Waher.Persistence.Attributes;

namespace Waher.Networking.CoAP.LWM2M
{
	/// <summary>
	/// LWM2M Device object instance.
	/// </summary>
	public class Lwm2mDeviceObjectInstance : Lwm2mObjectInstance
	{
		private string manufacturer;
		private string modelNr;
		private string serialNr;
		private string firmwareVersion;
		private string deviceType;
		private string hardwareVersion;
		private string softwareVersion;

		/// <summary>
		/// LWM2M Device object instance.
		/// </summary>
		public Lwm2mDeviceObjectInstance(string Manufacturer, string ModelNr, string SerialNr,
			string FirmwareVersion, string DeviceType, string HardwareVersion,
			string SoftwareVersion)
			: base(3, 0)
		{
			this.manufacturer = Manufacturer;
			this.modelNr = ModelNr;
			this.serialNr = SerialNr;
			this.firmwareVersion = FirmwareVersion;
			this.deviceType = DeviceType;
			this.hardwareVersion = HardwareVersion;
			this.softwareVersion = SoftwareVersion;
		}

		/// <summary>
		/// Exports resources.
		/// </summary>
		/// <param name="ResourceID">Resource ID, if a single resource is to be exported, otherwise null.</param>
		/// <param name="Writer">Output</param>
		public override void Export(int? ResourceID, ILwm2mWriter Writer)
		{
			bool All = !ResourceID.HasValue;

			if ((All || ResourceID.Value == 0) && this.manufacturer != null)
				Writer.Write(IdentifierType.Resource, 0, this.manufacturer);

			if ((All || ResourceID.Value == 1) && this.modelNr != null)
				Writer.Write(IdentifierType.Resource, 1, this.modelNr);

			if ((All || ResourceID.Value == 2) && this.serialNr != null)
				Writer.Write(IdentifierType.Resource, 2, this.serialNr);

			if ((All || ResourceID.Value == 3) && this.firmwareVersion != null)
				Writer.Write(IdentifierType.Resource, 3, this.firmwareVersion);

			if (All || ResourceID.Value == 11)
				Writer.Write(IdentifierType.Resource, 11, (sbyte)0);    // TODO: Error codes.

			if (All || ResourceID.Value == 13)
				Writer.Write(IdentifierType.Resource, 13, DateTime.Now);

			if (All || ResourceID.Value == 14)
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

				Writer.Write(IdentifierType.Resource, 14, sb.ToString());
			}

			if (All || ResourceID.Value == 16)
				Writer.Write(IdentifierType.Resource, 16, "U");

			if ((All || ResourceID.Value == 17) && this.deviceType != null)
				Writer.Write(IdentifierType.Resource, 17, this.deviceType);

			if ((All || ResourceID.Value == 18) && this.hardwareVersion != null)
				Writer.Write(IdentifierType.Resource, 18, this.hardwareVersion);

			if ((All || ResourceID.Value == 19) && this.softwareVersion != null)
				Writer.Write(IdentifierType.Resource, 19, this.softwareVersion);
		}

	}
}
