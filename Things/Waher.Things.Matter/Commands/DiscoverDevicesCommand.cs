using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using Waher.Events;
using Waher.Runtime.Language;
using Waher.Things.Matter.Controller;
using Waher.Things.Matter.DataModel;
using Waher.Things.Matter.Discovery;
using Waher.Things.Queries;

namespace Waher.Things.Matter.Commands
{
	/// <summary>
	/// Searches the network for commissionable Matter devices, using DNS-SD.
	/// </summary>
	public class DiscoverDevicesCommand : MatterCommand
	{
		private readonly MatterControllerNode controller;

		/// <summary>
		/// Searches the network for commissionable Matter devices, using DNS-SD.
		/// </summary>
		/// <param name="Controller">Controller node.</param>
		public DiscoverDevicesCommand(MatterControllerNode Controller)
			: base(Controller, "Matter.Discover", CommandType.Query, "2")
		{
			this.controller = Controller;
		}

		/// <inheritdoc/>
		public override ICommand Copy()
		{
			return new DiscoverDevicesCommand(this.controller);
		}

		/// <inheritdoc/>
		public override Task<string> GetNameAsync(Language Language)
		{
			return Language.GetStringAsync(typeof(MatterControllerNode), 309, "Find commissionable devices");
		}

		/// <inheritdoc/>
		public override Task<string> GetFailureStringAsync(Language Language)
		{
			return Language.GetStringAsync(typeof(MatterControllerNode), 310, "Unable to search for devices.");
		}

		/// <inheritdoc/>
		public override Task StartQueryExecutionAsync(Query Query, Language Language)
		{
			Task.Run(() => this.Execute(Query, Language));
			return Task.CompletedTask;
		}

		/// <summary>
		/// Executes the query.
		/// </summary>
		/// <param name="Query">Query.</param>
		/// <param name="Language">Language.</param>
		public async Task Execute(Query Query, Language Language)
		{
			try
			{
				await Query.Start();
				await Query.SetTitle(await Language.GetStringAsync(typeof(MatterControllerNode), 311, "Commissionable Matter devices"));
				await Query.SetStatus(await Language.GetStringAsync(typeof(MatterControllerNode), 312, "Searching..."));

				MatterController Controller = await this.controller.GetControllerAsync();
				CommissionableNode[] Nodes = await Controller.DiscoverCommissionableAsync();

				await Query.NewTable("Devices", await Language.GetStringAsync(typeof(MatterControllerNode), 313, "Devices"),
					new Column("Name", await Language.GetStringAsync(typeof(MatterControllerNode), 314, "Name"), null, null, null, null, ColumnAlignment.Left, null),
					new Column("DeviceType", await Language.GetStringAsync(typeof(MatterControllerNode), 212, "Device Type"), null, null, null, null, ColumnAlignment.Left, null),
					new Column("Discriminator", await Language.GetStringAsync(typeof(MatterControllerNode), 315, "Discriminator"), null, null, null, null, ColumnAlignment.Right, null),
					new Column("VendorId", await Language.GetStringAsync(typeof(MatterControllerNode), 316, "Vendor ID"), null, null, null, null, ColumnAlignment.Left, null),
					new Column("ProductId", await Language.GetStringAsync(typeof(MatterControllerNode), 317, "Product ID"), null, null, null, null, ColumnAlignment.Left, null),
					new Column("Mode", await Language.GetStringAsync(typeof(MatterControllerNode), 318, "Mode"), null, null, null, null, ColumnAlignment.Left, null),
					new Column("Address", await Language.GetStringAsync(typeof(MatterControllerNode), 319, "Address"), null, null, null, null, ColumnAlignment.Left, null),
					new Column("Port", await Language.GetStringAsync(typeof(MatterControllerNode), 320, "Port"), null, null, null, null, ColumnAlignment.Right, 0),
					new Column("Instance", await Language.GetStringAsync(typeof(MatterControllerNode), 321, "Instance"), null, null, null, null, ColumnAlignment.Left, null));

				List<Record> Records = new List<Record>();

				foreach (CommissionableNode Node in Nodes)
				{
					List<string> Addresses = new List<string>();

					foreach (IPAddress Address in Node.Instance.GetPreferredAddresses(Controller.Transport.SupportsIPv4, Controller.Transport.SupportsIPv6))
						Addresses.Add(Address.ToString());

					Records.Add(new Record(
						Node.DeviceName ?? string.Empty,
						Node.DeviceType.HasValue ? MatterDataModel.GetDeviceTypeName(Node.DeviceType.Value) : string.Empty,
						Node.Discriminator.HasValue ? (object)(int)Node.Discriminator.Value : string.Empty,
						Node.VendorId.HasValue ? "0x" + Node.VendorId.Value.ToString("X4") : string.Empty,
						Node.ProductId.HasValue ? "0x" + Node.ProductId.Value.ToString("X4") : string.Empty,
						Node.Mode.ToString(),
						string.Join(", ", Addresses),
						(int)Node.Instance.Port,
						Node.Instance.InstanceName));
				}

				if (Records.Count > 0)
					await Query.NewRecords("Devices", Records.ToArray());

				await Query.TableDone("Devices");
				await Query.SetStatus(string.Empty);
			}
			catch (Exception ex)
			{
				await Query.LogMessage(ex);
				Log.Exception(ex);
			}
			finally
			{
				await Query.Done();
			}
		}
	}
}
