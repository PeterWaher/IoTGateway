using System;
using System.Net;
using System.Threading.Tasks;
using Waher.Runtime.Language;
using Waher.Things.Attributes;
using Waher.Things.Matter.Controller;
using Waher.Things.Matter.DataModel;
using Waher.Things.Matter.InteractionModel;
using Waher.Things.Matter.Onboarding;
using Waher.Things.Metering;

namespace Waher.Things.Matter.Commands
{
	/// <summary>
	/// Commissions a Matter device into the fabric of a controller node, using its onboarding payload (QR code or
	/// manual pairing code), and creates a device node for it.
	/// </summary>
	public class CommissionDeviceCommand : MatterCommand
	{
		private readonly MatterControllerNode controller;
		private string onboardingCode = string.Empty;
		private string nodeName = string.Empty;
		private string address = string.Empty;
		private int port = MatterDeviceNode.DefaultPort;

		/// <summary>
		/// Commissions a Matter device into the fabric of a controller node.
		/// </summary>
		/// <param name="Controller">Controller node.</param>
		public CommissionDeviceCommand(MatterControllerNode Controller)
			: base(Controller, "Matter.Commission", CommandType.Parametrized, "1")
		{
			this.controller = Controller;
		}

		/// <summary>
		/// Onboarding code: QR code payload (starting with MT:) or manual pairing code (11 or 21 digits).
		/// </summary>
		[Page(4, "Device")]
		[Header(5, "Pairing code:")]
		[ToolTip(6, "QR code payload (starting with MT:) or manual pairing code (11 or 21 digits) of the device.")]
		[Required]
		public string OnboardingCode
		{
			get => this.onboardingCode;
			set => this.onboardingCode = value;
		}

		/// <summary>
		/// Node ID of the new device node. If empty, the product name of the device is used.
		/// </summary>
		[Page(4, "Device")]
		[Header(7, "Node ID:")]
		[ToolTip(8, "Node ID of the new device node. If empty, the product name reported by the device is used.")]
		public string NodeName
		{
			get => this.nodeName;
			set => this.nodeName = value;
		}

		/// <summary>
		/// Optional address of the device. If empty, the device is found using DNS-SD.
		/// </summary>
		[Page(4, "Device")]
		[Header(9, "Address:")]
		[ToolTip(10, "Optional IP address or host name of the device. If empty, the device is found on the network using DNS-SD.")]
		public string Address
		{
			get => this.address;
			set => this.address = value;
		}

		/// <summary>
		/// UDP port of the device, if address is provided.
		/// </summary>
		[Page(4, "Device")]
		[Header(11, "Port:")]
		[ToolTip(12, "UDP port of the device, if an address is provided.")]
		[Range(1, 65535)]
		public int Port
		{
			get => this.port;
			set => this.port = value;
		}

		/// <summary>
		/// Device node created by the command, after successful execution.
		/// </summary>
		public MatterDeviceNode Result { get; private set; }

		/// <inheritdoc/>
		public override ICommand Copy()
		{
			return new CommissionDeviceCommand(this.controller)
			{
				onboardingCode = this.onboardingCode,
				nodeName = this.nodeName,
				address = this.address,
				port = this.port
			};
		}

		/// <inheritdoc/>
		public override Task<string> GetNameAsync(Language Language)
		{
			return Language.GetStringAsync(typeof(MatterControllerNode), 306, "Commission device...");
		}

		/// <inheritdoc/>
		public override Task<string> GetSuccessStringAsync(Language Language)
		{
			return Language.GetStringAsync(typeof(MatterControllerNode), 307, "Device commissioned.");
		}

		/// <inheritdoc/>
		public override Task<string> GetFailureStringAsync(Language Language)
		{
			return Language.GetStringAsync(typeof(MatterControllerNode), 308, "Unable to commission device.");
		}

		/// <inheritdoc/>
		public override async Task ExecuteCommandAsync()
		{
			this.Result = await CommissionAsync(this.controller, this.onboardingCode, this.nodeName, this.address, this.port);
		}

		/// <summary>
		/// Commissions a device into the fabric of a controller node, and creates a device node for it.
		/// </summary>
		/// <param name="ControllerNode">Controller node.</param>
		/// <param name="OnboardingCode">QR code payload or manual pairing code.</param>
		/// <param name="NodeName">Optional Node ID of the new device node.</param>
		/// <param name="Address">Optional address of device.</param>
		/// <param name="Port">Port of device, if address is provided.</param>
		/// <returns>Device node.</returns>
		public static async Task<MatterDeviceNode> CommissionAsync(MatterControllerNode ControllerNode, string OnboardingCode,
			string NodeName, string Address, int Port)
		{
			if (!SetupPayload.TryParse(OnboardingCode?.Trim() ?? string.Empty, out SetupPayload Payload))
				throw new ArgumentException("Invalid pairing code.", nameof(OnboardingCode));

			IPEndPoint EndPoint = null;

			if (!string.IsNullOrEmpty(Address))
			{
				if (!IPAddress.TryParse(Address, out IPAddress IP))
				{
					IPAddress[] Addresses = await Dns.GetHostAddressesAsync(Address);
					if (Addresses.Length == 0)
						throw new ArgumentException("Unable to resolve address.", nameof(Address));

					IP = Addresses[0];
				}

				EndPoint = new IPEndPoint(IP, Port <= 0 ? MatterDeviceNode.DefaultPort : Port);
			}

			MatterController Controller = await ControllerNode.GetControllerAsync();
			ControllerNode.GetOptions(Controller);

			MatterNode Node = await Controller.CommissionAsync(Payload, EndPoint, null, (s) => Controller.Information(s));
			string ProductName = null;

			try
			{
				ReadResult Info = await Node.ReadAttributesAsync(
					new AttributePath(0, ClusterIds.BasicInformation, 0x0003),
					new AttributePath(0, ClusterIds.BasicInformation, 0x0005));

				ProductName = Info.GetValue(0, ClusterIds.BasicInformation, 0x0005)?.AsString();

				if (string.IsNullOrEmpty(ProductName))
					ProductName = Info.GetValue(0, ClusterIds.BasicInformation, 0x0003)?.AsString();
			}
			catch (Exception)
			{
				// Ignore
			}

			if (string.IsNullOrEmpty(NodeName))
				NodeName = string.IsNullOrEmpty(ProductName) ? "Matter Device" : ProductName;

			IPEndPoint DeviceAddress = Node.Address;
			MatterDeviceNode Device = new MatterDeviceNode()
			{
				NodeId = await MeteringNode.GetUniqueNodeId(NodeName),
				OperationalNodeId = Node.NodeId.ToString("X16"),
				Host = DeviceAddress?.Address.ToString() ?? string.Empty,
				Port = DeviceAddress?.Port ?? MatterDeviceNode.DefaultPort
			};

			await ControllerNode.AddAsync(Device);
			await Device.LogInformationAsync("Commissioning", "Device commissioned. Operational node ID: " + Device.OperationalNodeId);

			try
			{
				await Device.RescanAsync();
			}
			catch (Exception ex)
			{
				await Device.LogErrorAsync("Rescan", "Unable to scan device endpoints: " + ex.Message);
			}

			Device.StartSubscription();

			return Device;
		}
	}
}
