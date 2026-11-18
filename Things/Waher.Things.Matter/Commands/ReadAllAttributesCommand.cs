using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Waher.Events;
using Waher.Runtime.Language;
using Waher.Things.Matter.DataModel;
using Waher.Things.Matter.InteractionModel;
using Waher.Things.Metering;
using Waher.Things.Queries;

namespace Waher.Things.Matter.Commands
{
	/// <summary>
	/// Reads all attributes of a device, or of an endpoint, and presents them in a table.
	/// </summary>
	public class ReadAllAttributesCommand : MatterCommand
	{
		private readonly MatterDeviceNode device;
		private readonly MatterEndpointNode endpoint;

		/// <summary>
		/// Reads all attributes of a device, or of an endpoint, and presents them in a table.
		/// </summary>
		/// <param name="Device">Device node.</param>
		/// <param name="Endpoint">Endpoint node, or null if all endpoints are to be read.</param>
		public ReadAllAttributesCommand(MatterDeviceNode Device, MatterEndpointNode Endpoint)
			: base((MeteringNode)Endpoint ?? Device, "Matter.ReadAll", CommandType.Query, "5")
		{
			this.device = Device;
			this.endpoint = Endpoint;
		}

		/// <inheritdoc/>
		public override ICommand Copy() => new ReadAllAttributesCommand(this.device, this.endpoint);

		/// <inheritdoc/>
		public override Task<bool> CanExecuteAsync(RequestOrigin Caller) => this.Node.CanViewAsync(Caller);

		/// <inheritdoc/>
		public override Task<string> GetNameAsync(Language Language)
		{
			return Language.GetStringAsync(typeof(MatterControllerNode), 337, "Read all attributes");
		}

		/// <inheritdoc/>
		public override Task<string> GetFailureStringAsync(Language Language)
		{
			return Language.GetStringAsync(typeof(MatterControllerNode), 338, "Unable to read attributes.");
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
				await Query.SetTitle(await Language.GetStringAsync(typeof(MatterControllerNode), 339, "Attributes"));
				await Query.SetStatus(await Language.GetStringAsync(typeof(MatterControllerNode), 340, "Reading attributes..."));

				ushort? EndpointId = this.endpoint is null ? (ushort?)null : (ushort)this.endpoint.EndpointId;
				ReadResult Result = await this.device.ReadAttributesAsync(new AttributePath(EndpointId, null, null));
				List<AttributeReport> Reports = new List<AttributeReport>(Result.Attributes);

				Reports.Sort((x, y) =>
				{
					int i = (x.Path.EndpointId ?? 0).CompareTo(y.Path.EndpointId ?? 0);
					if (i != 0)
						return i;

					i = (x.Path.ClusterId ?? 0).CompareTo(y.Path.ClusterId ?? 0);
					if (i != 0)
						return i;

					return (x.Path.AttributeId ?? 0).CompareTo(y.Path.AttributeId ?? 0);
				});

				await Query.NewTable("Attributes", await Language.GetStringAsync(typeof(MatterControllerNode), 339, "Attributes"),
					new Column("Endpoint", await Language.GetStringAsync(typeof(MatterControllerNode), 211, "Endpoint"), null, null, null, null, ColumnAlignment.Right, 0),
					new Column("Cluster", await Language.GetStringAsync(typeof(MatterControllerNode), 341, "Cluster"), null, null, null, null, ColumnAlignment.Left, null),
					new Column("Attribute", await Language.GetStringAsync(typeof(MatterControllerNode), 342, "Attribute"), null, null, null, null, ColumnAlignment.Left, null),
					new Column("Value", await Language.GetStringAsync(typeof(MatterControllerNode), 343, "Value"), null, null, null, null, ColumnAlignment.Left, null),
					new Column("Writable", await Language.GetStringAsync(typeof(MatterControllerNode), 344, "Writable"), null, null, null, null, ColumnAlignment.Center, null));

				List<Record> Records = new List<Record>();

				foreach (AttributeReport Report in Reports)
				{
					uint ClusterId = Report.Path.ClusterId ?? 0;
					uint AttributeId = Report.Path.AttributeId ?? 0;
					AttributeDefinition Attribute = MatterDataModel.GetAttribute(ClusterId, AttributeId);
					string Value;

					if (Report.HasValue)
					{
						Value = MatterDataModel.FormatValue(ClusterId, Attribute?.Type, Attribute?.EntryType, Report.Data);

						if (Value.Length > 1000)
							Value = Value.Substring(0, 1000) + "...";
					}
					else
						Value = Report.Status?.ToString() ?? string.Empty;

					Records.Add(new Record(
						(int)(Report.Path.EndpointId ?? 0),
						MatterDataModel.GetClusterName(ClusterId),
						Attribute?.Name ?? ("0x" + AttributeId.ToString("X4")),
						Value,
						Attribute?.Writable ?? false));

					if (Records.Count >= 100)
					{
						await Query.NewRecords("Attributes", Records.ToArray());
						Records.Clear();
					}
				}

				if (Records.Count > 0)
					await Query.NewRecords("Attributes", Records.ToArray());

				await Query.TableDone("Attributes");
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
