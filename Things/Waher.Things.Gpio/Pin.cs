using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Devices.Gpio;
using Waher.Networking.XMPP.Concentrator.Attributes;
using Waher.Runtime.Language;
using Waher.Things.DisplayableParameters;
using Waher.Things.Metering;

namespace Waher.Things.Gpio
{
	public abstract class Pin : MeteringNode
	{
		private int pinNr = 0;

		public Pin()
			: base()
		{
		}

		[Page(2, "GPIO")]
		[Header(3, "Pin number:")]
		[ToolTip(4, "Pin number on parent controller.")]
		[Range(0, ushort.MaxValue)]
		[Required]
		public int PinNr
		{
			get { return this.pinNr; }
			set { this.pinNr = value; }
		}

		public GpioController Controller
		{
			get
			{
				if (this.Parent is Controller Controller)
					return Controller.GpioController;
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
			return Task.FromResult<bool>(Parent is Controller);
		}

	}
}
