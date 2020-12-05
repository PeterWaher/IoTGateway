using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using Waher.Content.Xml;
using Waher.Networking.XMPP.DataForms;

namespace Waher.Networking.XMPP.MUC
{
	/// <summary>
	/// Client managing communication with a Multi-User-Chat service.
	/// https://xmpp.org/extensions/xep-0045.html
	/// </summary>
	public class MultiUserChatClient : XmppExtension
	{
		/// <summary>
		/// http://jabber.org/protocol/muc
		/// </summary>
		public const string NamespaceMuc = "http://jabber.org/protocol/muc";

		/// <summary>
		/// http://jabber.org/protocol/muc#user
		/// </summary>
		public const string NamespaceMucUser = "http://jabber.org/protocol/muc#user";

		/// <summary>
		/// http://jabber.org/protocol/muc#admin
		/// </summary>
		public const string NamespaceMucAdmin = "http://jabber.org/protocol/muc#admin";

		/// <summary>
		/// http://jabber.org/protocol/muc#owner
		/// </summary>
		public const string NamespaceMucOwner = "http://jabber.org/protocol/muc#owner";

		private readonly string componentAddress;

		/// <summary>
		/// Client managing communication with a Multi-User-Chat service.
		/// https://xmpp.org/extensions/xep-0045.html
		/// </summary>
		/// <param name="Client">XMPP Client to use.</param>
		/// <param name="ComponentAddress">Address to the Publish/Subscribe component.</param>
		public MultiUserChatClient(XmppClient Client, string ComponentAddress)
			: base(Client)
		{
			this.componentAddress = ComponentAddress;

			this.client.RegisterPresenceHandler("x", NamespaceMucUser, this.UserPresenceHandler, true);
		}

		/// <summary>
		/// <see cref="IDisposable.Dispose"/>
		/// </summary>
		public override void Dispose()
		{
			this.client.UnregisterPresenceHandler("x", NamespaceMucUser, this.UserPresenceHandler, true);

			base.Dispose();
		}

		/// <summary>
		/// Publish/Subscribe component address.
		/// </summary>
		public string ComponentAddress
		{
			get { return this.componentAddress; }
		}

		/// <summary>
		/// Implemented extensions.
		/// </summary>
		public override string[] Extensions => new string[] { "XEP-0045" };

		private Task UserPresenceHandler(object Sender, PresenceEventArgs e)
		{
			if (TryParseUserPresence(e, out UserPresenceEventArgs e2))
				return this.UserPresence?.Invoke(this, e2) ?? Task.CompletedTask;
			else
				return Task.CompletedTask;
		}

		/// <summary>
		/// Event raised when user presence is received.
		/// </summary>
		public event UserPresenceEventHandlerAsync UserPresence;

		private static bool TryParseUserPresence(PresenceEventArgs e, out UserPresenceEventArgs Result)
		{
			if (!TryParseOccupantJid(e.From, out string RoomId, out string Domain, out string NickName))
			{
				Result = null;
				return false;
			}

			List<MucStatus> Status = null;
			Affiliation Affiliation = Affiliation.None;
			Role Role = Role.None;
			string FullJid = null;

			foreach (XmlNode N in e.Presence.ChildNodes)
			{
				if (!(N is XmlElement E) || E.LocalName != "x" || E.NamespaceURI != NamespaceMucUser)
					continue;

				foreach (XmlNode N2 in E.ChildNodes)
				{
					if (!(N2 is XmlElement E2) || E2.NamespaceURI != NamespaceMucUser)
						continue;

					switch (E2.LocalName)
					{
						case "item":
							Affiliation = ToAffiliation(XML.Attribute(E2, "affiliation"));
							Role = ToRole(XML.Attribute(E2, "role"));

							if (E2.HasAttribute("jid"))
								FullJid = E2.Attributes["jid"].Value;
							break;

						case "status":
							if (Status is null)
								Status = new List<MucStatus>();

							Status.Add((MucStatus)XML.Attribute(E2, "code", 0));
							break;
					}
				}
			}

			Result = new UserPresenceEventArgs(e, RoomId, Domain, NickName,
				Affiliation, Role, FullJid, Status?.ToArray() ?? new MucStatus[0]);

			return true;
		}

		private static Affiliation ToAffiliation(string s)
		{
			switch (s.ToLower())
			{
				case "owner": return Affiliation.Owner;
				case "admin": return Affiliation.Admin;
				case "member": return Affiliation.Member;
				case "outcast": return Affiliation.Outcast;
				default:
				case "none": return Affiliation.None;
			}
		}

