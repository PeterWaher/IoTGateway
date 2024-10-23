using System.Collections.Generic;
using System.Threading.Tasks;
using Waher.Networking.XMPP.Concentrator;
using Waher.Runtime.Language;
using Waher.Things.Metering;

namespace Waher.Things.Xmpp.Commands
{
	/// <summary>
	/// Scans a source node on a concentrator node for its child sources and root nodes.
	/// </summary>
	public class ScanSource : ConcentratorCommand
	{
		private readonly ConcentratorSourceNode sourceNode;

		/// <summary>
		/// Scans a source node on a concentrator node for its child sources and root nodes.
		/// </summary>
		/// <param name="Concentrator">Concentrator node.</param>
		/// <param name="SourceNode">Data source node.</param>
		public ScanSource(ConcentratorDevice Concentrator, ConcentratorSourceNode SourceNode)
			: base(Concentrator, "1")
		{
			this.sourceNode = SourceNode;
		}

		/// <summary>
		/// ID of command.
		/// </summary>
		public override string CommandID => nameof(ScanSource);

		/// <summary>
		/// Type of command.
		/// </summary>
		public override CommandType Type => CommandType.Simple;

		/// <summary>
		/// Gets the name of data source.
		/// </summary>
		/// <param name="Language">Language to use.</param>
		public override Task<string> GetNameAsync(Language Language)
		{
			return Language.GetStringAsync(typeof(ConcentratorDevice), 53, "Scan Source");
		}

		/// <summary>
		/// Executes the command.
		/// </summary>
		public override async Task ExecuteCommandAsync()
		{
			ConcentratorClient Client = await this.GetConcentratorClient();
			string FullJid = this.GetRemoteFullJid(Client.Client);

			LinkedList<Task> ChildTasks = new LinkedList<Task>();

			await this.ScanChildSources(Client, FullJid, ChildTasks);
			await this.ScanRootNodes(Client, FullJid, ChildTasks);

			if (!(ChildTasks.First is null))
				await Task.WhenAll(ChildTasks);
		}

		private async Task ScanChildSources(ConcentratorClient Client, string FullJid, LinkedList<Task> ChildTasks)
		{
			DataSourceReference[] Sources = await Client.GetChildDataSourcesAsync(
				FullJid, this.sourceNode.RemoteSourceID);

			Dictionary<string, ConcentratorSourceNode> BySourceId = new Dictionary<string, ConcentratorSourceNode>();

			foreach (INode Child in await this.sourceNode.ChildNodes)
			{
				if (Child is ConcentratorSourceNode SourceNode)
					BySourceId[SourceNode.SourceId] = SourceNode;
			}

			foreach (DataSourceReference Source in Sources)
			{
				if (BySourceId.ContainsKey(Source.SourceID))
					continue;

				ConcentratorSourceNode SourceNode = new ConcentratorSourceNode()
				{
					NodeId = await MeteringNode.GetUniqueNodeId(Source.SourceID),
					RemoteSourceID = Source.SourceID
				};

				await this.sourceNode.AddAsync(SourceNode);

				BySourceId[Source.SourceID] = SourceNode;

				ScanSource ScanSource = new ScanSource(this.Concentrator, SourceNode);
				ChildTasks.AddLast(ScanSource.ExecuteCommandAsync());
			}
		}

		private async Task ScanRootNodes(ConcentratorClient Client, string FullJid, LinkedList<Task> ChildTasks)
		{
			NodeInformation[] Nodes = await Client.GetRootNodesAsync(
				FullJid, this.sourceNode.RemoteSourceID, false, false, 
				string.Empty, string.Empty, string.Empty, string.Empty);

			Dictionary<string, ConcentratorNode> ByNodeId = new Dictionary<string, ConcentratorNode>();

			foreach (INode Child in await this.sourceNode.ChildNodes)
			{
				if (Child is ConcentratorNode XmppNode)
					ByNodeId[XmppNode.NodeId] = XmppNode;
			}

			foreach (NodeInformation Node in Nodes)
			{
				if (ByNodeId.ContainsKey(Node.NodeId))
					continue;

				ConcentratorNode NewNode;

				if (Node.IsReadable)
				{
					NewNode = new SensorNode()
					{
						NodeId = await MeteringNode.GetUniqueNodeId(Node.NodeId),
						RemoteNodeID = Node.NodeId
					};
				}
				else
				{
					NewNode = new ConcentratorNode()
					{
						NodeId = await MeteringNode.GetUniqueNodeId(Node.NodeId),
						RemoteNodeID = Node.NodeId
					};
				}

				await this.sourceNode.AddAsync(NewNode);

				ByNodeId[Node. NodeId] = NewNode;
			}
		}

		/// <summary>
		/// Creates a copy of the command object.
		/// </summary>
		/// <returns>Copy of command object.</returns>
		public override ICommand Copy()
		{
			return new ScanSource(this.Concentrator, this.sourceNode);
		}
	}
}
