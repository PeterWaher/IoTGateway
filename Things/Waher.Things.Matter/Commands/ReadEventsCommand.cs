using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Waher.Events;
using Waher.Runtime.Language;
using Waher.Things.Matter.Clusters;
using Waher.Things.Matter.Controller;
using Waher.Things.Matter.DataModel;
using Waher.Things.Matter.InteractionModel;
using Waher.Things.Metering;
using Waher.Things.Queries;

namespace Waher.Things.Matter.Commands
{
	/// <summary>
	/// Reads events buffered on a device, or on an endpoint, and presents them in a table.
	/// </summary>
	public class ReadEventsCommand : MatterCommand
	{
		private readonly MatterDeviceNode device;
		private readonly MatterEndpointNode endpoint;

		/// <summary>
		/// Reads events buffered on a device, or on an endpoint, and presents them in a table.
		/// </summary>
		/// <param name="Device">Device node.</param>
		/// <param name="Endpoint">Endpoint node, or null if all endpoints are to be read.</param>
		public ReadEventsCommand(MatterDeviceNode Device, MatterEndpointNode Endpoint)
			: base((MeteringNode)Endpoint ?? Device, "Matter.ReadEvents", CommandType.Query, "6")
		{
			this.device = Device;
			this.endpoint = Endpoint;
		}

		/// <inheritdoc/>
		public override ICommand Copy() => new ReadEventsCommand(this.device, this.endpoint);

		/// <inheritdoc/>
		public override Task<bool> CanExecuteAsync(RequestOrigin Caller) => this.Node.CanViewAsync(Caller);

		/// <inheritdoc/>
		public override Task<string> GetNameAsync(Language Language)
		{
			return Language.GetStringAsync(typeof(MatterControllerNode), 345, "Read events");
		}

		/// <inheritdoc/>
		public override Task<string> GetFailureStringAsync(Language Language)
		{
			return Language.GetStringAsync(typeof(MatterControllerNode), 346, "Unable to read events.");
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
				await Query.SetTitle(await Language.GetStringAsync(typeof(MatterControllerNode), 347, "Events"));
				await Query.SetStatus(await Language.GetStringAsync(typeof(MatterControllerNode), 348, "Reading events..."));

				ushort? EndpointId = this.endpoint is null ? (ushort?)null : (ushort)this.endpoint.EndpointId;
				MatterNode Node = await this.device.GetMatterNodeAsync();
				ReadResult Result = await Node.ReadAsync(null, new EventPath[] { new EventPath(EndpointId, null, null) });
				List<EventReport> Events = new List<EventReport>(Result.Events);

				Events.Sort((x, y) => x.EventNumber.CompareTo(y.EventNumber));

				await Query.NewTable("Events", await Language.GetStringAsync(typeof(MatterControllerNode), 347, "Events"),
					new Column("Number", await Language.GetStringAsync(typeof(MatterControllerNode), 349, "#"), null, null, null, null, ColumnAlignment.Right, 0),
					new Column("Timestamp", await Language.GetStringAsync(typeof(MatterControllerNode), 350, "Timestamp"), null, null, null, null, ColumnAlignment.Left, null),
					new Column("Endpoint", await Language.GetStringAsync(typeof(MatterControllerNode), 211, "Endpoint"), null, null, null, null, ColumnAlignment.Right, 0),
					new Column("Cluster", await Language.GetStringAsync(typeof(MatterControllerNode), 341, "Cluster"), null, null, null, null, ColumnAlignment.Left, null),
					new Column("Event", await Language.GetStringAsync(typeof(MatterControllerNode), 351, "Event"), null, null, null, null, ColumnAlignment.Left, null),
					new Column("Priority", await Language.GetStringAsync(typeof(MatterControllerNode), 352, "Priority"), null, null, null, null, ColumnAlignment.Left, null),
					new Column("Data", await Language.GetStringAsync(typeof(MatterControllerNode), 353, "Data"), null, null, null, null, ColumnAlignment.Left, null));

				List<Record> Records = new List<Record>();

				foreach (EventReport Event in Events)
				{
					uint ClusterId = Event.Path.ClusterId ?? 0;
					uint EventId = Event.Path.EventId ?? 0;
					EventDefinition Definition = MatterDataModel.GetCluster(ClusterId)?.GetEvent(EventId);
					string Data = Event.Data is null ? Event.Status?.ToString() ?? string.Empty :
						ValueConverter.FormatEventData(ClusterId, Definition, Event.Data);
					object Timestamp;

					if (Event.Timestamp.HasValue)
						Timestamp = Event.Timestamp.Value;
					else if (Event.SystemTimestampMs.HasValue)
						Timestamp = Event.SystemTimestampMs.Value.ToString() + " ms";
					else
						Timestamp = string.Empty;

					Records.Add(new Record(
						(long)Event.EventNumber,
						Timestamp,
						(int)(Event.Path.EndpointId ?? 0),
						MatterDataModel.GetClusterName(ClusterId),
						Definition?.Name ?? ("0x" + EventId.ToString("X2")),
						Event.Priority.ToString(),
						Data));

					if (Records.Count >= 100)
					{
						await Query.NewRecords("Events", Records.ToArray());
						Records.Clear();
					}
				}

				if (Records.Count > 0)
					await Query.NewRecords("Events", Records.ToArray());

				await Query.TableDone("Events");
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