		private static Role ToRole(string s)
		{
			switch (s.ToLower())
			{
				case "moderator": return Role.Moderator;
				case "participant": return Role.Participant;
				case "visitor": return Role.Visitor;
				default:
				case "none": return Role.None;
			}
		}

		private static bool TryParseOccupantJid(string OccupantJid, out string RoomId,
			out string Domain, out string NickName)
		{
			int i = OccupantJid.IndexOf('/');
			if (i < 0)
			{
				RoomId = Domain = NickName = null;
				return false;
			}

			NickName = OccupantJid.Substring(i + 1);
			OccupantJid = OccupantJid.Substring(0, i);

			i = OccupantJid.IndexOf('@');
			if (i < 0)
			{
				RoomId = Domain = null;
				return false;
			}

			Domain = OccupantJid.Substring(i + 1);
			RoomId = OccupantJid.Substring(0, i);

			return true;
		}

		/// <summary>
		/// Enter a chatroom.
		/// </summary>
		/// <param name="RoomId">Room ID.</param>
		/// <param name="Domain">Domain of service hosting the room.</param>
		/// <param name="NickName">Nickname to use in the chat room.</param>
		/// <param name="Callback">Method to call when response is returned.</param>
		/// <param name="State">State object to pass on to callback method.</param>
		public void EnterRoom(string RoomId, string Domain, string NickName,
			UserPresenceEventHandlerAsync Callback, object State)
		{
			this.client.SendDirectedPresence(RoomId + "@" + Domain + "/" + NickName,
				"<x xmlns='" + NamespaceMuc + "'/>", (sender, e) =>
				{
					if (!TryParseUserPresence(e, out UserPresenceEventArgs e2))
					{
						e2 = new UserPresenceEventArgs(e, RoomId, Domain, NickName,
							Affiliation.None, Role.None, string.Empty)
						{
							Ok = false
						};
					}

					return Callback?.Invoke(this, e2) ?? Task.CompletedTask;
				}, State);
		}

		/// <summary>
		/// Enter a chatroom.
		/// </summary>
		/// <param name="RoomName">Room name.</param>
		/// <param name="Domain">Domain of service hosting the room.</param>
		/// <param name="NickName">Nickname to use in the chat room.</param>
		/// <returns>Room entry response.</returns>
		public Task<UserPresenceEventArgs> EnterRoomAsync(string RoomName, string Domain,
			string NickName)
		{
			TaskCompletionSource<UserPresenceEventArgs> Result = new TaskCompletionSource<UserPresenceEventArgs>();

			this.EnterRoom(RoomName, Domain, NickName, (sender, e) =>
			{
				Result.TrySetResult(e);
				return Task.CompletedTask;
			}, null);

			return Result.Task;
		}

		/// <summary>
		/// Creates an instant room (with default settings).
		/// </summary>
		/// <param name="RoomName">Room name.</param>
		/// <param name="Domain">Domain of service hosting the room.</param>
		/// <param name="Callback">Method to call when response is returned.</param>
		/// <param name="State">State object to pass on to callback method.</param>
		public void CreateInstantRoom(string RoomName, string Domain,
			IqResultEventHandlerAsync Callback, object State)
		{
			StringBuilder Xml = new StringBuilder();

			Xml.Append("<query xmlns='");
			Xml.Append(NamespaceMucOwner);
			Xml.Append("'><x xmlns='");
			Xml.Append(XmppClient.NamespaceData);
			Xml.Append("' type='submit'/></query>");

			this.client.SendIqSet(RoomName + "@" + Domain, Xml.ToString(), Callback, State);
		}

		/// <summary>
		/// Creates an instant room (with default settings).
		/// </summary>
		/// <param name="RoomName">Room name.</param>
		/// <param name="Domain">Domain of service hosting the room.</param>
		/// <returns>Response to request.</returns>
		public Task<IqResultEventArgs> CreateInstantRoomAsync(string RoomName, string Domain)
		{
			TaskCompletionSource<IqResultEventArgs> Result = new TaskCompletionSource<IqResultEventArgs>();

			this.CreateInstantRoom(RoomName, Domain, (sender, e) =>
			{
				Result.TrySetResult(e);
				return Task.CompletedTask;
			}, null);

			return Result.Task;
		}

