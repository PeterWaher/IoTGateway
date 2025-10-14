using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using Waher.Content.Semantic;
using Waher.Content.Semantic.Model;
using Waher.Content.Semantic.Model.Literals;
using Waher.Content.Semantic.Ontologies;
using Waher.IoTGateway;
using Waher.IoTGateway.Setup;
using Waher.Networking.HTTP;
using Waher.Networking.XMPP.Sensor;
using Waher.Runtime.Inventory;
using Waher.Runtime.Language;
using Waher.Script.Model;
using Waher.Script.Persistence.SPARQL;
using Waher.Things.ControlParameters;
using Waher.Things.Semantic.Ontologies;
using Waher.Things.SensorData;

namespace Waher.Things.Semantic.Sources
{
	/// <summary>
	/// Makes harmonized data sources and nodes available as semantic information.
	/// </summary>
	public class DataSourceGraph : IGraphSource
	{
		/// <summary>
		/// Makes harmonized data sources and nodes available as semantic information.
		/// </summary>
		public DataSourceGraph()
		{
		}

		/// <summary>
		/// How well the source handles a given Graph URI.
		/// </summary>
		/// <param name="GraphUri">Graph URI</param>
		/// <returns>How well the URI is supported.</returns>
		public Grade Supports(Uri GraphUri)
		{
			if (!Gateway.IsDomain(GraphUri.Host, true) ||
				!string.IsNullOrEmpty(GraphUri.Query) ||
				!string.IsNullOrEmpty(GraphUri.Fragment) ||
				Gateway.ConcentratorServer is null)
			{
				return Grade.NotAtAll;
			}

			string s = GraphUri.AbsolutePath;
			string[] Parts = s.Split('/');
			int c = Parts.Length;

			if (c < 1 || !string.IsNullOrEmpty(Parts[0]))
				return Grade.NotAtAll;

			if (string.IsNullOrEmpty(Parts[c - 1]))
				c--;

			if (c == 1)
				return Grade.Ok;

			if (!Gateway.ConcentratorServer.TryGetDataSource(HttpUtility.UrlDecode(Parts[1]), out _))
				return Grade.NotAtAll;

			return Grade.Excellent;
		}

		/// <summary>
		/// Loads the graph
		/// </summary>
		/// <param name="GraphUri">Graph URI</param>
		/// <param name="Node">Node performing the loading.</param>
		/// <param name="NullIfNotFound">If null should be returned, if graph is not found.</param>
		/// <param name="Caller">Information about entity making the request.</param>
		/// <returns>Graph, if found, null if not found, and null can be returned.</returns>
		public async Task<ISemanticCube> LoadGraph(Uri GraphUri, ScriptNode Node, bool NullIfNotFound,
			RequestOrigin Caller)
		{
			if (!Gateway.IsDomain(GraphUri.Host, true) ||
				!string.IsNullOrEmpty(GraphUri.Query) ||
				!string.IsNullOrEmpty(GraphUri.Fragment) ||
				Gateway.ConcentratorServer is null)
			{
				return null;
			}

			string s = GraphUri.AbsolutePath;
			string[] Parts = s.Split('/');
			int c = Parts.Length;

			if (c < 1 || !string.IsNullOrEmpty(Parts[0]))
				return null;

			if (string.IsNullOrEmpty(Parts[c - 1]))
				c--;

			InMemorySemanticCube Result = new InMemorySemanticCube();
			Language Language = await Translator.GetDefaultLanguageAsync();  // TODO: Check Accept-Language HTTP header.

			if (c == 1)
			{
				await AppendBrokerInformation(Result);
				return Result;
			}

			string SourceID = HttpUtility.UrlDecode(Parts[1]);
			if (!Gateway.ConcentratorServer.TryGetDataSource(SourceID, out IDataSource Source))
				return null;

			if (!await Source.CanViewAsync(Caller))
				return null;

			if (c == 2) // DOMAIN/Source
			{
				await AppendSourceInformation(Result, Source, Language, null);
				return Result;
			}

			switch (c)
			{
				case 3: // /DOMAIN/Source/NodeID
					string NodeID = HttpUtility.UrlDecode(Parts[2]);
					INode NodeObj = await Source.GetNodeAsync(new ThingReference(NodeID, SourceID));
					if (NodeObj is null)
						return null;

					await AppendNodeInformation(Result, NodeObj, Language, Caller);
					break;

				case 4: // /DOMAIN/Source/Partition/NodeID
					string Partition = HttpUtility.UrlDecode(Parts[2]);
					NodeID = HttpUtility.UrlDecode(Parts[3]);
					NodeObj = await Source.GetNodeAsync(new ThingReference(NodeID, SourceID, Partition));
					if (NodeObj is null)
						return null;

					await AppendNodeInformation(Result, NodeObj, Language, Caller);
					break;

				case 5: // /DOMAIN/Source/NodeID/Category/Action
					NodeID = HttpUtility.UrlDecode(Parts[2]);
					NodeObj = await Source.GetNodeAsync(new ThingReference(NodeID, SourceID));
					if (NodeObj is null)
						return null;

					return await GetNodeActionGraph(Result, NodeObj, Language, HttpUtility.UrlDecode(Parts[3]), 
						HttpUtility.UrlDecode(Parts[4]), Caller);

				case 6: // /DOMAIN/Source/Partition/NodeID/Category/Action
					Partition = HttpUtility.UrlDecode(Parts[2]);
					NodeID = HttpUtility.UrlDecode(Parts[3]);
					NodeObj = await Source.GetNodeAsync(new ThingReference(NodeID, SourceID, Partition));
					if (NodeObj is null)
						return null;

					return await GetNodeActionGraph(Result, NodeObj, Language, HttpUtility.UrlDecode(Parts[4]), 
						HttpUtility.UrlDecode(Parts[5]), Caller);

				default:
					return null;
			}

			return Result;
		}

