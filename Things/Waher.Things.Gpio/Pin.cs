using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Windows.Devices.Gpio;
using Waher.Persistence.Attributes;
using Waher.Runtime.Language;
using Waher.Things.Attributes;
using Waher.Things.DisplayableParameters;
using Waher.Things.Metering;

namespace Waher.Things.Gpio
{
	/// <summary>
	/// TODO
	/// </summary>
	public abstract class Pin : ProvisionedMeteringNode
	{
		private byte pinNr = 0;

		/// <summary>
		/// TODO
		/// </summary>
		public Pin()
			: base()
		{
		}

		/// <summary>
		/// TODO
		/// </summary>
		[Page(2, "GPIO")]
		[Header(3, "Pin number:", 10)]
		[ToolTip(4, "Pin number on parent controller.")]
		[Range(0, byte.MaxValue)]
		[Required]
		[DefaultValue(0)]
		public byte PinNr
		{
			get => this.pinNr;
			set => this.pinNr = value;
		}

		/// <summary>
		/// TODO
		/// </summary>
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

		/// <summary>
		/// TODO
		/// </summary>
		public override Task<bool> AcceptsChildAsync(INode Child)
		{
			return Task.FromResult<bool>(false);
		}

		/// <summary>
		/// TODO
		/// </summary>
		public override Task<bool> AcceptsParentAsync(INode Parent)
		{
			return Task.FromResult<bool>(Parent is Controller);
		}

		/// <summary>
		/// TODO
		/// </summary>
		protected string GetStatusMessage(GpioOpenStatus Status)
		{
			switch (Status)
			{
				case GpioOpenStatus.MuxingConflict: return "The pin is currently opened for a different function, such as **I2c**, **Spi**, or **UART**. Ensure the pin is not in use by another function.";
				case GpioOpenStatus.PinOpened: return "The GPIO pin was successfully opened.";
				case GpioOpenStatus.PinUnavailable: return "The pin is reserved by the system and is not available to apps that run in user mode.";
				case GpioOpenStatus.SharingViolation: return "The pin is currently open in an incompatible sharing mode.";
				case GpioOpenStatus.UnknownError:
				default:
					return "The pin could not be opened.";
			}
		}

		/// <summary>
		/// TODO
		/// </summary>
		public override async Task<IEnumerable<Parameter>> GetDisplayableParametersAsync(Language Language, RequestOrigin Caller)
		{
			LinkedList<Parameter> Result = await base.GetDisplayableParametersAsync(Language, Caller) as LinkedList<Parameter>;

			Result.AddLast(new Int32Parameter("Pin", await Language.GetStringAsync(typeof(Controller), 22, "Pin"), this.PinNr));

			return Result;
		}

	}
}
