using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Waher.Content.Semantic;
using Waher.Content.Semantic.Model;
using Waher.Content.Semantic.Ontologies;
using Waher.Runtime.Collections;
using Waher.Runtime.Language;
using Waher.Things.ControlParameters;
using Waher.Things.Semantic.Ontologies;

namespace Waher.Things.Semantic.Sources.DynamicGraphs
{
	/// <summary>
	/// Dynamic graph of a node in a data source on the Neuron.
	/// </summary>
	public class NodeGraph : IDynamicGraph
	{
		private readonly INode node;

		/// <summary>
		/// Dynamic graph of a node in a data source on the Neuron.
		/// </summary>
		/// <param name="Node">Node</param>
		public NodeGraph(INode Node)
		{
			this.node = Node;
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

			Result.Add(NodeGraphUriNode, Rdf.type, IoTConcentrator.Node);

			Result.Add(NodeGraphUriNode, IoTConcentrator.typeName,
				await this.node.GetTypeNameAsync(Language));

			Result.Add(NodeGraphUriNode, IoTConcentrator.sourceId, this.node.SourceId);
			Result.Add(NodeGraphUriNode, IoTConcentrator.nodeId, this.node.NodeId);
			Result.Add(NodeGraphUriNode, IoTConcentrator.logId, this.node.LogId);
			Result.Add(NodeGraphUriNode, IoTConcentrator.localId, this.node.LocalId);
			Result.Add(NodeGraphUriNode, RdfSchema.label, this.node.LocalId);

			if (!string.IsNullOrEmpty(this.node.Partition))
				Result.Add(NodeGraphUriNode, IoTConcentrator.partition, this.node.Partition);

			if (!(this.node.Parent is null))
				Result.Add(NodeGraphUriNode, IoTConcentrator.parentNode, DataSourceGraph.GetNodeUri(this.node.Parent));

			Result.Add(NodeGraphUriNode, DublinCoreTerms.updated, this.node.LastChanged);
			Result.Add(NodeGraphUriNode, IoTConcentrator.hasChildSources, this.node.HasChildren);
			Result.Add(NodeGraphUriNode, IoTConcentrator.hasCommands, this.node.HasCommands);
			Result.Add(NodeGraphUriNode, IoTConcentrator.isControllable, this.node.IsControllable);
			Result.Add(NodeGraphUriNode, IoTConcentrator.isReadable, this.node.IsReadable);
			Result.Add(NodeGraphUriNode, IoTConcentrator.state, this.node.State.ToString(),
				IoTConcentrator.nodeState);

			IEnumerable<DisplayableParameters.Parameter> Parameters = await this.node.GetDisplayableParametersAsync(Language, Caller);
			ChunkedList<ISemanticElement> Items = new ChunkedList<ISemanticElement>();

			if (!(Parameters is null))
			{
				foreach (DisplayableParameters.Parameter P in Parameters)
				{
					BlankNode DisplayableParameter = new BlankNode("n" + Guid.NewGuid().ToString());
					Items.Add(DisplayableParameter);

					Result.Add(DisplayableParameter, RdfSchema.label, P.Name);
					Result.Add(DisplayableParameter, IoTConcentrator.parameterId, P.Id);

					Result.Add(DisplayableParameter, IoTSensorData.value,
						SemanticElements.Encapsulate(P.UntypedValue));
				}

				Result.AddContainer(NodeGraphUriNode, Rdf.Seq, IoTConcentrator.dispParam, Items);
				Items.Clear();
			}

			IEnumerable<DisplayableParameters.Message> NodeMessages = await this.node.GetMessagesAsync(Caller);
			if (!(NodeMessages is null))
			{
				foreach (DisplayableParameters.Message M in NodeMessages)
				{
					BlankNode Message = new BlankNode("n" + Guid.NewGuid().ToString());
					Items.Add(Message);

					Result.Add(Message, IoTConcentrator.body, M.Body);
					Result.Add(Message, IoTSensorData.timestamp, M.Timestamp);

					if (!string.IsNullOrEmpty(M.EventId))
						Result.Add(Message, IoTConcentrator.eventId, M.EventId);

					Result.Add(Message, Rdf.type, M.Type.ToString(), IoTConcentrator.messageType);
				}

				Result.AddContainer(NodeGraphUriNode, Rdf.Seq, IoTConcentrator.messages, Items);
				Items.Clear();
			}

			if (this.node.HasChildren)
			{
				foreach (INode ChildNode in await this.node.ChildNodes)
					Items.Add(new UriNode(DataSourceGraph.GetNodeUri(ChildNode)));

				Result.AddContainer(NodeGraphUriNode, Rdf.Bag, IoTConcentrator.childNodes, Items);
				Items.Clear();
			}

			if (this.node.HasCommands)
			{
				IEnumerable<ICommand> NodeCommands = await this.node.Commands;

				if (!(NodeCommands is null))
				{
					foreach (ICommand Command in NodeCommands)
					{
						if (!await Command.CanExecuteAsync(Caller))
							continue;

						UriNode CommandUriNode = new UriNode(DataSourceGraph.GetCommandUri(this.node, Command));

						Result.Add(CommandUriNode, Rdf.type, IoTConcentrator.Command);
						Result.Add(CommandUriNode, RdfSchema.label,
							await Command.GetNameAsync(Language), Language.Code);
						Result.Add(CommandUriNode, IoTConcentrator.commandId, Command.CommandID);
						Result.Add(CommandUriNode, IoTConcentrator.sortCategory, Command.SortCategory);
						Result.Add(CommandUriNode, IoTConcentrator.sortKey, Command.SortKey);
						Result.Add(CommandUriNode, IoTConcentrator.commandType,
							Command.Type.ToString(), IoTConcentrator.commandType);

						AddOptionalStringLiteral(Result, CommandUriNode,
							IoTConcentrator.success,
							await Command.GetSuccessStringAsync(Language));

						AddOptionalStringLiteral(Result, CommandUriNode,
							IoTConcentrator.failure,
							await Command.GetFailureStringAsync(Language));

						AddOptionalStringLiteral(Result, CommandUriNode,
							IoTConcentrator.confirmation,
							await Command.GetConfirmationStringAsync(Language));

						Items.Add(CommandUriNode);
					}
				}

				Result.AddContainer(NodeGraphUriNode, Rdf.Seq, IoTConcentrator.commands, Items);
				Items.Clear();
			}

			AddOperation(Result, Items, IoTConcentrator.edit, DataSourceGraph.GetOperationUri(this.node, "edit", "parameters"),
				await Language.GetStringAsync(typeof(DataSourceGraph), 1, "Editable parameters for the node."));

			if (this.node.IsReadable && this.node is ISensor)
			{
				AddOperation(Result, Items, IoTConcentrator.read, DataSourceGraph.GetOperationUri(this.node, "read", "all"),
					await Language.GetStringAsync(typeof(DataSourceGraph), 2, "Read all available sensor data from node."));

				AddOperation(Result, Items, IoTConcentrator.read, DataSourceGraph.GetOperationUri(this.node, "read", "momentary"),
					await Language.GetStringAsync(typeof(DataSourceGraph), 3, "Read momentary sensor data from node."));

				AddOperation(Result, Items, IoTConcentrator.read, DataSourceGraph.GetOperationUri(this.node, "read", "identity"),
					await Language.GetStringAsync(typeof(DataSourceGraph), 4, "Read identity sensor data from node."));

				AddOperation(Result, Items, IoTConcentrator.read, DataSourceGraph.GetOperationUri(this.node, "read", "status"),
					await Language.GetStringAsync(typeof(DataSourceGraph), 5, "Read status sensor data from node."));

				AddOperation(Result, Items, IoTConcentrator.read, DataSourceGraph.GetOperationUri(this.node, "read", "computed"),
					await Language.GetStringAsync(typeof(DataSourceGraph), 6, "Read computed sensor data from node."));

				AddOperation(Result, Items, IoTConcentrator.read, DataSourceGraph.GetOperationUri(this.node, "read", "peak"),
					await Language.GetStringAsync(typeof(DataSourceGraph), 7, "Read peak sensor data from node."));

				AddOperation(Result, Items, IoTConcentrator.read, DataSourceGraph.GetOperationUri(this.node, "read", "historical"),
					await Language.GetStringAsync(typeof(DataSourceGraph), 8, "Read historical sensor data from node."));

				AddOperation(Result, Items, IoTConcentrator.read, DataSourceGraph.GetOperationUri(this.node, "read", "nonHistorical"),
					await Language.GetStringAsync(typeof(DataSourceGraph), 9, "Read all non-historical sensor data from node."));
			}

			if (this.node.IsControllable && this.node is IActuator Actuator)
			{
				foreach (ControlParameter P in await Actuator.GetControlParameters())
				{
					AddOperation(Result, Items, IoTConcentrator.control, DataSourceGraph.GetOperationUri(this.node, "control", P.Name),
						string.IsNullOrEmpty(P.Description) ? await Language.GetStringAsync(typeof(DataSourceGraph), 10, "Control parameter.") : P.Description);
				}
			}

			Result.AddContainer(NodeGraphUriNode, Rdf.Seq, IoTConcentrator.operations, Items);
		}

		private static void AddOptionalStringLiteral(InMemorySemanticCube Result,
			ISemanticElement Subject, Uri PredicateUri, string Value)
		{
			if (!string.IsNullOrEmpty(Value))
				Result.Add(Subject, PredicateUri, Value);
		}

		private static void AddOperation(InMemorySemanticCube Result,
			ChunkedList<ISemanticElement> Operations, Uri Predicate, Uri Graph, string Label)
		{
			BlankNode Operation = new BlankNode("n" + Guid.NewGuid().ToString());
			Operations.Add(Operation);

			Result.Add(Operation, Predicate, Graph);
			Result.Add(Operation, RdfSchema.label, Label);
		}

	}
}
