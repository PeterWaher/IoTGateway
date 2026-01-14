using System.Threading.Tasks;

namespace Waher.Networking.XMPP.Chat.ChatStates
{
	/// <summary>
	/// Delegate invoked when chat state support has been determined for a contact.
	/// </summary>
	/// <param name="bareJid">Bare JID whose capability changed.</param>
	/// <param name="supported">If the contact supports chat states.</param>
	public delegate Task ChatStateSupportDeterminedEventHandler(string bareJid, bool supported);
}