		/// <summary>
		/// Gets the configuration form for a room.
		/// </summary>
		/// <param name="RoomName">Room name.</param>
		/// <param name="Domain">Domain of service hosting the room.</param>
		/// <param name="Callback">Method to call when response is returned.</param>
		/// <param name="State">State object to pass on to callback method.</param>
		public void GetRoomConfiguration(string RoomName, string Domain,
			DataFormEventHandler Callback, object State)
		{
			this.GetRoomConfiguration(RoomName, Domain, Callback, null, State);
		}

		/// <summary>
		/// Gets the configuration form for a room.
		/// </summary>
		/// <param name="RoomName">Room name.</param>
		/// <param name="Domain">Domain of service hosting the room.</param>
		/// <param name="Callback">Method to call when response is returned.</param>
		/// <param name="SubmissionCallback">Method to call when configuration has been submitted.</param>
		/// <param name="State">State object to pass on to callback method.</param>
		public void GetRoomConfiguration(string RoomName, string Domain,
			DataFormEventHandler Callback, IqResultEventHandlerAsync SubmissionCallback, object State)
		{
			StringBuilder Xml = new StringBuilder();

			Xml.Append("<query xmlns='");
			Xml.Append(NamespaceMucOwner);
			Xml.Append("'/>");

			this.client.SendIqGet(RoomName + "@" + Domain, Xml.ToString(), (sender, e) =>
			{
				DataForm Form = null;

				if (!(e.FirstElement is null))
				{
					foreach (XmlNode N in e.FirstElement.ChildNodes)
					{
						if (N is XmlElement E && E.LocalName == "x" && E.NamespaceURI == XmppClient.NamespaceData)
						{
							Form = new DataForm(this.client, e.FirstElement,
								(sender2, Form2) =>
								{
									StringBuilder Xml2 = new StringBuilder();

									Xml2.Append("<query xmlns='");
									Xml2.Append(NamespaceMucOwner);
									Xml2.Append("'>");

									Form2.SerializeSubmit(Xml2);

									Xml2.Append("</query>");

									this.client.SendIqSet(Form2.From, Xml2.ToString(), (sender3, e3) =>
									{
										return SubmissionCallback?.Invoke(this, e3) ?? Task.CompletedTask;
									}, null);

									return Task.CompletedTask;
								},
								(sender2, Form2) =>
								{
									StringBuilder Xml2 = new StringBuilder();

									Xml2.Append("<query xmlns='");
									Xml2.Append(NamespaceMucOwner);
									Xml2.Append("'>");

									Form2.SerializeCancel(Xml2);

									Xml2.Append("</query>");

									this.client.SendIqSet(Form2.From, Xml2.ToString(), (sender3, e3) =>
									{
										e3.Ok = false;
										return SubmissionCallback?.Invoke(this, e3) ?? Task.CompletedTask;
									}, null);

									return Task.CompletedTask;
								},
								e.From, e.To)
							{
								State = e.State
							};

							break;
						}
					}
				}

				return Callback(this, Form);
			}, State);
		}

		/// <summary>
		/// Gets the configuration form for a room.
		/// </summary>
		/// <param name="RoomName">Room name.</param>
		/// <param name="Domain">Domain of service hosting the room.</param>
		/// <returns>Configuration Form, if found, or null if not supported.</returns>
		public Task<DataForm> GetRoomConfigurationAsync(string RoomName, string Domain)
		{
			return this.GetRoomConfigurationAsync(RoomName, Domain, null, null);
		}

		/// <summary>
		/// Gets the configuration form for a room.
		/// </summary>
		/// <param name="RoomName">Room name.</param>
		/// <param name="Domain">Domain of service hosting the room.</param>
		/// <param name="SubmissionCallback">Method to call when configuration has been submitted.</param>
		/// <param name="State">State object to pass on to callback method.</param>
		/// <returns>Configuration Form, if found, or null if not supported.</returns>
		public Task<DataForm> GetRoomConfigurationAsync(string RoomName, string Domain,
			IqResultEventHandlerAsync SubmissionCallback, object State)
		{
			TaskCompletionSource<DataForm> Result = new TaskCompletionSource<DataForm>();

			this.GetRoomConfiguration(RoomName, Domain, (sender, Form) =>
			{
				Result.TrySetResult(Form);
				return Task.CompletedTask;
			}, SubmissionCallback, State);

			return Result.Task;
		}


	}
}
