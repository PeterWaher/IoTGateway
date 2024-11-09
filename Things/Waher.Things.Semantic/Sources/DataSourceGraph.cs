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

			if (c <= 1 || !string.IsNullOrEmpty(Parts[0]))
				return Grade.NotAtAll;

			if (string.IsNullOrEmpty(Parts[c - 1]))
				c--;

			if (c <= 2 || !Gateway.ConcentratorServer.TryGetDataSource(HttpUtility.UrlDecode(Parts[1]), out _))
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

			if (c <= 1 || !string.IsNullOrEmpty(Parts[0]))
				return null;

			if (string.IsNullOrEmpty(Parts[c - 1]))
				c--;

			if (c <= 2)
				return null;

			string SourceID = HttpUtility.UrlDecode(Parts[1]);
			if (!Gateway.ConcentratorServer.TryGetDataSource(SourceID, out IDataSource Source))
				return null;

			if (!await Source.CanViewAsync(Caller))
				return null;

			InMemorySemanticCube Result = new InMemorySemanticCube();
			Language Language = await Translator.GetDefaultLanguageAsync();  // TODO: Check Accept-Language HTTP header.

			switch (c)
			{
				case 2: // DOMAIN/Source
					await AppendSourceInformation(Result, Source, Language, null);
					break;

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

			Result.Add(new SemanticTriple(
				SourceGraphUriNode,
				new UriNode(DublinCore.Terms.Type),
				new UriNode(IoTConcentrator.DataSource)));

			Result.Add(new SemanticTriple(
				SourceGraphUriNode,
				new UriNode(IoTConcentrator.SourceId),
				new StringLiteral(Source.SourceID)));

			Result.Add(new SemanticTriple(
				SourceGraphUriNode,
				new UriNode(RdfSchema.Label),
				new StringLiteral(await Source.GetNameAsync(Language))));

			Result.Add(new SemanticTriple(
				SourceGraphUriNode,
				new UriNode(DublinCore.Terms.Updated),
				new DateTimeLiteral(Source.LastChanged)));

			Result.Add(new SemanticTriple(
				SourceGraphUriNode,
				new UriNode(IoTConcentrator.HasChildSource),
				new BooleanLiteral(Source.HasChildren)));

			if (Source.HasChildren)
			{
				foreach (IDataSource ChildSource in Source.ChildSources)
				{
					Result.Add(new SemanticTriple(
						SourceGraphUriNode,
						new UriNode(IoTConcentrator.ChildSource),
						new UriNode(new Uri(Gateway.GetUrl("/" + HttpUtility.UrlEncode(ChildSource.SourceID))))));
				}
			}

			IEnumerable<INode> RootNodes = Source.RootNodes;

			if (!(RootNodes is null))
			{
				foreach (INode RootNode in RootNodes)
				{
					Result.Add(new SemanticTriple(
						SourceGraphUriNode,
						new UriNode(IoTConcentrator.RootNode),
						new UriNode(GetNodeUri(RootNode))));
				}
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

			Result.Add(new SemanticTriple(
				NodeGraphUriNode,
				new UriNode(DublinCore.Terms.Type),
				new UriNode(IoTConcentrator.Node)));

			Result.Add(new SemanticTriple(
				NodeGraphUriNode,
				new UriNode(IoTConcentrator.TypeName),
				new StringLiteral(await Node.GetTypeNameAsync(Language))));

			Result.Add(new SemanticTriple(
				NodeGraphUriNode,
				new UriNode(IoTConcentrator.SourceId),
				new StringLiteral(Node.SourceId)));

			Result.Add(new SemanticTriple(
				NodeGraphUriNode,
				new UriNode(IoTConcentrator.NodeId),
				new StringLiteral(Node.NodeId)));

			Result.Add(new SemanticTriple(
				NodeGraphUriNode,
				new UriNode(IoTConcentrator.LogId),
				new StringLiteral(Node.LogId)));

			Result.Add(new SemanticTriple(
				NodeGraphUriNode,
				new UriNode(IoTConcentrator.LocalId),
				new StringLiteral(Node.LocalId)));

			Result.Add(new SemanticTriple(
				NodeGraphUriNode,
				new UriNode(RdfSchema.Label),
				new StringLiteral(Node.LocalId)));

			if (!string.IsNullOrEmpty(Node.Partition))
			{
				Result.Add(new SemanticTriple(
					NodeGraphUriNode,
					new UriNode(IoTConcentrator.Partition),
					new StringLiteral(Node.Partition)));
			}

			if (!(Node.Parent is null))
			{
				Result.Add(new SemanticTriple(
					NodeGraphUriNode,
					new UriNode(IoTConcentrator.ParentNode),
					new UriNode(GetNodeUri(Node.Parent))));
			}

			Result.Add(new SemanticTriple(
				NodeGraphUriNode,
				new UriNode(DublinCore.Terms.Updated),
				new DateTimeLiteral(Node.LastChanged)));

			Result.Add(new SemanticTriple(
				NodeGraphUriNode,
				new UriNode(IoTConcentrator.HasChildSource),
				new BooleanLiteral(Node.HasChildren)));

			Result.Add(new SemanticTriple(
				NodeGraphUriNode,
				new UriNode(IoTConcentrator.HasCommands),
				new BooleanLiteral(Node.HasCommands)));

			Result.Add(new SemanticTriple(
				NodeGraphUriNode,
				new UriNode(IoTConcentrator.IsControllable),
				new BooleanLiteral(Node.IsControllable)));

			Result.Add(new SemanticTriple(
				NodeGraphUriNode,
				new UriNode(IoTConcentrator.IsReadable),
				new BooleanLiteral(Node.IsReadable)));

			Result.Add(new SemanticTriple(
				NodeGraphUriNode,
				new UriNode(IoTConcentrator.State),
				new CustomLiteral(Node.State.ToString(), IoTConcentrator.NodeState.AbsoluteUri)));

			IEnumerable<DisplayableParameters.Parameter> Parameters = await Node.GetDisplayableParametersAsync(Language, Caller);
			int ItemIndex;

			if (!(Parameters is null))
			{
				BlankNode DisplayableParameters = new BlankNode("n" + Guid.NewGuid().ToString());
				ItemIndex = 0;

				Result.Add(new SemanticTriple(
					NodeGraphUriNode,
					new UriNode(IoTConcentrator.DisplayableParameters),
					DisplayableParameters));

				foreach (DisplayableParameters.Parameter P in Parameters)
				{
					BlankNode DisplayableParameter = new BlankNode("n" + Guid.NewGuid().ToString());

					Result.Add(new SemanticTriple(
						DisplayableParameters,
						new UriNode(Rdf.ListItem(++ItemIndex)),
						DisplayableParameter));

					Result.Add(new SemanticTriple(
						DisplayableParameter,
						new UriNode(RdfSchema.Label),
						new StringLiteral(P.Name)));

					Result.Add(new SemanticTriple(
						DisplayableParameter,
						new UriNode(IoTConcentrator.ParameterId),
						new StringLiteral(P.Id)));

					Result.Add(new SemanticTriple(
						DisplayableParameter,
						new UriNode(IoTSensorData.Value),
						SemanticElements.Encapsulate(P.UntypedValue)));
				}
			}

			IEnumerable<DisplayableParameters.Message> NodeMessages = await Node.GetMessagesAsync(Caller);
			if (!(NodeMessages is null))
			{
				BlankNode Messages = new BlankNode("n" + Guid.NewGuid().ToString());
				ItemIndex = 0;

				Result.Add(new SemanticTriple(
					NodeGraphUriNode,
					new UriNode(IoTConcentrator.Messages),
					Messages));

				foreach (DisplayableParameters.Message M in NodeMessages)
				{
					BlankNode Message = new BlankNode("n" + Guid.NewGuid().ToString());

					Result.Add(new SemanticTriple(
						Messages,
						new UriNode(Rdf.ListItem(++ItemIndex)),
						Message));

					Result.Add(new SemanticTriple(
						Message,
						new UriNode(IoTConcentrator.Body),
						new StringLiteral(M.Body)));

					Result.Add(new SemanticTriple(
						Message,
						new UriNode(IoTSensorData.Timestamp),
						new DateTimeLiteral(M.Timestamp)));

					if (!string.IsNullOrEmpty(M.EventId))
					{
						Result.Add(new SemanticTriple(
							Message,
							new UriNode(IoTConcentrator.EventId),
							new StringLiteral(M.EventId)));
					}

					Result.Add(new SemanticTriple(
						Message,
						new UriNode(Rdf.Type),
						new CustomLiteral(M.Type.ToString(), IoTConcentrator.MessageType.AbsoluteUri)));
				}
			}

			if (Node.HasChildren)
			{
				foreach (INode ChildNode in await Node.ChildNodes)
				{
					Result.Add(new SemanticTriple(
						NodeGraphUriNode,
						new UriNode(IoTConcentrator.ChildNode),
						new UriNode(GetNodeUri(ChildNode))));
				}
			}

			if (Node.HasCommands)
			{
				BlankNode Commands = new BlankNode("n" + Guid.NewGuid().ToString());
				ItemIndex = 0;

				Result.Add(new SemanticTriple(
					NodeGraphUriNode,
					new UriNode(IoTConcentrator.Commands),
					Commands));

				IEnumerable<ICommand> NodeCommands = await Node.Commands;

				if (!(Commands is null))
				{
					foreach (ICommand Command in NodeCommands)
					{
						if (!await Command.CanExecuteAsync(Caller))
							continue;

						UriNode CommandUriNode = new UriNode(GetCommandUri(Node, Command));

						Result.Add(new SemanticTriple(
							Commands,
							new UriNode(Rdf.ListItem(++ItemIndex)),
							CommandUriNode));

						Result.Add(new SemanticTriple(
							CommandUriNode,
							new UriNode(DublinCore.Terms.Type),
							new UriNode(IoTConcentrator.Command)));

						Result.Add(new SemanticTriple(
							CommandUriNode,
							new UriNode(RdfSchema.Label),
							new StringLiteral(await Command.GetNameAsync(Language))));

						Result.Add(new SemanticTriple(
							CommandUriNode,
							new UriNode(IoTConcentrator.CommandId),
							new StringLiteral(Command.CommandID)));

						Result.Add(new SemanticTriple(
							CommandUriNode,
							new UriNode(IoTConcentrator.SortCategory),
							new StringLiteral(Command.SortCategory)));

						Result.Add(new SemanticTriple(
							CommandUriNode,
							new UriNode(IoTConcentrator.SortKey),
							new StringLiteral(Command.SortKey)));

						Result.Add(new SemanticTriple(
							CommandUriNode,
							new UriNode(IoTConcentrator.CommandType),
							new CustomLiteral(Command.Type.ToString(), IoTConcentrator.CommandType.AbsoluteUri)));

						AddOptionalStringLiteral(Result, CommandUriNode, IoTConcentrator.Success,
							await Command.GetSuccessStringAsync(Language));

						AddOptionalStringLiteral(Result, CommandUriNode, IoTConcentrator.Failure,
							await Command.GetFailureStringAsync(Language));

						AddOptionalStringLiteral(Result, CommandUriNode, IoTConcentrator.Confirmation,
							await Command.GetConfirmationStringAsync(Language));
					}
				}
			}

			BlankNode Operations = new BlankNode("n" + Guid.NewGuid().ToString());
			ItemIndex = 0;

			Result.Add(new SemanticTriple(
				NodeGraphUriNode,
				new UriNode(IoTConcentrator.Operations),
				Operations));

			AddOperation(Result, Operations, ref ItemIndex, IoTConcentrator.Edit, GetOperationUri(Node, "edit", "parameters"),
				await Language.GetStringAsync(typeof(DataSourceGraph), 1, "Editable parameters for the node."));

			if (Node.IsReadable && Node is ISensor)
			{
				AddOperation(Result, Operations, ref ItemIndex, IoTConcentrator.Read, GetOperationUri(Node, "read", "all"),
					await Language.GetStringAsync(typeof(DataSourceGraph), 2, "Read all available sensor data from node."));

				AddOperation(Result, Operations, ref ItemIndex, IoTConcentrator.Read, GetOperationUri(Node, "read", "momentary"),
					await Language.GetStringAsync(typeof(DataSourceGraph), 3, "Read momentary sensor data from node."));

				AddOperation(Result, Operations, ref ItemIndex, IoTConcentrator.Read, GetOperationUri(Node, "read", "identity"),
					await Language.GetStringAsync(typeof(DataSourceGraph), 4, "Read identity sensor data from node."));

				AddOperation(Result, Operations, ref ItemIndex, IoTConcentrator.Read, GetOperationUri(Node, "read", "status"),
					await Language.GetStringAsync(typeof(DataSourceGraph), 5, "Read status sensor data from node."));

				AddOperation(Result, Operations, ref ItemIndex, IoTConcentrator.Read, GetOperationUri(Node, "read", "computed"),
					await Language.GetStringAsync(typeof(DataSourceGraph), 6, "Read computed sensor data from node."));

				AddOperation(Result, Operations, ref ItemIndex, IoTConcentrator.Read, GetOperationUri(Node, "read", "peak"),
					await Language.GetStringAsync(typeof(DataSourceGraph), 7, "Read peak sensor data from node."));

				AddOperation(Result, Operations, ref ItemIndex, IoTConcentrator.Read, GetOperationUri(Node, "read", "historical"),
					await Language.GetStringAsync(typeof(DataSourceGraph), 8, "Read historical sensor data from node."));

				AddOperation(Result, Operations, ref ItemIndex, IoTConcentrator.Read, GetOperationUri(Node, "read", "nonHistorical"),
					await Language.GetStringAsync(typeof(DataSourceGraph), 9, "Read all non-historical sensor data from node."));
			}

			if (Node.IsControllable && Node is IActuator Actuator)
			{
				foreach (ControlParameter P in await Actuator.GetControlParameters())
				{
					AddOperation(Result, Operations, ref ItemIndex, IoTConcentrator.Control, GetOperationUri(Node, "control", P.Name),
						string.IsNullOrEmpty(P.Description) ? await Language.GetStringAsync(typeof(DataSourceGraph), 10, "Control parameter.") : P.Description);
				}
			}
		}

		private static void AddOptionalStringLiteral(InMemorySemanticCube Result,
			ISemanticElement Subject, Uri PredicateUri, string Value)
		{
			if (!string.IsNullOrEmpty(Value))
			{
				Result.Add(new SemanticTriple(
					Subject,
					new UriNode(PredicateUri),
					new StringLiteral(Value)));
			}
		}

		private static void AddOperation(InMemorySemanticCube Result, BlankNode Operations, ref int ItemIndex, Uri Predicate, Uri Graph, string Label)
		{
			BlankNode Operation = new BlankNode("n" + Guid.NewGuid().ToString());

			Result.Add(new SemanticTriple(
				Operations,
				new UriNode(Rdf.ListItem(++ItemIndex)),
				Operation));

			Result.Add(new SemanticTriple(
				Operation,
				new UriNode(Predicate),
				new UriNode(Graph)));

			Result.Add(new SemanticTriple(
				Operation,
				new UriNode(RdfSchema.Label),
				new StringLiteral(Label)));
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

					Result.Add(new SemanticTriple(
						NodeGraphUriNode,
						new UriNode(IoTSensorData.Fields),
						Fields));

					BlankNode Errors = new BlankNode("n" + Guid.NewGuid().ToString());
					int ErrorIndex = 0;

					Result.Add(new SemanticTriple(
						NodeGraphUriNode,
						new UriNode(IoTSensorData.Errors),
						Errors));

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

								Result.Add(new SemanticTriple(
									Fields,
									new UriNode(Rdf.ListItem(++FieldIndex)),
									FieldNode));

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
								{
									Result.Add(new SemanticTriple(
										FieldNode,
										new UriNode(RdfSchema.Label),
										new StringLiteral(F.Name)));
								}
								else if (LocalizedName == F.Name)
								{
									Result.Add(new SemanticTriple(
										FieldNode,
										new UriNode(RdfSchema.Label),
										new StringLiteral(F.Name, Language.Code)));
								}
								else
								{
									Result.Add(new SemanticTriple(
										FieldNode,
										new UriNode(RdfSchema.Label),
										new StringLiteral(F.Name)));

									Result.Add(new SemanticTriple(
										FieldNode,
										new UriNode(RdfSchema.Label),
										new StringLiteral(LocalizedName, Language.Code)));
								}

								Result.Add(new SemanticTriple(
									FieldNode,
									new UriNode(IoTSensorData.Value),
									SemanticElements.Encapsulate(F.ObjectValue)));

								Result.Add(new SemanticTriple(
									FieldNode,
									new UriNode(IoTSensorData.Timestamp),
									new DateTimeLiteral(F.Timestamp)));

								if (F.QoS.HasFlag(FieldQoS.Missing))
								{
									Result.Add(new SemanticTriple(
										FieldNode,
										new UriNode(IoTSensorData.QoSUri),
										new CustomLiteral(nameof(FieldQoS.Missing), IoTSensorData.QoS)));
								}

								if (F.QoS.HasFlag(FieldQoS.InProgress))
								{
									Result.Add(new SemanticTriple(
										FieldNode,
										new UriNode(IoTSensorData.QoSUri),
										new CustomLiteral(nameof(FieldQoS.InProgress), IoTSensorData.QoS)));
								}

								if (F.QoS.HasFlag(FieldQoS.AutomaticEstimate))
								{
									Result.Add(new SemanticTriple(
										FieldNode,
										new UriNode(IoTSensorData.QoSUri),
										new CustomLiteral(nameof(FieldQoS.AutomaticEstimate), IoTSensorData.QoS)));
								}

								if (F.QoS.HasFlag(FieldQoS.ManualEstimate))
								{
									Result.Add(new SemanticTriple(
										FieldNode,
										new UriNode(IoTSensorData.QoSUri),
										new CustomLiteral(nameof(FieldQoS.ManualEstimate), IoTSensorData.QoS)));
								}

								if (F.QoS.HasFlag(FieldQoS.ManualReadout))
								{
									Result.Add(new SemanticTriple(
										FieldNode,
										new UriNode(IoTSensorData.QoSUri),
										new CustomLiteral(nameof(FieldQoS.ManualReadout), IoTSensorData.QoS)));
								}

								if (F.QoS.HasFlag(FieldQoS.AutomaticReadout))
								{
									Result.Add(new SemanticTriple(
										FieldNode,
										new UriNode(IoTSensorData.QoSUri),
										new CustomLiteral(nameof(FieldQoS.AutomaticReadout), IoTSensorData.QoS)));
								}

								if (F.QoS.HasFlag(FieldQoS.TimeOffset))
								{
									Result.Add(new SemanticTriple(
										FieldNode,
										new UriNode(IoTSensorData.QoSUri),
										new CustomLiteral(nameof(FieldQoS.TimeOffset), IoTSensorData.QoS)));
								}

								if (F.QoS.HasFlag(FieldQoS.Warning))
								{
									Result.Add(new SemanticTriple(
										FieldNode,
										new UriNode(IoTSensorData.QoSUri),
										new CustomLiteral(nameof(FieldQoS.Warning), IoTSensorData.QoS)));
								}

								if (F.QoS.HasFlag(FieldQoS.Error))
								{
									Result.Add(new SemanticTriple(
										FieldNode,
										new UriNode(IoTSensorData.QoSUri),
										new CustomLiteral(nameof(FieldQoS.Error), IoTSensorData.QoS)));
								}

								if (F.QoS.HasFlag(FieldQoS.Signed))
								{
									Result.Add(new SemanticTriple(
										FieldNode,
										new UriNode(IoTSensorData.QoSUri),
										new CustomLiteral(nameof(FieldQoS.Signed), IoTSensorData.QoS)));
								}

								if (F.QoS.HasFlag(FieldQoS.Invoiced))
								{
									Result.Add(new SemanticTriple(
										FieldNode,
										new UriNode(IoTSensorData.QoSUri),
										new CustomLiteral(nameof(FieldQoS.Invoiced), IoTSensorData.QoS)));
								}

								if (F.QoS.HasFlag(FieldQoS.EndOfSeries))
								{
									Result.Add(new SemanticTriple(
										FieldNode,
										new UriNode(IoTSensorData.QoSUri),
										new CustomLiteral(nameof(FieldQoS.EndOfSeries), IoTSensorData.QoS)));
								}

								if (F.QoS.HasFlag(FieldQoS.PowerFailure))
								{
									Result.Add(new SemanticTriple(
										FieldNode,
										new UriNode(IoTSensorData.QoSUri),
										new CustomLiteral(nameof(FieldQoS.PowerFailure), IoTSensorData.QoS)));
								}

								if (F.QoS.HasFlag(FieldQoS.InvoiceConfirmed))
								{
									Result.Add(new SemanticTriple(
										FieldNode,
										new UriNode(IoTSensorData.QoSUri),
										new CustomLiteral(nameof(FieldQoS.InvoiceConfirmed), IoTSensorData.QoS)));
								}

								Result.Add(new SemanticTriple(
									FieldNode,
									new UriNode(IoTConcentrator.IsControllable),
									new BooleanLiteral(F.Writable)));

								if (F.Type.HasFlag(FieldType.Momentary))
								{
									Result.Add(new SemanticTriple(
										FieldNode,
										new UriNode(Rdf.Type),
										new CustomLiteral(nameof(FieldType.Momentary), IoTSensorData.FieldType)));
								}

								if (F.Type.HasFlag(FieldType.Identity))
								{
									Result.Add(new SemanticTriple(
										FieldNode,
										new UriNode(Rdf.Type),
										new CustomLiteral(nameof(FieldType.Identity), IoTSensorData.FieldType)));
								}

								if (F.Type.HasFlag(FieldType.Status))
								{
									Result.Add(new SemanticTriple(
										FieldNode,
										new UriNode(Rdf.Type),
										new CustomLiteral(nameof(FieldType.Status), IoTSensorData.FieldType)));
								}

								if (F.Type.HasFlag(FieldType.Computed))
								{
									Result.Add(new SemanticTriple(
										FieldNode,
										new UriNode(Rdf.Type),
										new CustomLiteral(nameof(FieldType.Computed), IoTSensorData.FieldType)));
								}

								if (F.Type.HasFlag(FieldType.Peak))
								{
									Result.Add(new SemanticTriple(
										FieldNode,
										new UriNode(Rdf.Type),
										new CustomLiteral(nameof(FieldType.Peak), IoTSensorData.FieldType)));
								}

								if (F.Type.HasFlag(FieldType.Historical))
								{
									Result.Add(new SemanticTriple(
										FieldNode,
										new UriNode(Rdf.Type),
										new CustomLiteral(nameof(FieldType.Historical), IoTSensorData.FieldType)));
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

								Result.Add(new SemanticTriple(
									Errors,
									new UriNode(Rdf.ListItem(++ErrorIndex)),
									ErrorNode));

								Result.Add(new SemanticTriple(
									ErrorNode,
									new UriNode(RdfSchema.Label),
									new StringLiteral(Error.ErrorMessage)));

								Result.Add(new SemanticTriple(
									ErrorNode,
									new UriNode(IoTSensorData.Timestamp),
									new DateTimeLiteral(Error.Timestamp)));

								if (!string.IsNullOrEmpty((Error.NodeId)))
								{
									Result.Add(new SemanticTriple(
										ErrorNode,
										new UriNode(IoTConcentrator.NodeId),
										new StringLiteral(Error.NodeId)));
								}

								if (!string.IsNullOrEmpty((Error.SourceId)))
								{
									Result.Add(new SemanticTriple(
										ErrorNode,
										new UriNode(IoTConcentrator.SourceId),
										new StringLiteral(Error.SourceId)));
								}

								if (!string.IsNullOrEmpty((Error.Partition)))
								{
									Result.Add(new SemanticTriple(
										ErrorNode,
										new UriNode(IoTConcentrator.Partition),
										new StringLiteral(Error.Partition)));
								}
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

						Result.Add(new SemanticTriple(
							Errors,
							new UriNode(Rdf.ListItem(++ErrorIndex)),
							ErrorNode));

						Result.Add(new SemanticTriple(
							ErrorNode,
							new UriNode(RdfSchema.Label),
							new StringLiteral("Timeout.")));

						Result.Add(new SemanticTriple(
							ErrorNode,
							new UriNode(IoTSensorData.Timestamp),
							new DateTimeLiteral(DateTime.UtcNow)));
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
