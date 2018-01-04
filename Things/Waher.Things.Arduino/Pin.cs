using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Maker.RemoteWiring;
using Waher.Persistence.Attributes;
using Waher.Runtime.Language;
using Waher.Things.Attributes;
using Waher.Things.DisplayableParameters;
using Waher.Things.Metering;

namespace Waher.Things.Arduino
{
	public abstract class Pin : MeteringNode
	{
		private byte pinNr = 0;

		public Pin()
			: base()
		{
		}

		[Page(5, "Arduino")]
		[Header(6, "Pin number:")]
		[ToolTip(7, "Pin number on parent controller.")]
		[Range(0, byte.MaxValue)]
		[Required]
		[DefaultValue(0)]
		public byte PinNr
		{
			get { return this.pinNr; }
			set { this.pinNr = value; }
		}

		public abstract string PinNrStr
		{
			get;
		}

		public RemoteDevice Device
		{
			get
			{
				if (this.Parent is UsbConnectedDevice Device)
					return Device.Device;
				else
					return null;
			}
		}

		public override Task<bool> AcceptsChildAsync(INode Child)
		{
			return Task.FromResult<bool>(false);
		}

		public override Task<bool> AcceptsParentAsync(INode Parent)
		{
			return Task.FromResult<bool>(Parent is UsbConnectedDevice);
		}

		public override Task DestroyAsync()
		{
			Task Result = base.DestroyAsync();

			if (this.Parent is UsbConnectedDevice UsbConnectedDevice)
				UsbConnectedDevice.RemovePin(this.PinNrStr, this);

			return Result;
		}

	}
}
