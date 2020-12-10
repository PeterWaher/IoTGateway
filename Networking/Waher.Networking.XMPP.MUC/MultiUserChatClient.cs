using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using Waher.Content.Xml;
using Waher.Events;
using Waher.Networking.XMPP.DataForms;
using Waher.Networking.XMPP.ServiceDiscovery;

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
			this.client.RegisterMessageHandler("x", NamespaceMucUser, this.UserMessageHandler, false);

			this.client.OnGroupChatMessage += Client_OnGroupChatMessage;
		}

		/// <summary>
		/// <see cref="IDisposable.Dispose"/>
		/// </summary>
		public override void Dispose()
		{
			this.client.UnregisterPresenceHandler("x", NamespaceMucUser, this.UserPresenceHandler, true);
			this.client.UnregisterMessageHandler("x", NamespaceMucUser, this.UserMessageHandler, false);

			this.client.OnGroupChatMessage -= Client_OnGroupChatMessage;

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
			if (!TryParseOccupantJid(e.From, true, out string RoomId, out string Domain, out string NickName))
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

		private static bool TryParseOccupantJid(string OccupantJid, bool RequireNick,
			out string RoomId, out string Domain, out string NickName)
		{
			int i = OccupantJid.IndexOf('/');
			if (i < 0)
			{
				if (RequireNick)
				{
					RoomId = Domain = NickName = null;
					return false;
				}
				else
					NickName = string.Empty;
			}
			else
			{
				NickName = OccupantJid.Substring(i + 1);
				OccupantJid = OccupantJid.Substring(0, i);
			}

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
		/// Enter a chat room.
		/// </summary>
		/// <param name="RoomId">Room ID.</param>
		/// <param name="Domain">Domain of service hosting the room.</param>
		/// <param name="NickName">Nickname to use in the chat room.</param>
		/// <param name="Callback">Method to call when response is returned.</param>
		/// <param name="State">State object to pass on to callback method.</param>
		public void EnterRoom(string RoomId, string Domain, string NickName,
			UserPresenceEventHandlerAsync Callback, object State)
		{
			this.EnterRoom(RoomId, Domain, NickName, string.Empty, Callback, State);
		}

		/// <summary>
		/// Enter a chat room.
		/// </summary>
		/// <param name="RoomId">Room ID.</param>
		/// <param name="Domain">Domain of service hosting the room.</param>
		/// <param name="NickName">Nickname to use in the chat room.</param>
		/// <param name="Password">Password</param>
		/// <param name="Callback">Method to call when response is returned.</param>
		/// <param name="State">State object to pass on to callback method.</param>
		public void EnterRoom(string RoomId, string Domain, string NickName, string Password,
			UserPresenceEventHandlerAsync Callback, object State)
		{
			StringBuilder Xml = new StringBuilder();

			Xml.Append("<x xmlns='");
			Xml.Append(NamespaceMuc);

			if (string.IsNullOrEmpty(Password))
				Xml.Append("'/>");
			else
			{
				Xml.Append("'><password>");
				Xml.Append(XML.Encode(Password));
				Xml.Append("</password></x>");
			}

			this.client.SendDirectedPresence(string.Empty, RoomId + "@" + Domain + "/" + NickName,
				Xml.ToString(), (sender, e) =>
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
		/// Enter a chat room.
		/// </summary>
		/// <param name="RoomId">Room ID.</param>
		/// <param name="Domain">Domain of service hosting the room.</param>
		/// <param name="NickName">Nickname to use in the chat room.</param>
		/// <returns>Room entry response.</returns>
		public Task<UserPresenceEventArgs> EnterRoomAsync(string RoomId, string Domain, string NickName)
		{
			return this.EnterRoomAsync(RoomId, Domain, NickName, string.Empty);
		}

		/// <summary>
		/// Enter a chat room.
		/// </summary>
		/// <param name="RoomId">Room ID.</param>
		/// <param name="Domain">Domain of service hosting the room.</param>
		/// <param name="NickName">Nickname to use in the chat room.</param>
		/// <param name="Password">Password</param>
		/// <returns>Room entry response.</returns>
		public Task<UserPresenceEventArgs> EnterRoomAsync(string RoomId, string Domain, string NickName, string Password)
		{
			TaskCompletionSource<UserPresenceEventArgs> Result = new TaskCompletionSource<UserPresenceEventArgs>();

			this.EnterRoom(RoomId, Domain, NickName, Password, (sender, e) =>
			{
				Result.TrySetResult(e);
				return Task.CompletedTask;
			}, null);

			return Result.Task;
		}

		/// <summary>
		/// Leave a chat room.
		/// </summary>
		/// <param name="RoomId">Room ID.</param>
		/// <param name="Domain">Domain of service hosting the room.</param>
		/// <param name="NickName">Nickname to use in the chat room.</param>
		/// <param name="Callback">Method to call when response is returned.</param>
		/// <param name="State">State object to pass on to callback method.</param>
		public void LeaveRoom(string RoomId, string Domain, string NickName,
			UserPresenceEventHandlerAsync Callback, object State)
		{
			this.client.SendDirectedPresence("unavailable", RoomId + "@" + Domain + "/" + NickName,
				string.Empty, (sender, e) =>
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
		/// Leave a chat room.
		/// </summary>
		/// <param name="RoomId">Room ID.</param>
		/// <param name="Domain">Domain of service hosting the room.</param>
		/// <param name="NickName">Nickname to use in the chat room.</param>
		/// <returns>Room entry response.</returns>
		public Task<UserPresenceEventArgs> LeaveRoomAsync(string RoomId, string Domain,
			string NickName)
		{
			TaskCompletionSource<UserPresenceEventArgs> Result = new TaskCompletionSource<UserPresenceEventArgs>();

			this.LeaveRoom(RoomId, Domain, NickName, (sender, e) =>
			{
				Result.TrySetResult(e);
				return Task.CompletedTask;
			}, null);

			return Result.Task;
		}

		/// <summary>
		/// Creates an instant room (with default settings).
		/// </summary>
		/// <param name="RoomId">Room ID.</param>
		/// <param name="Domain">Domain of service hosting the room.</param>
		/// <param name="Callback">Method to call when response is returned.</param>
		/// <param name="State">State object to pass on to callback method.</param>
		public void CreateInstantRoom(string RoomId, string Domain,
			IqResultEventHandlerAsync Callback, object State)
		{
			StringBuilder Xml = new StringBuilder();

			Xml.Append("<query xmlns='");
			Xml.Append(NamespaceMucOwner);
			Xml.Append("'><x xmlns='");
			Xml.Append(XmppClient.NamespaceData);
			Xml.Append("' type='submit'/></query>");

			this.client.SendIqSet(RoomId + "@" + Domain, Xml.ToString(), Callback, State);
		}

		/// <summary>
		/// Creates an instant room (with default settings).
		/// </summary>
		/// <param name="RoomId">Room ID.</param>
		/// <param name="Domain">Domain of service hosting the room.</param>
		/// <returns>Response to request.</returns>
		public Task<IqResultEventArgs> CreateInstantRoomAsync(string RoomId, string Domain)
		{
			TaskCompletionSource<IqResultEventArgs> Result = new TaskCompletionSource<IqResultEventArgs>();

			this.CreateInstantRoom(RoomId, Domain, (sender, e) =>
			{
				Result.TrySetResult(e);
				return Task.CompletedTask;
			}, null);

			return Result.Task;
		}

		/// <summary>
		/// Gets the configuration form for a room.
		/// </summary>
		/// <param name="RoomId">Room ID.</param>
		/// <param name="Domain">Domain of service hosting the room.</param>
		/// <param name="Callback">Method to call when response is returned.</param>
		/// <param name="State">State object to pass on to callback method.</param>
		public void GetRoomConfiguration(string RoomId, string Domain,
			DataFormEventHandler Callback, object State)
		{
			this.GetRoomConfiguration(RoomId, Domain, Callback, null, State);
		}

		/// <summary>
		/// Gets the configuration form for a room.
		/// </summary>
		/// <param name="RoomId">Room ID.</param>
		/// <param name="Domain">Domain of service hosting the room.</param>
		/// <param name="Callback">Method to call when response is returned.</param>
		/// <param name="SubmissionCallback">Method to call when configuration has been submitted.</param>
		/// <param name="State">State object to pass on to callback method.</param>
		public void GetRoomConfiguration(string RoomId, string Domain,
			DataFormEventHandler Callback, IqResultEventHandlerAsync SubmissionCallback, object State)
		{
			StringBuilder Xml = new StringBuilder();

			Xml.Append("<query xmlns='");
			Xml.Append(NamespaceMucOwner);
			Xml.Append("'/>");

			this.client.SendIqGet(RoomId + "@" + Domain, Xml.ToString(), (sender, e) =>
			{
				DataForm Form = null;

				if (!(e.FirstElement is null))
				{
					foreach (XmlNode N in e.FirstElement.ChildNodes)
					{
						if (N is XmlElement E && E.LocalName == "x" && E.NamespaceURI == XmppClient.NamespaceData)
						{
							Form = new DataForm(this.client, E,
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
		/// <param name="RoomId">Room ID.</param>
		/// <param name="Domain">Domain of service hosting the room.</param>
		/// <returns>Configuration Form, if found, or null if not supported.</returns>
		public Task<DataForm> GetRoomConfigurationAsync(string RoomId, string Domain)
		{
			return this.GetRoomConfigurationAsync(RoomId, Domain, null, null);
		}

		/// <summary>
		/// Gets the configuration form for a room.
		/// </summary>
		/// <param name="RoomId">Room ID.</param>
		/// <param name="Domain">Domain of service hosting the room.</param>
		/// <param name="SubmissionCallback">Method to call when configuration has been submitted.</param>
		/// <param name="State">State object to pass on to callback method.</param>
		/// <returns>Configuration Form, if found, or null if not supported.</returns>
		public Task<DataForm> GetRoomConfigurationAsync(string RoomId, string Domain,
			IqResultEventHandlerAsync SubmissionCallback, object State)
		{
			TaskCompletionSource<DataForm> Result = new TaskCompletionSource<DataForm>();

			this.GetRoomConfiguration(RoomId, Domain, (sender, Form) =>
			{
				Result.TrySetResult(Form);
				return Task.CompletedTask;
			}, SubmissionCallback, State);

			return Result.Task;
		}

		/// <summary>
		/// Sends a simple group chat message to a chat room.
		/// 
		/// Note: The client must be an occupant of the chat room before messages
		/// can be properly propagated to other occupants of the room.
		/// </summary>
		/// <param name="RoomId">Room ID</param>
		/// <param name="Domain">Domain hosting the room.</param>
		/// <param name="Message">Message body.</param>
		public void SendGroupChatMessage(string RoomId, string Domain, string Message)
		{
			this.SendGroupChatMessage(RoomId, Domain, Message, string.Empty, string.Empty, string.Empty);
		}

		/// <summary>
		/// Sends a simple group chat message to a chat room.
		/// 
		/// Note: The client must be an occupant of the chat room before messages
		/// can be properly propagated to other occupants of the room.
		/// </summary>
		/// <param name="RoomId">Room ID</param>
		/// <param name="Domain">Domain hosting the room.</param>
		/// <param name="Message">Message body.</param>
		/// <param name="Language">Language</param>
		public void SendGroupChatMessage(string RoomId, string Domain, string Message, string Language)
		{
			this.SendGroupChatMessage(RoomId, Domain, Message, Language, string.Empty, string.Empty);
		}

		/// <summary>
		/// Sends a simple group chat message to a chat room.
		/// 
		/// Note: The client must be an occupant of the chat room before messages
		/// can be properly propagated to other occupants of the room.
		/// </summary>
		/// <param name="RoomId">Room ID</param>
		/// <param name="Domain">Domain hosting the room.</param>
		/// <param name="Message">Message body.</param>
		/// <param name="Language">Language</param>
		/// <param name="ThreadId">Thread ID</param>
		public void SendGroupChatMessage(string RoomId, string Domain, string Message, string Language, string ThreadId)
		{
			this.SendGroupChatMessage(RoomId, Domain, Message, Language, ThreadId, string.Empty);
		}

		/// <summary>
		/// Sends a simple group chat message to a chat room.
		/// 
		/// Note: The client must be an occupant of the chat room before messages
		/// can be properly propagated to other occupants of the room.
		/// </summary>
		/// <param name="RoomId">Room ID</param>
		/// <param name="Domain">Domain hosting the room.</param>
		/// <param name="Message">Message body.</param>
		/// <param name="Language">Language</param>
		/// <param name="ThreadId">Thread ID</param>
		/// <param name="ParentThreadId">Parent Thread ID</param>
		public void SendGroupChatMessage(string RoomId, string Domain, string Message, string Language, string ThreadId, string ParentThreadId)
		{
			this.client.SendMessage(MessageType.GroupChat, RoomId + "@" + Domain,
				string.Empty, Message, string.Empty, Language, ThreadId, ParentThreadId);
		}

		/// <summary>
		/// Sends a simple group chat message to a chat room.
		/// 
		/// Note: The client must be an occupant of the chat room before messages
		/// can be properly propagated to other occupants of the room.
		/// </summary>
		/// <param name="RoomId">Room ID</param>
		/// <param name="Domain">Domain hosting the room.</param>
		/// <param name="Xml">Message body.</param>
		public void SendCustomGroupChatMessage(string RoomId, string Domain, string Xml)
		{
			this.SendCustomGroupChatMessage(RoomId, Domain, Xml, string.Empty,
				string.Empty, string.Empty);
		}

		/// <summary>
		/// Sends a simple group chat message to a chat room.
		/// 
		/// Note: The client must be an occupant of the chat room before messages
		/// can be properly propagated to other occupants of the room.
		/// </summary>
		/// <param name="RoomId">Room ID</param>
		/// <param name="Domain">Domain hosting the room.</param>
		/// <param name="Xml">Message body.</param>
		/// <param name="Language">Language</param>
		public void SendCustomGroupChatMessage(string RoomId, string Domain, string Xml, string Language)
		{
			this.SendCustomGroupChatMessage(RoomId, Domain, Xml, Language, string.Empty, string.Empty);
		}

		/// <summary>
		/// Sends a simple group chat message to a chat room.
		/// 
		/// Note: The client must be an occupant of the chat room before messages
		/// can be properly propagated to other occupants of the room.
		/// </summary>
		/// <param name="RoomId">Room ID</param>
		/// <param name="Domain">Domain hosting the room.</param>
		/// <param name="Xml">Message body.</param>
		/// <param name="Language">Language</param>
		/// <param name="ThreadId">Thread ID</param>
		public void SendCustomGroupChatMessage(string RoomId, string Domain, string Xml, string Language, string ThreadId)
		{
			this.SendCustomGroupChatMessage(RoomId, Domain, Xml, Language, ThreadId, string.Empty);
		}

		/// <summary>
		/// Sends a simple group chat message to a chat room.
		/// 
		/// Note: The client must be an occupant of the chat room before messages
		/// can be properly propagated to other occupants of the room.
		/// </summary>
		/// <param name="RoomId">Room ID</param>
		/// <param name="Domain">Domain hosting the room.</param>
		/// <param name="Xml">Message body.</param>
		/// <param name="Language">Language</param>
		/// <param name="ThreadId">Thread ID</param>
		/// <param name="ParentThreadId">Parent Thread ID</param>
		public void SendCustomGroupChatMessage(string RoomId, string Domain,
			string Xml, string Language, string ThreadId, string ParentThreadId)
		{
			this.client.SendMessage(MessageType.GroupChat, RoomId + "@" + Domain,
				 Xml, string.Empty, string.Empty, Language, ThreadId, ParentThreadId);
		}

		private Task Client_OnGroupChatMessage(object Sender, MessageEventArgs e)
		{
			if (!TryParseOccupantJid(e.From, false, out string RoomId, out string Domain, out string NickName))
				return Task.CompletedTask;

			try
			{
				if (string.IsNullOrEmpty(NickName))
				{
					RoomMessageEventArgs e2 = new RoomMessageEventArgs(e, RoomId, Domain);
					this.RoomMessage?.Invoke(this, e2);
				}
				else
				{
					RoomOccupantMessageEventArgs e2 = new RoomOccupantMessageEventArgs(e, RoomId, Domain, NickName);

					if (string.IsNullOrEmpty(e.Body) && !string.IsNullOrEmpty(e.Subject))
						this.RoomSubject?.Invoke(this, e2);
					else
						this.RoomOccupantMessage?.Invoke(this, e2);
				}
			}
			catch (Exception ex)
			{
				Log.Critical(ex);
			}

			return Task.CompletedTask;
		}

		/// <summary>
		/// Event raised when a group chat message from a MUC room was received.
		/// </summary>
		public event RoomMessageEventHandler RoomMessage;

		/// <summary>
		/// Event raised when a group chat message from a MUC room occupant was received.
		/// </summary>
		public event RoomOccupantMessageEventHandler RoomOccupantMessage;

		/// <summary>
		/// Event raised when a room subject message was received.
		/// </summary>
		public event RoomOccupantMessageEventHandler RoomSubject;

		/// <summary>
		/// Sends a simple group chat message to a chat room.
		/// 
		/// Note: The client must be an occupant of the chat room before messages
		/// can be properly propagated to other occupants of the room.
		/// </summary>
		/// <param name="RoomId">Room ID</param>
		/// <param name="Domain">Domain hosting the room.</param>
		/// <param name="Subject">Room subject.</param>
		public void ChangeRoomSubject(string RoomId, string Domain, string Subject)
		{
			this.ChangeRoomSubject(RoomId, Domain, Subject, string.Empty);
		}

		/// <summary>
		/// Sends a simple group chat message to a chat room.
		/// 
		/// Note: The client must be an occupant of the chat room before messages
		/// can be properly propagated to other occupants of the room.
		/// </summary>
		/// <param name="RoomId">Room ID</param>
		/// <param name="Domain">Domain hosting the room.</param>
		/// <param name="Subject">Room subject.</param>
		/// <param name="Language">Language</param>
		public void ChangeRoomSubject(string RoomId, string Domain, string Subject, string Language)
		{
			this.client.SendMessage(MessageType.GroupChat, RoomId + "@" + Domain,
				string.Empty, string.Empty, Subject, Language, string.Empty, string.Empty);
		}

		/// <summary>
		/// Sends a simple private message to a chat room.
		/// 
		/// Note: The client must be an occupant of the chat room before messages
		/// can be properly propagated to other occupants of the room.
		/// </summary>
		/// <param name="RoomId">Room ID</param>
		/// <param name="Domain">Domain hosting the room.</param>
		/// <param name="NickName">Nick-name of recipient.</param>
		/// <param name="Message">Message body.</param>
		public void SendPrivateMessage(string RoomId, string Domain, string NickName, string Message)
		{
			this.SendPrivateMessage(RoomId, Domain, NickName, Message, string.Empty, string.Empty, string.Empty);
		}

		/// <summary>
		/// Sends a simple private message to a chat room.
		/// 
		/// Note: The client must be an occupant of the chat room before messages
		/// can be properly propagated to other occupants of the room.
		/// </summary>
		/// <param name="RoomId">Room ID</param>
		/// <param name="Domain">Domain hosting the room.</param>
		/// <param name="NickName">Nick-name of recipient.</param>
		/// <param name="Message">Message body.</param>
		/// <param name="Language">Language</param>
		public void SendPrivateMessage(string RoomId, string Domain, string NickName, string Message, string Language)
		{
			this.SendPrivateMessage(RoomId, Domain, NickName, Message, Language, string.Empty, string.Empty);
		}

		/// <summary>
		/// Sends a simple private message to a chat room.
		/// 
		/// Note: The client must be an occupant of the chat room before messages
		/// can be properly propagated to other occupants of the room.
		/// </summary>
		/// <param name="RoomId">Room ID</param>
		/// <param name="Domain">Domain hosting the room.</param>
		/// <param name="NickName">Nick-name of recipient.</param>
		/// <param name="Message">Message body.</param>
		/// <param name="Language">Language</param>
		/// <param name="ThreadId">Thread ID</param>
		public void SendPrivateMessage(string RoomId, string Domain, string NickName, string Message, string Language, string ThreadId)
		{
			this.SendPrivateMessage(RoomId, Domain, NickName, Message, Language, ThreadId, string.Empty);
		}

		/// <summary>
		/// Sends a simple private message to a chat room.
		/// 
		/// Note: The client must be an occupant of the chat room before messages
		/// can be properly propagated to other occupants of the room.
		/// </summary>
		/// <param name="RoomId">Room ID</param>
		/// <param name="Domain">Domain hosting the room.</param>
		/// <param name="NickName">Nick-name of recipient.</param>
		/// <param name="Message">Message body.</param>
		/// <param name="Language">Language</param>
		/// <param name="ThreadId">Thread ID</param>
		/// <param name="ParentThreadId">Parent Thread ID</param>
		public void SendPrivateMessage(string RoomId, string Domain, string NickName, string Message, string Language, string ThreadId, string ParentThreadId)
		{
			this.client.SendMessage(MessageType.Chat, RoomId + "@" + Domain + "/" + NickName,
				"<x xmlns='" + NamespaceMucUser + "'/>", Message, string.Empty,
				Language, ThreadId, ParentThreadId);
		}

		/// <summary>
		/// Sends a simple private message to a chat room.
		/// 
		/// Note: The client must be an occupant of the chat room before messages
		/// can be properly propagated to other occupants of the room.
		/// </summary>
		/// <param name="RoomId">Room ID</param>
		/// <param name="Domain">Domain hosting the room.</param>
		/// <param name="NickName">Nick-name of recipient.</param>
		/// <param name="Xml">Message body.</param>
		public void SendCustomPrivateMessage(string RoomId, string Domain, string NickName, string Xml)
		{
			this.SendCustomPrivateMessage(RoomId, Domain, NickName, Xml, string.Empty,
				string.Empty, string.Empty);
		}

		/// <summary>
		/// Sends a simple private message to a chat room.
		/// 
		/// Note: The client must be an occupant of the chat room before messages
		/// can be properly propagated to other occupants of the room.
		/// </summary>
		/// <param name="RoomId">Room ID</param>
		/// <param name="Domain">Domain hosting the room.</param>
		/// <param name="NickName">Nick-name of recipient.</param>
		/// <param name="Xml">Message body.</param>
		/// <param name="Language">Language</param>
		public void SendCustomPrivateMessage(string RoomId, string Domain, string NickName, string Xml, string Language)
		{
			this.SendCustomPrivateMessage(RoomId, Domain, NickName, Xml, Language, string.Empty, string.Empty);
		}

		/// <summary>
		/// Sends a simple private message to a chat room.
		/// 
		/// Note: The client must be an occupant of the chat room before messages
		/// can be properly propagated to other occupants of the room.
		/// </summary>
		/// <param name="RoomId">Room ID</param>
		/// <param name="Domain">Domain hosting the room.</param>
		/// <param name="NickName">Nick-name of recipient.</param>
		/// <param name="Xml">Message body.</param>
		/// <param name="Language">Language</param>
		/// <param name="ThreadId">Thread ID</param>
		public void SendCustomPrivateMessage(string RoomId, string Domain, string NickName, string Xml, string Language, string ThreadId)
		{
			this.SendCustomPrivateMessage(RoomId, Domain, NickName, Xml, Language, ThreadId, string.Empty);
		}

		/// <summary>
		/// Sends a simple private message to a chat room.
		/// 
		/// Note: The client must be an occupant of the chat room before messages
		/// can be properly propagated to other occupants of the room.
		/// </summary>
		/// <param name="RoomId">Room ID</param>
		/// <param name="Domain">Domain hosting the room.</param>
		/// <param name="NickName">Nick-name of recipient.</param>
		/// <param name="Xml">Message body.</param>
		/// <param name="Language">Language</param>
		/// <param name="ThreadId">Thread ID</param>
		/// <param name="ParentThreadId">Parent Thread ID</param>
		public void SendCustomPrivateMessage(string RoomId, string Domain, string NickName,
			string Xml, string Language, string ThreadId, string ParentThreadId)
		{
			this.client.SendMessage(MessageType.Chat, RoomId + "@" + Domain + "/" + NickName,
				 Xml + "<x xmlns='" + NamespaceMucUser + "'/>", string.Empty,
				 string.Empty, Language, ThreadId, ParentThreadId);
		}

		private Task UserMessageHandler(object Sender, MessageEventArgs e)
		{
			if (!TryParseOccupantJid(e.From, false, out string RoomId, out string Domain, out string NickName))
				return Task.CompletedTask;

			if (string.IsNullOrEmpty(NickName))
			{
				string InvitationFrom = null;
				string DeclinedFrom = null;
				string Password = null;
				string Reason = null;

				foreach (XmlNode N in e.Content.ChildNodes)
				{
					if (N is XmlElement E && E.NamespaceURI == NamespaceMucUser)
					{
						bool CheckReason = false;

						switch (E.LocalName)
						{
							case "invite":
								InvitationFrom = XML.Attribute(E, "from");
								CheckReason = true;
								break;

							case "decline":
								DeclinedFrom = XML.Attribute(E, "from");
								CheckReason = true;
								break;

							case "password":
								Password = E.InnerText;
								break;
						}

						if (CheckReason)
						{
							foreach (XmlNode N2 in E.ChildNodes)
							{
								if (N2 is XmlElement E2 && E2.NamespaceURI == NamespaceMucUser && E2.LocalName == "reason")
								{
									Reason = E2.InnerText;
									break;
								}
							}
						}
					}
				}

				if (!string.IsNullOrEmpty(InvitationFrom))
				{
					try
					{
						RoomInvitationMessageEventArgs e2 = new RoomInvitationMessageEventArgs(this, e, RoomId, Domain,
							InvitationFrom, Reason, Password);

						this.RoomInvitationReceived?.Invoke(this, e2);
					}
					catch (Exception ex)
					{
						Log.Critical(ex);
					}
				}
				else if (!string.IsNullOrEmpty(DeclinedFrom))
				{
					try
					{
						RoomDeclinedMessageEventArgs e2 = new RoomDeclinedMessageEventArgs(e, RoomId, Domain, DeclinedFrom, Reason);

						this.RoomDeclinedInvitationReceived?.Invoke(this, e2);
					}
					catch (Exception ex)
					{
						Log.Critical(ex);
					}
				}
			}
			else
			{
				try
				{
					RoomOccupantMessageEventArgs e2 = new RoomOccupantMessageEventArgs(e, RoomId, Domain, NickName);

					this.PrivateMessageReceived?.Invoke(this, e2);
				}
				catch (Exception ex)
				{
					Log.Critical(ex);
				}
			}

			return Task.CompletedTask;
		}

		/// <summary>
		/// Event raised when a group chat message from a MUC room occupant was received.
		/// </summary>
		public event RoomOccupantMessageEventHandler PrivateMessageReceived;

		/// <summary>
		/// Event raised when an invitation from a MUC room has been received.
		/// </summary>
		public event RoomInvitationMessageEventHandler RoomInvitationReceived;

		/// <summary>
		/// Event raised when an invitation from a MUC room has been declined.
		/// </summary>
		public event RoomDeclinedMessageEventHandler RoomDeclinedInvitationReceived;

		/// <summary>
		/// Changes to a new nick-name.
		/// </summary>
		/// <param name="RoomId">Room ID.</param>
		/// <param name="Domain">Domain of service hosting the room.</param>
		/// <param name="NickName">Nickname to use in the chat room.</param>
		/// <param name="Callback">Method to call when response is returned.</param>
		/// <param name="State">State object to pass on to callback method.</param>
		public void ChangeNickName(string RoomId, string Domain, string NickName,
			UserPresenceEventHandlerAsync Callback, object State)
		{
			this.client.SendDirectedPresence(string.Empty, RoomId + "@" + Domain + "/" + NickName,
				string.Empty, (sender, e) =>
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
		/// Changes to a new nick-name.
		/// </summary>
		/// <param name="RoomId">Room ID.</param>
		/// <param name="Domain">Domain of service hosting the room.</param>
		/// <param name="NickName">Nickname to use in the chat room.</param>
		/// <returns>Room entry response.</returns>
		public Task<UserPresenceEventArgs> ChangeNickNameAsync(string RoomId, string Domain,
			string NickName)
		{
			TaskCompletionSource<UserPresenceEventArgs> Result = new TaskCompletionSource<UserPresenceEventArgs>();

			this.ChangeNickName(RoomId, Domain, NickName, (sender, e) =>
			{
				Result.TrySetResult(e);
				return Task.CompletedTask;
			}, null);

			return Result.Task;
		}

		/// <summary>
		/// Sets the presence of the occupant in a room.
		/// </summary>
		/// <param name="RoomId">Room ID.</param>
		/// <param name="Domain">Domain of service hosting the room.</param>
		/// <param name="NickName">Nickname of the occupant.</param>
		/// <param name="Availability">Occupant availability.</param>
		/// <param name="Callback">Method to call when stanza has been sent.</param>
		/// <param name="State">State object to pass on to callback method.</param>
		public void SetPresence(string RoomId, string Domain, string NickName, Availability Availability,
			PresenceEventHandlerAsync Callback, object State)
		{
			string Xml = string.Empty;
			string Type = string.Empty;

			switch (Availability)
			{
				case Availability.Online:
				default:
					break;

				case Availability.Away:
					Xml = "<show>away</show>";
					break;

				case Availability.Chat:
					Xml = "<show>chat</show>";
					break;

				case Availability.DoNotDisturb:
					Xml = "<show>dnd</show>";
					break;

				case Availability.ExtendedAway:
					Xml = "<show>xa</show>";
					break;

				case Availability.Offline:
					Type = "unavailable";
					break;
			}

			this.client.SendDirectedPresence(Type, RoomId + "@" + Domain + "/" + NickName, Xml, Callback, State);
		}

		/// <summary>
		/// Sets the presence of the occupant in a room.
		/// </summary>
		/// <param name="RoomId">Room ID.</param>
		/// <param name="Domain">Domain of service hosting the room.</param>
		/// <param name="NickName">Nickname of the occupant.</param>
		/// <param name="Availability">Occupant availability.</param>
		/// <returns>Task object that finishes when stanza has been sent.</returns>
		public Task SetPresenceAsync(string RoomId, string Domain, string NickName,
			Availability Availability)
		{
			TaskCompletionSource<bool> Result = new TaskCompletionSource<bool>();
			this.SetPresence(RoomId, Domain, NickName, Availability, (sender, e) =>
			{
				Result.TrySetResult(true);
				return Task.CompletedTask;
			}, null);

			return Result.Task;
		}

		/// <summary>
		/// Sends an invitation to the room.
		/// </summary>
		/// <param name="RoomId">Room ID</param>
		/// <param name="Domain">Domain hosting the room.</param>
		/// <param name="BareJid">Bare JID of entity to invite.</param>
		public void Invite(string RoomId, string Domain, string BareJid)
		{
			this.Invite(RoomId, Domain, BareJid, string.Empty, string.Empty);
		}

		/// <summary>
		/// Sends an invitation to the room.
		/// </summary>
		/// <param name="RoomId">Room ID</param>
		/// <param name="Domain">Domain hosting the room.</param>
		/// <param name="BareJid">Bare JID of entity to invite.</param>
		/// <param name="Reason">Reason for sending the invitation.</param>
		public void Invite(string RoomId, string Domain, string BareJid, string Reason)
		{
			this.Invite(RoomId, Domain, BareJid, Reason, string.Empty);
		}

		/// <summary>
		/// Sends an invitation to the room.
		/// </summary>
		/// <param name="RoomId">Room ID</param>
		/// <param name="Domain">Domain hosting the room.</param>
		/// <param name="BareJid">Bare JID of entity to invite.</param>
		/// <param name="Reason">Reason for sending the invitation.</param>
		/// <param name="Language">Language</param>
		public void Invite(string RoomId, string Domain, string BareJid, string Reason, string Language)
		{
			StringBuilder Xml = new StringBuilder();

			Xml.Append("<x xmlns='");
			Xml.Append(NamespaceMucUser);
			Xml.Append("'><invite to='");
			Xml.Append(XML.Encode(BareJid));
			Xml.Append("'>");

			if (!string.IsNullOrEmpty(Reason))
			{
				Xml.Append("<reason>");
				Xml.Append(XML.Encode(Reason));
				Xml.Append("</reason>");
			}

			Xml.Append("</invite></x>");

			this.client.SendMessage(MessageType.Normal, RoomId + "@" + Domain,
				Xml.ToString(), string.Empty, string.Empty, Language, string.Empty, string.Empty);
		}

		/// <summary>
		/// Declines an invitation from a room.
		/// </summary>
		/// <param name="RoomId">Room ID</param>
		/// <param name="Domain">Domain hosting the room.</param>
		/// <param name="InviteFrom">Original invitation was sent from.</param>
		/// <param name="Reason">Reason for declining the invitation.</param>
		/// <param name="Language">Language</param>
		public void DeclineInvitation(string RoomId, string Domain, string InviteFrom, string Reason, string Language)
		{
			StringBuilder Xml = new StringBuilder();

			Xml.Append("<x xmlns='");
			Xml.Append(NamespaceMucUser);
			Xml.Append("'><decline to='");
			Xml.Append(XML.Encode(XmppClient.GetBareJID(InviteFrom)));
			Xml.Append("'>");

			if (!string.IsNullOrEmpty(Reason))
			{
				Xml.Append("<reason>");
				Xml.Append(XML.Encode(Reason));
				Xml.Append("</reason>");
			}

			Xml.Append("</decline></x>");

			this.client.SendMessage(MessageType.Normal, RoomId + "@" + Domain,
				Xml.ToString(), string.Empty, string.Empty, Language, string.Empty, string.Empty);
		}

		/// <summary>
		/// Starts the registration process with a room.
		/// </summary>
		/// <param name="RoomId">Room ID.</param>
		/// <param name="Domain">Domain of service hosting the room.</param>
		/// <param name="Callback">Method to call when response has been returned.</param>
		/// <param name="State">State object to pass on to callback method.</param>
		public void RegisterWithRoom(string RoomId, string Domain, RoomRegistrationEventHandler Callback,
			object State)
		{
			this.RegisterWithRoom(RoomId, Domain, Callback, null, State);
		}

		/// <summary>
		/// Starts the registration process with a room.
		/// </summary>
		/// <param name="RoomId">Room ID.</param>
		/// <param name="Domain">Domain of service hosting the room.</param>
		/// <param name="Callback">Method to call when response has been returned.</param>
		/// <param name="SubmissionCallback">Method to call when configuration has been submitted.</param>
		/// <param name="State">State object to pass on to callback method.</param>
		public void RegisterWithRoom(string RoomId, string Domain, RoomRegistrationEventHandler Callback,
			IqResultEventHandlerAsync SubmissionCallback, object State)
		{
			StringBuilder Xml = new StringBuilder();

			Xml.Append("<query xmlns='");
			Xml.Append(XmppClient.NamespaceRegister);
			Xml.Append("'/>");

			this.client.SendIqGet(RoomId + "@" + Domain, Xml.ToString(), (sender, e) =>
			{
				DataForm Form = null;
				bool AlreadyRegistered = false;
				string UserName = null;

				if (!(e.FirstElement is null))
				{
					foreach (XmlNode N in e.FirstElement.ChildNodes)
					{
						if (N is XmlElement E)
						{
							switch (E.LocalName)
							{
								case "registered":
									AlreadyRegistered = true;
									break;

								case "username":
									UserName = E.InnerText;
									break;

								case "x":
									if (E.NamespaceURI != XmppClient.NamespaceData)
										break;

									Form = new DataForm(this.client, E,
										(sender2, Form2) =>
										{
											StringBuilder Xml2 = new StringBuilder();

											Xml2.Append("<query xmlns='");
											Xml2.Append(XmppClient.NamespaceRegister);
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
											Xml2.Append(XmppClient.NamespaceRegister);
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
				}

				RoomRegistrationEventArgs e2 = new RoomRegistrationEventArgs(e, Form, AlreadyRegistered, UserName);

				try
				{
					Callback(this, e2);
				}
				catch (Exception ex)
				{
					Log.Critical(ex);
				}

				return Task.CompletedTask;
			}, State);
		}

		/// <summary>
		/// Starts the registration process with a room.
		/// </summary>
		/// <param name="RoomId">Room ID.</param>
		/// <param name="Domain">Domain of service hosting the room.</param>
		/// <returns>Room Registration response.</returns>
		public Task<RoomRegistrationEventArgs> RegisterWithRoomAsync(string RoomId, string Domain)
		{
			return this.RegisterWithRoomAsync(RoomId, Domain, null, null);
		}

		/// <summary>
		/// Starts the registration process with a room.
		/// </summary>
		/// <param name="RoomId">Room ID.</param>
		/// <param name="Domain">Domain of service hosting the room.</param>
		/// <param name="SubmissionCallback">Method to call when configuration has been submitted.</param>
		/// <param name="State">State object to pass on to callback method.</param>
		/// <returns>Room Registration response.</returns>
		public Task<RoomRegistrationEventArgs> RegisterWithRoomAsync(string RoomId, string Domain,
			IqResultEventHandlerAsync SubmissionCallback, object State)
		{
			TaskCompletionSource<RoomRegistrationEventArgs> Result = new TaskCompletionSource<RoomRegistrationEventArgs>();

			this.RegisterWithRoom(RoomId, Domain, (sender, e) => Result.TrySetResult(e), 
				SubmissionCallback, State);

			return Result.Task;
		}

		/// <summary>
		/// Gets information about a room.
		/// </summary>
		/// <param name="RoomId">Room ID.</param>
		/// <param name="Domain">Domain of service hosting the room.</param>
		/// <param name="Callback">Method to call when response has been returned.</param>
		/// <param name="State">State object to pass on to callback method.</param>
		public void GetRoomInformation(string RoomId, string Domain, RoomInformationEventHandler Callback, object State)
		{
			this.client.SendServiceDiscoveryRequest(RoomId + "@" + Domain, (sender, e) =>
			{
				return Callback?.Invoke(this, new RoomInformationEventArgs(e)) ?? Task.CompletedTask;
			}, State);
		}

		/// <summary>
		/// Gets information about a room.
		/// </summary>
		/// <param name="RoomId">Room ID.</param>
		/// <param name="Domain">Domain of service hosting the room.</param>
		public Task<RoomInformationEventArgs> GetRoomInformationAsync(string RoomId, string Domain)
		{
			TaskCompletionSource<RoomInformationEventArgs> Result = new TaskCompletionSource<RoomInformationEventArgs>();
			
			this.GetRoomInformation(RoomId, Domain, (sender, e) =>
			{
				Result.TrySetResult(e);
				return Task.CompletedTask;
			}, null);

			return Result.Task;
		}
	}
}
