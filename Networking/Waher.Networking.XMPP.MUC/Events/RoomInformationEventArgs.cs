using System;
using System.Threading.Tasks;
using Waher.Networking.XMPP.ServiceDiscovery;

namespace Waher.Networking.XMPP.MUC
{
	/// <summary>
	/// Delegate for room information response event handlers.
	/// </summary>
	/// <param name="Sender">Sender of event.</param>
	/// <param name="e">Event arguments.</param>
	public delegate Task RoomInformationEventHandler(Object Sender, RoomInformationEventArgs e);

	/// <summary>
	/// Room information response event arguments.
	/// </summary>
	public class RoomInformationEventArgs : ServiceDiscoveryEventArgs
	{
		private readonly bool userRegistration;
		private readonly bool roomConfiguration;
		private readonly bool roomInformation;
		private readonly bool stableId;
		private readonly bool hidden;
		private readonly bool membersOnly;
		private readonly bool moderated;
		private readonly bool nonAnonymous;
		private readonly bool open;
		private readonly bool passwordProtected;
		private readonly bool persistent;
		private readonly bool _public;
		private readonly bool rooms;
		private readonly bool semiAnonymous;
		private readonly bool temporary;
		private readonly bool unmoderated;
		private readonly bool unsecured;

		/// <summary>
		/// Room information response event arguments.
		/// </summary>
		/// <param name="e">Service discovery response.</param>
		public RoomInformationEventArgs(ServiceDiscoveryEventArgs e)
			: base(e)
		{
			this.userRegistration = e.HasFeature("http://jabber.org/protocol/muc#register");
			this.roomConfiguration = e.HasFeature("http://jabber.org/protocol/muc#roomconfig");
			this.roomInformation = e.HasFeature("http://jabber.org/protocol/muc#roominfo");
			this.stableId = e.HasFeature("http://jabber.org/protocol/muc#stable_id");
			this.hidden = e.HasFeature("muc_hidden");
			this.membersOnly = e.HasFeature("muc_membersonly");
			this.moderated = e.HasFeature("muc_moderated");
			this.nonAnonymous = e.HasFeature("muc_nonanonymous");
			this.open = e.HasFeature("muc_open");
			this.passwordProtected = e.HasFeature("muc_passwordprotected");
			this.persistent = e.HasFeature("muc_persistent");
			this._public = e.HasFeature("muc_public");
			this.rooms = e.HasFeature("muc_rooms");
			this.semiAnonymous = e.HasFeature("muc_semianonymous");
			this.temporary = e.HasFeature("muc_temporary");
			this.unmoderated = e.HasFeature("muc_unmoderated");
			this.unsecured = e.HasFeature("muc_unsecured");
		}

		/// <summary>
		/// Support for the muc#register FORM_TYPE
		/// </summary>
		public bool UserRegistration => this.userRegistration;

		/// <summary>
		/// Support for the muc#roomconfig FORM_TYPE
		/// </summary>
		public bool RoomConfiguration => this.roomConfiguration;

		/// <summary>
		/// Support for the muc#roominfo FORM_TYPE
		/// </summary>
		public bool RoomInformation => this.roomInformation;

		/// <summary>
		/// This MUC will reflect the original message 'id' in 'groupchat' messages.
		/// </summary>
		public bool StableId => this.stableId;

		/// <summary>
		/// Hidden room in Multi-User Chat (MUC)
		/// </summary>
		public bool Hidden => this.hidden;

		/// <summary>
		/// Members-only room in Multi-User Chat (MUC)
		/// </summary>
		public bool MembersOnly => this.membersOnly;

		/// <summary>
		/// Moderated room in Multi-User Chat (MUC)
		/// </summary>
		public bool Moderated => this.moderated;

		/// <summary>
		/// Non-anonymous room in Multi-User Chat (MUC)
		/// </summary>
		public bool NonAnonymous => this.nonAnonymous;

		/// <summary>
		/// Open room in Multi-User Chat (MUC)
		/// </summary>
		public bool Open => this.open;

		/// <summary>
		/// Password-protected room in Multi-User Chat (MUC)
		/// </summary>
		public bool PasswordProtected => this.passwordProtected;

		/// <summary>
		/// Persistent room in Multi-User Chat (MUC)
		/// </summary>
		public bool Persistent => this.persistent;

		/// <summary>
		/// Public room in Multi-User Chat (MUC)
		/// </summary>
		public bool Public => this._public;

		/// <summary>
		/// List of MUC rooms (each as a separate item)
		/// </summary>
		public bool Rooms => this.rooms;

		/// <summary>
		/// Semi-anonymous room in Multi-User Chat (MUC)
		/// </summary>
		public bool SemiAnonymous => this.semiAnonymous;

		/// <summary>
		/// Temporary room in Multi-User Chat (MUC)
		/// </summary>
		public bool Temporary => this.temporary;

		/// <summary>
		/// Unmoderated room in Multi-User Chat (MUC)
		/// </summary>
		public bool Unmoderated => this.unmoderated;

		/// <summary>
		/// Unsecured room in Multi-User Chat (MUC)
		/// </summary>
		public bool Unsecured => this.unsecured;
	}
}
