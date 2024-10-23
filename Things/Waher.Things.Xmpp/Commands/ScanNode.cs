using System.Collections.Generic;
using System.Threading.Tasks;
using Waher.Networking.XMPP.Concentrator;
using Waher.Runtime.Language;
using Waher.Things.Metering;

namespace Waher.Things.Xmpp.Commands
{
	/// <summary>
	/// Scans a node on a concentrator node for its child nodes.
	/// </summary>
	public class ScanNode : ConcentratorCommand
	{
		private readonly XmppNode node;

		/// <summary>
		/// Scans a node on a concentrator node for its child nodes.
		/// </summary>
		/// <param name="Concentrator">Concentrator node.</param>
		/// <param name="Node">Node.</param>
		public ScanNode(ConcentratorDevice Concentrator, XmppNode Node)
			: base(Concentrator, "1")
		{
			this.node = Node;
		}

		/// <summary>
		/// ID of command.
		/// </summary>
		public override string CommandID => nameof(ScanNode);

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
			return Language.GetStringAsync(typeof(ConcentratorDevice), 55, "Scan Node");
		}

		/// <summary>
		/// Executes the command.
		/// </summary>
		public override async Task ExecuteCommandAsync()
		{
			ConcentratorClient Client = await this.GetClient();
			string FullJid = this.GetRemoteFullJid(Client);

			LinkedList<Task> ChildTasks = null;

			NodeInformation[] Nodes = await Client.GetChildNodesAsync(FullJid, this.node,
				false, false, string.Empty, string.Empty, string.Empty, string.Empty);

			Dictionary<string, XmppNode> ByNodeId = new Dictionary<string, XmppNode>();

			foreach (INode Child in await this.node.ChildNodes)
			{
				if (Child is XmppNode Node)
					ByNodeId[Node.NodeId] = Node;
			}

			foreach (NodeInformation Node in Nodes)
			{
				if (ByNodeId.ContainsKey(node.NodeId))
					continue;

				XmppNode NewNode = new XmppNode()
				{
					NodeId = await MeteringNode.GetUniqueNodeId(Node.NodeId),
					RemoteNodeID = Node.NodeId
				};

				await this.node.AddAsync(NewNode);

				ByNodeId[Node.NodeId] = NewNode;

				if (ChildTasks is null)
					ChildTasks = new LinkedList<Task>();

				ScanNode ScanNode = new ScanNode(this.Concentrator, NewNode);
				ChildTasks.AddLast(ScanNode.ExecuteCommandAsync());
			}

			if (!(ChildTasks is null))
				await Task.WhenAll(ChildTasks);
		}

		/// <summary>
		/// Creates a copy of the command object.
		/// </summary>
		/// <returns>Copy of command object.</returns>
		public override ICommand Copy()
		{
			return new ScanNode(this.Concentrator, this.node);
		}
	}
}
