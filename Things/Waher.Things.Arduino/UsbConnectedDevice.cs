using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Maker.RemoteWiring;
using Waher.Persistence.Attributes;
using Waher.Runtime.Language;
using Waher.Things.Attributes;
using Waher.Things.Metering;
using Waher.Things.Metering.NodeTypes;

namespace Waher.Things.Arduino
{
	/// <summary>
	/// TODO
	/// </summary>
	public class UsbConnectedDevice : MeteringNode
	{
		private string portName = string.Empty;

		/// <summary>
		/// TODO
		/// </summary>
		public UsbConnectedDevice()
			: base()
		{
		}

		/// <summary>
		/// TODO
		/// </summary>
		[Page(2, "Port")]
		[Header(3, "Name:")]
		[ToolTip(4, "Name of USB serial port.")]
		[DefaultValueStringEmpty]
		public string PortName
		{
			get => this.portName;
			set => this.portName = value;
		}

		/// <summary>
		/// TODO
		/// </summary>
		public RemoteDevice Device
		{
			get
			{
				return Module.GetDevice(this.portName);
			}
		}

		/// <summary>
		/// TODO
		/// </summary>
		public override Task AddAsync(INode Child)
		{
			Task Result = base.AddAsync(Child);

			UsbState State = Module.GetState(this.portName);
			if (!(State is null) && Child is Pin Pin)
				State.AddPin(Pin.PinNrStr, Pin);

			return Result;
		}

		/// <summary>
		/// TODO
		/// </summary>
		public override async Task<bool> RemoveAsync(INode Child)
		{
			bool Result = await base.RemoveAsync(Child);

			UsbState State = Module.GetState(this.portName);
			if (!(State is null) && Child is Pin Pin)
				State.RemovePin(Pin.PinNrStr, Pin);

			return Result;
		}

		/// <summary>
		/// TODO
		/// </summary>
		protected override void SortChildrenAfterLoadLocked(List<MeteringNode> Children)
		{
			base.SortChildrenAfterLoadLocked(Children);

			if (!(Children is null))
			{
				UsbState State = Module.GetState(this.portName);
				if (!(State is null))
				{
					MeteringNode[] Children2 = Children.ToArray();

					Task.Run(() =>
					{
						lock (State.Pins)
						{
							foreach (INode Node in Children2)
							{
								if (Node is Pin Pin)
									State.Pins[Pin.PinNrStr] = Pin;
							}
						}
					});
				}
			}
		}

		/// <summary>
		/// TODO
		/// </summary>
		public override Task<bool> AcceptsChildAsync(INode Child)
		{
			return Task.FromResult<bool>(Child is Pin);
		}

		/// <summary>
		/// TODO
		/// </summary>
		public override Task<bool> AcceptsParentAsync(INode Parent)
		{
			return Task.FromResult<bool>(Parent is Root);
		}

		/// <summary>
		/// TODO
		/// </summary>
		public override Task<string> GetTypeNameAsync(Language Language)
		{
			return Language.GetStringAsync(typeof(Module), 1, "USB Connected Device");
		}
	}
}

