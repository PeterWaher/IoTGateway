using System;
using System.Threading.Tasks;
using Windows.Devices.Gpio;
using Waher.Runtime.Language;
using Waher.Things.Metering;
using Waher.Things.Metering.NodeTypes;

namespace Waher.Things.Gpio
{
	/// <summary>
	/// Node representing a GPIO conroller.
	/// </summary>
	public class Controller : MeteringNode
	{
		private readonly GpioController controller;

		/// <summary>
		/// Node representing a GPIO conroller.
		/// </summary>
		public Controller()
			: base()
		{
			this.controller = GpioController.GetDefault();
		}

		/// <summary>
		/// TODO
		/// </summary>
		public GpioController GpioController => this.controller;

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
		public override async Task<bool> AcceptsParentAsync(INode Parent)
		{
			return (Parent is Root) && !((await GpioController.GetDefaultAsync() is null));
		}

		/// <summary>
		/// TODO
		/// </summary>
		public override Task<string> GetTypeNameAsync(Language Language)
		{
			return Language.GetStringAsync(typeof(Controller), 1, "General Purpose I/O");
		}
	}
}
