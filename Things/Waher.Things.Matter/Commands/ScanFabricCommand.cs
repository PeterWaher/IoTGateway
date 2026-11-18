using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using Waher.Events;
using Waher.Runtime.Language;
using Waher.Things.Attributes;
using Waher.Things.Matter.Controller;
using Waher.Things.Matter.Discovery;
using Waher.Things.Metering;
using Waher.Things.Queries;

namespace Waher.Things.Matter.Commands
{
	/// <summary>
	/// Searches the network for operational nodes in the fabric of a controller node, using DNS-SD.
	/// Optionally, device nodes are created for operational nodes not found in the topology.
	/// </summary>
	public class ScanFabricCommand : MatterCommand
	{
		private readonly MatterControllerNode controller;
		private bool addMissing = false;

		/// <summary>
		/// Searches the network for operational nodes in the fabric of a controller node, using DNS-SD.
		/// </summary>
		/// <param name="Controller">Controller node.</param>
		public ScanFabricCommand(MatterControllerNode Controller)
			: base(Controller, "Matter.ScanFabric", CommandType.Query, "3")
		{
			this.controller = Controller;
		}

		/// <summary>
		/// If device nodes should be created for operational nodes not found in the topology.
		/// </summary>
		[Page(13, "Scan")]
		[Header(14, "Add missing devices.")]
		[ToolTip(15, "If device nodes should be created for devices found in the fabric, that are not represented in the topology.")]
		public bool AddMissing
		{
			get => this.addMissing;
			set => this.addMissing = value;
		}

		/// <inheritdoc/>
		public override ICommand Copy()
		{
			return new ScanFabricCommand(this.controller)
			{
				addMissing = this.addMissing
			};
		}

		/// <inheritdoc/>
		public override Task<string> GetNameAsync(Language Language)
		{
			return Language.GetStringAsync(typeof(MatterControllerNode), 322, "Scan fabric...");
		}

		/// <inheritdoc/>
		public override Task<string> GetFailureStringAsync(Language Language)
		{
			return Language.GetStringAsync(typeof(MatterControllerNode), 323, "Unable to scan fabric.");
		}

		/// <inheritdoc/>
		public override Task StartQueryExecutionAsync(Query Query, Language Language)
		{
			bool AddMissing = this.addMissing;
			Task.Run(() => this.Execute(Query, Language, AddMissing));
			return Task.CompletedTask;
		}

		/// <summary>
		/// Executes the query.
		/// </summary>
		/// <param name="Query">Query.</param>
		/// <param name="Language">Language.</param>
		/// <param name="AddMissing">If missing devices should be added to the topology.</param>
		public async Task Execute(Query Query, Language Language, bool AddMissing)
		{
			try
			{
				await Query.Start();
				await Query.SetTitle(await Language.GetStringAsync(typeof(MatterControllerNode), 324, "Operational nodes in fabric"));
				await Query.SetStatus(await Language.GetStringAsync(typeof(MatterControllerNode), 312, "Searching..."));

				MatterController Controller = await this.controller.GetControllerAsync();
				OperationalNode[] Nodes = await Controller.BrowseOperationalAsync();

				await Query.NewTable("Nodes", await Language.GetStringAsync(typeof(MatterControllerNode), 325, "Nodes"),
					new Column("NodeId", await Language.GetStringAsync(typeof(MatterControllerNode), 133, "Node ID"), null, null, null, null, ColumnAlignment.Left, null),
					new Column("Address", await Language.GetStringAsync(typeof(MatterControllerNode), 319, "Address"), null, null, null, null, ColumnAlignment.Left, null),
					new Column("Port", await Language.GetStringAsync(typeof(MatterControllerNode), 320, "Port"), null, null, null, null, ColumnAlignment.Right, 0),
					new Column("Device", await Language.GetStringAsync(typeof(MatterControllerNode), 326, "Device Node"), MeteringTopology.SourceID, null, null, null, ColumnAlignment.Left, null));

				List<Record> Records = new List<Record>();

				foreach (OperationalNode Node in Nodes)
				{
					MatterDeviceNode Device = await this.controller.FindDeviceAsync(Node.NodeId);
					List<string> Addresses = new List<string>();

					foreach (IPAddress Address in Node.Instance.GetPreferredAddresses(Controller.Transport.SupportsIPv4, Controller.Transport.SupportsIPv6))
						Addresses.Add(Address.ToString());

					if (Device is null && AddMissing)
					{
						try
						{
							IPEndPoint EndPoint = MatterDiscovery.GetEndpoint(Node.Instance, Controller.Transport);

							Device = new MatterDeviceNode()
							{
								NodeId = await MeteringNode.GetUniqueNodeId("Matter Device " + Node.NodeId.ToString("X16")),
								OperationalNodeId = Node.NodeId.ToString("X16"),
								Host = EndPoint?.Address.ToString() ?? string.Empty,
								Port = EndPoint?.Port ?? MatterDeviceNode.DefaultPort
							};

							await this.controller.AddAsync(Device);
							await Device.RescanAsync();
							Device.StartSubscription();
						}
						catch (Exception ex)
						{
							await Query.LogMessage(QueryEventType.Error, QueryEventLevel.Minor,
								"Unable to add device " + Node.NodeId.ToString("X16") + ": " + ex.Message);
						}
					}

					Records.Add(new Record(
						Node.NodeId.ToString("X16"),
						string.Join(", ", Addresses),
						(int)Node.Instance.Port,
						Device?.NodeId ?? string.Empty));
				}

				if (Records.Count > 0)
					await Query.NewRecords("Nodes", Records.ToArray());

				await Query.TableDone("Nodes");
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