		/// <summary>
		/// Appends semantic information about the broker to a graph.
		/// </summary>
		/// <param name="Result">Graph being generated.</param>
		public static Task AppendBrokerInformation(InMemorySemanticCube Result)
		{
			string BrokerPath = "/";
			UriNode BrokerGraphUriNode = new UriNode(new Uri(Gateway.GetUrl(BrokerPath)));
			int i, c;

			Result.Add(BrokerGraphUriNode, Rdf.type, IoTConcentrator.Broker);

			if (!string.IsNullOrEmpty(DomainConfiguration.Instance.HumanReadableName))
			{
				Result.Add(BrokerGraphUriNode, RdfSchema.label,
					DomainConfiguration.Instance.HumanReadableName,
					DomainConfiguration.Instance.HumanReadableNameLanguage);
			}

			if ((c = (DomainConfiguration.Instance.LocalizedNames?.Length) ?? 0) > 0)
			{
				ISemanticElement[] Items = new ISemanticElement[c];

				for (i = 0; i < Items.Length; i++)
				{
					Items[i] = new StringLiteral(
						DomainConfiguration.Instance.LocalizedNames[i].Value,
						DomainConfiguration.Instance.LocalizedNames[i].Key);
				}

				if (!string.IsNullOrEmpty(DomainConfiguration.Instance.HumanReadableName))
				{
					Result.Add(BrokerGraphUriNode, Skos.prefLabel,
						DomainConfiguration.Instance.HumanReadableName,
						DomainConfiguration.Instance.HumanReadableNameLanguage);
				}

				Result.AddLinkedList(BrokerGraphUriNode, Skos.altLabel, Items);
			}

			if (!string.IsNullOrEmpty(DomainConfiguration.Instance.HumanReadableDescription))
			{
				Result.Add(BrokerGraphUriNode, RdfSchema.comment,
					DomainConfiguration.Instance.HumanReadableDescription,
					DomainConfiguration.Instance.HumanReadableDescriptionLanguage);
			}

			if ((c = (DomainConfiguration.Instance.LocalizedDescriptions?.Length) ?? 0) > 0)
			{
				ISemanticElement[] Items = new ISemanticElement[c];

				for (i = 0; i < Items.Length; i++)
				{
					Items[i] = new StringLiteral(
						DomainConfiguration.Instance.LocalizedDescriptions[i].Value,
						DomainConfiguration.Instance.LocalizedDescriptions[i].Key);
				}

				Result.AddLinkedList(BrokerGraphUriNode, RdfSchema.comment, Items);
			}

			if (DomainConfiguration.Instance.UseDomainName)
			{
				Result.Add(BrokerGraphUriNode, 
					IoTConcentrator.mainDomain,
					DomainConfiguration.Instance.Domain);

				if ((DomainConfiguration.Instance.AlternativeDomains?.Length ?? 0) > 0)
				{
					Result.AddLinkedList(BrokerGraphUriNode,
						IoTConcentrator.alternativeDomains,
						DomainConfiguration.Instance.AlternativeDomains);
				}
			}

			foreach (IDataSource RootSource in Gateway.ConcentratorServer.RootDataSources)
			{
				Result.Add(BrokerGraphUriNode, IoTConcentrator.rootSource,
					new Uri(Gateway.GetUrl("/" + HttpUtility.UrlEncode(RootSource.SourceID))));
			}

			return Task.CompletedTask;
		}

