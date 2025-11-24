using System.Collections.Generic;
using System.Threading.Tasks;
using Waher.Runtime.Language;
using Waher.Things.Attributes;
using Waher.Things.Metering;
using Waher.Things.Xmpp.Commands;

namespace Waher.Things.Xmpp
{
	/// <summary>
	/// Node representing a data source in an XMPP concentrator.
	/// </summary>
	public class ConcentratorSourceNode : ProvisionedMeteringNode
	{
		/// <summary>
		/// Node representing a data source in an XMPP concentrator.
		/// </summary>
		public ConcentratorSourceNode()
			: base()
		{
		}

		/// <summary>
		/// Source ID
		/// </summary>
		[Page(2, "XMPP", 100)]
		[Header(5, "Source ID:")]
		[ToolTip(6, "Source ID in concentrator.")]
		public string RemoteSourceID { get; set; }

		/// <summary>
		/// If provided, an ID for the node, but unique locally between siblings. Can be null, if Local ID equal to Node ID.
		/// </summary>
		public override string LocalId => this.RemoteSourceID;

		/// <summary>
		/// Gets the type name of the node.
		/// </summary>
		/// <param name="Language">Language to use.</param>
		/// <returns>Localized type node.</returns>
		public override Task<string> GetTypeNameAsync(Language Language)
		{
			return Language.GetStringAsync(typeof(ConcentratorDevice), 6, "Source ID");
		}

		/// <summary>
		/// If the node accepts a presumptive parent, i.e. can be added to that parent (if that parent accepts the node as a child).
		/// </summary>
		/// <param name="Parent">Presumptive parent node.</param>
		/// <returns>If the parent is acceptable.</returns>
		public override Task<bool> AcceptsParentAsync(INode Parent)
		{
			return Task.FromResult(Parent is ConcentratorDevice || Parent is XmppBrokerNode);
		}

		/// <summary>
		/// If the node accepts a presumptive child, i.e. can receive as a child (if that child accepts the node as a parent).
		/// </summary>
		/// <param name="Child">Presumptive child node.</param>
		/// <returns>If the child is acceptable.</returns>
		public override Task<bool> AcceptsChildAsync(INode Child)
		{
			return Task.FromResult(
				Child is ConcentratorPartitionNode || 
				Child is ConcentratorNode);
		}

		/// <summary>
		/// Available command objects. If no commands are available, null is returned.
		/// </summary>
		public override Task<IEnumerable<ICommand>> Commands => this.GetCommands();

		private async Task<IEnumerable<ICommand>> GetCommands()
		{
			List<ICommand> Result = new List<ICommand>();
			Result.AddRange(await base.Commands);

			ConcentratorDevice Concentrator = await this.GetAncestor<ConcentratorDevice>();
			if (!(Concentrator is null))
				Result.Add(new ScanSource(Concentrator, this));

			return Result.ToArray();
		}

	}
}
