using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Waher.Content.Semantic;
using Waher.Content.Semantic.Model;
using Waher.Content.Semantic.Ontologies;
using Waher.IoTGateway;
using Waher.Networking.DNS.Enumerations;
using Waher.Networking.HTTP;
using Waher.Networking.XMPP.Sensor;
using Waher.Runtime.Language;
using Waher.Things.Semantic.Ontologies;
using Waher.Things.SensorData;

namespace Waher.Things.Semantic.Sources.DynamicGraphs
{
	/// <summary>
	/// Dynamic graph of a node node action.
	/// </summary>
	public class NodeReadActionGraph : IDynamicGraph
	{
		private readonly INode node;
		private readonly string action;

		/// <summary>
		/// Dynamic graph of a node node action.
		/// </summary>
		/// <param name="Node">Node</param>
		/// <param name="Action">Node action.</param>
		public NodeReadActionGraph(INode Node, string Action)
		{
			this.node = Node;
			this.action = Action;
		}

		/// <summary>
		/// Generates the semantic graph.
		/// </summary>
		/// <param name="Result">Result set will be output here.</param>
		/// <param name="Language">Preferred language.</param>
		/// <param name="Caller">Origin of request.</param>
		public async Task GenerateGraph(InMemorySemanticCube Result, Language Language, RequestOrigin Caller)
		{
			UriNode NodeGraphUriNode = new UriNode(DataSourceGraph.GetNodeUri(this.node));

			if (!(this.node is ISensor Sensor))
				return;

			FieldType FieldTypes = 0;

			foreach (string Part in this.action.Split(' '))
			{
				switch (Part.Trim().ToLower())
				{
					case "all":
						FieldTypes |= FieldType.All;
						break;

					case "momentary":
						FieldTypes |= FieldType.Momentary;
						break;

					case "identity":
						FieldTypes |= FieldType.Identity;
						break;

					case "status":
						FieldTypes |= FieldType.Status;
						break;

					case "computed":
						FieldTypes |= FieldType.Computed;
						break;

					case "peak":
						FieldTypes |= FieldType.Peak;
						break;

					case "historical":
						FieldTypes |= FieldType.Historical;
						break;

					case "nonhistorical":
						FieldTypes |= FieldType.AllExceptHistorical;
						break;
				}
			}

			if (FieldTypes == 0)
				return;

			BlankNode Timestamps = new BlankNode("n" + Guid.NewGuid().ToString());
			int TimestampIndex = 0;

			Result.Add(NodeGraphUriNode, IoTSensorData.timestamps, Timestamps);
			Result.Add(Timestamps, Rdf.type, Rdf.Seq);

			BlankNode Errors = new BlankNode("n" + Guid.NewGuid().ToString());
			int ErrorIndex = 0;

			Result.Add(NodeGraphUriNode, IoTSensorData.errors, Errors);
			Result.Add(Errors, Rdf.type, Rdf.Seq);

			Dictionary<DateTime, TimestampRec> TimestampNodes = new Dictionary<DateTime, TimestampRec>();
			IThingReference[] Nodes = new IThingReference[] { this.node };
			ApprovedReadoutParameters Approval = await Gateway.ConcentratorServer.SensorServer.CanReadAsync(FieldTypes, Nodes, null, Caller)
				?? throw new ForbiddenException("Not authorized to read sensor-data from node.");
			DateTime Timestamp = DateTime.MinValue;
			TimestampRec TimestampRec = null;

			TaskCompletionSource<bool> ReadoutCompleted = new TaskCompletionSource<bool>();
			InternalReadoutRequest Request = await Gateway.ConcentratorServer.SensorServer.DoInternalReadout(Caller.From,
				Approval.Nodes, Approval.FieldTypes, Approval.FieldNames, DateTime.MinValue, DateTime.MaxValue,
				async (Sender, e) =>
				{
					foreach (Field F in e.Fields)
					{
						if (TimestampRec is null || F.Timestamp != Timestamp)
						{
							Timestamp = F.Timestamp;
							if (!TimestampNodes.TryGetValue(Timestamp, out TimestampRec))
							{
								TimestampNodes[Timestamp] = TimestampRec = new TimestampRec()
								{
									Node = new BlankNode("n" + Guid.NewGuid().ToString()),
									Fields = new BlankNode("n" + Guid.NewGuid().ToString()),
									FieldIndex = 0
								};

								Result.Add(Timestamps, Rdf.ListItem(++TimestampIndex), TimestampRec.Node);
								Result.Add(TimestampRec.Node, IoTSensorData.timestamp, Timestamp);
								Result.Add(TimestampRec.Node, IoTSensorData.fields, TimestampRec.Fields);
								Result.Add(TimestampRec.Fields, Rdf.type, Rdf.Seq);
							}
						}

						BlankNode FieldNode = new BlankNode("n" + Guid.NewGuid().ToString());

						Result.Add(TimestampRec.Fields, Rdf.ListItem(++TimestampRec.FieldIndex), FieldNode);

						string LocalizedName;

						if (F.StringIdSteps is null || F.StringIdSteps.Length == 0)
							LocalizedName = null;
						else
						{
							Namespace BaseModule;

							if (string.IsNullOrEmpty(F.Module))
								BaseModule = null;
							else
								BaseModule = await Language.GetNamespaceAsync(F.Module);

							LocalizedName = await LocalizationStep.TryGetLocalization(Language, BaseModule, F.StringIdSteps);
						}

						if (string.IsNullOrEmpty(LocalizedName))
							Result.Add(FieldNode, RdfSchema.label, F.Name);
						else if (LocalizedName == F.Name)
							Result.Add(FieldNode, RdfSchema.label, F.Name, Language.Code);
						else
						{
							Result.Add(FieldNode, RdfSchema.label, F.Name);
							Result.Add(FieldNode, RdfSchema.label, LocalizedName, Language.Code);
						}

						Result.Add(FieldNode, IoTSensorData.value, F.ObjectValue);

						if (F.QoS.HasFlag(FieldQoS.Missing))
						{
							Result.Add(FieldNode, IoTSensorData.qos,
								nameof(FieldQoS.Missing), IoTSensorData.qos);
						}

						if (F.QoS.HasFlag(FieldQoS.InProgress))
						{
							Result.Add(FieldNode, IoTSensorData.qos,
								nameof(FieldQoS.InProgress), IoTSensorData.qos);
						}

						if (F.QoS.HasFlag(FieldQoS.AutomaticEstimate))
						{
							Result.Add(FieldNode, IoTSensorData.qos,
								nameof(FieldQoS.AutomaticEstimate), IoTSensorData.qos);
						}

						if (F.QoS.HasFlag(FieldQoS.ManualEstimate))
						{
							Result.Add(FieldNode, IoTSensorData.qos,
								nameof(FieldQoS.ManualEstimate), IoTSensorData.qos);
						}

						if (F.QoS.HasFlag(FieldQoS.ManualReadout))
						{
							Result.Add(FieldNode, IoTSensorData.qos,
								nameof(FieldQoS.ManualReadout), IoTSensorData.qos);
						}

						if (F.QoS.HasFlag(FieldQoS.AutomaticReadout))
						{
							Result.Add(FieldNode, IoTSensorData.qos,
								nameof(FieldQoS.AutomaticReadout), IoTSensorData.qos);
						}

						if (F.QoS.HasFlag(FieldQoS.TimeOffset))
						{
							Result.Add(FieldNode, IoTSensorData.qos,
								nameof(FieldQoS.TimeOffset), IoTSensorData.qos);
						}

						if (F.QoS.HasFlag(FieldQoS.Warning))
						{
							Result.Add(FieldNode, IoTSensorData.qos,
								nameof(FieldQoS.Warning), IoTSensorData.qos);
						}

						if (F.QoS.HasFlag(FieldQoS.Error))
						{
							Result.Add(FieldNode, IoTSensorData.qos,
								nameof(FieldQoS.Error), IoTSensorData.qos);
						}

						if (F.QoS.HasFlag(FieldQoS.Signed))
						{
							Result.Add(FieldNode, IoTSensorData.qos,
								nameof(FieldQoS.Signed), IoTSensorData.qos);
						}

						if (F.QoS.HasFlag(FieldQoS.Invoiced))
						{
							Result.Add(FieldNode, IoTSensorData.qos,
								nameof(FieldQoS.Invoiced), IoTSensorData.qos);
						}

						if (F.QoS.HasFlag(FieldQoS.EndOfSeries))
						{
							Result.Add(FieldNode, IoTSensorData.qos,
								nameof(FieldQoS.EndOfSeries), IoTSensorData.qos);
						}

						if (F.QoS.HasFlag(FieldQoS.PowerFailure))
						{
							Result.Add(FieldNode, IoTSensorData.qos,
								nameof(FieldQoS.PowerFailure), IoTSensorData.qos);
						}

						if (F.QoS.HasFlag(FieldQoS.InvoiceConfirmed))
						{
							Result.Add(FieldNode, IoTSensorData.qos,
								nameof(FieldQoS.InvoiceConfirmed), IoTSensorData.qos);
						}

						Result.Add(FieldNode, IoTConcentrator.isControllable, F.Writable);

						if (F.Type.HasFlag(FieldType.Momentary))
						{
							Result.Add(FieldNode, Rdf.type,
								nameof(FieldType.Momentary), IoTSensorData.fieldType);
						}

						if (F.Type.HasFlag(FieldType.Identity))
						{
							Result.Add(FieldNode, Rdf.type,
								nameof(FieldType.Identity), IoTSensorData.fieldType);
						}

						if (F.Type.HasFlag(FieldType.Status))
						{
							Result.Add(FieldNode, Rdf.type,
								nameof(FieldType.Status), IoTSensorData.fieldType);
						}

						if (F.Type.HasFlag(FieldType.Computed))
						{
							Result.Add(FieldNode, Rdf.type,
								nameof(FieldType.Computed), IoTSensorData.fieldType);
						}

						if (F.Type.HasFlag(FieldType.Peak))
						{
							Result.Add(FieldNode, Rdf.type,
								nameof(FieldType.Peak), IoTSensorData.fieldType);
						}

						if (F.Type.HasFlag(FieldType.Historical))
						{
							Result.Add(FieldNode, Rdf.type,
								nameof(FieldType.Historical), IoTSensorData.fieldType);
						}
					}

					if (e.Done)
						ReadoutCompleted.TrySetResult(true);
				},
				(Sender, e) =>
				{
					foreach (ThingError Error in e.Errors)
					{
						BlankNode ErrorNode = new BlankNode("n" + Guid.NewGuid().ToString());

						Result.Add(Errors, Rdf.ListItem(++ErrorIndex), ErrorNode);
						Result.Add(ErrorNode, RdfSchema.label, Error.ErrorMessage);
						Result.Add(ErrorNode, IoTSensorData.timestamp, Error.Timestamp);

						if (!string.IsNullOrEmpty(Error.NodeId))
							Result.Add(ErrorNode, IoTConcentrator.nodeId, Error.NodeId);

						if (!string.IsNullOrEmpty(Error.SourceId))
							Result.Add(ErrorNode, IoTConcentrator.sourceId, Error.SourceId);

						if (!string.IsNullOrEmpty(Error.Partition))
							Result.Add(ErrorNode, IoTConcentrator.partition, Error.Partition);
					}

					if (e.Done)
						ReadoutCompleted.TrySetResult(true);

					return Task.CompletedTask;
				}, null);

			Task Timeout = Task.Delay(60000);
			Task T = await Task.WhenAny(ReadoutCompleted.Task, Timeout);

			if (!ReadoutCompleted.Task.IsCompleted)
			{
				BlankNode ErrorNode = new BlankNode("n" + Guid.NewGuid().ToString());

				Result.Add(Errors, Rdf.ListItem(++ErrorIndex), ErrorNode);
				Result.Add(ErrorNode, RdfSchema.label, "Timeout.");
				Result.Add(ErrorNode, IoTSensorData.timestamp, DateTime.UtcNow);
			}
		}

		private class TimestampRec
		{
			public BlankNode Node;
			public BlankNode Fields;
			public int FieldIndex;
		}

	}
}