		/// <summary>
		/// Appends semantic information about a data source to a graph.
		/// </summary>
		/// <param name="Result">Graph being generated.</param>
		/// <param name="Source">Source to append information about.</param>
		/// <param name="Language">Language</param>
		/// <param name="Caller">Caller; can be null if authorization has been performed at a higher level.</param>
		public static async Task AppendSourceInformation(InMemorySemanticCube Result,
			IDataSource Source, Language Language, RequestOrigin Caller)
		{
			if (Source is null)
				return;

			if (!(Caller is null) && !await Source.CanViewAsync(Caller))
				return;

			string SourcePath = "/" + HttpUtility.UrlEncode(Source.SourceID);
			UriNode SourceGraphUriNode = new UriNode(new Uri(Gateway.GetUrl(SourcePath)));

			Result.Add(SourceGraphUriNode, Rdf.type, IoTConcentrator.DataSource);
			Result.Add(SourceGraphUriNode, IoTConcentrator.sourceId, Source.SourceID);
			Result.Add(SourceGraphUriNode, RdfSchema.label, await Source.GetNameAsync(Language));
			Result.Add(SourceGraphUriNode, DublinCore.Terms.updated, Source.LastChanged);
			Result.Add(SourceGraphUriNode, IoTConcentrator.hasChildSource, Source.HasChildren);

			if (Source.HasChildren)
			{
				foreach (IDataSource ChildSource in Source.ChildSources)
				{
					Result.Add(SourceGraphUriNode, IoTConcentrator.childSource,
						new Uri(Gateway.GetUrl("/" + HttpUtility.UrlEncode(ChildSource.SourceID))));
				}
			}

			IEnumerable<INode> RootNodes = Source.RootNodes;

			if (!(RootNodes is null))
			{
				foreach (INode RootNode in RootNodes)
					Result.Add(SourceGraphUriNode, IoTConcentrator.rootNode, GetNodeUri(RootNode));
			}
		}

		/// <summary>
		/// Gets the Graph URI for a node.
		/// </summary>
		/// <param name="Node">Node reference.</param>
		/// <returns>Graph URI for Node Graph.</returns>
		public static Uri GetNodeUri(INode Node)
		{
			StringBuilder sb = new StringBuilder();
			GetNodeUri(Node, sb);
			return new Uri(Gateway.GetUrl(sb.ToString()));
		}

		/// <summary>
		/// Gets the Graph URI for a node.
		/// </summary>
		/// <param name="Node">Node reference.</param>
		/// <param name="sb">URI being built.</param>
		private static void GetNodeUri(INode Node, StringBuilder sb)
		{
			sb.Append('/');
			sb.Append(HttpUtility.UrlEncode(Node.SourceId));

			if (!string.IsNullOrEmpty(Node.Partition))
			{
				sb.Append('/');
				sb.Append(HttpUtility.UrlEncode(Node.Partition));
			}

			sb.Append('/');
			sb.Append(HttpUtility.UrlEncode(Node.NodeId));
		}

