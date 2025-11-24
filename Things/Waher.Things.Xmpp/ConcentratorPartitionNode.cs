using System.Threading.Tasks;
using Waher.Runtime.Language;
using Waher.Things.Attributes;
using Waher.Things.Metering;

namespace Waher.Things.Xmpp
{
	/// <summary>
	/// Node representing a partition in a data source in an XMPP concentrator.
	/// </summary>
	public class ConcentratorPartitionNode : ProvisionedMeteringNode
	{
		/// <summary>
		/// Node representing a partition in a data source in an XMPP concentrator.
		/// </summary>
		public ConcentratorPartitionNode()
			: base()
		{
		}

		/// <summary>
		/// Partition ID
		/// </summary>
		[Page(2, "XMPP", 100)]
		[Header(7, "Partition:")]
		[ToolTip(8, "Partition in data source.")]
		public string RemotePartitionID { get; set; }

		/// <summary>
		/// If provided, an ID for the node, but unique locally between siblings. Can be null, if Local ID equal to Node ID.
		/// </summary>
		public override string LocalId => this.RemotePartitionID;

		/// <summary>
		/// Gets the type name of the node.
		/// </summary>
		/// <param name="Language">Language to use.</param>
		/// <returns>Localized type node.</returns>
		public override Task<string> GetTypeNameAsync(Language Language)
		{
			return Language.GetStringAsync(typeof(ConcentratorDevice), 9, "Partition");
		}

		/// <summary>
		/// If the node accepts a presumptive parent, i.e. can be added to that parent (if that parent accepts the node as a child).
		/// </summary>
		/// <param name="Parent">Presumptive parent node.</param>
		/// <returns>If the parent is acceptable.</returns>
		public override Task<bool> AcceptsParentAsync(INode Parent)
		{
			return Task.FromResult(Parent is ConcentratorSourceNode || Parent is XmppBrokerNode);
		}

		/// <summary>
		/// If the node accepts a presumptive child, i.e. can receive as a child (if that child accepts the node as a parent).
		/// </summary>
		/// <param name="Child">Presumptive child node.</param>
		/// <returns>If the child is acceptable.</returns>
		public override Task<bool> AcceptsChildAsync(INode Child)
		{
			return Task.FromResult(Child is ConcentratorNode);
		}

	}
}
