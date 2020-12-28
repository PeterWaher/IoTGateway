using System;
using System.Threading.Tasks;

namespace Waher.Networking.XMPP.MUC
{
	/// <summary>
	/// Delegate for Direct invitation event handlers.
	/// </summary>
	/// <param name="Sender">Sender of event.</param>
	/// <param name="e">Event arguments.</param>
	public delegate Task DirectInvitationMessageEventHandler(object Sender, DirectInvitationMessageEventArgs e);

	/// <summary>
	/// Message from a MUC room containing an invitation.
	/// </summary>
	public class DirectInvitationMessageEventArgs : RoomMessageEventArgs
	{
		private readonly MultiUserChatClient mucClient;
		private readonly string reason;
		private readonly string password;
		private readonly string threadId;
		private readonly bool continuation;

		/// <summary>
		/// Message from a MUC room containing an invitation.
		/// </summary>
		/// <param name="MucClient">Multi-User Chat Client.</param>
		/// <param name="e">Message event arguments.</param>
		/// <param name="RoomId">Room ID</param>
		/// <param name="Domain">Domain hosting the room.</param>
		/// <param name="Reason">Reason for invitation.</param>
		/// <param name="Password">Password, if used.</param>
		/// <param name="Continuation">If the invitation is a continuation of a private thread.</param>
		/// <param name="ThreadId">Thread Id being continued.</param>
		public DirectInvitationMessageEventArgs(MultiUserChatClient MucClient, MessageEventArgs e, string RoomId, string Domain, 
			string Reason, string Password, bool Continuation, string ThreadId)
			: base(e, RoomId, Domain)
		{
			this.mucClient = MucClient;
			this.reason = Reason;
			this.password = Password;
			this.threadId = ThreadId;
			this.continuation = Continuation;
		}

		/// <summary>
		/// Multi-User Chat Client.
		/// </summary>
		public MultiUserChatClient MucClient => this.mucClient;

		/// <summary>
		/// Reason for invitation.
		/// </summary>
		public string Reason => this.reason;

		/// <summary>
		/// Password, if used.
		/// </summary>
		public string Password => this.password;

		/// <summary>
		/// If the invitation is a continuation of a private thread.
		/// </summary>
		public bool Continuation => this.continuation;

		/// <summary>
		/// Thread Id being continued.
		/// </summary>
		public string ThreadId => this.threadId;
	}
}