		/// <summary>
		/// Appends semantic information about a node to a graph.
		/// </summary>
		/// <param name="Result">Graph being generated.</param>
		/// <param name="Node">NOde to append information about.</param>
		/// <param name="Language">Language</param>
		/// <param name="Caller">Caller, required.</param>
		public static async Task AppendNodeInformation(InMemorySemanticCube Result,
			INode Node, Language Language, RequestOrigin Caller)
		{
			if (Node is null)
				return;

			if (!await Node.CanViewAsync(Caller))
				return;

			UriNode NodeGraphUriNode = new UriNode(GetNodeUri(Node));

			Result.Add(NodeGraphUriNode, Rdf.type, IoTConcentrator.Node);

			Result.Add(NodeGraphUriNode, IoTConcentrator.typeName,
				await Node.GetTypeNameAsync(Language));

			Result.Add(NodeGraphUriNode, IoTConcentrator.sourceId, Node.SourceId);
			Result.Add(NodeGraphUriNode, IoTConcentrator.nodeId, Node.NodeId);
			Result.Add(NodeGraphUriNode, IoTConcentrator.logId, Node.LogId);
			Result.Add(NodeGraphUriNode, IoTConcentrator.localId, Node.LocalId);
			Result.Add(NodeGraphUriNode, RdfSchema.label, Node.LocalId);

			if (!string.IsNullOrEmpty(Node.Partition))
				Result.Add(NodeGraphUriNode, IoTConcentrator.partition, Node.Partition);

			if (!(Node.Parent is null))
				Result.Add(NodeGraphUriNode, IoTConcentrator.parentNode, GetNodeUri(Node.Parent));

			Result.Add(NodeGraphUriNode, DublinCore.Terms.updated, Node.LastChanged);
			Result.Add(NodeGraphUriNode, IoTConcentrator.hasChildSource, Node.HasChildren);
			Result.Add(NodeGraphUriNode, IoTConcentrator.hasCommands, Node.HasCommands);
			Result.Add(NodeGraphUriNode, IoTConcentrator.isControllable, Node.IsControllable);
			Result.Add(NodeGraphUriNode, IoTConcentrator.isReadable, Node.IsReadable);
			Result.Add(NodeGraphUriNode, IoTConcentrator.state, Node.State.ToString(), 
				IoTConcentrator.nodeState);

			IEnumerable<DisplayableParameters.Parameter> Parameters = await Node.GetDisplayableParametersAsync(Language, Caller);
			int ItemIndex;

			if (!(Parameters is null))
			{
				BlankNode DisplayableParameters = new BlankNode("n" + Guid.NewGuid().ToString());
				ItemIndex = 0;

				Result.Add(NodeGraphUriNode, IoTConcentrator.dispParam,
					DisplayableParameters);

				foreach (DisplayableParameters.Parameter P in Parameters)
				{
					BlankNode DisplayableParameter = new BlankNode("n" + Guid.NewGuid().ToString());

					Result.Add(DisplayableParameters, Rdf.ListItem(++ItemIndex),
						DisplayableParameter);

					Result.Add(DisplayableParameter, RdfSchema.label, P.Name);
					Result.Add(DisplayableParameter, IoTConcentrator.parameterId, P.Id);

					Result.Add(DisplayableParameter, IoTSensorData.value,
						SemanticElements.Encapsulate(P.UntypedValue));
				}
			}

			IEnumerable<DisplayableParameters.Message> NodeMessages = await Node.GetMessagesAsync(Caller);
			if (!(NodeMessages is null))
			{
				BlankNode Messages = new BlankNode("n" + Guid.NewGuid().ToString());
				ItemIndex = 0;

				Result.Add(NodeGraphUriNode, IoTConcentrator.messages, Messages);

				foreach (DisplayableParameters.Message M in NodeMessages)
				{
					BlankNode Message = new BlankNode("n" + Guid.NewGuid().ToString());

					Result.Add(Messages, Rdf.ListItem(++ItemIndex), Message);
					Result.Add(Message, IoTConcentrator.body, M.Body);
					Result.Add(Message, IoTSensorData.timestamp, M.Timestamp);

					if (!string.IsNullOrEmpty(M.EventId))
						Result.Add(Message, IoTConcentrator.eventId, M.EventId);

					Result.Add(Message, Rdf.type, M.Type.ToString(), IoTConcentrator.messageType);
				}
			}

			if (Node.HasChildren)
			{
				foreach (INode ChildNode in await Node.ChildNodes)
				{
					Result.Add(NodeGraphUriNode, IoTConcentrator.childNode, 
						GetNodeUri(ChildNode));
				}
			}

			if (Node.HasCommands)
			{
				BlankNode Commands = new BlankNode("n" + Guid.NewGuid().ToString());
				ItemIndex = 0;

				Result.Add(NodeGraphUriNode, IoTConcentrator.commands, Commands);

				IEnumerable<ICommand> NodeCommands = await Node.Commands;

				if (!(Commands is null))
				{
					foreach (ICommand Command in NodeCommands)
					{
						if (!await Command.CanExecuteAsync(Caller))
							continue;

						UriNode CommandUriNode = new UriNode(GetCommandUri(Node, Command));

						Result.Add(Commands, Rdf.ListItem(++ItemIndex), CommandUriNode);
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
					}
				}
			}

			BlankNode Operations = new BlankNode("n" + Guid.NewGuid().ToString());
			ItemIndex = 0;

			Result.Add(NodeGraphUriNode, IoTConcentrator.operations, Operations);

			AddOperation(Result, Operations, ref ItemIndex, IoTConcentrator.edit, GetOperationUri(Node, "edit", "parameters"),
				await Language.GetStringAsync(typeof(DataSourceGraph), 1, "Editable parameters for the node."));

			if (Node.IsReadable && Node is ISensor)
			{
				AddOperation(Result, Operations, ref ItemIndex, IoTConcentrator.read, GetOperationUri(Node, "read", "all"),
					await Language.GetStringAsync(typeof(DataSourceGraph), 2, "Read all available sensor data from node."));

				AddOperation(Result, Operations, ref ItemIndex, IoTConcentrator.read, GetOperationUri(Node, "read", "momentary"),
					await Language.GetStringAsync(typeof(DataSourceGraph), 3, "Read momentary sensor data from node."));

				AddOperation(Result, Operations, ref ItemIndex, IoTConcentrator.read, GetOperationUri(Node, "read", "identity"),
					await Language.GetStringAsync(typeof(DataSourceGraph), 4, "Read identity sensor data from node."));

				AddOperation(Result, Operations, ref ItemIndex, IoTConcentrator.read, GetOperationUri(Node, "read", "status"),
					await Language.GetStringAsync(typeof(DataSourceGraph), 5, "Read status sensor data from node."));

				AddOperation(Result, Operations, ref ItemIndex, IoTConcentrator.read, GetOperationUri(Node, "read", "computed"),
					await Language.GetStringAsync(typeof(DataSourceGraph), 6, "Read computed sensor data from node."));

				AddOperation(Result, Operations, ref ItemIndex, IoTConcentrator.read, GetOperationUri(Node, "read", "peak"),
					await Language.GetStringAsync(typeof(DataSourceGraph), 7, "Read peak sensor data from node."));

				AddOperation(Result, Operations, ref ItemIndex, IoTConcentrator.read, GetOperationUri(Node, "read", "historical"),
					await Language.GetStringAsync(typeof(DataSourceGraph), 8, "Read historical sensor data from node."));

				AddOperation(Result, Operations, ref ItemIndex, IoTConcentrator.read, GetOperationUri(Node, "read", "nonHistorical"),
					await Language.GetStringAsync(typeof(DataSourceGraph), 9, "Read all non-historical sensor data from node."));
			}

			if (Node.IsControllable && Node is IActuator Actuator)
			{
				foreach (ControlParameter P in await Actuator.GetControlParameters())
				{
					AddOperation(Result, Operations, ref ItemIndex, IoTConcentrator.control, GetOperationUri(Node, "control", P.Name),
						string.IsNullOrEmpty(P.Description) ? await Language.GetStringAsync(typeof(DataSourceGraph), 10, "Control parameter.") : P.Description);
				}
			}
		}

