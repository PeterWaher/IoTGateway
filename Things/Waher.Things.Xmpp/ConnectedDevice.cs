using System.Collections.Generic;
using System.Threading.Tasks;
using Waher.Networking.XMPP;
using Waher.Runtime.Language;
using Waher.Things.Attributes;
using Waher.Things.Metering.NodeTypes;
using Waher.Things.Xmpp.Commands;

namespace Waher.Things.Xmpp
{
	/// <summary>
	/// Node representing a device that is connected to XMPP.
	/// </summary>
	public class ConnectedDevice : XmppDevice
	{
		/// <summary>
		/// Node representing a device that is connected to XMPP.
		/// </summary>
		public ConnectedDevice()
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
			return Language.GetStringAsync(typeof(ConcentratorDevice), 56, "XMPP Device");
		}

		/// <summary>
		/// If the node accepts a presumptive parent, i.e. can be added to that parent (if that parent accepts the node as a child).
		/// </summary>
		/// <param name="Parent">Presumptive parent node.</param>
		/// <returns>If the parent is acceptable.</returns>
		public override Task<bool> AcceptsParentAsync(INode Parent)
		{
			return Task.FromResult(
				Parent is Root ||
				Parent is NodeCollection);
		}

		/// <summary>
		/// If the node accepts a presumptive child, i.e. can receive as a child (if that child accepts the node as a parent).
		/// </summary>
		/// <param name="Child">Presumptive child node.</param>
		/// <returns>If the child is acceptable.</returns>
		public override Task<bool> AcceptsChildAsync(INode Child)
		{
			return Task.FromResult(false);
		}

		/// <summary>
		/// Gets the Full JID of the connected device.
		/// </summary>
		/// <param name="Client">Concentrator client.</param>
		/// <param name="HasContact">If a contact with the Bare JID was found.</param>
		/// <param name="HasSubscription">If a subscription was found, but no presence.</param>
		/// <returns>Full JID, if found, null or empty otherwise.</returns>
		public string GetRemoteFullJid(XmppClient Client, out bool HasContact, out bool HasSubscription)
		{
			HasContact = false;
			HasSubscription = false;

			RosterItem Contact = Client[this.JID];
			if (Contact is null)
				return null;
				
			HasContact = true;

			if (!(Contact.State == SubscriptionState.Both || Contact.State == SubscriptionState.To))
				return null;

			HasSubscription = true;

			return Contact.LastPresenceFullJid;
		}

		/// <summary>
		/// Available command objects. If no commands are available, null is returned.
		/// </summary>
		public override Task<IEnumerable<ICommand>> Commands => this.GetCommands();

		private async Task<IEnumerable<ICommand>> GetCommands()
		{
			List<ICommand> Result = new List<ICommand>();
			Result.AddRange(await base.Commands);

			XmppClient Client = await this.GetClient();
			if (!(Client is null))
			{
				this.GetRemoteFullJid(Client, out bool HasContact, out bool HasSubscription);

				if (!HasContact || !HasSubscription)
					Result.Add(new SubscribeToPresence(this));
			}

			return Result.ToArray();
		}
	}
}
