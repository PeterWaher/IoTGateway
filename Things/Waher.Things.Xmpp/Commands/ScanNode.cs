using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Waher.Events;
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
		private readonly ConcentratorNode node;

		/// <summary>
		/// Scans a node on a concentrator node for its child nodes.
		/// </summary>
		/// <param name="Concentrator">Concentrator node.</param>
		/// <param name="Node">Node.</param>
		public ScanNode(ConcentratorDevice Concentrator, ConcentratorNode Node)
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
		public override Task ExecuteCommandAsync()
		{
			this.StartSearch();
			return Task.CompletedTask;
		}

		private async void StartSearch()
		{
			try
			{
				ConcentratorClient Client = await this.GetConcentratorClient();
				string FullJid = this.GetRemoteFullJid(Client.Client);

				string RemoteSourceID = string.Empty;
				string RemotePartition = string.Empty;
				INode Loop = await this.node.GetParent();

				while (!(Loop is null))
				{
					if (Loop is ConcentratorPartitionNode PartitionNode)
						RemotePartition = PartitionNode.RemotePartitionID;
					else if (Loop is ConcentratorSourceNode SourceNode)
					{
						RemoteSourceID = SourceNode.RemoteSourceID;
						break;
					}

					if (Loop is MeteringNode MeteringNode)
						Loop = await MeteringNode.GetParent();
					else
						Loop = Loop.Parent;
				}

				NodeInformation[] Nodes = await Client.GetChildNodesAsync(FullJid,
					this.node.RemoteNodeID, RemoteSourceID, RemotePartition,
					false, false, string.Empty, string.Empty, string.Empty, string.Empty);

				Dictionary<string, ConcentratorNode> ByNodeId = new Dictionary<string, ConcentratorNode>();

				foreach (INode Child in await this.node.ChildNodes)
				{
					if (Child is ConcentratorNode Node)
						ByNodeId[Node.RemoteNodeID] = Node;
				}

				LinkedList<ScanNode> NewScans = null;

				foreach (NodeInformation Node in Nodes)
				{
					if (ByNodeId.ContainsKey(Node.NodeId))
						continue;

					ConcentratorNode NewNode = new ConcentratorNode()
					{
						NodeId = await MeteringNode.GetUniqueNodeId(Node.NodeId),
						RemoteNodeID = Node.NodeId
					};

					await this.node.AddAsync(NewNode);

					ByNodeId[Node.NodeId] = NewNode;

					if (NewScans is null)
						NewScans = new LinkedList<ScanNode>();

					NewScans.AddLast(new ScanNode(this.Concentrator, NewNode));
				}

				if (!(NewScans is null))
				{
					foreach (ScanNode ScanNode in NewScans)
						await ScanNode.ExecuteCommandAsync();
				}
			}
			catch (Exception ex)
			{
				Log.Exception(ex);
			}
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