		private static void AddOptionalStringLiteral(InMemorySemanticCube Result,
			ISemanticElement Subject, Uri PredicateUri, string Value)
		{
			if (!string.IsNullOrEmpty(Value))
				Result.Add(Subject, PredicateUri, Value);
		}

		private static void AddOperation(InMemorySemanticCube Result, BlankNode Operations, ref int ItemIndex, Uri Predicate, Uri Graph, string Label)
		{
			BlankNode Operation = new BlankNode("n" + Guid.NewGuid().ToString());

			Result.Add(Operations, Rdf.ListItem(++ItemIndex), Operation);
			Result.Add(Operation, Predicate, Graph);
			Result.Add(Operation, RdfSchema.label, Label);
		}

		/// <summary>
		/// Gets the Graph URI for a node command.
		/// </summary>
		/// <param name="Node">Node reference.</param>
		/// <param name="Command">Command reference.</param>
		/// <returns>Graph URI for Node Command Graph.</returns>
		public static Uri GetCommandUri(INode Node, ICommand Command)
		{
			StringBuilder sb = new StringBuilder();
			GetCommandUri(Node, Command, sb);
			return new Uri(Gateway.GetUrl(sb.ToString()));
		}

		/// <summary>
		/// Gets the Graph URI for a node command.
		/// </summary>
		/// <param name="Node">Node reference.</param>
		/// <param name="Command">Command reference.</param>
		/// <param name="sb">URI building built.</param>
		private static void GetCommandUri(INode Node, ICommand Command, StringBuilder sb)
		{
			GetNodeUri(Node, sb);
			sb.Append("/cmd/");
			sb.Append(HttpUtility.UrlEncode(Command.CommandID));
		}

