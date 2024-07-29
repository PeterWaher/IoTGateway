using System.Threading.Tasks;
using Microsoft.Maker.RemoteWiring;
using Waher.Persistence.Attributes;
using Waher.Things.Attributes;
using Waher.Things.Metering;

namespace Waher.Things.Arduino
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
		[Page(5, "Arduino")]
		[Header(6, "Pin number:", 10)]
		[ToolTip(7, "Pin number on parent controller.")]
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
		public abstract string PinNrStr
		{
			get;
		}

		/// <summary>
		/// TODO
		/// </summary>
		public abstract void Initialize();

		/// <summary>
		/// TODO
		/// </summary>
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
			return Task.FromResult<bool>(Parent is UsbConnectedDevice);
		}

		/// <summary>
		/// TODO
		/// </summary>
		public override Task DestroyAsync()
		{
			Task Result = base.DestroyAsync();

			if (this.Parent is UsbConnectedDevice UsbConnectedDevice)
			{
				UsbState State = Module.GetState(UsbConnectedDevice.PortName);
				State?.RemovePin(this.PinNrStr, this);
			}

			return Result;
		}

	}
}
