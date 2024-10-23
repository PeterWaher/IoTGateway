using System.Collections.Generic;
using System.Threading.Tasks;
using Waher.Networking.XMPP;
using Waher.Networking.XMPP.Concentrator;
using Waher.Runtime.Language;
using Waher.Things.Attributes;
using Waher.Things.Metering.NodeTypes;
using Waher.Things.Xmpp.Commands;

namespace Waher.Things.Xmpp
{
	/// <summary>
	/// Node representing an XMPP concentrator.
	/// </summary>
	public class ConcentratorDevice : XmppDevice
	{
		/// <summary>
		/// Node representing an XMPP concentrator.
		/// </summary>
		public ConcentratorDevice()
			: base()
		{
		}

		/// <summary>
		/// XMPP Address
		/// </summary>
		[Page(2, "XMPP", 100)]
		[Header(3, "JID:")]
		[ToolTip(4, "XMPP Address.")]
		public string JID { get; set; }

		/// <summary>
		/// Gets the type name of the node.
		/// </summary>
		/// <param name="Language">Language to use.</param>
		/// <returns>Localized type node.</returns>
		public override Task<string> GetTypeNameAsync(Language Language)
		{
			return Language.GetStringAsync(typeof(ConcentratorDevice), 1, "XMPP Concentrator");
		}

		/// <summary>
		/// If the node accepts a presumptive parent, i.e. can be added to that parent (if that parent accepts the node as a child).
		/// </summary>
		/// <param name="Parent">Presumptive parent node.</param>
		/// <returns>If the parent is acceptable.</returns>
		public override Task<bool> AcceptsParentAsync(INode Parent)
		{
			return Task.FromResult(Parent is Root);
		}

		/// <summary>
		/// If the node accepts a presumptive child, i.e. can receive as a child (if that child accepts the node as a parent).
		/// </summary>
		/// <param name="Child">Presumptive child node.</param>
		/// <returns>If the child is acceptable.</returns>
		public override Task<bool> AcceptsChildAsync(INode Child)
		{
			return Task.FromResult(Child is SourceNode || Child is XmppNode);
		}


		/// <summary>
		/// Available command objects. If no commands are available, null is returned.
		/// </summary>
		public override Task<IEnumerable<ICommand>> Commands => this.GetCommands();

		private async Task<IEnumerable<ICommand>> GetCommands()
		{
			List<ICommand> Result = new List<ICommand>();
			Result.AddRange(await base.Commands);

			Result.Add(new ScanRootSources(this));

			return Result.ToArray();
		}

		/// <summary>
		/// Gets the concentrator client associated with the XMPP client associated
		/// with the node.
		/// </summary>
		/// <returns>Concentrator Client, if found, null otherwise.</returns>
		public async Task<ConcentratorClient> GetConcentratorClient()
		{
			XmppClient Client = await this.GetClient();
			if (Client is null)
				return null;

			if (!Client.TryGetExtension(out ConcentratorClient ConcentratorClient))
				return null;

			return ConcentratorClient;
		}
	}
}
