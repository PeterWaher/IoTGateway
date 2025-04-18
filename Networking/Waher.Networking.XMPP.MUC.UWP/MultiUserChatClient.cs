﻿using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using Waher.Content.Xml;
using Waher.Events;
using Waher.Networking.XMPP.DataForms;
using Waher.Networking.XMPP.Events;
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

			this.client.OnGroupChatMessage += this.Client_OnGroupChatMessage;
		}

		/// <inheritdoc/>
		public override void Dispose()
		{
			this.client.UnregisterPresenceHandler("x", NamespaceMucUser, this.UserPresenceHandler, true);
			this.client.UnregisterMessageHandler("x", NamespaceMucUser, this.UserMessageHandler, false);
			this.client.UnregisterMessageHandler("x", NamespaceJabberConference, this.DirectInvitationMessageHandler, true);
			this.client.UnregisterMessageFormHandler(FormTypeRequest, this.UserRequestHandler);
			this.client.UnregisterMessageFormHandler(FormTypeRegister, this.RegistrationRequestHandler);

			this.client.OnGroupChatMessage -= this.Client_OnGroupChatMessage;

			base.Dispose();
		}

		/// <summary>
		/// Publish/Subscribe component address.
		/// </summary>
		public string ComponentAddress => this.componentAddress;

		/// <summary>
		/// Implemented extensions.
		/// </summary>
		public override string[] Extensions => new string[] { "XEP-0045" };

		private Task UserPresenceHandler(object Sender, PresenceEventArgs e)
		{
			if (TryParseUserPresence(e, out UserPresenceEventArgs e2))
			{
				if (e2.RoomDestroyed)
					return this.RoomDestroyed.Raise(this, e2);
				else if (string.IsNullOrEmpty(e2.NickName))
					return this.RoomPresence.Raise(this, e2);
				else
					return this.OccupantPresence.Raise(this, e2);
			}

			return Task.CompletedTask;
		}

		/// <summary>
		/// Event raised when user presence is received from a room occupant.
		/// </summary>
		public event EventHandlerAsync<UserPresenceEventArgs> OccupantPresence;

		/// <summary>
		/// Event raised when user presence is received from a room.
		/// </summary>
		public event EventHandlerAsync<UserPresenceEventArgs> RoomPresence;

		/// <summary>
		/// Event raised when a room where the client is an occupant is destroyed.
		/// </summary>
		public event EventHandlerAsync<UserPresenceEventArgs> RoomDestroyed;

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
				Status?.ToArray() ?? Array.Empty<MucStatus>());

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
		public Task EnterRoom(string RoomId, string Domain, string NickName,
			EventHandlerAsync<UserPresenceEventArgs> Callback, object State)
		{
			return this.EnterRoom(RoomId, Domain, NickName, string.Empty, Callback, State);
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
		public Task EnterRoom(string RoomId, string Domain, string NickName, string Password,
			EventHandlerAsync<UserPresenceEventArgs> Callback, object State)
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

			return this.client.SendDirectedPresence(string.Empty, RoomId + "@" + Domain + "/" + NickName,
				Xml.ToString(), (Sender, e) =>
				{
					if (!TryParseUserPresence(e, out UserPresenceEventArgs e2))
					{
						e2 = new UserPresenceEventArgs(e, RoomId, Domain, NickName,
							Affiliation.None, Role.None, string.Empty, string.Empty, false)
						{
							Ok = false
						};
					}

					return Callback.Raise(this, e2);
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
		public async Task<UserPresenceEventArgs> EnterRoomAsync(string RoomId, string Domain, string NickName, string Password)
		{
			TaskCompletionSource<UserPresenceEventArgs> Result = new TaskCompletionSource<UserPresenceEventArgs>();

			await this.EnterRoom(RoomId, Domain, NickName, Password, (Sender, e) =>
			{
				Result.TrySetResult(e);
				return Task.CompletedTask;
			}, null);

			return await Result.Task;
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
		public Task LeaveRoom(string RoomId, string Domain, string NickName,
			EventHandlerAsync<UserPresenceEventArgs> Callback, object State)
		{
			return this.client.SendDirectedPresence("unavailable", RoomId + "@" + Domain + "/" + NickName,
				string.Empty, (Sender, e) =>
				{
					if (!TryParseUserPresence(e, out UserPresenceEventArgs e2))
					{
						e2 = new UserPresenceEventArgs(e, RoomId, Domain, NickName,
							Affiliation.None, Role.None, string.Empty, string.Empty, false)
						{
							Ok = false
						};
					}

					return Callback.Raise(this, e2);
				}, State);
		}

		/// <summary>
		/// Leave a chat room.
		/// </summary>
		/// <param name="RoomId">Room ID.</param>
		/// <param name="Domain">Domain of service hosting the room.</param>
		/// <param name="NickName">Nickname to use in the chat room.</param>
		/// <returns>Room entry response.</returns>
		public async Task<UserPresenceEventArgs> LeaveRoomAsync(string RoomId, string Domain,
			string NickName)
		{
			TaskCompletionSource<UserPresenceEventArgs> Result = new TaskCompletionSource<UserPresenceEventArgs>();

			await this.LeaveRoom(RoomId, Domain, NickName, (Sender, e) =>
			{
				Result.TrySetResult(e);
				return Task.CompletedTask;
			}, null);

			return await Result.Task;
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
		public Task CreateInstantRoom(string RoomId, string Domain,
			EventHandlerAsync<IqResultEventArgs> Callback, object State)
		{
			StringBuilder Xml = new StringBuilder();

			Xml.Append("<query xmlns='");
			Xml.Append(NamespaceMucOwner);
			Xml.Append("'><x xmlns='");
			Xml.Append(XmppClient.NamespaceData);
			Xml.Append("' type='submit'/></query>");

			return this.client.SendIqSet(RoomId + "@" + Domain, Xml.ToString(), Callback, State);
		}

		/// <summary>
		/// Creates an instant room (with default settings).
		/// </summary>
		/// <param name="RoomId">Room ID.</param>
		/// <param name="Domain">Domain of service hosting the room.</param>
		/// <returns>Response to request.</returns>
		public async Task<IqResultEventArgs> CreateInstantRoomAsync(string RoomId, string Domain)
		{
			TaskCompletionSource<IqResultEventArgs> Result = new TaskCompletionSource<IqResultEventArgs>();

			await this.CreateInstantRoom(RoomId, Domain, (Sender, e) =>
			{
				Result.TrySetResult(e);
				return Task.CompletedTask;
			}, null);

			return await Result.Task;
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
		public Task GetRoomConfiguration(string RoomId, string Domain, EventHandlerAsync<DataFormEventArgs> Callback, object State)
		{
			return this.GetRoomConfiguration(RoomId, Domain, Callback, null, State);
		}

		/// <summary>
		/// Gets the configuration form for a room.
		/// </summary>
		/// <param name="RoomId">Room ID.</param>
		/// <param name="Domain">Domain of service hosting the room.</param>
		/// <param name="Callback">Method to call when response is returned.</param>
		/// <param name="SubmissionCallback">Method to call when configuration has been submitted.</param>
		/// <param name="State">State object to pass on to callback method.</param>
		public Task GetRoomConfiguration(string RoomId, string Domain, EventHandlerAsync<DataFormEventArgs> Callback,
			EventHandlerAsync<IqResultEventArgs> SubmissionCallback, object State)
		{
			StringBuilder Xml = new StringBuilder();

			Xml.Append("<query xmlns='");
			Xml.Append(NamespaceMucOwner);
			Xml.Append("'/>");

			return this.client.SendIqGet(RoomId + "@" + Domain, Xml.ToString(), (Sender, e) =>
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

				return Callback.Raise(this, new DataFormEventArgs(Form, e));
			}, State);
		}

		private class ConfigState
		{
			public EventHandlerAsync<IqResultEventArgs> SubmissionCallback;
			public object State;
		}

		/// <summary>
		/// Submits a room configuration form.
		/// </summary>
		/// <param name="ConfigurationForm">Room configuration form.</param>
		public async Task SubmitConfigurationForm(DataForm ConfigurationForm)
		{
			TaskCompletionSource<bool> Result = new TaskCompletionSource<bool>();

			ConfigurationForm.State = new ConfigState()
			{
				SubmissionCallback = (Sender, e) =>
				{
					if (e.Ok)
						Result.TrySetResult(true);
					else
						Result.TrySetException(e.StanzaError is null ? new Exception("Unable to configure room.") : e.StanzaError);

					return Task.CompletedTask;
				}
			};

			await this.SubmitConfigurationForm(this, ConfigurationForm);

			await Result.Task;
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

			return this.client.SendIqSet(ConfigurationForm.From, Xml2.ToString(), (sender3, e3) =>
			{
				e3.State = State?.State;
				return State?.SubmissionCallback.Raise(this, e3);
			}, null);
		}

		/// <summary>
		/// Cancels a room configuration form.
		/// </summary>
		/// <param name="ConfigurationForm">Room configuration form.</param>
		public async Task CancelConfigurationForm(DataForm ConfigurationForm)
		{
			TaskCompletionSource<bool> Result = new TaskCompletionSource<bool>();

			ConfigurationForm.State = new ConfigState()
			{
				SubmissionCallback = (Sender, e) =>
				{
					Result.TrySetResult(true);
					return Task.CompletedTask;
				}
			};

			await this.CancelConfigurationForm(this, ConfigurationForm);

			await Result.Task;
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

			return this.client.SendIqSet(ConfigurationForm.From, Xml2.ToString(), (sender3, e3) =>
			{
				e3.Ok = false;
				e3.State = State?.State;
				return State?.SubmissionCallback.Raise(this, e3);
			}, null);
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
		public async Task<DataForm> GetRoomConfigurationAsync(string RoomId, string Domain,
			EventHandlerAsync<IqResultEventArgs> SubmissionCallback, object State)
		{
			TaskCompletionSource<DataForm> Result = new TaskCompletionSource<DataForm>();

			await this.GetRoomConfiguration(RoomId, Domain, (Sender, e) =>
			{
				if (!e.Ok)
					throw e.StanzaError ?? new XmppException("Unable to get room configuration.");

				Result.TrySetResult(e.Form);
				return Task.CompletedTask;
			}, SubmissionCallback, State);

			return await Result.Task;
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
		public Task SendGroupChatMessage(string RoomId, string Domain, string Message)
		{
			return this.SendGroupChatMessage(RoomId, Domain, Message, string.Empty, string.Empty, string.Empty);
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
		public Task SendGroupChatMessage(string RoomId, string Domain, string Message, string Language)
		{
			return this.SendGroupChatMessage(RoomId, Domain, Message, Language, string.Empty, string.Empty);
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
		public Task SendGroupChatMessage(string RoomId, string Domain, string Message, string Language, string ThreadId)
		{
			return this.SendGroupChatMessage(RoomId, Domain, Message, Language, ThreadId, string.Empty);
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
		public Task SendGroupChatMessage(string RoomId, string Domain, string Message, string Language, string ThreadId, string ParentThreadId)
		{
			return this.client.SendMessage(MessageType.GroupChat, RoomId + "@" + Domain,
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
		public Task SendCustomGroupChatMessage(string RoomId, string Domain, string Xml)
		{
			return this.SendCustomGroupChatMessage(RoomId, Domain, Xml, string.Empty,
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
		public Task SendCustomGroupChatMessage(string RoomId, string Domain, string Xml, string Language)
		{
			return this.SendCustomGroupChatMessage(RoomId, Domain, Xml, Language, string.Empty, string.Empty);
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
		public Task SendCustomGroupChatMessage(string RoomId, string Domain, string Xml, string Language, string ThreadId)
		{
			return this.SendCustomGroupChatMessage(RoomId, Domain, Xml, Language, ThreadId, string.Empty);
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
		public Task SendCustomGroupChatMessage(string RoomId, string Domain,
			string Xml, string Language, string ThreadId, string ParentThreadId)
		{
			return this.client.SendMessage(MessageType.GroupChat, RoomId + "@" + Domain,
				 Xml, string.Empty, string.Empty, Language, ThreadId, ParentThreadId);
		}

		private async Task Client_OnGroupChatMessage(object Sender, MessageEventArgs e)
		{
			if (!TryParseOccupantJid(e.From, false, out string RoomId, out string Domain, out string NickName))
				return;

			if (string.IsNullOrEmpty(NickName))
			{
				RoomMessageEventArgs e2 = new RoomMessageEventArgs(e, RoomId, Domain);

				await this.RoomMessage.Raise(this, e2);
			}
			else
			{
				RoomOccupantMessageEventArgs e2 = new RoomOccupantMessageEventArgs(e, RoomId, Domain, NickName);

				if (string.IsNullOrEmpty(e.Body) && !string.IsNullOrEmpty(e.Subject))
					await this.RoomSubject.Raise(this, e2);
				else
					await this.RoomOccupantMessage.Raise(this, e2);
			}
		}

		/// <summary>
		/// Event raised when a group chat message from a MUC room was received.
		/// </summary>
		public event EventHandlerAsync<RoomMessageEventArgs> RoomMessage;

		/// <summary>
		/// Event raised when a group chat message from a MUC room occupant was received.
		/// </summary>
		public event EventHandlerAsync<RoomOccupantMessageEventArgs> RoomOccupantMessage;

		#endregion

		#region Room Subjects

		/// <summary>
		/// Event raised when a room subject message was received.
		/// </summary>
		public event EventHandlerAsync<RoomOccupantMessageEventArgs> RoomSubject;

		/// <summary>
		/// Sends a simple group chat message to a chat room.
		/// 
		/// Note: The client must be an occupant of the chat room before messages
		/// can be properly propagated to other occupants of the room.
		/// </summary>
		/// <param name="RoomId">Room ID</param>
		/// <param name="Domain">Domain hosting the room.</param>
		/// <param name="Subject">Room subject.</param>
		public Task ChangeRoomSubject(string RoomId, string Domain, string Subject)
		{
			return this.ChangeRoomSubject(RoomId, Domain, Subject, string.Empty);
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
		public Task ChangeRoomSubject(string RoomId, string Domain, string Subject, string Language)
		{
			return this.client.SendMessage(MessageType.GroupChat, RoomId + "@" + Domain,
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
		public Task SendPrivateMessage(string RoomId, string Domain, string NickName, string Message)
		{
			return this.SendPrivateMessage(RoomId, Domain, NickName, Message, string.Empty, string.Empty, string.Empty);
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
		public Task SendPrivateMessage(string RoomId, string Domain, string NickName, string Message, string Language)
		{
			return this.SendPrivateMessage(RoomId, Domain, NickName, Message, Language, string.Empty, string.Empty);
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
		public Task SendPrivateMessage(string RoomId, string Domain, string NickName, string Message, string Language, string ThreadId)
		{
			return this.SendPrivateMessage(RoomId, Domain, NickName, Message, Language, ThreadId, string.Empty);
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
		public Task SendPrivateMessage(string RoomId, string Domain, string NickName, string Message, string Language, string ThreadId, string ParentThreadId)
		{
			return this.client.SendMessage(MessageType.Chat, RoomId + "@" + Domain + "/" + NickName,
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
		public Task SendCustomPrivateMessage(string RoomId, string Domain, string NickName, string Xml)
		{
			return this.SendCustomPrivateMessage(RoomId, Domain, NickName, Xml, string.Empty, string.Empty, string.Empty);
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
		public Task SendCustomPrivateMessage(string RoomId, string Domain, string NickName, string Xml, string Language)
		{
			return this.SendCustomPrivateMessage(RoomId, Domain, NickName, Xml, Language, string.Empty, string.Empty);
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
		public Task SendCustomPrivateMessage(string RoomId, string Domain, string NickName, string Xml, string Language, string ThreadId)
		{
			return this.SendCustomPrivateMessage(RoomId, Domain, NickName, Xml, Language, ThreadId, string.Empty);
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
		public Task SendCustomPrivateMessage(string RoomId, string Domain, string NickName,
			string Xml, string Language, string ThreadId, string ParentThreadId)
		{
			return this.SendCustomPrivateMessage(string.Empty, RoomId, Domain, NickName, Xml, Language, ThreadId, ParentThreadId);
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
		public Task SendCustomPrivateMessage(string MessageId, string RoomId, string Domain, string NickName,
			string Xml, string Language, string ThreadId, string ParentThreadId)
		{
			return this.client.SendMessage(QoSLevel.Unacknowledged, MessageType.Chat, MessageId, RoomId + "@" + Domain + "/" + NickName,
				 Xml + "<x xmlns='" + NamespaceMucUser + "'/>", string.Empty, string.Empty, Language, ThreadId, ParentThreadId, null, null);
		}

		#endregion

		#region Room Invitations

		private async Task UserMessageHandler(object Sender, MessageEventArgs e)
		{
			if (!TryParseOccupantJid(e.From, false, out string RoomId, out string Domain, out string NickName))
				return;

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
					await this.RoomInvitationReceived.Raise(this, new RoomInvitationMessageEventArgs(this, e, RoomId, Domain,
						InvitationFrom, Reason, Password));
				}
				else if (!string.IsNullOrEmpty(DeclinedFrom))
				{
					await this.RoomDeclinedInvitationReceived.Raise(this, new RoomDeclinedMessageEventArgs(e, RoomId, Domain,
						DeclinedFrom, Reason));
				}
			}
			else
				await this.PrivateMessageReceived.Raise(this, new RoomOccupantMessageEventArgs(e, RoomId, Domain, NickName));
		}

		/// <summary>
		/// Event raised when a group chat message from a MUC room occupant was received.
		/// </summary>
		public event EventHandlerAsync<RoomOccupantMessageEventArgs> PrivateMessageReceived;

		/// <summary>
		/// Event raised when an invitation from a MUC room has been received.
		/// </summary>
		public event EventHandlerAsync<RoomInvitationMessageEventArgs> RoomInvitationReceived;

		/// <summary>
		/// Event raised when an invitation from a MUC room has been declined.
		/// </summary>
		public event EventHandlerAsync<RoomDeclinedMessageEventArgs> RoomDeclinedInvitationReceived;

		/// <summary>
		/// Sends an invitation to the room.
		/// </summary>
		/// <param name="RoomId">Room ID</param>
		/// <param name="Domain">Domain hosting the room.</param>
		/// <param name="BareJid">Bare JID of entity to invite.</param>
		public Task Invite(string RoomId, string Domain, string BareJid)
		{
			return this.Invite(RoomId, Domain, BareJid, string.Empty, string.Empty);
		}

		/// <summary>
		/// Sends an invitation to the room.
		/// </summary>
		/// <param name="RoomId">Room ID</param>
		/// <param name="Domain">Domain hosting the room.</param>
		/// <param name="BareJid">Bare JID of entity to invite.</param>
		/// <param name="Reason">Reason for sending the invitation.</param>
		public Task Invite(string RoomId, string Domain, string BareJid, string Reason)
		{
			return this.Invite(RoomId, Domain, BareJid, Reason, string.Empty);
		}

		/// <summary>
		/// Sends an invitation to the room.
		/// </summary>
		/// <param name="RoomId">Room ID</param>
		/// <param name="Domain">Domain hosting the room.</param>
		/// <param name="BareJid">Bare JID of entity to invite.</param>
		/// <param name="Reason">Reason for sending the invitation.</param>
		/// <param name="Language">Language</param>
		public Task Invite(string RoomId, string Domain, string BareJid, string Reason, string Language)
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

			return this.client.SendMessage(MessageType.Normal, RoomId + "@" + Domain,
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
		public Task DeclineInvitation(string RoomId, string Domain, string InviteFrom, string Reason, string Language)
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

			return this.client.SendMessage(MessageType.Normal, RoomId + "@" + Domain,
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
		public Task InviteDirect(string RoomId, string Domain, string BareJid)
		{
			return this.InviteDirect(RoomId, Domain, BareJid, string.Empty, string.Empty, string.Empty, string.Empty);
		}

		/// <summary>
		/// Sends a direct invitation to the room.
		/// </summary>
		/// <param name="RoomId">Room ID</param>
		/// <param name="Domain">Domain hosting the room.</param>
		/// <param name="BareJid">Bare JID of entity to invite.</param>
		/// <param name="Reason">Reason for sending the invitation.</param>
		public Task InviteDirect(string RoomId, string Domain, string BareJid, string Reason)
		{
			return this.InviteDirect(RoomId, Domain, BareJid, Reason, string.Empty, string.Empty, string.Empty);
		}

		/// <summary>
		/// Sends a direct invitation to the room.
		/// </summary>
		/// <param name="RoomId">Room ID</param>
		/// <param name="Domain">Domain hosting the room.</param>
		/// <param name="BareJid">Bare JID of entity to invite.</param>
		/// <param name="Reason">Reason for sending the invitation.</param>
		/// <param name="Language">Language</param>
		public Task InviteDirect(string RoomId, string Domain, string BareJid, string Reason, string Language)
		{
			return this.InviteDirect(RoomId, Domain, BareJid, Reason, Language, string.Empty, string.Empty);
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
		public Task InviteDirect(string RoomId, string Domain, string BareJid, string Reason, string Language, string Password)
		{
			return this.InviteDirect(RoomId, Domain, BareJid, Reason, Language, Password, string.Empty);
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
		public Task InviteDirect(string RoomId, string Domain, string BareJid, string Reason, string Language, string Password, string ThreadId)
		{
			return this.InviteDirect(null, RoomId, Domain, BareJid, Reason, Language, Password, ThreadId);
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
		public Task InviteDirect(IEndToEndEncryption Endpoint, string RoomId, string Domain, string BareJid, string Reason, string Language,
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
				return this.client.SendMessage(MessageType.Normal, BareJid, Xml.ToString(), string.Empty, string.Empty, Language,
					string.Empty, string.Empty);
			}
			else
			{
				return Endpoint.SendMessage(this.Client, E2ETransmission.NormalIfNotE2E, QoSLevel.Unacknowledged, MessageType.Normal,
					string.Empty, BareJid, Xml.ToString(), string.Empty, string.Empty, Language, string.Empty, string.Empty, null, null);
			}
		}

		private async Task DirectInvitationMessageHandler(object Sender, MessageEventArgs e)
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

			await this.DirectInvitationReceived.Raise(this, new DirectInvitationMessageEventArgs(this, e, RoomId,
				Domain, Reason, Password, Continue, ThreadId));
		}

		/// <summary>
		/// Event raised when a direct invitation from a peer has been received.
		/// </summary>
		public event EventHandlerAsync<DirectInvitationMessageEventArgs> DirectInvitationReceived;

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
		public Task ChangeNickName(string RoomId, string Domain, string NickName,
			EventHandlerAsync<UserPresenceEventArgs> Callback, object State)
		{
			return this.client.SendDirectedPresence(string.Empty, RoomId + "@" + Domain + "/" + NickName,
				string.Empty, (Sender, e) =>
				{
					if (!TryParseUserPresence(e, out UserPresenceEventArgs e2))
					{
						e2 = new UserPresenceEventArgs(e, RoomId, Domain, NickName,
							Affiliation.None, Role.None, string.Empty, string.Empty, false)
						{
							Ok = false
						};
					}

					return Callback.Raise(this, e2);
				}, State);
		}

		/// <summary>
		/// Changes to a new nick-name.
		/// </summary>
		/// <param name="RoomId">Room ID.</param>
		/// <param name="Domain">Domain of service hosting the room.</param>
		/// <param name="NickName">Nickname to use in the chat room.</param>
		/// <returns>Room entry response.</returns>
		public async Task<UserPresenceEventArgs> ChangeNickNameAsync(string RoomId, string Domain,
			string NickName)
		{
			TaskCompletionSource<UserPresenceEventArgs> Result = new TaskCompletionSource<UserPresenceEventArgs>();

			await this.ChangeNickName(RoomId, Domain, NickName, (Sender, e) =>
			{
				Result.TrySetResult(e);
				return Task.CompletedTask;
			}, null);

			return await Result.Task;
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
		public Task SetPresence(string RoomId, string Domain, string NickName, Availability Availability,
			EventHandlerAsync<PresenceEventArgs> Callback, object State)
		{
			return this.SetPresence(RoomId, Domain, NickName, Availability, null, Callback, State);
		}

		/// <summary>
		/// Sets the presence of the occupant in a room.
		/// </summary>
		/// <param name="RoomId">Room ID.</param>
		/// <param name="Domain">Domain of service hosting the room.</param>
		/// <param name="NickName">Nickname of the occupant.</param>
		/// <param name="Availability">Occupant availability.</param>
		/// <param name="Status">Custom status</param>
		/// <param name="Callback">Method to call when stanza has been sent.</param>
		/// <param name="State">State object to pass on to callback method.</param>
		public async Task SetPresence(string RoomId, string Domain, string NickName, Availability Availability,
			KeyValuePair<string, string>[] Status, EventHandlerAsync<PresenceEventArgs> Callback, object State)
		{
			string Type = string.Empty;
			StringBuilder Xml = new StringBuilder();

			switch (Availability)
			{
				case Availability.Online:
				default:
					break;

				case Availability.Away:
					Xml.Append("<show>away</show>");
					break;

				case Availability.Chat:
					Xml.Append("<show>chat</show>");
					break;

				case Availability.DoNotDisturb:
					Xml.Append("<show>dnd</show>");
					break;

				case Availability.ExtendedAway:
					Xml.Append("<show>xa</show>");
					break;

				case Availability.Offline:
					Type = "unavailable";
					break;
			}

			if (!(Status is null))
			{
				foreach (KeyValuePair<string, string> P in Status)
				{
					Xml.Append("<status");

					if (!string.IsNullOrEmpty(P.Key))
					{
						Xml.Append(" xml:lang='");
						Xml.Append(XML.Encode(P.Key));
						Xml.Append("'>");
					}
					else
						Xml.Append('>');

					Xml.Append(XML.Encode(P.Value));
					Xml.Append("</status>");
				}
			}

			await this.client.AddCustomPresenceXml(Availability, Xml);

			await this.client.SendDirectedPresence(Type, RoomId + "@" + Domain + "/" + NickName, Xml.ToString(), Callback, State);
		}

		/// <summary>
		/// Sets the presence of the occupant in a room.
		/// </summary>
		/// <param name="RoomId">Room ID.</param>
		/// <param name="Domain">Domain of service hosting the room.</param>
		/// <param name="NickName">Nickname of the occupant.</param>
		/// <param name="Availability">Occupant availability.</param>
		/// <returns>Task object that finishes when stanza has been sent.</returns>
		public Task SetPresenceAsync(string RoomId, string Domain, string NickName, Availability Availability)
		{
			return this.SetPresenceAsync(RoomId, Domain, NickName, Availability, null);
		}

		/// <summary>
		/// Sets the presence of the occupant in a room.
		/// </summary>
		/// <param name="RoomId">Room ID.</param>
		/// <param name="Domain">Domain of service hosting the room.</param>
		/// <param name="NickName">Nickname of the occupant.</param>
		/// <param name="Availability">Occupant availability.</param>
		/// <param name="Status">Custom status</param>
		/// <returns>Task object that finishes when stanza has been sent.</returns>
		public async Task SetPresenceAsync(string RoomId, string Domain, string NickName,
			Availability Availability, params KeyValuePair<string, string>[] Status)
		{
			TaskCompletionSource<bool> Result = new TaskCompletionSource<bool>();
			await this.SetPresence(RoomId, Domain, NickName, Availability, Status, (Sender, e) =>
			{
				Result.TrySetResult(true);
				return Task.CompletedTask;
			}, null);

			await Result.Task;
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
		public Task GetRoomRegistrationForm(string RoomId, string Domain, EventHandlerAsync<RoomRegistrationEventArgs> Callback,
			object State)
		{
			return this.GetRoomRegistrationForm(RoomId, Domain, Callback, null, State);
		}

		/// <summary>
		/// Starts the registration process with a room.
		/// </summary>
		/// <param name="RoomId">Room ID.</param>
		/// <param name="Domain">Domain of service hosting the room.</param>
		/// <param name="Callback">Method to call when response has been returned.</param>
		/// <param name="SubmissionCallback">Method to call when configuration has been submitted.</param>
		/// <param name="State">State object to pass on to callback method.</param>
		public async Task GetRoomRegistrationForm(string RoomId, string Domain, EventHandlerAsync<RoomRegistrationEventArgs> Callback,
			EventHandlerAsync<IqResultEventArgs> SubmissionCallback, object State)
		{
			StringBuilder Xml = new StringBuilder();

			Xml.Append("<query xmlns='");
			Xml.Append(XmppClient.NamespaceRegister);
			Xml.Append("'/>");

			await this.client.SendIqGet(RoomId + "@" + Domain, Xml.ToString(), async (Sender, e) =>
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

											return this.client.SendIqSet(Form2.From, Xml2.ToString(), (sender3, e3) =>
											{
												return SubmissionCallback.Raise(this, e3);
											}, null);
										},
										(sender2, Form2) =>
										{
											StringBuilder Xml2 = new StringBuilder();

											Xml2.Append("<query xmlns='");
											Xml2.Append(XmppClient.NamespaceRegister);
											Xml2.Append("'>");

											Form2.SerializeCancel(Xml2);

											Xml2.Append("</query>");

											return this.client.SendIqSet(Form2.From, Xml2.ToString(), (sender3, e3) =>
											{
												e3.Ok = false;
												return SubmissionCallback.Raise(this, e3);
											}, null);
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

				await Callback.Raise(this, e2);
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
		public async Task<RoomRegistrationEventArgs> GetRoomRegistrationFormAsync(string RoomId, string Domain,
			EventHandlerAsync<IqResultEventArgs> SubmissionCallback, object State)
		{
			TaskCompletionSource<RoomRegistrationEventArgs> Result = new TaskCompletionSource<RoomRegistrationEventArgs>();

			await this.GetRoomRegistrationForm(RoomId, Domain, (Sender, e) =>
			{
				Result.TrySetResult(e);
				return Task.CompletedTask;
			}, SubmissionCallback, State);

			return await Result.Task;
		}

		private Task RegistrationRequestHandler(object Sender, MessageFormEventArgs e)
		{
			return this.RegistrationRequest.Raise(this, e);
		}

		/// <summary>
		/// Event raised when someone has made a request to register with a room to which the client is an administrator.
		/// </summary>
		public event EventHandlerAsync<MessageFormEventArgs> RegistrationRequest;

		#endregion

		#region Room Information

		/// <summary>
		/// Gets information about a room.
		/// </summary>
		/// <param name="RoomId">Room ID.</param>
		/// <param name="Domain">Domain of service hosting the room.</param>
		/// <param name="Callback">Method to call when response has been returned.</param>
		/// <param name="State">State object to pass on to callback method.</param>
		public Task GetRoomInformation(string RoomId, string Domain, EventHandlerAsync<RoomInformationEventArgs> Callback, object State)
		{
			return this.client.SendServiceDiscoveryRequest(RoomId + "@" + Domain, (Sender, e) =>
			{
				return Callback.Raise(this, new RoomInformationEventArgs(e));
			}, State);
		}

		/// <summary>
		/// Gets information about a room.
		/// </summary>
		/// <param name="RoomId">Room ID.</param>
		/// <param name="Domain">Domain of service hosting the room.</param>
		/// <returns>Room information response.</returns>
		public async Task<RoomInformationEventArgs> GetRoomInformationAsync(string RoomId, string Domain)
		{
			TaskCompletionSource<RoomInformationEventArgs> Result = new TaskCompletionSource<RoomInformationEventArgs>();

			await this.GetRoomInformation(RoomId, Domain, (Sender, e) =>
			{
				Result.TrySetResult(e);
				return Task.CompletedTask;
			}, null);

			return await Result.Task;
		}

		/// <summary>
		/// Gets information about my registered nick-name.
		/// </summary>
		/// <param name="RoomId">Room ID.</param>
		/// <param name="Domain">Domain of service hosting the room.</param>
		/// <param name="Callback">Method to call when response has been returned.</param>
		/// <param name="State">State object to pass on to callback method.</param>
		public Task GetMyNickName(string RoomId, string Domain, EventHandlerAsync<ServiceDiscoveryEventArgs> Callback, object State)
		{
			return this.client.SendServiceDiscoveryRequest(RoomId + "@" + Domain, "x-roomuser-item", (Sender, e) =>
			{
				return Callback.Raise(this, e);
			}, State);
		}

		/// <summary>
		/// Gets information about my registered nick-name.
		/// </summary>
		/// <param name="RoomId">Room ID.</param>
		/// <param name="Domain">Domain of service hosting the room.</param>
		/// <returns>Information about my registered nick-name, if any, and if supported by the service.</returns>
		public async Task<ServiceDiscoveryEventArgs> GetMyNickNameAsync(string RoomId, string Domain)
		{
			TaskCompletionSource<ServiceDiscoveryEventArgs> Result = new TaskCompletionSource<ServiceDiscoveryEventArgs>();

			await this.GetMyNickName(RoomId, Domain, (Sender, e) =>
			{
				Result.TrySetResult(e);
				return Task.CompletedTask;
			}, null);

			return await Result.Task;
		}

		/// <summary>
		/// Gets information about items (occupants) in a room.
		/// </summary>
		/// <param name="RoomId">Room ID.</param>
		/// <param name="Domain">Domain of service hosting the room.</param>
		/// <param name="Callback">Method to call when response has been returned.</param>
		/// <param name="State">State object to pass on to callback method.</param>
		public Task GetRoomItems(string RoomId, string Domain, EventHandlerAsync<ServiceItemsDiscoveryEventArgs> Callback, object State)
		{
			return this.client.SendServiceItemsDiscoveryRequest(RoomId + "@" + Domain, Callback, State);
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
		public Task OccupantServiceDiscovery(string RoomId, string Domain, string NickName,
			EventHandlerAsync<ServiceDiscoveryEventArgs> Callback, object State)
		{
			return this.client.SendServiceDiscoveryRequest(RoomId + "@" + Domain + "/" + NickName, Callback, State);
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
		public Task OccupantServiceItemsDiscovery(string RoomId, string Domain, string NickName,
			EventHandlerAsync<ServiceItemsDiscoveryEventArgs> Callback, object State)
		{
			return this.client.SendServiceItemsDiscoveryRequest(RoomId + "@" + Domain + "/" + NickName, Callback, State);
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
		public Task ConfigureOccupant(string RoomId, string Domain, string OccupantNickName,
			Role Role, string Reason, EventHandlerAsync<IqResultEventArgs> Callback, object State)
		{
			return this.ConfigureOccupant(RoomId, Domain,
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
		public Task ConfigureOccupant(string RoomId, string Domain, string OccupantBareJid,
			Affiliation Affiliation, string Reason, EventHandlerAsync<IqResultEventArgs> Callback, object State)
		{
			return this.ConfigureOccupant(RoomId, Domain,
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
		public Task ConfigureOccupant(string RoomId, string Domain, string OccupantBareJid, string OccupantNickName,
			Affiliation? Affiliation, Role? Role, string Reason, EventHandlerAsync<IqResultEventArgs> Callback, object State)
		{
			return this.ConfigureOccupant(RoomId, Domain,
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
		public Task ConfigureOccupant(string RoomId, string Domain, MucOccupantConfiguration Change,
			EventHandlerAsync<IqResultEventArgs> Callback, object State)
		{
			return this.ConfigureOccupants(RoomId, Domain, new MucOccupantConfiguration[] { Change }, Callback, State);
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
		public Task ConfigureOccupants(string RoomId, string Domain, MucOccupantConfiguration[] Changes,
			EventHandlerAsync<IqResultEventArgs> Callback, object State)
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

			return this.client.SendIqSet(RoomId + "@" + Domain, Xml.ToString(), Callback, State);
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
		public async Task<IqResultEventArgs> ConfigureOccupantsAsync(string RoomId, string Domain, params MucOccupantConfiguration[] Changes)
		{
			TaskCompletionSource<IqResultEventArgs> Result = new TaskCompletionSource<IqResultEventArgs>();

			await this.ConfigureOccupants(RoomId, Domain, Changes, (Sender, e) =>
			{
				Result.TrySetResult(e);
				return Task.CompletedTask;
			}, null);

			return await Result.Task;
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
		public Task Kick(string RoomId, string Domain, string OccupantNickName,
			EventHandlerAsync<IqResultEventArgs> Callback, object State)
		{
			return this.Kick(RoomId, Domain, OccupantNickName, string.Empty, Callback, State);
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
		public Task Kick(string RoomId, string Domain, string OccupantNickName,
			string Reason, EventHandlerAsync<IqResultEventArgs> Callback, object State)
		{
			return this.ConfigureOccupant(RoomId, Domain, OccupantNickName, Role.None, Reason, Callback, State);
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
		public Task GrantVoice(string RoomId, string Domain, string OccupantNickName,
			EventHandlerAsync<IqResultEventArgs> Callback, object State)
		{
			return this.GrantVoice(RoomId, Domain, OccupantNickName, string.Empty, Callback, State);
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
		public Task GrantVoice(string RoomId, string Domain, string OccupantNickName,
			string Reason, EventHandlerAsync<IqResultEventArgs> Callback, object State)
		{
			return this.ConfigureOccupant(RoomId, Domain, OccupantNickName, Role.Participant, Reason, Callback, State);
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
		public Task RevokeVoice(string RoomId, string Domain, string OccupantNickName,
			EventHandlerAsync<IqResultEventArgs> Callback, object State)
		{
			return this.RevokeVoice(RoomId, Domain, OccupantNickName, string.Empty, Callback, State);
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
		public Task RevokeVoice(string RoomId, string Domain, string OccupantNickName,
			string Reason, EventHandlerAsync<IqResultEventArgs> Callback, object State)
		{
			return this.ConfigureOccupant(RoomId, Domain, OccupantNickName, Role.Visitor, Reason, Callback, State);
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
		public Task GetOccupants(string RoomId, string Domain, Affiliation Affiliation,
			EventHandlerAsync<OccupantListEventArgs> Callback, object State)
		{
			return this.GetOccupants(RoomId, Domain, Affiliation, null, Callback, State);
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
		public Task GetOccupants(string RoomId, string Domain, Role Role,
			EventHandlerAsync<OccupantListEventArgs> Callback, object State)
		{
			return this.GetOccupants(RoomId, Domain, null, Role, Callback, State);
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
		public Task GetOccupants(string RoomId, string Domain, Affiliation? Affiliation,
			Role? Role, EventHandlerAsync<OccupantListEventArgs> Callback, object State)
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

			return this.client.SendIqGet(RoomId + "@" + Domain, Xml.ToString(), (Sender, e) =>
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

				return Callback.Raise(this, new OccupantListEventArgs(e, Occupants.ToArray()));
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
		public async Task<OccupantListEventArgs> GetOccupantsAsync(string RoomId, string Domain, Affiliation? Affiliation, Role? Role)
		{
			TaskCompletionSource<OccupantListEventArgs> Result = new TaskCompletionSource<OccupantListEventArgs>();

			await this.GetOccupants(RoomId, Domain, Affiliation, Role, (Sender, e) =>
			{
				Result.TrySetResult(e);
				return Task.CompletedTask;
			}, null);

			return await Result.Task;
		}

		/// <summary>
		/// Request voice privileges in a room.
		/// </summary>
		/// <param name="RoomId">Room ID.</param>
		/// <param name="Domain">Domain of service hosting the room.</param>
		public Task RequestVoice(string RoomId, string Domain)
		{
			return this.RequestRole(RoomId, Domain, Role.Participant);
		}

		/// <summary>
		/// Requests privileges corresponding to a specific role in a room.
		/// </summary>
		/// <param name="RoomId">Room ID.</param>
		/// <param name="Domain">Domain of service hosting the room.</param>
		/// <param name="Role">Requested role.</param>
		public Task RequestRole(string RoomId, string Domain, Role Role)
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

			return this.client.SendMessage(MessageType.Normal, RoomId + "@" + Domain, Xml.ToString(),
				string.Empty, string.Empty, string.Empty, string.Empty, string.Empty);
		}

		private Task UserRequestHandler(object Sender, MessageFormEventArgs e)
		{
			return this.OccupantRequest.Raise(this, e);
		}

		/// <summary>
		/// Event raised when a request has been received from an occupant in a room.
		/// </summary>
		public event EventHandlerAsync<MessageFormEventArgs> OccupantRequest;

		/// <summary>
		/// Gets a list of occupants with voice (participants).
		/// </summary>
		/// <param name="RoomId">Room ID.</param>
		/// <param name="Domain">Domain of service hosting the room.</param>
		/// <param name="Callback">Method to call when response is returned from room.</param>
		/// <param name="State">State object to pass on to callback method.</param>
		public Task GetVoiceList(string RoomId, string Domain,
			EventHandlerAsync<OccupantListEventArgs> Callback, object State)
		{
			return this.GetOccupants(RoomId, Domain, null, Role.Participant, Callback, State);
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
		public Task Ban(string RoomId, string Domain, string OccupantBareJid,
			EventHandlerAsync<IqResultEventArgs> Callback, object State)
		{
			return this.Ban(RoomId, Domain, OccupantBareJid, string.Empty, Callback, State);
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
		public Task Ban(string RoomId, string Domain, string OccupantBareJid,
			string Reason, EventHandlerAsync<IqResultEventArgs> Callback, object State)
		{
			return this.ConfigureOccupant(RoomId, Domain, OccupantBareJid, Affiliation.Outcast, Reason, Callback, State);
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
		public Task GetBannedOccupants(string RoomId, string Domain,
			EventHandlerAsync<OccupantListEventArgs> Callback, object State)
		{
			return this.GetOccupants(RoomId, Domain, Affiliation.Outcast, null, Callback, State);
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
		public Task GrantMembership(string RoomId, string Domain, string OccupantBareJid,
			EventHandlerAsync<IqResultEventArgs> Callback, object State)
		{
			return this.GrantMembership(RoomId, Domain, OccupantBareJid, string.Empty, string.Empty, Callback, State);
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
		public Task GrantMembership(string RoomId, string Domain, string OccupantBareJid,
			string Reason, EventHandlerAsync<IqResultEventArgs> Callback, object State)
		{
			return this.GrantMembership(RoomId, Domain, OccupantBareJid, string.Empty, Reason, Callback, State);
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
		public Task GrantMembership(string RoomId, string Domain, string OccupantBareJid,
			string DefaultNickName, string Reason, EventHandlerAsync<IqResultEventArgs> Callback, object State)
		{
			return this.ConfigureOccupant(RoomId, Domain,
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
		public Task RevokeMembership(string RoomId, string Domain, string OccupantBareJid,
			EventHandlerAsync<IqResultEventArgs> Callback, object State)
		{
			return this.RevokeMembership(RoomId, Domain, OccupantBareJid, string.Empty, Callback, State);
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
		public Task RevokeMembership(string RoomId, string Domain, string OccupantBareJid,
			string Reason, EventHandlerAsync<IqResultEventArgs> Callback, object State)
		{
			return this.ConfigureOccupant(RoomId, Domain, OccupantBareJid, Affiliation.None, Reason, Callback, State);
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
		public Task GetMemberList(string RoomId, string Domain,
			EventHandlerAsync<OccupantListEventArgs> Callback, object State)
		{
			return this.GetOccupants(RoomId, Domain, Affiliation.Member, null, Callback, State);
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
		public Task GrantModeratorStatus(string RoomId, string Domain, string OccupantNickName,
			EventHandlerAsync<IqResultEventArgs> Callback, object State)
		{
			return this.GrantModeratorStatus(RoomId, Domain, OccupantNickName, string.Empty, Callback, State);
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
		public Task GrantModeratorStatus(string RoomId, string Domain, string OccupantNickName,
			string Reason, EventHandlerAsync<IqResultEventArgs> Callback, object State)
		{
			return this.ConfigureOccupant(RoomId, Domain, OccupantNickName, Role.Moderator, Reason, Callback, State);
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
		public Task RevokeModeratorStatus(string RoomId, string Domain, string OccupantNickName,
			EventHandlerAsync<IqResultEventArgs> Callback, object State)
		{
			return this.RevokeModeratorStatus(RoomId, Domain, OccupantNickName, string.Empty, Callback, State);
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
		public Task RevokeModeratorStatus(string RoomId, string Domain, string OccupantNickName,
			string Reason, EventHandlerAsync<IqResultEventArgs> Callback, object State)
		{
			return this.ConfigureOccupant(RoomId, Domain, OccupantNickName, Role.Participant, Reason, Callback, State);
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
		public Task GetModeratorList(string RoomId, string Domain,
			EventHandlerAsync<OccupantListEventArgs> Callback, object State)
		{
			return this.GetOccupants(RoomId, Domain, null, Role.Moderator, Callback, State);
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
		public Task GrantOwnership(string RoomId, string Domain, string OccupantBareJid,
			EventHandlerAsync<IqResultEventArgs> Callback, object State)
		{
			return this.GrantOwnership(RoomId, Domain, OccupantBareJid, string.Empty, Callback, State);
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
		public Task GrantOwnership(string RoomId, string Domain, string OccupantBareJid,
			string Reason, EventHandlerAsync<IqResultEventArgs> Callback, object State)
		{
			return this.ConfigureOccupant(RoomId, Domain,
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
		public Task RevokeOwnership(string RoomId, string Domain, string OccupantBareJid,
			EventHandlerAsync<IqResultEventArgs> Callback, object State)
		{
			return this.RevokeOwnership(RoomId, Domain, OccupantBareJid, string.Empty, Callback, State);
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
		public Task RevokeOwnership(string RoomId, string Domain, string OccupantBareJid,
			string Reason, EventHandlerAsync<IqResultEventArgs> Callback, object State)
		{
			return this.ConfigureOccupant(RoomId, Domain, OccupantBareJid, Affiliation.Admin, Reason, Callback, State);
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
		public Task GetOwnerList(string RoomId, string Domain,
			EventHandlerAsync<OccupantListEventArgs> Callback, object State)
		{
			return this.GetOccupants(RoomId, Domain, Affiliation.Owner, null, Callback, State);
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
		public Task GrantAdministrator(string RoomId, string Domain, string OccupantBareJid,
			EventHandlerAsync<IqResultEventArgs> Callback, object State)
		{
			return this.GrantAdministrator(RoomId, Domain, OccupantBareJid, string.Empty, Callback, State);
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
		public Task GrantAdministrator(string RoomId, string Domain, string OccupantBareJid,
			string Reason, EventHandlerAsync<IqResultEventArgs> Callback, object State)
		{
			return this.ConfigureOccupant(RoomId, Domain,
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
		public Task RevokeAdministrator(string RoomId, string Domain, string OccupantBareJid,
			EventHandlerAsync<IqResultEventArgs> Callback, object State)
		{
			return this.RevokeAdministrator(RoomId, Domain, OccupantBareJid, string.Empty, Callback, State);
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
		public Task RevokeAdministrator(string RoomId, string Domain, string OccupantBareJid,
			string Reason, EventHandlerAsync<IqResultEventArgs> Callback, object State)
		{
			return this.ConfigureOccupant(RoomId, Domain, OccupantBareJid, Affiliation.Member, Reason, Callback, State);
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
		public Task GetAdminList(string RoomId, string Domain,
			EventHandlerAsync<OccupantListEventArgs> Callback, object State)
		{
			return this.GetOccupants(RoomId, Domain, Affiliation.Admin, null, Callback, State);
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
		public Task DestroyRoom(string RoomId, string Domain, EventHandlerAsync<IqResultEventArgs> Callback, object State)
		{
			return this.DestroyRoom(RoomId, Domain, string.Empty, string.Empty, Callback, State);
		}

		/// <summary>
		/// Destroyes a room.
		/// </summary>
		/// <param name="RoomId">Room ID.</param>
		/// <param name="Domain">Domain of service hosting the room.</param>
		/// <param name="Reason">Optional reason for destroying the room.</param>
		/// <param name="Callback">Method to call when response is returned from room.</param>
		/// <param name="State">State object to pass on to callback method.</param>
		public Task DestroyRoom(string RoomId, string Domain, string Reason,
			EventHandlerAsync<IqResultEventArgs> Callback, object State)
		{
			return this.DestroyRoom(RoomId, Domain, Reason, string.Empty, Callback, State);
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
		public Task DestroyRoom(string RoomId, string Domain, string Reason,
			string AlternateRoomJid, EventHandlerAsync<IqResultEventArgs> Callback, object State)
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

			return this.client.SendIqSet(RoomId + "@" + Domain, Xml.ToString(), Callback, State);
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
		public async Task<IqResultEventArgs> DestroyRoomAsync(string RoomId, string Domain, string Reason,
			string AlternateRoomJid)
		{
			TaskCompletionSource<IqResultEventArgs> Result = new TaskCompletionSource<IqResultEventArgs>();

			await this.DestroyRoom(RoomId, Domain, Reason, AlternateRoomJid, (Sender, e) =>
			{
				Result.TrySetResult(e);
				return Task.CompletedTask;
			}, null);

			return await Result.Task;
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
		public Task SelfPing(string RoomId, string Domain, string NickName, EventHandlerAsync<IqResultEventArgs> Callback, object State)
		{
			return this.client.SendIqGet(RoomId + "@" + Domain + "/" + NickName, "<ping xmlns='" + XmppClient.NamespacePing + "'/>", Callback, State);
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
		public async Task<IqResultEventArgs> SelfPingAsync(string RoomId, string Domain, string NickName)
		{
			TaskCompletionSource<IqResultEventArgs> Result = new TaskCompletionSource<IqResultEventArgs>();

			await this.client.SendIqGet(RoomId + "@" + Domain + "/" + NickName, "<ping xmlns='" + XmppClient.NamespacePing + "'/>", (Sender, e) =>
			{
				Result.TrySetResult(e);
				return Task.CompletedTask;
			}, null);

			return await Result.Task;
		}

		#endregion

	}
}
