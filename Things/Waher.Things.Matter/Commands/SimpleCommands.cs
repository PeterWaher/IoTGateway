using System.Threading.Tasks;
using Waher.Runtime.Language;

namespace Waher.Things.Matter.Commands
{
	/// <summary>
	/// Restarts the Matter controller of a controller node.
	/// </summary>
	public class RestartControllerCommand : MatterCommand
	{
		private readonly MatterControllerNode controller;

		/// <summary>
		/// Restarts the Matter controller of a controller node.
		/// </summary>
		/// <param name="Controller">Controller node.</param>
		public RestartControllerCommand(MatterControllerNode Controller)
			: base(Controller, "Matter.Restart", CommandType.Simple, "9")
		{
			this.controller = Controller;
		}

		/// <inheritdoc/>
		public override ICommand Copy() => new RestartControllerCommand(this.controller);

		/// <inheritdoc/>
		public override Task<string> GetNameAsync(Language Language)
		{
			return Language.GetStringAsync(typeof(MatterControllerNode), 327, "Restart controller");
		}

		/// <inheritdoc/>
		public override Task<string> GetSuccessStringAsync(Language Language)
		{
			return Language.GetStringAsync(typeof(MatterControllerNode), 328, "Controller restarted.");
		}

		/// <inheritdoc/>
		public override Task ExecuteCommandAsync()
		{
			return this.controller.RestartControllerAsync();
		}
	}

	/// <summary>
	/// Scans the endpoints of a device, creating or updating endpoint nodes.
	/// </summary>
	public class RescanEndpointsCommand : MatterCommand
	{
		private readonly MatterDeviceNode device;

		/// <summary>
		/// Scans the endpoints of a device, creating or updating endpoint nodes.
		/// </summary>
		/// <param name="Device">Device node.</param>
		public RescanEndpointsCommand(MatterDeviceNode Device)
			: base(Device, "Matter.Rescan", CommandType.Simple, "1")
		{
			this.device = Device;
		}

		/// <inheritdoc/>
		public override ICommand Copy() => new RescanEndpointsCommand(this.device);

		/// <inheritdoc/>
		public override Task<string> GetNameAsync(Language Language)
		{
			return Language.GetStringAsync(typeof(MatterControllerNode), 329, "Scan endpoints");
		}

		/// <inheritdoc/>
		public override Task<string> GetSuccessStringAsync(Language Language)
		{
			return Language.GetStringAsync(typeof(MatterControllerNode), 330, "Endpoints scanned.");
		}

		/// <inheritdoc/>
		public override Task ExecuteCommandAsync()
		{
			return this.device.RescanAsync();
		}
	}

	/// <summary>
	/// Closes the secure session with a device, and reconnects and resubscribes.
	/// </summary>
	public class ReconnectCommand : MatterCommand
	{
		private readonly MatterDeviceNode device;

		/// <summary>
		/// Closes the secure session with a device, and reconnects and resubscribes.
		/// </summary>
		/// <param name="Device">Device node.</param>
		public ReconnectCommand(MatterDeviceNode Device)
			: base(Device, "Matter.Reconnect", CommandType.Simple, "2")
		{
			this.device = Device;
		}

		/// <inheritdoc/>
		public override ICommand Copy() => new ReconnectCommand(this.device);

		/// <inheritdoc/>
		public override Task<bool> CanExecuteAsync(RequestOrigin Caller) => Task.FromResult(true);

		/// <inheritdoc/>
		public override Task<string> GetNameAsync(Language Language)
		{
			return Language.GetStringAsync(typeof(MatterControllerNode), 331, "Reconnect");
		}

		/// <inheritdoc/>
		public override Task<string> GetSuccessStringAsync(Language Language)
		{
			return Language.GetStringAsync(typeof(MatterControllerNode), 332, "Reconnecting to device.");
		}

		/// <inheritdoc/>
		public override async Task ExecuteCommandAsync()
		{
			this.device.StopSubscription();
			await this.device.DisconnectAsync();
			await (await this.device.GetMatterNodeAsync()).ConnectAsync();
			this.device.StartSubscription();
		}
	}

	/// <summary>
	/// Removes a device from the fabric (unpairs the device), and deletes the device node.
	/// </summary>
	public class RemoveFromFabricCommand : MatterCommand
	{
		private readonly MatterDeviceNode device;

		/// <summary>
		/// Removes a device from the fabric (unpairs the device), and deletes the device node.
		/// </summary>
		/// <param name="Device">Device node.</param>
		public RemoveFromFabricCommand(MatterDeviceNode Device)
			: base(Device, "Matter.Remove", CommandType.Simple, "9")
		{
			this.device = Device;
		}

		/// <inheritdoc/>
		public override ICommand Copy() => new RemoveFromFabricCommand(this.device);

		/// <inheritdoc/>
		public override Task<bool> CanExecuteAsync(RequestOrigin Caller) => this.device.CanDestroyAsync(Caller);

		/// <inheritdoc/>
		public override Task<string> GetNameAsync(Language Language)
		{
			return Language.GetStringAsync(typeof(MatterControllerNode), 333, "Remove device from fabric");
		}

		/// <inheritdoc/>
		public override Task<string> GetConfirmationStringAsync(Language Language)
		{
			return Language.GetStringAsync(typeof(MatterControllerNode), 334,
				"Are you sure you want to remove the device from the fabric? The device node will be deleted, and the device must be commissioned again to be used by the gateway.");
		}

		/// <inheritdoc/>
		public override Task<string> GetSuccessStringAsync(Language Language)
		{
			return Language.GetStringAsync(typeof(MatterControllerNode), 335, "Device removed from fabric.");
		}

		/// <inheritdoc/>
		public override Task<string> GetFailureStringAsync(Language Language)
		{
			return Language.GetStringAsync(typeof(MatterControllerNode), 336, "Unable to remove device from fabric.");
		}

		/// <inheritdoc/>
		public override async Task ExecuteCommandAsync()
		{
			await this.device.UnpairAsync();

			MatterControllerNode Controller = await this.device.GetControllerNodeAsync();
			await Controller.RemoveAsync(this.device);
			await this.device.DestroyAsync();
		}
	}
}
