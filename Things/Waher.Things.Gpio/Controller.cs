using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Devices.Gpio;
using Waher.Runtime.Language;
using Waher.Things.DisplayableParameters;
using Waher.Things.Metering;
using Waher.Things.Metering.NodeTypes;

namespace Waher.Things.Gpio
{
	/// <summary>
	/// Node representing a GPIO conroller.
	/// </summary>
	public class Controller : MeteringNode
	{
		private GpioController controller;

		/// <summary>
		/// Node representing a GPIO conroller.
		/// </summary>
		///	<param name="Id">Node ID.</param>
		/// <param name="Controller">GPIO Controller</param>
		public Controller()
			: base()
		{
			this.controller = GpioController.GetDefault();
		}

		public GpioController GpioController => this.controller;

		public override Task<bool> AcceptsChildAsync(INode Child)
		{
			return Task.FromResult<bool>(Child is Pin);
		}

		public override async Task<bool> AcceptsParentAsync(INode Parent)
		{
			return (Parent is Root) && (await GpioController.GetDefaultAsync() != null);
		}

		public override Task<string> GetTypeNameAsync(Language Language)
		{
			return Language.GetStringAsync(typeof(Controller), 1, "General Purpose I/O");
		}
	}
}
