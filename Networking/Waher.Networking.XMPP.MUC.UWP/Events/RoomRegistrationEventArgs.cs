using Waher.Networking.XMPP.DataForms;
using Waher.Networking.XMPP.Events;

namespace Waher.Networking.XMPP.MUC
{
	/// <summary>
	/// Event arguments for a room registration event handlers.
	/// </summary>
	public class RoomRegistrationEventArgs : IqResultEventArgs
	{
		private readonly DataForm form;
		private readonly string userName;
		private readonly bool alreadyRegistered;

		/// <summary>
		/// Event arguments for a room registration event handlers.
		/// </summary>
		/// <param name="e">IQ Result event arguments.</param>
		/// <param name="Form">Registration form. Can be null if already registered.</param>
		/// <param name="AlreadyRegistered">If the Bare JID was already registered.</param>
		/// <param name="UserName">Nick-Name of the Bare JID.</param>
		public RoomRegistrationEventArgs(IqResultEventArgs e, DataForm Form, 
			bool AlreadyRegistered, string UserName) 
			: base(e)
		{
			this.form = Form;
			this.alreadyRegistered = AlreadyRegistered;
			this.userName = UserName;
		}

		/// <summary>
		/// Registration form. Can be null if already registered.
		/// </summary>
		public DataForm Form => this.form;

		/// <summary>
		/// User Name of Bare Jid, if already registered.
		/// </summary>
		public string UserName => this.userName;

		/// <summary>
		/// If already registered. The nick-name is available in <see cref="UserName"/>.
		/// </summary>
		public bool AlreadyRegistered => this.alreadyRegistered;
	}
}
