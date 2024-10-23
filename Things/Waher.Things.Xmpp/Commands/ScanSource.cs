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
		private readonly SourceNode sourceNode;

		/// <summary>
		/// Scans a source node on a concentrator node for its child sources and root nodes.
		/// </summary>
		/// <param name="Concentrator">Concentrator node.</param>
		/// <param name="SourceNode">Data source node.</param>
		public ScanSource(ConcentratorDevice Concentrator, SourceNode SourceNode)
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
			ConcentratorClient Client = await this.GetClient();
			string FullJid = this.GetRemoteFullJid(Client);

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

			Dictionary<string, SourceNode> BySourceId = new Dictionary<string, SourceNode>();

			foreach (INode Child in await this.sourceNode.ChildNodes)
			{
				if (Child is SourceNode SourceNode)
					BySourceId[SourceNode.SourceId] = SourceNode;
			}

			foreach (DataSourceReference Source in Sources)
			{
				if (BySourceId.ContainsKey(Source.SourceID))
					continue;

				SourceNode SourceNode = new SourceNode()
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

			Dictionary<string, XmppNode> ByNodeId = new Dictionary<string, XmppNode>();

			foreach (INode Child in await this.sourceNode.ChildNodes)
			{
				if (Child is XmppNode XmppNode)
					ByNodeId[XmppNode.NodeId] = XmppNode;
			}

			foreach (NodeInformation Node in Nodes)
			{
				if (ByNodeId.ContainsKey(Node.NodeId))
					continue;

				XmppNode NewNode;

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
					NewNode = new XmppNode()
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