		/// <summary>
		/// Gets the Graph URI for a node operation.
		/// </summary>
		/// <param name="Node">Node reference.</param>
		/// <param name="Category">Operation category.</param>
		/// <param name="Action">Operation action</param>
		private static Uri GetOperationUri(INode Node, string Category, string Action)
		{
			StringBuilder sb = new StringBuilder();
			GetOperationUri(Node, Category, Action, sb);
			return new Uri(Gateway.GetUrl(sb.ToString()));
		}

		/// <summary>
		/// Gets the Graph URI for a node operation.
		/// </summary>
		/// <param name="Node">Node reference.</param>
		/// <param name="Category">Operation category.</param>
		/// <param name="Action">Operation action</param>
		/// <param name="sb">URI building built.</param>
		private static void GetOperationUri(INode Node, string Category, string Action, StringBuilder sb)
		{
			GetNodeUri(Node, sb);
			sb.Append('/');
			sb.Append(Category);
			sb.Append('/');
			sb.Append(Action);
		}

		private static async Task<ISemanticCube> GetNodeActionGraph(InMemorySemanticCube Result,
			INode Node, Language Language, string Category, string Action, RequestOrigin Caller)
		{
			UriNode NodeGraphUriNode = new UriNode(GetNodeUri(Node));

			switch (Category)
			{
				case "read":
					if (!(Node is ISensor Sensor))
						return null;

					FieldType FieldTypes = 0;

					foreach (string Part in Action.Split(' '))
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
						return null;

					BlankNode Fields = new BlankNode("n" + Guid.NewGuid().ToString());
					int FieldIndex = 0;

					Result.Add(NodeGraphUriNode, IoTSensorData.fields, Fields);

					BlankNode Errors = new BlankNode("n" + Guid.NewGuid().ToString());
					int ErrorIndex = 0;

					Result.Add(NodeGraphUriNode, IoTSensorData.errors, Errors);

					IThingReference[] Nodes = new IThingReference[] { Node };
					ApprovedReadoutParameters Approval = await Gateway.ConcentratorServer.SensorServer.CanReadAsync(FieldTypes, Nodes, null, Caller)
						?? throw new ForbiddenException("Not authorized to read sensor-data from node.");

					TaskCompletionSource<bool> ReadoutCompleted = new TaskCompletionSource<bool>();
					InternalReadoutRequest Request = await Gateway.ConcentratorServer.SensorServer.DoInternalReadout(Caller.From,
						Approval.Nodes, Approval.FieldTypes, Approval.FieldNames, DateTime.MinValue, DateTime.MaxValue,
						async (Sender, e) =>
						{
							foreach (Field F in e.Fields)
							{
								BlankNode FieldNode = new BlankNode("n" + Guid.NewGuid().ToString());

								Result.Add(Fields, Rdf.ListItem(++FieldIndex), FieldNode);

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
								Result.Add(FieldNode, IoTSensorData.timestamp, F.Timestamp);

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
					break;

				case "cmd":     // TODO
				case "edit":    // TODO
				case "control": // TODO
				default:
					return null;
			}

			return Result;
		}

	}
}
