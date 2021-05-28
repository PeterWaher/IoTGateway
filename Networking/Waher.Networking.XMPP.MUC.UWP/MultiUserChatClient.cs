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
		/// jabber:x:conference
		/// </summary>
		public const string NamespaceJabberConference = "jabber:x:conference";

		/// <summary>
		/// http://jabber.org/protocol/muc#owner
		/// </summary>
		public const string NamespaceMucOwner = "http://jabber.org/protocol/muc#owner";

		/// <summary>
		/// http://jabber.org/protocol/muc#register
		/// </summary>
		public const string FormTypeRegister = "http://jabber.org/protocol/muc#register";

		/// <summary>
		/// http://jabber.org/protocol/muc#request
		/// </summary>
		public const string FormTypeRequest = "http://jabber.org/protocol/muc#request";

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
			this.client.RegisterMessageHandler("x", NamespaceJabberConference, this.DirectInvitationMessageHandler, true);
			this.client.RegisterMessageFormHandler(FormTypeRequest, this.UserRequestHandler);
			this.client.RegisterMessageFormHandler(FormTypeRegister, this.RegistrationRequestHandler);

			this.client.OnGroupChatMessage += Client_OnGroupChatMessage;
		}

		/// <summary>
		/// <see cref="IDisposable.Dispose"/>
		/// </summary>
		public override void Dispose()
		{
			this.client.UnregisterPresenceHandler("x", NamespaceMucUser, this.UserPresenceHandler, true);
			this.client.UnregisterMessageHandler("x", NamespaceMucUser, this.UserMessageHandler, false);
			this.client.UnregisterMessageHandler("x", NamespaceJabberConference, this.DirectInvitationMessageHandler, true);
			this.client.UnregisterMessageFormHandler(FormTypeRequest, this.UserRequestHandler);
			this.client.UnregisterMessageFormHandler(FormTypeRegister, this.RegistrationRequestHandler);

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
			{
				if (e2.RoomDestroyed)
					return this.RoomDestroyed?.Invoke(this, e2) ?? Task.CompletedTask;
				else if (string.IsNullOrEmpty(e2.NickName))
					return this.RoomPresence?.Invoke(this, e2) ?? Task.CompletedTask;
				else
					return this.OccupantPresence?.Invoke(this, e2) ?? Task.CompletedTask;
			}
			else
				return Task.CompletedTask;
		}

		/// <summary>
		/// Event raised when user presence is received from a room occupant.
		/// </summary>
		public event UserPresenceEventHandlerAsync OccupantPresence;

		/// <summary>
		/// Event raised when user presence is received from a room.
		/// </summary>
		public event UserPresenceEventHandlerAsync RoomPresence;

		/// <summary>
		/// Event raised when a room where the client is an occupant is destroyed.
		/// </summary>
		public event UserPresenceEventHandlerAsync RoomDestroyed;

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
			string FullJid = string.Empty;
			string Reason = string.Empty;
			bool RoomDestroyed = false;

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

							foreach (XmlNode N3 in E2.ChildNodes)
							{
								if (!(N3 is XmlElement E3) || E3.NamespaceURI != NamespaceMucUser)
									continue;

								switch (E3.LocalName)
								{
									case "actor":
										NickName = XML.Attribute(E3, "nick", NickName);
										break;

									case "reason":
										Reason = E3.InnerText;
										break;
								}
							}
							break;

						case "status":
							if (Status is null)
								Status = new List<MucStatus>();

							Status.Add((MucStatus)XML.Attribute(E2, "code", 0));
							break;

						case "destroy":
							RoomDestroyed = true;

							foreach (XmlNode N3 in E2.ChildNodes)
							{
								if (!(N3 is XmlElement E3) || E3.NamespaceURI != NamespaceMucUser)
									continue;

								switch (E3.LocalName)
								{
									case "reason":
										Reason = E3.InnerText;
										break;
								}
							}
							break;
					}
				}
			}

			Result = new UserPresenceEventArgs(e, RoomId, Domain, NickName,
				Affiliation, Role, FullJid, Reason, RoomDestroyed,
				Status?.ToArray() ?? new MucStatus[0]);

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

		#region Enter Room

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
							Affiliation.None, Role.None, string.Empty, string.Empty, false)
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

		#endregion

		#region Leave Room

		/// <summary>
		/// Leave a chat room.
		/// </summary>
		/// <param name="RoomId">Room ID.</param>
		/// <param name="Domain">Domain of service hosting the room.</param>
		/// <param name="NickName">Nickname used in the chat room.</param>
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
							Affiliation.None, Role.None, string.Empty, string.Empty, false)
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

		#endregion

		#region Create Instant Room

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

		#endregion

		#region Room Configuration

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
								this.SubmitConfigurationForm,
								this.CancelConfigurationForm,
								e.From, e.To)
							{
								State = new ConfigState()
								{
									SubmissionCallback = SubmissionCallback,
									State = e.State
								}
							};

							break;
						}
					}
				}

				return Callback?.Invoke(this, Form) ?? Task.CompletedTask;
			}, State);
		}

		private class ConfigState
		{
			public IqResultEventHandlerAsync SubmissionCallback;
			public object State;
		}

		/// <summary>
		/// Submits a room configuration form.
		/// </summary>
		/// <param name="ConfigurationForm">Room configuration form.</param>
		public Task SubmitConfigurationForm(DataForm ConfigurationForm)
		{
			TaskCompletionSource<bool> Result = new TaskCompletionSource<bool>();

			ConfigurationForm.State = new ConfigState()
			{
				SubmissionCallback = (sender, e) =>
				{
					if (e.Ok)
						Result.TrySetResult(true);
					else
						Result.TrySetException(e.StanzaError is null ? new Exception("Unable to configure room.") : e.StanzaError);
						
					return Task.CompletedTask;
				}
			};

			this.SubmitConfigurationForm(this, ConfigurationForm);

			return Result.Task;
		}

		private Task SubmitConfigurationForm(object Sender, DataForm ConfigurationForm)
		{
			StringBuilder Xml2 = new StringBuilder();
			ConfigState State = ConfigurationForm.State as ConfigState;

			Xml2.Append("<query xmlns='");
			Xml2.Append(NamespaceMucOwner);
			Xml2.Append("'>");

			ConfigurationForm.SerializeSubmit(Xml2);

			Xml2.Append("</query>");

			this.client.SendIqSet(ConfigurationForm.From, Xml2.ToString(), (sender3, e3) =>
			{
				e3.State = State?.State;
				return State?.SubmissionCallback?.Invoke(this, e3) ?? Task.CompletedTask;
			}, null);

			return Task.CompletedTask;
		}

		/// <summary>
		/// Cancels a room configuration form.
		/// </summary>
		/// <param name="ConfigurationForm">Room configuration form.</param>
		public Task CancelConfigurationForm(DataForm ConfigurationForm)
		{
			TaskCompletionSource<bool> Result = new TaskCompletionSource<bool>();

			ConfigurationForm.State = new ConfigState()
			{
				SubmissionCallback = (sender, e) =>
				{
					Result.TrySetResult(true);
					return Task.CompletedTask;
				}
			};

			this.CancelConfigurationForm(this, ConfigurationForm);

			return Result.Task;
		}

		private Task CancelConfigurationForm(object Sender, DataForm ConfigurationForm)
		{
			StringBuilder Xml2 = new StringBuilder();
			ConfigState State = ConfigurationForm.State as ConfigState;

			Xml2.Append("<query xmlns='");
			Xml2.Append(NamespaceMucOwner);
			Xml2.Append("'>");

			ConfigurationForm.SerializeCancel(Xml2);

			Xml2.Append("</query>");

			this.client.SendIqSet(ConfigurationForm.From, Xml2.ToString(), (sender3, e3) =>
			{
				e3.Ok = false;
				e3.State = State?.State;
				return State?.SubmissionCallback?.Invoke(this, e3) ?? Task.CompletedTask;
			}, null);

			return Task.CompletedTask;
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

		#endregion

		#region Group Chat Messages

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

		#endregion

		#region Room Subjects

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

		#endregion

		#region Private Messages

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
			this.SendCustomPrivateMessage(string.Empty, RoomId, Domain, NickName, Xml, Language, ThreadId, ParentThreadId);
		}

		/// <summary>
		/// Sends a simple private message to a chat room.
		/// 
		/// Note: The client must be an occupant of the chat room before messages
		/// can be properly propagated to other occupants of the room.
		/// </summary>
		/// <param name="MessageId">Message ID</param>
		/// <param name="RoomId">Room ID</param>
		/// <param name="Domain">Domain hosting the room.</param>
		/// <param name="NickName">Nick-name of recipient.</param>
		/// <param name="Xml">Message body.</param>
		/// <param name="Language">Language</param>
		/// <param name="ThreadId">Thread ID</param>
		/// <param name="ParentThreadId">Parent Thread ID</param>
		public void SendCustomPrivateMessage(string MessageId, string RoomId, string Domain, string NickName,
			string Xml, string Language, string ThreadId, string ParentThreadId)
		{
			this.client.SendMessage(QoSLevel.Unacknowledged, MessageType.Chat, MessageId, RoomId + "@" + Domain + "/" + NickName,
				 Xml + "<x xmlns='" + NamespaceMucUser + "'/>", string.Empty, string.Empty, Language, ThreadId, ParentThreadId, null, null);
		}

		#endregion

		#region Room Invitations

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

		#endregion

		#region Direct Invitation (XEP-0249)

		/// <summary>
		/// Sends a direct invitation to the room.
		/// </summary>
		/// <param name="RoomId">Room ID</param>
		/// <param name="Domain">Domain hosting the room.</param>
		/// <param name="BareJid">Bare JID of entity to invite.</param>
		public void InviteDirect(string RoomId, string Domain, string BareJid)
		{
			this.InviteDirect(RoomId, Domain, BareJid, string.Empty, string.Empty, string.Empty, string.Empty);
		}

		/// <summary>
		/// Sends a direct invitation to the room.
		/// </summary>
		/// <param name="RoomId">Room ID</param>
		/// <param name="Domain">Domain hosting the room.</param>
		/// <param name="BareJid">Bare JID of entity to invite.</param>
		/// <param name="Reason">Reason for sending the invitation.</param>
		public void InviteDirect(string RoomId, string Domain, string BareJid, string Reason)
		{
			this.InviteDirect(RoomId, Domain, BareJid, Reason, string.Empty, string.Empty, string.Empty);
		}

		/// <summary>
		/// Sends a direct invitation to the room.
		/// </summary>
		/// <param name="RoomId">Room ID</param>
		/// <param name="Domain">Domain hosting the room.</param>
		/// <param name="BareJid">Bare JID of entity to invite.</param>
		/// <param name="Reason">Reason for sending the invitation.</param>
		/// <param name="Language">Language</param>
		public void InviteDirect(string RoomId, string Domain, string BareJid, string Reason, string Language)
		{
			this.InviteDirect(RoomId, Domain, BareJid, Reason, Language, string.Empty, string.Empty);
		}

		/// <summary>
		/// Sends a direct invitation to the room.
		/// </summary>
		/// <param name="RoomId">Room ID</param>
		/// <param name="Domain">Domain hosting the room.</param>
		/// <param name="BareJid">Bare JID of entity to invite.</param>
		/// <param name="Reason">Reason for sending the invitation.</param>
		/// <param name="Language">Language</param>
		/// <param name="Password">Password required to enter room.</param>
		public void InviteDirect(string RoomId, string Domain, string BareJid, string Reason, string Language, string Password)
		{
			this.InviteDirect(RoomId, Domain, BareJid, Reason, Language, Password, string.Empty);
		}

		/// <summary>
		/// Sends a direct invitation to the room.
		/// </summary>
		/// <param name="RoomId">Room ID</param>
		/// <param name="Domain">Domain hosting the room.</param>
		/// <param name="BareJid">Bare JID of entity to invite.</param>
		/// <param name="Reason">Reason for sending the invitation.</param>
		/// <param name="Language">Language</param>
		/// <param name="Password">Password required to enter room.</param>
		/// <param name="ThreadId">If the invitation is a continuation of a private thread.</param>
		public void InviteDirect(string RoomId, string Domain, string BareJid, string Reason, string Language, string Password, string ThreadId)
		{
			this.InviteDirect(null, RoomId, Domain, BareJid, Reason, Language, Password, ThreadId);
		}

		/// <summary>
		/// Sends a direct invitation to the room.
		/// </summary>
		/// <param name="Endpoint">End-to-End Encryption endpoint.</param>
		/// <param name="RoomId">Room ID</param>
		/// <param name="Domain">Domain hosting the room.</param>
		/// <param name="BareJid">Bare JID of entity to invite.</param>
		/// <param name="Reason">Reason for sending the invitation.</param>
		/// <param name="Language">Language</param>
		/// <param name="Password">Password required to enter room.</param>
		/// <param name="ThreadId">If the invitation is a continuation of a private thread.</param>
		public void InviteDirect(IEndToEndEncryption Endpoint, string RoomId, string Domain, string BareJid, string Reason, string Language, 
			string Password, string ThreadId)
		{
			StringBuilder Xml = new StringBuilder();

			Xml.Append("<x xmlns='");
			Xml.Append(NamespaceJabberConference);
			Xml.Append("' jid='");
			Xml.Append(XML.Encode(RoomId));
			Xml.Append('@');
			Xml.Append(XML.Encode(Domain));

			if (!string.IsNullOrEmpty(Password))
			{
				Xml.Append("' password='");
				Xml.Append(XML.Encode(Password));
			}

			if (!string.IsNullOrEmpty(Reason))
			{
				Xml.Append("' reason='");
				Xml.Append(XML.Encode(Reason));
			}

			if (!string.IsNullOrEmpty(ThreadId))
			{
				Xml.Append("' continue='true' thread='");
				Xml.Append(XML.Encode(ThreadId));
			}

			Xml.Append("'/>");

			if (Endpoint is null)
			{
				this.client.SendMessage(MessageType.Normal, BareJid, Xml.ToString(), string.Empty, string.Empty, Language,
					string.Empty, string.Empty);
			}
			else
			{
				Endpoint.SendMessage(this.Client, E2ETransmission.NormalIfNotE2E, QoSLevel.Unacknowledged, MessageType.Normal,
					string.Empty, BareJid, Xml.ToString(), string.Empty, string.Empty, Language, string.Empty, string.Empty, null, null);
			}
		}

		private async Task DirectInvitationMessageHandler(object Sender, MessageEventArgs e)
		{
			DirectInvitationMessageEventHandler h = this.DirectInvitationReceived;
			if (!(h is null))
			{
				try
				{
					string RoomJid = XML.Attribute(e.Content, "jid");
					string Password = XML.Attribute(e.Content, "password");
					string Reason = XML.Attribute(e.Content, "reason");
					string ThreadId = XML.Attribute(e.Content, "thread");
					bool Continue = XML.Attribute(e.Content, "continue", false);
					int i = RoomJid.IndexOf('@');

					if (i < 0)
						return;

					string RoomId = RoomJid.Substring(0, i);
					string Domain = RoomJid.Substring(i + 1);

					await h(this, new DirectInvitationMessageEventArgs(this, e, RoomId, Domain, Reason, Password, Continue, ThreadId));
				}
				catch (Exception ex)
				{
					Log.Critical(ex);
				}
			}
		}

		/// <summary>
		/// Event raised when a direct invitation from a peer has been received.
		/// </summary>
		public event DirectInvitationMessageEventHandler DirectInvitationReceived;

		#endregion

		#region Changing Nick-Name

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
							Affiliation.None, Role.None, string.Empty, string.Empty, false)
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

		#endregion

		#region Setting Room Presence

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

		#endregion

		#region Room Registration

		/// <summary>
		/// Starts the registration process with a room.
		/// </summary>
		/// <param name="RoomId">Room ID.</param>
		/// <param name="Domain">Domain of service hosting the room.</param>
		/// <param name="Callback">Method to call when response has been returned.</param>
		/// <param name="State">State object to pass on to callback method.</param>
		public void GetRoomRegistrationForm(string RoomId, string Domain, RoomRegistrationEventHandler Callback,
			object State)
		{
			this.GetRoomRegistrationForm(RoomId, Domain, Callback, null, State);
		}

		/// <summary>
		/// Starts the registration process with a room.
		/// </summary>
		/// <param name="RoomId">Room ID.</param>
		/// <param name="Domain">Domain of service hosting the room.</param>
		/// <param name="Callback">Method to call when response has been returned.</param>
		/// <param name="SubmissionCallback">Method to call when configuration has been submitted.</param>
		/// <param name="State">State object to pass on to callback method.</param>
		public void GetRoomRegistrationForm(string RoomId, string Domain, RoomRegistrationEventHandler Callback,
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
					Callback?.Invoke(this, e2);
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
		public Task<RoomRegistrationEventArgs> GetRoomRegistrationFormAsync(string RoomId, string Domain)
		{
			return this.GetRoomRegistrationFormAsync(RoomId, Domain, null, null);
		}

		/// <summary>
		/// Starts the registration process with a room.
		/// </summary>
		/// <param name="RoomId">Room ID.</param>
		/// <param name="Domain">Domain of service hosting the room.</param>
		/// <param name="SubmissionCallback">Method to call when configuration has been submitted.</param>
		/// <param name="State">State object to pass on to callback method.</param>
		/// <returns>Room Registration response.</returns>
		public Task<RoomRegistrationEventArgs> GetRoomRegistrationFormAsync(string RoomId, string Domain,
			IqResultEventHandlerAsync SubmissionCallback, object State)
		{
			TaskCompletionSource<RoomRegistrationEventArgs> Result = new TaskCompletionSource<RoomRegistrationEventArgs>();

			this.GetRoomRegistrationForm(RoomId, Domain, (sender, e) => Result.TrySetResult(e),
				SubmissionCallback, State);

			return Result.Task;
		}

		private Task RegistrationRequestHandler(object Sender, MessageFormEventArgs e)
		{
			return this.RegistrationRequest?.Invoke(this, e) ?? Task.CompletedTask;
		}

		/// <summary>
		/// Event raised when someone has made a request to register with a room to which the client is an administrator.
		/// </summary>
		public event MessageFormEventHandlerAsync RegistrationRequest;

		#endregion

		#region Room Information

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
		/// <returns>Room information response.</returns>
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

		/// <summary>
		/// Gets information about my registered nick-name.
		/// </summary>
		/// <param name="RoomId">Room ID.</param>
		/// <param name="Domain">Domain of service hosting the room.</param>
		/// <param name="Callback">Method to call when response has been returned.</param>
		/// <param name="State">State object to pass on to callback method.</param>
		public void GetMyNickName(string RoomId, string Domain, ServiceDiscoveryEventHandler Callback, object State)
		{
			this.client.SendServiceDiscoveryRequest(RoomId + "@" + Domain, "x-roomuser-item", (sender, e) =>
			{
				return Callback?.Invoke(this, e) ?? Task.CompletedTask;
			}, State);
		}

		/// <summary>
		/// Gets information about my registered nick-name.
		/// </summary>
		/// <param name="RoomId">Room ID.</param>
		/// <param name="Domain">Domain of service hosting the room.</param>
		/// <returns>Information about my registered nick-name, if any, and if supported by the service.</returns>
		public Task<ServiceDiscoveryEventArgs> GetMyNickNameAsync(string RoomId, string Domain)
		{
			TaskCompletionSource<ServiceDiscoveryEventArgs> Result = new TaskCompletionSource<ServiceDiscoveryEventArgs>();

			this.GetMyNickName(RoomId, Domain, (sender, e) =>
			{
				Result.TrySetResult(e);
				return Task.CompletedTask;
			}, null);

			return Result.Task;
		}

		/// <summary>
		/// Gets information about items (occupants) in a room.
		/// </summary>
		/// <param name="RoomId">Room ID.</param>
		/// <param name="Domain">Domain of service hosting the room.</param>
		/// <param name="Callback">Method to call when response has been returned.</param>
		/// <param name="State">State object to pass on to callback method.</param>
		public void GetRoomItems(string RoomId, string Domain, ServiceItemsDiscoveryEventHandler Callback, object State)
		{
			this.client.SendServiceItemsDiscoveryRequest(RoomId + "@" + Domain, Callback, State);
		}

		/// <summary>
		/// Gets information about items (occupants) in a room.
		/// </summary>
		/// <param name="RoomId">Room ID.</param>
		/// <param name="Domain">Domain of service hosting the room.</param>
		/// <returns>Room items response.</returns>
		public Task<ServiceItemsDiscoveryEventArgs> GetRoomItemsAsync(string RoomId, string Domain)
		{
			return this.client.ServiceItemsDiscoveryAsync(RoomId + "@" + Domain);
		}

		/// <summary>
		/// Performs a service discocery request on an occupant of a room.
		/// </summary>
		/// <param name="RoomId">Room ID.</param>
		/// <param name="Domain">Domain of service hosting the room.</param>
		/// <param name="NickName">Nick-name of occupant.</param>
		/// <param name="Callback">Method to call when response has been returned.</param>
		/// <param name="State">State object to pass on to callback method.</param>
		public void OccupantServiceDiscovery(string RoomId, string Domain, string NickName,
			ServiceDiscoveryEventHandler Callback, object State)
		{
			this.client.SendServiceDiscoveryRequest(RoomId + "@" + Domain + "/" + NickName, Callback, State);
		}

		/// <summary>
		/// Performs a service discocery request on an occupant of a room.
		/// </summary>
		/// <param name="RoomId">Room ID.</param>
		/// <param name="Domain">Domain of service hosting the room.</param>
		/// <param name="NickName">Nick-name of occupant.</param>
		/// <returns>Response to service discovery request.</returns>
		public Task<ServiceDiscoveryEventArgs> OccupantServiceDiscoveryAsync(string RoomId, string Domain, string NickName)
		{
			return this.client.ServiceDiscoveryAsync(RoomId + "@" + Domain + "/" + NickName);
		}

		/// <summary>
		/// Performs a service items discocery request on an occupant of a room.
		/// </summary>
		/// <param name="RoomId">Room ID.</param>
		/// <param name="Domain">Domain of service hosting the room.</param>
		/// <param name="NickName">Nick-name of occupant.</param>
		/// <param name="Callback">Method to call when response has been returned.</param>
		/// <param name="State">State object to pass on to callback method.</param>
		public void OccupantServiceItemsDiscovery(string RoomId, string Domain, string NickName,
			ServiceItemsDiscoveryEventHandler Callback, object State)
		{
			this.client.SendServiceItemsDiscoveryRequest(RoomId + "@" + Domain + "/" + NickName, Callback, State);
		}

		/// <summary>
		/// Performs a service items discocery request on an occupant of a room.
		/// </summary>
		/// <param name="RoomId">Room ID.</param>
		/// <param name="Domain">Domain of service hosting the room.</param>
		/// <param name="NickName">Nick-name of occupant.</param>
		/// <returns>Response to service items discovery request.</returns>
		public Task<ServiceItemsDiscoveryEventArgs> OccupantServiceItemsDiscoveryAsync(string RoomId, string Domain, string NickName)
		{
			return this.client.ServiceItemsDiscoveryAsync(RoomId + "@" + Domain + "/" + NickName);
		}

		#endregion

		#region Configuring Occupants

		/// <summary>
		/// Configures the role of an occupant.
		/// 
		/// Note: You will need moderator, admin or owner privileges, depending
		/// on the type of change requested.
		/// </summary>
		/// <param name="RoomId">Room ID.</param>
		/// <param name="Domain">Domain of service hosting the room.</param>
		/// <param name="OccupantNickName">Nick-name of the occupant to modify.</param>
		/// <param name="Role">New role.</param>
		/// <param name="Reason">Reason for change.</param>
		/// <param name="Callback">Method to call when response is returned from room.</param>
		/// <param name="State">State object to pass on to callback method.</param>
		public void ConfigureOccupant(string RoomId, string Domain, string OccupantNickName,
			Role Role, string Reason, IqResultEventHandlerAsync Callback, object State)
		{
			this.ConfigureOccupant(RoomId, Domain,
				new MucOccupantConfiguration(OccupantNickName, Role, Reason),
				Callback, State);
		}

		/// <summary>
		/// Configures the affiliation of an occupant.
		/// 
		/// Note: You will need moderator, admin or owner privileges, depending
		/// on the type of change requested.
		/// </summary>
		/// <param name="RoomId">Room ID.</param>
		/// <param name="Domain">Domain of service hosting the room.</param>
		/// <param name="OccupantBareJid">Bare JID of the occupant to modify.</param>
		/// <param name="Affiliation">New affiliation.</param>
		/// <param name="Reason">Reason for change.</param>
		/// <param name="Callback">Method to call when response is returned from room.</param>
		/// <param name="State">State object to pass on to callback method.</param>
		public void ConfigureOccupant(string RoomId, string Domain, string OccupantBareJid,
			Affiliation Affiliation, string Reason, IqResultEventHandlerAsync Callback, object State)
		{
			this.ConfigureOccupant(RoomId, Domain,
				new MucOccupantConfiguration(OccupantBareJid, Affiliation, Reason),
				Callback, State);
		}

		/// <summary>
		/// Sets the state (affiliation and/or role) of an occupant.
		/// 
		/// Note: You will need moderator, admin or owner privileges, depending
		/// on the type of change requested.
		/// </summary>
		/// <param name="RoomId">Room ID.</param>
		/// <param name="Domain">Domain of service hosting the room.</param>
		/// <param name="OccupantBareJid">Bare JID of the occupant to modify.</param>
		/// <param name="OccupantNickName">Nick-Name of the occupant to modify.</param>
		/// <param name="Affiliation">Optional new affiliation.</param>
		/// <param name="Role">Optional new role.</param>
		/// <param name="Reason">Reason for change.</param>
		/// <param name="Callback">Method to call when response is returned from room.</param>
		/// <param name="State">State object to pass on to callback method.</param>
		public void ConfigureOccupant(string RoomId, string Domain, string OccupantBareJid, string OccupantNickName,
			Affiliation? Affiliation, Role? Role, string Reason, IqResultEventHandlerAsync Callback, object State)
		{
			this.ConfigureOccupant(RoomId, Domain,
				new MucOccupantConfiguration(OccupantNickName, OccupantBareJid, Affiliation, Role, Reason),
				Callback, State);
		}

		/// <summary>
		/// Configures the states (affiliations and/or roles) of occupants.
		/// 
		/// Note: You will need moderator, admin or owner privileges, depending
		/// on the type of change requested.
		/// </summary>
		/// <param name="RoomId">Room ID.</param>
		/// <param name="Domain">Domain of service hosting the room.</param>
		/// <param name="Change">Configuration to perform.</param>
		/// <param name="Callback">Method to call when response is returned from room.</param>
		/// <param name="State">State object to pass on to callback method.</param>
		public void ConfigureOccupant(string RoomId, string Domain, MucOccupantConfiguration Change,
			IqResultEventHandlerAsync Callback, object State)
		{
			this.ConfigureOccupants(RoomId, Domain, new MucOccupantConfiguration[] { Change }, Callback, State);
		}

		/// <summary>
		/// Configures the states (affiliations and/or roles) of occupants.
		/// 
		/// Note: You will need moderator, admin or owner privileges, depending
		/// on the type of change requested.
		/// </summary>
		/// <param name="RoomId">Room ID.</param>
		/// <param name="Domain">Domain of service hosting the room.</param>
		/// <param name="Changes">Configurations to perform.</param>
		/// <param name="Callback">Method to call when response is returned from room.</param>
		/// <param name="State">State object to pass on to callback method.</param>
		public void ConfigureOccupants(string RoomId, string Domain, MucOccupantConfiguration[] Changes,
			IqResultEventHandlerAsync Callback, object State)
		{
			StringBuilder Xml = new StringBuilder();

			Xml.Append("<query xmlns='");
			Xml.Append(NamespaceMucAdmin);
			Xml.Append("'>");

			foreach (MucOccupantConfiguration Config in Changes)
			{
				Xml.Append("<item");

				if (!string.IsNullOrEmpty(Config.Jid))
				{
					Xml.Append(" jid='");
					Xml.Append(XML.Encode(Config.Jid));
					Xml.Append('\'');
				}

				if (!string.IsNullOrEmpty(Config.NickName))
				{
					Xml.Append(" nick='");
					Xml.Append(XML.Encode(Config.NickName));
					Xml.Append('\'');
				}

				if (Config.Affiliation.HasValue)
				{
					Xml.Append(" affiliation='");
					Xml.Append(Config.Affiliation.Value.ToString().ToLower());
					Xml.Append('\'');
				}
				else if (Config.Role.HasValue)
				{
					Xml.Append(" role='");
					Xml.Append(Config.Role.Value.ToString().ToLower());
					Xml.Append('\'');
				}

				if (string.IsNullOrEmpty(Config.Reason))
					Xml.Append("/>");
				else
				{
					Xml.Append("><reason>");
					Xml.Append(XML.Encode(Config.Reason));
					Xml.Append("</reason></item>");
				}
			}

			Xml.Append("</query>");

			this.client.SendIqSet(RoomId + "@" + Domain, Xml.ToString(), Callback, State);
		}

		/// <summary>
		/// Configures the role of an occupant.
		/// 
		/// Note: You will need moderator, admin or owner privileges, depending
		/// on the type of change requested.
		/// </summary>
		/// <param name="RoomId">Room ID.</param>
		/// <param name="Domain">Domain of service hosting the room.</param>
		/// <param name="OccupantNickName">Nick-name of the occupant to modify.</param>
		/// <param name="Role">New role.</param>
		/// <param name="Reason">Reason for change.</param>
		/// <returns>Response to request.</returns>
		public Task<IqResultEventArgs> ConfigureOccupantAsync(string RoomId, string Domain, string OccupantNickName,
			Role Role, string Reason)
		{
			return this.ConfigureOccupantsAsync(RoomId, Domain,
				new MucOccupantConfiguration(OccupantNickName, Role, Reason));
		}

		/// <summary>
		/// Configures the affiliation of an occupant.
		/// 
		/// Note: You will need moderator, admin or owner privileges, depending
		/// on the type of change requested.
		/// </summary>
		/// <param name="RoomId">Room ID.</param>
		/// <param name="Domain">Domain of service hosting the room.</param>
		/// <param name="OccupantBareJid">Bare JID of the occupant to modify.</param>
		/// <param name="Affiliation">New affiliation.</param>
		/// <param name="Reason">Reason for change.</param>
		/// <returns>Response to request.</returns>
		public Task<IqResultEventArgs> ConfigureOccupantAsync(string RoomId, string Domain, string OccupantBareJid,
			Affiliation Affiliation, string Reason)
		{
			return this.ConfigureOccupantsAsync(RoomId, Domain,
				new MucOccupantConfiguration(OccupantBareJid, Affiliation, Reason));
		}

		/// <summary>
		/// Configures the state (affiliation and/or role) of an occupant.
		/// 
		/// Note: You will need moderator, admin or owner privileges, depending
		/// on the type of change requested.
		/// </summary>
		/// <param name="RoomId">Room ID.</param>
		/// <param name="Domain">Domain of service hosting the room.</param>
		/// <param name="OccupantBareJid">Bare JID of the occupant to modify.</param>
		/// <param name="OccupantNickName">Nick-Name of the occupant to modify.</param>
		/// <param name="Affiliation">Optional new affiliation.</param>
		/// <param name="Role">Optional new role.</param>
		/// <param name="Reason">Reason for change.</param>
		/// <returns>Response to request.</returns>
		public Task<IqResultEventArgs> ConfigureOccupantAsync(string RoomId, string Domain, string OccupantBareJid, string OccupantNickName,
			Affiliation? Affiliation, Role? Role, string Reason)
		{
			return this.ConfigureOccupantsAsync(RoomId, Domain,
				new MucOccupantConfiguration(OccupantBareJid, OccupantNickName, Affiliation, Role, Reason));
		}

		/// <summary>
		/// Configures the state (affiliation and/or role) of an occupant.
		/// 
		/// Note: You will need moderator, admin or owner privileges, depending
		/// on the type of change requested.
		/// </summary>
		/// <param name="RoomId">Room ID.</param>
		/// <param name="Domain">Domain of service hosting the room.</param>
		/// <param name="Changes">Configurations to perform.</param>
		/// <returns>Response to request.</returns>
		public Task<IqResultEventArgs> ConfigureOccupantsAsync(string RoomId, string Domain, params MucOccupantConfiguration[] Changes)
		{
			TaskCompletionSource<IqResultEventArgs> Result = new TaskCompletionSource<IqResultEventArgs>();

			this.ConfigureOccupants(RoomId, Domain, Changes, (sender, e) =>
			{
				Result.TrySetResult(e);
				return Task.CompletedTask;
			}, null);

			return Result.Task;
		}

		#endregion

		#region Kicking occupants

		/// <summary>
		/// Kicks an occupant out of a room.
		/// 
		/// Note: You will need moderator, admin or owner privileges, depending
		/// on the type of change requested.
		/// </summary>
		/// <param name="RoomId">Room ID.</param>
		/// <param name="Domain">Domain of service hosting the room.</param>
		/// <param name="OccupantNickName">Nick-name of the occupant to modify.</param>
		/// <param name="Callback">Method to call when response is returned from room.</param>
		/// <param name="State">State object to pass on to callback method.</param>
		public void Kick(string RoomId, string Domain, string OccupantNickName,
			IqResultEventHandlerAsync Callback, object State)
		{
			this.Kick(RoomId, Domain, OccupantNickName, string.Empty, Callback, State);
		}

		/// <summary>
		/// Kicks an occupant out of a room.
		/// 
		/// Note: You will need moderator, admin or owner privileges, depending
		/// on the type of change requested.
		/// </summary>
		/// <param name="RoomId">Room ID.</param>
		/// <param name="Domain">Domain of service hosting the room.</param>
		/// <param name="OccupantNickName">Nick-name of the occupant to modify.</param>
		/// <param name="Reason">Reason for change.</param>
		/// <param name="Callback">Method to call when response is returned from room.</param>
		/// <param name="State">State object to pass on to callback method.</param>
		public void Kick(string RoomId, string Domain, string OccupantNickName,
			string Reason, IqResultEventHandlerAsync Callback, object State)
		{
			this.ConfigureOccupant(RoomId, Domain, OccupantNickName, Role.None, Reason, Callback, State);
		}

		/// <summary>
		/// Kicks an occupant out of a room.
		/// 
		/// Note: You will need moderator, admin or owner privileges, depending
		/// on the type of change requested.
		/// </summary>
		/// <param name="RoomId">Room ID.</param>
		/// <param name="Domain">Domain of service hosting the room.</param>
		/// <param name="OccupantNickName">Nick-name of the occupant to modify.</param>
		/// <returns>Response to request.</returns>
		public Task<IqResultEventArgs> KickAsync(string RoomId, string Domain, string OccupantNickName)
		{
			return this.KickAsync(RoomId, Domain, OccupantNickName, string.Empty);
		}

		/// <summary>
		/// Kicks an occupant out of a room.
		/// 
		/// Note: You will need moderator, admin or owner privileges, depending
		/// on the type of change requested.
		/// </summary>
		/// <param name="RoomId">Room ID.</param>
		/// <param name="Domain">Domain of service hosting the room.</param>
		/// <param name="OccupantNickName">Nick-name of the occupant to modify.</param>
		/// <param name="Reason">Reason for change.</param>
		/// <returns>Response to request.</returns>
		public Task<IqResultEventArgs> KickAsync(string RoomId, string Domain, string OccupantNickName,
			string Reason)
		{
			return this.ConfigureOccupantAsync(RoomId, Domain, OccupantNickName, Role.None, Reason);
		}

		#endregion

		#region Voice privileges

		/// <summary>
		/// Grants voice to a visitor of a room.
		/// 
		/// Note: You will need moderator, admin or owner privileges, depending
		/// on the type of change requested.
		/// </summary>
		/// <param name="RoomId">Room ID.</param>
		/// <param name="Domain">Domain of service hosting the room.</param>
		/// <param name="OccupantNickName">Nick-name of the occupant to modify.</param>
		/// <param name="Callback">Method to call when response is returned from room.</param>
		/// <param name="State">State object to pass on to callback method.</param>
		public void GrantVoice(string RoomId, string Domain, string OccupantNickName,
			IqResultEventHandlerAsync Callback, object State)
		{
			this.GrantVoice(RoomId, Domain, OccupantNickName, string.Empty, Callback, State);
		}

		/// <summary>
		/// Grants voice to a visitor of a room.
		/// 
		/// Note: You will need moderator, admin or owner privileges, depending
		/// on the type of change requested.
		/// </summary>
		/// <param name="RoomId">Room ID.</param>
		/// <param name="Domain">Domain of service hosting the room.</param>
		/// <param name="OccupantNickName">Nick-name of the occupant to modify.</param>
		/// <param name="Reason">Reason for change.</param>
		/// <param name="Callback">Method to call when response is returned from room.</param>
		/// <param name="State">State object to pass on to callback method.</param>
		public void GrantVoice(string RoomId, string Domain, string OccupantNickName,
			string Reason, IqResultEventHandlerAsync Callback, object State)
		{
			this.ConfigureOccupant(RoomId, Domain, OccupantNickName, Role.Participant, Reason, Callback, State);
		}

		/// <summary>
		/// Grants voice to a visitor of a room.
		/// 
		/// Note: You will need moderator, admin or owner privileges, depending
		/// on the type of change requested.
		/// </summary>
		/// <param name="RoomId">Room ID.</param>
		/// <param name="Domain">Domain of service hosting the room.</param>
		/// <param name="OccupantNickName">Nick-name of the occupant to modify.</param>
		/// <returns>Response to request.</returns>
		public Task<IqResultEventArgs> GrantVoiceAsync(string RoomId, string Domain, string OccupantNickName)
		{
			return this.GrantVoiceAsync(RoomId, Domain, OccupantNickName, string.Empty);
		}

		/// <summary>
		/// Grants voice to a visitor of a room.
		/// 
		/// Note: You will need moderator, admin or owner privileges, depending
		/// on the type of change requested.
		/// </summary>
		/// <param name="RoomId">Room ID.</param>
		/// <param name="Domain">Domain of service hosting the room.</param>
		/// <param name="OccupantNickName">Nick-name of the occupant to modify.</param>
		/// <param name="Reason">Reason for change.</param>
		/// <returns>Response to request.</returns>
		public Task<IqResultEventArgs> GrantVoiceAsync(string RoomId, string Domain, string OccupantNickName,
			string Reason)
		{
			return this.ConfigureOccupantAsync(RoomId, Domain, OccupantNickName, Role.Participant, Reason);
		}

		/// <summary>
		/// Revokes voice to a participant of a room.
		/// 
		/// Note: You will need moderator, admin or owner privileges, depending
		/// on the type of change requested.
		/// </summary>
		/// <param name="RoomId">Room ID.</param>
		/// <param name="Domain">Domain of service hosting the room.</param>
		/// <param name="OccupantNickName">Nick-name of the occupant to modify.</param>
		/// <param name="Callback">Method to call when response is returned from room.</param>
		/// <param name="State">State object to pass on to callback method.</param>
		public void RevokeVoice(string RoomId, string Domain, string OccupantNickName,
			IqResultEventHandlerAsync Callback, object State)
		{
			this.RevokeVoice(RoomId, Domain, OccupantNickName, string.Empty, Callback, State);
		}

		/// <summary>
		/// Revokes voice to a participant of a room.
		/// 
		/// Note: You will need moderator, admin or owner privileges, depending
		/// on the type of change requested.
		/// </summary>
		/// <param name="RoomId">Room ID.</param>
		/// <param name="Domain">Domain of service hosting the room.</param>
		/// <param name="OccupantNickName">Nick-name of the occupant to modify.</param>
		/// <param name="Reason">Reason for change.</param>
		/// <param name="Callback">Method to call when response is returned from room.</param>
		/// <param name="State">State object to pass on to callback method.</param>
		public void RevokeVoice(string RoomId, string Domain, string OccupantNickName,
			string Reason, IqResultEventHandlerAsync Callback, object State)
		{
			this.ConfigureOccupant(RoomId, Domain, OccupantNickName, Role.Visitor, Reason, Callback, State);
		}

		/// <summary>
		/// Revokes voice to a participant of a room.
		/// 
		/// Note: You will need moderator, admin or owner privileges, depending
		/// on the type of change requested.
		/// </summary>
		/// <param name="RoomId">Room ID.</param>
		/// <param name="Domain">Domain of service hosting the room.</param>
		/// <param name="OccupantNickName">Nick-name of the occupant to modify.</param>
		/// <returns>Response to request.</returns>
		public Task<IqResultEventArgs> RevokeVoiceAsync(string RoomId, string Domain, string OccupantNickName)
		{
			return this.RevokeVoiceAsync(RoomId, Domain, OccupantNickName);
		}

		/// <summary>
		/// Revokes voice to a participant of a room.
		/// 
		/// Note: You will need moderator, admin or owner privileges, depending
		/// on the type of change requested.
		/// </summary>
		/// <param name="RoomId">Room ID.</param>
		/// <param name="Domain">Domain of service hosting the room.</param>
		/// <param name="OccupantNickName">Nick-name of the occupant to modify.</param>
		/// <param name="Reason">Reason for change.</param>
		/// <returns>Response to request.</returns>
		public Task<IqResultEventArgs> RevokeVoiceAsync(string RoomId, string Domain, string OccupantNickName,
			string Reason)
		{
			return this.ConfigureOccupantAsync(RoomId, Domain, OccupantNickName, Role.Visitor, Reason);
		}

		/// <summary>
		/// Gets a list of occupants having a specific affiliation.
		/// 
		/// Note: You will need moderator, admin or owner privileges, depending
		/// on the type of change requested.
		/// </summary>
		/// <param name="RoomId">Room ID.</param>
		/// <param name="Domain">Domain of service hosting the room.</param>
		/// <param name="Affiliation">Optional affiliation.</param>
		/// <param name="Callback">Method to call when response is returned from room.</param>
		/// <param name="State">State object to pass on to callback method.</param>
		public void GetOccupants(string RoomId, string Domain, Affiliation Affiliation,
			OccupantListEventHandler Callback, object State)
		{
			this.GetOccupants(RoomId, Domain, Affiliation, null, Callback, State);
		}

		/// <summary>
		/// Gets a list of occupants having a specific role.
		/// 
		/// Note: You will need moderator, admin or owner privileges, depending
		/// on the type of change requested.
		/// </summary>
		/// <param name="RoomId">Room ID.</param>
		/// <param name="Domain">Domain of service hosting the room.</param>
		/// <param name="Role">Role.</param>
		/// <param name="Callback">Method to call when response is returned from room.</param>
		/// <param name="State">State object to pass on to callback method.</param>
		public void GetOccupants(string RoomId, string Domain, Role Role,
			OccupantListEventHandler Callback, object State)
		{
			this.GetOccupants(RoomId, Domain, null, Role, Callback, State);
		}

		/// <summary>
		/// Gets a list of occupants having a specific affiliation or role.
		/// 
		/// Note: You will need moderator, admin or owner privileges, depending
		/// on the type of change requested.
		/// </summary>
		/// <param name="RoomId">Room ID.</param>
		/// <param name="Domain">Domain of service hosting the room.</param>
		/// <param name="Affiliation">Optional affiliation.</param>
		/// <param name="Role">Optional role.</param>
		/// <param name="Callback">Method to call when response is returned from room.</param>
		/// <param name="State">State object to pass on to callback method.</param>
		public void GetOccupants(string RoomId, string Domain, Affiliation? Affiliation,
			Role? Role, OccupantListEventHandler Callback, object State)
		{
			StringBuilder Xml = new StringBuilder();

			Xml.Append("<query xmlns='");
			Xml.Append(NamespaceMucAdmin);
			Xml.Append("'><item");

			if (Affiliation.HasValue)
			{
				Xml.Append(" affiliation='");
				Xml.Append(Affiliation.Value.ToString().ToLower());
				Xml.Append("'");
			}

			if (Role.HasValue)
			{
				Xml.Append(" role='");
				Xml.Append(Role.Value.ToString().ToLower());
				Xml.Append("'");
			}

			Xml.Append("/></query>");

			this.client.SendIqGet(RoomId + "@" + Domain, Xml.ToString(), (sender, e) =>
			{
				List<MucOccupant> Occupants = new List<MucOccupant>();

				if (e.Ok && !(e.FirstElement is null) && e.FirstElement.LocalName == "query" &&
					e.FirstElement.NamespaceURI == NamespaceMucAdmin)
				{
					foreach (XmlNode N in e.FirstElement.ChildNodes)
					{
						if (N is XmlElement E && E.LocalName == "item" && E.NamespaceURI == NamespaceMucAdmin)
						{
							Affiliation Affiliation2 = ToAffiliation(XML.Attribute(E, "affiliation"));
							Role Role2 = ToRole(XML.Attribute(E, "role"));
							string Jid = XML.Attribute(E, "jid");
							string NickName = XML.Attribute(E, "nick");

							Occupants.Add(new MucOccupant(this, RoomId, Domain, NickName, Jid, Affiliation2, Role2));
						}
					}
				}

				return Callback?.Invoke(this, new OccupantListEventArgs(e, Occupants.ToArray())) ?? Task.CompletedTask;
			}, State);
		}

		/// <summary>
		/// Gets a list of occupants having a specific affiliation.
		/// 
		/// Note: You will need moderator, admin or owner privileges, depending
		/// on the type of change requested.
		/// </summary>
		/// <param name="RoomId">Room ID.</param>
		/// <param name="Domain">Domain of service hosting the room.</param>
		/// <param name="Affiliation">Optional affiliation.</param>
		/// <returns>Response with occupants, if ok.</returns>
		public Task<OccupantListEventArgs> GetOccupantsAsync(string RoomId, string Domain, Affiliation Affiliation)
		{
			return this.GetOccupantsAsync(RoomId, Domain, Affiliation, null);
		}

		/// <summary>
		/// Gets a list of occupants having a specific role.
		/// 
		/// Note: You will need moderator, admin or owner privileges, depending
		/// on the type of change requested.
		/// </summary>
		/// <param name="RoomId">Room ID.</param>
		/// <param name="Domain">Domain of service hosting the room.</param>
		/// <param name="Role">Role.</param>
		/// <returns>Response with occupants, if ok.</returns>
		public Task<OccupantListEventArgs> GetOccupantsAsync(string RoomId, string Domain, Role Role)
		{
			return this.GetOccupantsAsync(RoomId, Domain, null, Role);
		}

		/// <summary>
		/// Gets a list of occupants having a specific affiliation or role.
		/// 
		/// Note: You will need moderator, admin or owner privileges, depending
		/// on the type of change requested.
		/// </summary>
		/// <param name="RoomId">Room ID.</param>
		/// <param name="Domain">Domain of service hosting the room.</param>
		/// <param name="Affiliation">Optional affiliation.</param>
		/// <param name="Role">Optional role.</param>
		/// <returns>Response with occupants, if ok.</returns>
		public Task<OccupantListEventArgs> GetOccupantsAsync(string RoomId, string Domain, Affiliation? Affiliation, Role? Role)
		{
			TaskCompletionSource<OccupantListEventArgs> Result = new TaskCompletionSource<OccupantListEventArgs>();

			this.GetOccupants(RoomId, Domain, Affiliation, Role, (sender, e) =>
			{
				Result.TrySetResult(e);
				return Task.CompletedTask;
			}, null);

			return Result.Task;
		}

		/// <summary>
		/// Request voice privileges in a room.
		/// </summary>
		/// <param name="RoomId">Room ID.</param>
		/// <param name="Domain">Domain of service hosting the room.</param>
		public void RequestVoice(string RoomId, string Domain)
		{
			this.RequestRole(RoomId, Domain, Role.Participant);
		}

		/// <summary>
		/// Requests privileges corresponding to a specific role in a room.
		/// </summary>
		/// <param name="RoomId">Room ID.</param>
		/// <param name="Domain">Domain of service hosting the room.</param>
		/// <param name="Role">Requested role.</param>
		public void RequestRole(string RoomId, string Domain, Role Role)
		{ 
			StringBuilder Xml = new StringBuilder();

			Xml.Append("<x xmlns='");
			Xml.Append(XmppClient.NamespaceData);
			Xml.Append("' type='submit'><field var='FORM_TYPE'>");
			Xml.Append("<value>");
			Xml.Append(FormTypeRequest);
			Xml.Append("</value>");
			Xml.Append("</field><field var='muc#role' type='list-single'>");
			Xml.Append("<value>");
			Xml.Append(Role.ToString().ToLower());
			Xml.Append("</value></field></x>");

			this.client.SendMessage(MessageType.Normal, RoomId + "@" + Domain, Xml.ToString(),
				string.Empty, string.Empty, string.Empty, string.Empty, string.Empty);
		}

		private Task UserRequestHandler(object Sender, MessageFormEventArgs e)
		{
			return this.OccupantRequest?.Invoke(this, e) ?? Task.CompletedTask;
		}

		/// <summary>
		/// Event raised when a request has been received from an occupant in a room.
		/// </summary>
		public event MessageFormEventHandlerAsync OccupantRequest;

		/// <summary>
		/// Gets a list of occupants with voice (participants).
		/// </summary>
		/// <param name="RoomId">Room ID.</param>
		/// <param name="Domain">Domain of service hosting the room.</param>
		/// <param name="Callback">Method to call when response is returned from room.</param>
		/// <param name="State">State object to pass on to callback method.</param>
		public void GetVoiceList(string RoomId, string Domain,
			OccupantListEventHandler Callback, object State)
		{
			this.GetOccupants(RoomId, Domain, null, Role.Participant, Callback, State);
		}

		/// <summary>
		/// Gets a list of occupants with voice (participants).
		/// </summary>
		/// <param name="RoomId">Room ID.</param>
		/// <param name="Domain">Domain of service hosting the room.</param>
		/// <returns>Response with occupants with voice, if ok.</returns>
		public Task<OccupantListEventArgs> GetVoiceListAsync(string RoomId, string Domain)
		{
			return this.GetOccupantsAsync(RoomId, Domain, null, Role.Participant);
		}

		#endregion

		#region Banning occupants

		/// <summary>
		/// Bans an occupant out of a room.
		/// 
		/// Note: You will need moderator, admin or owner privileges, depending
		/// on the type of change requested.
		/// </summary>
		/// <param name="RoomId">Room ID.</param>
		/// <param name="Domain">Domain of service hosting the room.</param>
		/// <param name="OccupantBareJid">Bare JID of the occupant to modify.</param>
		/// <param name="Callback">Method to call when response is returned from room.</param>
		/// <param name="State">State object to pass on to callback method.</param>
		public void Ban(string RoomId, string Domain, string OccupantBareJid,
			IqResultEventHandlerAsync Callback, object State)
		{
			this.Ban(RoomId, Domain, OccupantBareJid, string.Empty, Callback, State);
		}

		/// <summary>
		/// Bans an occupant out of a room.
		/// 
		/// Note: You will need moderator, admin or owner privileges, depending
		/// on the type of change requested.
		/// </summary>
		/// <param name="RoomId">Room ID.</param>
		/// <param name="Domain">Domain of service hosting the room.</param>
		/// <param name="OccupantBareJid">Bare JID of the occupant to modify.</param>
		/// <param name="Reason">Reason for change.</param>
		/// <param name="Callback">Method to call when response is returned from room.</param>
		/// <param name="State">State object to pass on to callback method.</param>
		public void Ban(string RoomId, string Domain, string OccupantBareJid,
			string Reason, IqResultEventHandlerAsync Callback, object State)
		{
			this.ConfigureOccupant(RoomId, Domain, OccupantBareJid, Affiliation.Outcast, Reason, Callback, State);
		}

		/// <summary>
		/// Bans an occupant out of a room.
		/// 
		/// Note: You will need moderator, admin or owner privileges, depending
		/// on the type of change requested.
		/// </summary>
		/// <param name="RoomId">Room ID.</param>
		/// <param name="Domain">Domain of service hosting the room.</param>
		/// <param name="OccupantBareJid">Bare JID of the occupant to modify.</param>
		/// <returns>Response to request.</returns>
		public Task<IqResultEventArgs> BanAsync(string RoomId, string Domain, string OccupantBareJid)
		{
			return this.BanAsync(RoomId, Domain, OccupantBareJid, string.Empty);
		}

		/// <summary>
		/// Bans an occupant out of a room.
		/// 
		/// Note: You will need moderator, admin or owner privileges, depending
		/// on the type of change requested.
		/// </summary>
		/// <param name="RoomId">Room ID.</param>
		/// <param name="Domain">Domain of service hosting the room.</param>
		/// <param name="OccupantBareJid">Bare JID of the occupant to modify.</param>
		/// <param name="Reason">Reason for change.</param>
		/// <returns>Response to request.</returns>
		public Task<IqResultEventArgs> BanAsync(string RoomId, string Domain, string OccupantBareJid,
			string Reason)
		{
			return this.ConfigureOccupantAsync(RoomId, Domain, OccupantBareJid, Affiliation.Outcast, Reason);
		}

		/// <summary>
		/// Gets a list of banned occupants.
		/// </summary>
		/// <param name="RoomId">Room ID.</param>
		/// <param name="Domain">Domain of service hosting the room.</param>
		/// <param name="Callback">Method to call when response is returned from room.</param>
		/// <param name="State">State object to pass on to callback method.</param>
		public void GetBannedOccupants(string RoomId, string Domain,
			OccupantListEventHandler Callback, object State)
		{
			this.GetOccupants(RoomId, Domain, Affiliation.Outcast, null, Callback, State);
		}

		/// <summary>
		/// Gets a list of banned occupants.
		/// </summary>
		/// <param name="RoomId">Room ID.</param>
		/// <param name="Domain">Domain of service hosting the room.</param>
		/// <returns>Response with banned occupants, if ok.</returns>
		public Task<OccupantListEventArgs> GetBannedOccupantsAsync(string RoomId, string Domain)
		{
			return this.GetOccupantsAsync(RoomId, Domain, Affiliation.Outcast, null);
		}

		#endregion

		#region Membership affiliation

		/// <summary>
		/// Grants membership to an occupant of a room.
		/// 
		/// Note: You will need moderator, admin or owner privileges, depending
		/// on the type of change requested.
		/// </summary>
		/// <param name="RoomId">Room ID.</param>
		/// <param name="Domain">Domain of service hosting the room.</param>
		/// <param name="OccupantBareJid">Bare JID of the occupant to modify.</param>
		/// <param name="Callback">Method to call when response is returned from room.</param>
		/// <param name="State">State object to pass on to callback method.</param>
		public void GrantMembership(string RoomId, string Domain, string OccupantBareJid,
			IqResultEventHandlerAsync Callback, object State)
		{
			this.GrantMembership(RoomId, Domain, OccupantBareJid, string.Empty, string.Empty, Callback, State);
		}

		/// <summary>
		/// Grants membership to an occupant of a room.
		/// 
		/// Note: You will need moderator, admin or owner privileges, depending
		/// on the type of change requested.
		/// </summary>
		/// <param name="RoomId">Room ID.</param>
		/// <param name="Domain">Domain of service hosting the room.</param>
		/// <param name="OccupantBareJid">Bare JID of the occupant to modify.</param>
		/// <param name="Reason">Reason for change.</param>
		/// <param name="Callback">Method to call when response is returned from room.</param>
		/// <param name="State">State object to pass on to callback method.</param>
		public void GrantMembership(string RoomId, string Domain, string OccupantBareJid,
			string Reason, IqResultEventHandlerAsync Callback, object State)
		{
			this.GrantMembership(RoomId, Domain, OccupantBareJid, string.Empty, Reason, Callback, State);
		}

		/// <summary>
		/// Grants membership to an occupant of a room.
		/// 
		/// Note: You will need moderator, admin or owner privileges, depending
		/// on the type of change requested.
		/// </summary>
		/// <param name="RoomId">Room ID.</param>
		/// <param name="Domain">Domain of service hosting the room.</param>
		/// <param name="OccupantBareJid">Bare JID of the occupant to modify.</param>
		/// <param name="DefaultNickName">Default nick-name to use for the occupant.</param>
		/// <param name="Reason">Reason for change.</param>
		/// <param name="Callback">Method to call when response is returned from room.</param>
		/// <param name="State">State object to pass on to callback method.</param>
		public void GrantMembership(string RoomId, string Domain, string OccupantBareJid,
			string DefaultNickName, string Reason, IqResultEventHandlerAsync Callback, object State)
		{
			this.ConfigureOccupant(RoomId, Domain,
				new MucOccupantConfiguration(OccupantBareJid, DefaultNickName, Affiliation.Member, Reason),
				Callback, State);
		}

		/// <summary>
		/// Grants membership to an occupant of a room.
		/// 
		/// Note: You will need moderator, admin or owner privileges, depending
		/// on the type of change requested.
		/// </summary>
		/// <param name="RoomId">Room ID.</param>
		/// <param name="Domain">Domain of service hosting the room.</param>
		/// <param name="OccupantBareJid">Bare JID of the occupant to modify.</param>
		/// <returns>Response to request.</returns>
		public Task<IqResultEventArgs> GrantMembershipAsync(string RoomId, string Domain, string OccupantBareJid)
		{
			return this.GrantMembershipAsync(RoomId, Domain, OccupantBareJid, string.Empty, string.Empty);
		}

		/// <summary>
		/// Grants membership to an occupant of a room.
		/// 
		/// Note: You will need moderator, admin or owner privileges, depending
		/// on the type of change requested.
		/// </summary>
		/// <param name="RoomId">Room ID.</param>
		/// <param name="Domain">Domain of service hosting the room.</param>
		/// <param name="OccupantBareJid">Bare JID of the occupant to modify.</param>
		/// <param name="Reason">Reason for change.</param>
		/// <returns>Response to request.</returns>
		public Task<IqResultEventArgs> GrantMembershipAsync(string RoomId, string Domain, string OccupantBareJid,
			string Reason)
		{
			return this.GrantMembershipAsync(RoomId, Domain, OccupantBareJid, string.Empty, Reason);
		}

		/// <summary>
		/// Grants membership to an occupant of a room.
		/// 
		/// Note: You will need moderator, admin or owner privileges, depending
		/// on the type of change requested.
		/// </summary>
		/// <param name="RoomId">Room ID.</param>
		/// <param name="Domain">Domain of service hosting the room.</param>
		/// <param name="OccupantBareJid">Bare JID of the occupant to modify.</param>
		/// <param name="DefaultNickName">Default nick-name to use for the occupant.</param>
		/// <param name="Reason">Reason for change.</param>
		/// <returns>Response to request.</returns>
		public Task<IqResultEventArgs> GrantMembershipAsync(string RoomId, string Domain, string OccupantBareJid,
			string DefaultNickName, string Reason)
		{
			return this.ConfigureOccupantsAsync(RoomId, Domain,
				new MucOccupantConfiguration(OccupantBareJid, DefaultNickName, Affiliation.Member, Reason));
		}

		/// <summary>
		/// Revokes the membership from an occupant in a room.
		/// 
		/// Note: You will need moderator, admin or owner privileges, depending
		/// on the type of change requested.
		/// </summary>
		/// <param name="RoomId">Room ID.</param>
		/// <param name="Domain">Domain of service hosting the room.</param>
		/// <param name="OccupantBareJid">Bare JID of the occupant to modify.</param>
		/// <param name="Callback">Method to call when response is returned from room.</param>
		/// <param name="State">State object to pass on to callback method.</param>
		public void RevokeMembership(string RoomId, string Domain, string OccupantBareJid,
			IqResultEventHandlerAsync Callback, object State)
		{
			this.RevokeMembership(RoomId, Domain, OccupantBareJid, string.Empty, Callback, State);
		}

		/// <summary>
		/// Revokes the membership from an occupant in a room.
		/// 
		/// Note: You will need moderator, admin or owner privileges, depending
		/// on the type of change requested.
		/// </summary>
		/// <param name="RoomId">Room ID.</param>
		/// <param name="Domain">Domain of service hosting the room.</param>
		/// <param name="OccupantBareJid">Bare JID of the occupant to modify.</param>
		/// <param name="Reason">Reason for change.</param>
		/// <param name="Callback">Method to call when response is returned from room.</param>
		/// <param name="State">State object to pass on to callback method.</param>
		public void RevokeMembership(string RoomId, string Domain, string OccupantBareJid,
			string Reason, IqResultEventHandlerAsync Callback, object State)
		{
			this.ConfigureOccupant(RoomId, Domain, OccupantBareJid, Affiliation.None, Reason, Callback, State);
		}

		/// <summary>
		/// Revokes the membership from an occupant in a room.
		/// 
		/// Note: You will need moderator, admin or owner privileges, depending
		/// on the type of change requested.
		/// </summary>
		/// <param name="RoomId">Room ID.</param>
		/// <param name="Domain">Domain of service hosting the room.</param>
		/// <param name="OccupantBareJid">Bare JID of the occupant to modify.</param>
		/// <returns>Response to request.</returns>
		public Task<IqResultEventArgs> RevokeMembershipAsync(string RoomId, string Domain, string OccupantBareJid)
		{
			return this.RevokeMembershipAsync(RoomId, Domain, OccupantBareJid, string.Empty);
		}

		/// <summary>
		/// Revokes the membership from an occupant in a room.
		/// 
		/// Note: You will need moderator, admin or owner privileges, depending
		/// on the type of change requested.
		/// </summary>
		/// <param name="RoomId">Room ID.</param>
		/// <param name="Domain">Domain of service hosting the room.</param>
		/// <param name="OccupantBareJid">Bare JID of the occupant to modify.</param>
		/// <param name="Reason">Reason for change.</param>
		/// <returns>Response to request.</returns>
		public Task<IqResultEventArgs> RevokeMembershipAsync(string RoomId, string Domain, string OccupantBareJid,
			string Reason)
		{
			return this.ConfigureOccupantAsync(RoomId, Domain, OccupantBareJid, Affiliation.None, Reason);
		}

		/// <summary>
		/// Gets a list of members of a room.
		/// </summary>
		/// <param name="RoomId">Room ID.</param>
		/// <param name="Domain">Domain of service hosting the room.</param>
		/// <param name="Callback">Method to call when response is returned from room.</param>
		/// <param name="State">State object to pass on to callback method.</param>
		public void GetMemberList(string RoomId, string Domain,
			OccupantListEventHandler Callback, object State)
		{
			this.GetOccupants(RoomId, Domain, Affiliation.Member, null, Callback, State);
		}

		/// <summary>
		/// Gets a list of members of a room.
		/// </summary>
		/// <param name="RoomId">Room ID.</param>
		/// <param name="Domain">Domain of service hosting the room.</param>
		/// <returns>Response with occupants with voice, if ok.</returns>
		public Task<OccupantListEventArgs> GetMemberListAsync(string RoomId, string Domain)
		{
			return this.GetOccupantsAsync(RoomId, Domain, Affiliation.Member, null);
		}

		#endregion

		#region Moderator Role

		/// <summary>
		/// Grants moderator status to a visitor of a room.
		/// 
		/// Note: You will need moderator, admin or owner privileges, depending
		/// on the type of change requested.
		/// </summary>
		/// <param name="RoomId">Room ID.</param>
		/// <param name="Domain">Domain of service hosting the room.</param>
		/// <param name="OccupantNickName">Nick-name of the occupant to modify.</param>
		/// <param name="Callback">Method to call when response is returned from room.</param>
		/// <param name="State">State object to pass on to callback method.</param>
		public void GrantModeratorStatus(string RoomId, string Domain, string OccupantNickName,
			IqResultEventHandlerAsync Callback, object State)
		{
			this.GrantModeratorStatus(RoomId, Domain, OccupantNickName, string.Empty, Callback, State);
		}

		/// <summary>
		/// Grants moderator status to a visitor of a room.
		/// 
		/// Note: You will need moderator, admin or owner privileges, depending
		/// on the type of change requested.
		/// </summary>
		/// <param name="RoomId">Room ID.</param>
		/// <param name="Domain">Domain of service hosting the room.</param>
		/// <param name="OccupantNickName">Nick-name of the occupant to modify.</param>
		/// <param name="Reason">Reason for change.</param>
		/// <param name="Callback">Method to call when response is returned from room.</param>
		/// <param name="State">State object to pass on to callback method.</param>
		public void GrantModeratorStatus(string RoomId, string Domain, string OccupantNickName,
			string Reason, IqResultEventHandlerAsync Callback, object State)
		{
			this.ConfigureOccupant(RoomId, Domain, OccupantNickName, Role.Moderator, Reason, Callback, State);
		}

		/// <summary>
		/// Grants moderator status to a visitor of a room.
		/// 
		/// Note: You will need moderator, admin or owner privileges, depending
		/// on the type of change requested.
		/// </summary>
		/// <param name="RoomId">Room ID.</param>
		/// <param name="Domain">Domain of service hosting the room.</param>
		/// <param name="OccupantNickName">Nick-name of the occupant to modify.</param>
		/// <returns>Response to request.</returns>
		public Task<IqResultEventArgs> GrantModeratorStatusAsync(string RoomId, string Domain, string OccupantNickName)
		{
			return this.GrantModeratorStatusAsync(RoomId, Domain, OccupantNickName, string.Empty);
		}

		/// <summary>
		/// Grants moderator status to a visitor of a room.
		/// 
		/// Note: You will need moderator, admin or owner privileges, depending
		/// on the type of change requested.
		/// </summary>
		/// <param name="RoomId">Room ID.</param>
		/// <param name="Domain">Domain of service hosting the room.</param>
		/// <param name="OccupantNickName">Nick-name of the occupant to modify.</param>
		/// <param name="Reason">Reason for change.</param>
		/// <returns>Response to request.</returns>
		public Task<IqResultEventArgs> GrantModeratorStatusAsync(string RoomId, string Domain, string OccupantNickName,
			string Reason)
		{
			return this.ConfigureOccupantAsync(RoomId, Domain, OccupantNickName, Role.Moderator, Reason);
		}

		/// <summary>
		/// Revokes moderator status from an occupant in the room.
		/// 
		/// Note: You will need moderator, admin or owner privileges, depending
		/// on the type of change requested.
		/// </summary>
		/// <param name="RoomId">Room ID.</param>
		/// <param name="Domain">Domain of service hosting the room.</param>
		/// <param name="OccupantNickName">Nick-name of the occupant to modify.</param>
		/// <param name="Callback">Method to call when response is returned from room.</param>
		/// <param name="State">State object to pass on to callback method.</param>
		public void RevokeModeratorStatus(string RoomId, string Domain, string OccupantNickName,
			IqResultEventHandlerAsync Callback, object State)
		{
			this.RevokeModeratorStatus(RoomId, Domain, OccupantNickName, string.Empty, Callback, State);
		}

		/// <summary>
		/// Revokes moderator status from an occupant in the room.
		/// 
		/// Note: You will need moderator, admin or owner privileges, depending
		/// on the type of change requested.
		/// </summary>
		/// <param name="RoomId">Room ID.</param>
		/// <param name="Domain">Domain of service hosting the room.</param>
		/// <param name="OccupantNickName">Nick-name of the occupant to modify.</param>
		/// <param name="Reason">Reason for change.</param>
		/// <param name="Callback">Method to call when response is returned from room.</param>
		/// <param name="State">State object to pass on to callback method.</param>
		public void RevokeModeratorStatus(string RoomId, string Domain, string OccupantNickName,
			string Reason, IqResultEventHandlerAsync Callback, object State)
		{
			this.ConfigureOccupant(RoomId, Domain, OccupantNickName, Role.Participant, Reason, Callback, State);
		}

		/// <summary>
		/// Revokes moderator status from an occupant in the room.
		/// 
		/// Note: You will need moderator, admin or owner privileges, depending
		/// on the type of change requested.
		/// </summary>
		/// <param name="RoomId">Room ID.</param>
		/// <param name="Domain">Domain of service hosting the room.</param>
		/// <param name="OccupantNickName">Nick-name of the occupant to modify.</param>
		/// <returns>Response to request.</returns>
		public Task<IqResultEventArgs> RevokeModeratorStatusAsync(string RoomId, string Domain, string OccupantNickName)
		{
			return this.RevokeModeratorStatusAsync(RoomId, Domain, OccupantNickName);
		}

		/// <summary>
		/// Revokes moderator status from an occupant in the room.
		/// 
		/// Note: You will need moderator, admin or owner privileges, depending
		/// on the type of change requested.
		/// </summary>
		/// <param name="RoomId">Room ID.</param>
		/// <param name="Domain">Domain of service hosting the room.</param>
		/// <param name="OccupantNickName">Nick-name of the occupant to modify.</param>
		/// <param name="Reason">Reason for change.</param>
		/// <returns>Response to request.</returns>
		public Task<IqResultEventArgs> RevokeModeratorStatusAsync(string RoomId, string Domain, string OccupantNickName,
			string Reason)
		{
			return this.ConfigureOccupantAsync(RoomId, Domain, OccupantNickName, Role.Participant, Reason);
		}

		/// <summary>
		/// Gets a list of moderators of a room.
		/// </summary>
		/// <param name="RoomId">Room ID.</param>
		/// <param name="Domain">Domain of service hosting the room.</param>
		/// <param name="Callback">Method to call when response is returned from room.</param>
		/// <param name="State">State object to pass on to callback method.</param>
		public void GetModeratorList(string RoomId, string Domain,
			OccupantListEventHandler Callback, object State)
		{
			this.GetOccupants(RoomId, Domain, null, Role.Moderator, Callback, State);
		}

		/// <summary>
		/// Gets a list of moderators of a room.
		/// </summary>
		/// <param name="RoomId">Room ID.</param>
		/// <param name="Domain">Domain of service hosting the room.</param>
		/// <returns>Response with occupants with voice, if ok.</returns>
		public Task<OccupantListEventArgs> GetModeratorListAsync(string RoomId, string Domain)
		{
			return this.GetOccupantsAsync(RoomId, Domain, null, Role.Moderator);
		}

		#endregion

		#region Ownership affiliation

		/// <summary>
		/// Grants ownership to a visitor of a room.
		/// 
		/// Note: You will need moderator, admin or owner privileges, depending
		/// on the type of change requested.
		/// </summary>
		/// <param name="RoomId">Room ID.</param>
		/// <param name="Domain">Domain of service hosting the room.</param>
		/// <param name="OccupantBareJid">Bare JID of the occupant to modify.</param>
		/// <param name="Callback">Method to call when response is returned from room.</param>
		/// <param name="State">State object to pass on to callback method.</param>
		public void GrantOwnership(string RoomId, string Domain, string OccupantBareJid,
			IqResultEventHandlerAsync Callback, object State)
		{
			this.GrantOwnership(RoomId, Domain, OccupantBareJid, string.Empty, Callback, State);
		}

		/// <summary>
		/// Grants ownership to a visitor of a room.
		/// 
		/// Note: You will need moderator, admin or owner privileges, depending
		/// on the type of change requested.
		/// </summary>
		/// <param name="RoomId">Room ID.</param>
		/// <param name="Domain">Domain of service hosting the room.</param>
		/// <param name="OccupantBareJid">Bare JID of the occupant to modify.</param>
		/// <param name="Reason">Reason for change.</param>
		/// <param name="Callback">Method to call when response is returned from room.</param>
		/// <param name="State">State object to pass on to callback method.</param>
		public void GrantOwnership(string RoomId, string Domain, string OccupantBareJid,
			string Reason, IqResultEventHandlerAsync Callback, object State)
		{
			this.ConfigureOccupant(RoomId, Domain,
				new MucOccupantConfiguration(OccupantBareJid, string.Empty, Affiliation.Owner, Reason),
				Callback, State);
		}

		/// <summary>
		/// Grants ownership to a visitor of a room.
		/// 
		/// Note: You will need moderator, admin or owner privileges, depending
		/// on the type of change requested.
		/// </summary>
		/// <param name="RoomId">Room ID.</param>
		/// <param name="Domain">Domain of service hosting the room.</param>
		/// <param name="OccupantBareJid">Bare JID of the occupant to modify.</param>
		/// <returns>Response to request.</returns>
		public Task<IqResultEventArgs> GrantOwnershipAsync(string RoomId, string Domain, string OccupantBareJid)
		{
			return this.GrantOwnershipAsync(RoomId, Domain, OccupantBareJid, string.Empty);
		}

		/// <summary>
		/// Grants ownership to a visitor of a room.
		/// 
		/// Note: You will need moderator, admin or owner privileges, depending
		/// on the type of change requested.
		/// </summary>
		/// <param name="RoomId">Room ID.</param>
		/// <param name="Domain">Domain of service hosting the room.</param>
		/// <param name="OccupantBareJid">Bare JID of the occupant to modify.</param>
		/// <param name="Reason">Reason for change.</param>
		/// <returns>Response to request.</returns>
		public Task<IqResultEventArgs> GrantOwnershipAsync(string RoomId, string Domain, string OccupantBareJid,
			string Reason)
		{
			return this.ConfigureOccupantsAsync(RoomId, Domain,
				new MucOccupantConfiguration(OccupantBareJid, string.Empty, Affiliation.Owner, Reason));
		}

		/// <summary>
		/// Revokes the ownership from an occupant in a room.
		/// 
		/// Note: You will need moderator, admin or owner privileges, depending
		/// on the type of change requested.
		/// </summary>
		/// <param name="RoomId">Room ID.</param>
		/// <param name="Domain">Domain of service hosting the room.</param>
		/// <param name="OccupantBareJid">Bare JID of the occupant to modify.</param>
		/// <param name="Callback">Method to call when response is returned from room.</param>
		/// <param name="State">State object to pass on to callback method.</param>
		public void RevokeOwnership(string RoomId, string Domain, string OccupantBareJid,
			IqResultEventHandlerAsync Callback, object State)
		{
			this.RevokeOwnership(RoomId, Domain, OccupantBareJid, string.Empty, Callback, State);
		}

		/// <summary>
		/// Revokes the ownership from an occupant in a room.
		/// 
		/// Note: You will need moderator, admin or owner privileges, depending
		/// on the type of change requested.
		/// </summary>
		/// <param name="RoomId">Room ID.</param>
		/// <param name="Domain">Domain of service hosting the room.</param>
		/// <param name="OccupantBareJid">Bare JID of the occupant to modify.</param>
		/// <param name="Reason">Reason for change.</param>
		/// <param name="Callback">Method to call when response is returned from room.</param>
		/// <param name="State">State object to pass on to callback method.</param>
		public void RevokeOwnership(string RoomId, string Domain, string OccupantBareJid,
			string Reason, IqResultEventHandlerAsync Callback, object State)
		{
			this.ConfigureOccupant(RoomId, Domain, OccupantBareJid, Affiliation.Admin, Reason, Callback, State);
		}

		/// <summary>
		/// Revokes the ownership from an occupant in a room.
		/// 
		/// Note: You will need moderator, admin or owner privileges, depending
		/// on the type of change requested.
		/// </summary>
		/// <param name="RoomId">Room ID.</param>
		/// <param name="Domain">Domain of service hosting the room.</param>
		/// <param name="OccupantBareJid">Bare JID of the occupant to modify.</param>
		/// <returns>Response to request.</returns>
		public Task<IqResultEventArgs> RevokeOwnershipAsync(string RoomId, string Domain, string OccupantBareJid)
		{
			return this.RevokeOwnershipAsync(RoomId, Domain, OccupantBareJid, string.Empty);
		}

		/// <summary>
		/// Revokes the ownership from an occupant in a room.
		/// 
		/// Note: You will need moderator, admin or owner privileges, depending
		/// on the type of change requested.
		/// </summary>
		/// <param name="RoomId">Room ID.</param>
		/// <param name="Domain">Domain of service hosting the room.</param>
		/// <param name="OccupantBareJid">Bare JID of the occupant to modify.</param>
		/// <param name="Reason">Reason for change.</param>
		/// <returns>Response to request.</returns>
		public Task<IqResultEventArgs> RevokeOwnershipAsync(string RoomId, string Domain, string OccupantBareJid,
			string Reason)
		{
			return this.ConfigureOccupantAsync(RoomId, Domain, OccupantBareJid, Affiliation.Admin, Reason);
		}

		/// <summary>
		/// Gets a list of owners of a room.
		/// </summary>
		/// <param name="RoomId">Room ID.</param>
		/// <param name="Domain">Domain of service hosting the room.</param>
		/// <param name="Callback">Method to call when response is returned from room.</param>
		/// <param name="State">State object to pass on to callback method.</param>
		public void GetOwnerList(string RoomId, string Domain,
			OccupantListEventHandler Callback, object State)
		{
			this.GetOccupants(RoomId, Domain, Affiliation.Owner, null, Callback, State);
		}

		/// <summary>
		/// Gets a list of owners of a room.
		/// </summary>
		/// <param name="RoomId">Room ID.</param>
		/// <param name="Domain">Domain of service hosting the room.</param>
		/// <returns>Response with occupants with voice, if ok.</returns>
		public Task<OccupantListEventArgs> GetOwnerListAsync(string RoomId, string Domain)
		{
			return this.GetOccupantsAsync(RoomId, Domain, Affiliation.Owner, null);
		}

		#endregion

		#region Administrator affiliation

		/// <summary>
		/// Grants administrator to a visitor of a room.
		/// 
		/// Note: You will need moderator, admin or admin privileges, depending
		/// on the type of change requested.
		/// </summary>
		/// <param name="RoomId">Room ID.</param>
		/// <param name="Domain">Domain of service hosting the room.</param>
		/// <param name="OccupantBareJid">Bare JID of the occupant to modify.</param>
		/// <param name="Callback">Method to call when response is returned from room.</param>
		/// <param name="State">State object to pass on to callback method.</param>
		public void GrantAdministrator(string RoomId, string Domain, string OccupantBareJid,
			IqResultEventHandlerAsync Callback, object State)
		{
			this.GrantAdministrator(RoomId, Domain, OccupantBareJid, string.Empty, Callback, State);
		}

		/// <summary>
		/// Grants administrator to a visitor of a room.
		/// 
		/// Note: You will need moderator, admin or admin privileges, depending
		/// on the type of change requested.
		/// </summary>
		/// <param name="RoomId">Room ID.</param>
		/// <param name="Domain">Domain of service hosting the room.</param>
		/// <param name="OccupantBareJid">Bare JID of the occupant to modify.</param>
		/// <param name="Reason">Reason for change.</param>
		/// <param name="Callback">Method to call when response is returned from room.</param>
		/// <param name="State">State object to pass on to callback method.</param>
		public void GrantAdministrator(string RoomId, string Domain, string OccupantBareJid,
			string Reason, IqResultEventHandlerAsync Callback, object State)
		{
			this.ConfigureOccupant(RoomId, Domain,
				new MucOccupantConfiguration(OccupantBareJid, string.Empty, Affiliation.Admin, Reason),
				Callback, State);
		}

		/// <summary>
		/// Grants administrator to a visitor of a room.
		/// 
		/// Note: You will need moderator, admin or admin privileges, depending
		/// on the type of change requested.
		/// </summary>
		/// <param name="RoomId">Room ID.</param>
		/// <param name="Domain">Domain of service hosting the room.</param>
		/// <param name="OccupantBareJid">Bare JID of the occupant to modify.</param>
		/// <returns>Response to request.</returns>
		public Task<IqResultEventArgs> GrantAdministratorAsync(string RoomId, string Domain, string OccupantBareJid)
		{
			return this.GrantAdministratorAsync(RoomId, Domain, OccupantBareJid, string.Empty);
		}

		/// <summary>
		/// Grants administrator to a visitor of a room.
		/// 
		/// Note: You will need moderator, admin or admin privileges, depending
		/// on the type of change requested.
		/// </summary>
		/// <param name="RoomId">Room ID.</param>
		/// <param name="Domain">Domain of service hosting the room.</param>
		/// <param name="OccupantBareJid">Bare JID of the occupant to modify.</param>
		/// <param name="Reason">Reason for change.</param>
		/// <returns>Response to request.</returns>
		public Task<IqResultEventArgs> GrantAdministratorAsync(string RoomId, string Domain, string OccupantBareJid,
			string Reason)
		{
			return this.ConfigureOccupantsAsync(RoomId, Domain,
				new MucOccupantConfiguration(OccupantBareJid, string.Empty, Affiliation.Admin, Reason));
		}

		/// <summary>
		/// Revokes the administrator from an occupant in a room.
		/// 
		/// Note: You will need moderator, admin or admin privileges, depending
		/// on the type of change requested.
		/// </summary>
		/// <param name="RoomId">Room ID.</param>
		/// <param name="Domain">Domain of service hosting the room.</param>
		/// <param name="OccupantBareJid">Bare JID of the occupant to modify.</param>
		/// <param name="Callback">Method to call when response is returned from room.</param>
		/// <param name="State">State object to pass on to callback method.</param>
		public void RevokeAdministrator(string RoomId, string Domain, string OccupantBareJid,
			IqResultEventHandlerAsync Callback, object State)
		{
			this.RevokeAdministrator(RoomId, Domain, OccupantBareJid, string.Empty, Callback, State);
		}

		/// <summary>
		/// Revokes the administrator from an occupant in a room.
		/// 
		/// Note: You will need moderator, admin or admin privileges, depending
		/// on the type of change requested.
		/// </summary>
		/// <param name="RoomId">Room ID.</param>
		/// <param name="Domain">Domain of service hosting the room.</param>
		/// <param name="OccupantBareJid">Bare JID of the occupant to modify.</param>
		/// <param name="Reason">Reason for change.</param>
		/// <param name="Callback">Method to call when response is returned from room.</param>
		/// <param name="State">State object to pass on to callback method.</param>
		public void RevokeAdministrator(string RoomId, string Domain, string OccupantBareJid,
			string Reason, IqResultEventHandlerAsync Callback, object State)
		{
			this.ConfigureOccupant(RoomId, Domain, OccupantBareJid, Affiliation.Member, Reason, Callback, State);
		}

		/// <summary>
		/// Revokes the administrator from an occupant in a room.
		/// 
		/// Note: You will need moderator, admin or admin privileges, depending
		/// on the type of change requested.
		/// </summary>
		/// <param name="RoomId">Room ID.</param>
		/// <param name="Domain">Domain of service hosting the room.</param>
		/// <param name="OccupantBareJid">Bare JID of the occupant to modify.</param>
		/// <returns>Response to request.</returns>
		public Task<IqResultEventArgs> RevokeAdministratorAsync(string RoomId, string Domain, string OccupantBareJid)
		{
			return this.RevokeAdministratorAsync(RoomId, Domain, OccupantBareJid, string.Empty);
		}

		/// <summary>
		/// Revokes the administrator from an occupant in a room.
		/// 
		/// Note: You will need moderator, admin or admin privileges, depending
		/// on the type of change requested.
		/// </summary>
		/// <param name="RoomId">Room ID.</param>
		/// <param name="Domain">Domain of service hosting the room.</param>
		/// <param name="OccupantBareJid">Bare JID of the occupant to modify.</param>
		/// <param name="Reason">Reason for change.</param>
		/// <returns>Response to request.</returns>
		public Task<IqResultEventArgs> RevokeAdministratorAsync(string RoomId, string Domain, string OccupantBareJid,
			string Reason)
		{
			return this.ConfigureOccupantAsync(RoomId, Domain, OccupantBareJid, Affiliation.Member, Reason);
		}

		/// <summary>
		/// Gets a list of admins of a room.
		/// </summary>
		/// <param name="RoomId">Room ID.</param>
		/// <param name="Domain">Domain of service hosting the room.</param>
		/// <param name="Callback">Method to call when response is returned from room.</param>
		/// <param name="State">State object to pass on to callback method.</param>
		public void GetAdminList(string RoomId, string Domain,
			OccupantListEventHandler Callback, object State)
		{
			this.GetOccupants(RoomId, Domain, Affiliation.Admin, null, Callback, State);
		}

		/// <summary>
		/// Gets a list of admins of a room.
		/// </summary>
		/// <param name="RoomId">Room ID.</param>
		/// <param name="Domain">Domain of service hosting the room.</param>
		/// <returns>Response with occupants with voice, if ok.</returns>
		public Task<OccupantListEventArgs> GetAdminListAsync(string RoomId, string Domain)
		{
			return this.GetOccupantsAsync(RoomId, Domain, Affiliation.Admin, null);
		}

		#endregion

		#region Destroy room

		/// <summary>
		/// Destroyes a room.
		/// </summary>
		/// <param name="RoomId">Room ID.</param>
		/// <param name="Domain">Domain of service hosting the room.</param>
		/// <param name="Callback">Method to call when response is returned from room.</param>
		/// <param name="State">State object to pass on to callback method.</param>
		public void DestroyRoom(string RoomId, string Domain, IqResultEventHandlerAsync Callback, object State)
		{
			this.DestroyRoom(RoomId, Domain, string.Empty, string.Empty, Callback, State);
		}

		/// <summary>
		/// Destroyes a room.
		/// </summary>
		/// <param name="RoomId">Room ID.</param>
		/// <param name="Domain">Domain of service hosting the room.</param>
		/// <param name="Reason">Optional reason for destroying the room.</param>
		/// <param name="Callback">Method to call when response is returned from room.</param>
		/// <param name="State">State object to pass on to callback method.</param>
		public void DestroyRoom(string RoomId, string Domain, string Reason,
			IqResultEventHandlerAsync Callback, object State)
		{
			this.DestroyRoom(RoomId, Domain, Reason, string.Empty, Callback, State);
		}

		/// <summary>
		/// Destroyes a room.
		/// </summary>
		/// <param name="RoomId">Room ID.</param>
		/// <param name="Domain">Domain of service hosting the room.</param>
		/// <param name="Reason">Optional reason for destroying the room.</param>
		/// <param name="AlternateRoomJid">Optional alternative Room JID</param>
		/// <param name="Callback">Method to call when response is returned from room.</param>
		/// <param name="State">State object to pass on to callback method.</param>
		public void DestroyRoom(string RoomId, string Domain, string Reason,
			string AlternateRoomJid, IqResultEventHandlerAsync Callback, object State)
		{
			StringBuilder Xml = new StringBuilder();

			Xml.Append("<query xmlns='");
			Xml.Append(NamespaceMucOwner);
			Xml.Append("'><destroy");

			if (!string.IsNullOrEmpty(AlternateRoomJid))
			{
				Xml.Append(" jid='");
				Xml.Append(XML.Encode(AlternateRoomJid));
				Xml.Append('\'');
			}

			if (string.IsNullOrEmpty(Reason))
				Xml.Append("/>");
			else
			{
				Xml.Append("><reason>");
				Xml.Append(XML.Encode(Reason));
				Xml.Append("</reason></destroy>");
			}

			Xml.Append("</query>");

			this.client.SendIqSet(RoomId + "@" + Domain, Xml.ToString(), Callback, State);
		}

		/// <summary>
		/// Destroyes a room.
		/// </summary>
		/// <param name="RoomId">Room ID.</param>
		/// <param name="Domain">Domain of service hosting the room.</param>
		public Task<IqResultEventArgs> DestroyRoomAsync(string RoomId, string Domain)
		{
			return this.DestroyRoomAsync(RoomId, Domain, string.Empty, string.Empty);
		}

		/// <summary>
		/// Destroyes a room.
		/// </summary>
		/// <param name="RoomId">Room ID.</param>
		/// <param name="Domain">Domain of service hosting the room.</param>
		/// <param name="Reason">Optional reason for destroying the room.</param>
		public Task<IqResultEventArgs> DestroyRoomAsync(string RoomId, string Domain, string Reason)
		{
			return this.DestroyRoomAsync(RoomId, Domain, Reason, string.Empty);
		}

		/// <summary>
		/// Destroyes a room.
		/// </summary>
		/// <param name="RoomId">Room ID.</param>
		/// <param name="Domain">Domain of service hosting the room.</param>
		/// <param name="Reason">Optional reason for destroying the room.</param>
		/// <param name="AlternateRoomJid">Optional alternative Room JID</param>
		public Task<IqResultEventArgs> DestroyRoomAsync(string RoomId, string Domain, string Reason,
			string AlternateRoomJid)
		{
			TaskCompletionSource<IqResultEventArgs> Result = new TaskCompletionSource<IqResultEventArgs>();

			this.DestroyRoom(RoomId, Domain, Reason, AlternateRoomJid, (sender, e) =>
			{
				Result.TrySetResult(e);
				return Task.CompletedTask;
			}, null);

			return Result.Task;
		}

		#endregion

		#region Self-ping

		/// <summary>
		/// Performs a self-ping to check if the connection to a Multi-User Chat Room is still valid. If not, a
		/// <see cref="IqResultEventArgs.Ok"/> will be false, and <see cref="IqResultEventArgs.StanzaError"/> will
		/// contains a <see cref="StanzaErrors.NotAcceptableException"/> stanza-error, and the room should be
		/// re-entered.
		/// </summary>
		/// <param name="RoomId">Room ID.</param>
		/// <param name="Domain">Domain of service hosting the room.</param>
		/// <param name="NickName">Nick-name used in room.</param>
		/// <param name="Callback">Method to call when response is returned from room.</param>
		/// <param name="State">State object to pass on to callback method.</param>
		public void SelfPing(string RoomId, string Domain, string NickName, IqResultEventHandlerAsync Callback, object State)
		{
			this.client.SendIqGet(RoomId + "@" + Domain + "/" + NickName, "<ping xmlns='" + XmppClient.NamespacePing + "'/>", Callback, State);
		}

		/// <summary>
		/// Performs a self-ping to check if the connection to a Multi-User Chat Room is still valid. If not, a
		/// <see cref="IqResultEventArgs.Ok"/> will be false, and <see cref="IqResultEventArgs.StanzaError"/> will
		/// contains a <see cref="StanzaErrors.NotAcceptableException"/> stanza-error, and the room should be
		/// re-entered.
		/// </summary>
		/// <param name="RoomId">Room ID.</param>
		/// <param name="Domain">Domain of service hosting the room.</param>
		/// <param name="NickName">Nick-name used in room.</param>
		/// <returns>Response to ping request.</returns>
		public Task<IqResultEventArgs> SelfPingAsync(string RoomId, string Domain, string NickName)
		{
			TaskCompletionSource<IqResultEventArgs> Result = new TaskCompletionSource<IqResultEventArgs>();

			this.client.SendIqGet(RoomId + "@" + Domain + "/" + NickName, "<ping xmlns='" + XmppClient.NamespacePing + "'/>", (sender, e) =>
			{
				Result.TrySetResult(e);
				return Task.CompletedTask;
			}, null);

			return Result.Task;
		}

		#endregion

	}
}
