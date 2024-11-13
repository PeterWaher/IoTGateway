using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Waher.Content;
using Waher.Content.Html;
using Waher.Content.Json;
using Waher.Content.Markdown;
using Waher.Content.Text;
using Waher.Events;
using Waher.Networking;
using Waher.Networking.HTTP;
using Waher.Networking.XMPP;
using Waher.Networking.XMPP.Events;
using Waher.Persistence;
using Waher.Runtime.Language;
using Waher.Script;

namespace Waher.IoTGateway.Setup
{
	/// <summary>
	/// Allows the user to configure the XMPP Roster of the gateway.
	/// </summary>
	public class RosterConfiguration : SystemConfiguration
	{
		private static RosterConfiguration instance = null;

		private HttpResource connectToJID;
		private HttpResource removeContact;
		private HttpResource unsubscribeContact;
		private HttpResource subscribeToContact;
		private HttpResource renameContact;
		private HttpResource updateContactGroups;
		private HttpResource getGroups;
		private HttpResource acceptRequest;
		private HttpResource declineRequest;

		/// <summary>
		/// Allows the user to configure the XMPP Roster of the gateway.
		/// </summary>
		public RosterConfiguration()
			: base()
		{
		}

		/// <summary>
		/// Instance of configuration object.
		/// </summary>
		public static RosterConfiguration Instance => instance;

		/// <summary>
		/// Resource to be redirected to, to perform the configuration.
		/// </summary>
		public override string Resource => "/Settings/Roster.md";

		/// <summary>
		/// Priority of the setting. Configurations are sorted in ascending order.
		/// </summary>
		public override int Priority => 400;

		/// <summary>
		/// Gets a title for the system configuration.
		/// </summary>
		/// <param name="Language">Current language.</param>
		/// <returns>Title string</returns>
		public override Task<string> Title(Language Language)
		{
			return Language.GetStringAsync(typeof(Gateway), 11, "Roster");
		}

		/// <summary>
		/// Is called during startup to configure the system.
		/// </summary>
		public override Task ConfigureSystem()
		{
			this.AddHandlers();
			return Task.CompletedTask;
		}

		private void AddHandlers()
		{
			if (!this.handlersAdded || Gateway.XmppClient != this.prevClient)
			{
				this.handlersAdded = true;
				this.prevClient = Gateway.XmppClient;

				Gateway.XmppClient.OnRosterItemAdded += this.XmppClient_OnRosterItemAdded;
				Gateway.XmppClient.OnRosterItemRemoved += this.XmppClient_OnRosterItemRemoved;
				Gateway.XmppClient.OnRosterItemUpdated += this.XmppClient_OnRosterItemUpdated;
				Gateway.XmppClient.OnPresence += this.XmppClient_OnPresence;
				Gateway.XmppClient.OnPresenceSubscribe += this.XmppClient_OnPresenceSubscribe;
				Gateway.XmppClient.OnStateChanged += this.XmppClient_OnStateChanged;
			}
		}

		private bool handlersAdded = false;
		private XmppClient prevClient = null;

		private async Task XmppClient_OnStateChanged(object _, XmppState NewState)
		{
			if (NewState == XmppState.Offline || NewState == XmppState.Error || NewState == XmppState.Connected)
			{
				string[] TabIDs = this.GetTabIDs();
				if (TabIDs.Length > 0 && !(Gateway.XmppClient is null))
				{
					string Json = JSON.Encode(new KeyValuePair<string, object>[]
					{
						new KeyValuePair<string, object>("html", await this.RosterItemsHtml(Gateway.XmppClient.Roster, Gateway.XmppClient.SubscriptionRequests))
					}, false);

					Task _2 = ClientEvents.PushEvent(TabIDs, "UpdateRoster", Json, true, "User");
				}

				while (Gateway.XmppClient?.State == XmppState.Connected)
				{
					(string Jid, string Name, string[] Groups) = this.PopToAdd();
					
					if (!string.IsNullOrEmpty(Jid))
					{
						RosterItem Item = Gateway.XmppClient.GetRosterItem(Jid);
						
						if (Item is null)
							await Gateway.XmppClient.AddRosterItem(new RosterItem(Jid, Name, Groups));
						else if (NeedsUpdate(Item, Name, Groups))
							await Gateway.XmppClient.AddRosterItem(new RosterItem(Jid, Name, Union(Item.Groups, Groups)));
						
						continue;
					}

					Jid = this.PopToSubscribe();
					if (!string.IsNullOrEmpty(Jid))
					{
						RosterItem Item = Gateway.XmppClient.GetRosterItem(Jid);

						if (Item is null || Item.State == SubscriptionState.None || Item.State == SubscriptionState.From)
							await Gateway.XmppClient.RequestPresenceSubscription(Jid);
						
						continue;
					}

					break;
				}
			}
		}

		private static bool NeedsUpdate(RosterItem Item, string Name, string[] Groups)
		{
			if (Item.Name != Name)
				return true;

			if (Groups is null)
				return false;

			Dictionary<string, bool> Found = new Dictionary<string, bool>();

			if (!(Item.Groups is null))
			{
				foreach (string Group in Item.Groups)
					Found[Group] = true;
			}

			if (!(Groups is null))
			{
				foreach (string Group in Groups)
				{
					if (!Found.ContainsKey(Group))
						return true;
				}
			}

			return false;
		}

		private static string[] Union(string[] A1, string[] A2)
		{
			SortedDictionary<string, bool> Entries = new SortedDictionary<string, bool>();

			if (!(A1 is null))
			{
				foreach (string s in A1)
					Entries[s] = true;
			}

			if (!(A2 is null))
			{
				foreach (string s in A2)
					Entries[s] = true;
			}

			string[] Result = new string[Entries.Count];
			Entries.Keys.CopyTo(Result, 0);

			return Result;
		}

		private async Task XmppClient_OnPresenceSubscribe(object Sender, PresenceEventArgs e)
		{
			if (string.Compare(e.FromBareJID, Gateway.XmppClient.BareJID, true) == 0)
				return;

			if (this.AcceptSubscriptionRequest(e.FromBareJID))
			{
				await e.Accept();
				return;
			}

			StringBuilder Markdown = new StringBuilder();

			Markdown.Append("Presence subscription request received from **");
			Markdown.Append(MarkdownDocument.Encode(e.FromBareJID));
			Markdown.Append("**. You can accept or decline the request from the roster configuration in the Administration portal.");

			await Gateway.SendNotification(Markdown.ToString());

			string[] TabIDs = this.GetTabIDs();
			if (TabIDs.Length > 0)
			{
				string Json = JSON.Encode(new KeyValuePair<string, object>[]
				{
					new KeyValuePair<string, object>("bareJid", e.FromBareJID),
					new KeyValuePair<string, object>("html", await this.RosterItemsHtml(new RosterItem[0], new PresenceEventArgs[] { e }))
				}, false);

				Task _ = ClientEvents.PushEvent(TabIDs, "UpdateRosterItem", Json, true, "User");
			}
		}

		private async Task XmppClient_OnPresence(object Sender, PresenceEventArgs e)
		{
			RosterItem Item = Gateway.XmppClient?.GetRosterItem(e.FromBareJID);
			if (!(Item is null))
				await this.XmppClient_OnRosterItemUpdated(Sender, Item);
		}

		private async Task XmppClient_OnRosterItemUpdated(object _, RosterItem Item)
		{
			string[] TabIDs = this.GetTabIDs();
			if (TabIDs.Length > 0)
			{
				string Json = JSON.Encode(new KeyValuePair<string, object>[]
				{
					new KeyValuePair<string, object>("bareJid", Item.BareJid),
					new KeyValuePair<string, object>("html", await this.RosterItemsHtml(new RosterItem[]{ Item }, new PresenceEventArgs[0]))
				}, false);

				await ClientEvents.PushEvent(TabIDs, "UpdateRosterItem", Json, true, "User");
			}
		}

		private Task XmppClient_OnRosterItemRemoved(object _, RosterItem Item)
		{
			this.RosterItemRemoved(Item.BareJid);
			return Task.CompletedTask;
		}

		private void RosterItemRemoved(string BareJid)
		{
			string[] TabIDs = this.GetTabIDs();
			if (TabIDs.Length > 0)
			{
				string Json = JSON.Encode(new KeyValuePair<string, object>[]
				{
					new KeyValuePair<string, object>("bareJid", BareJid)
				}, false);

				Task _ = ClientEvents.PushEvent(TabIDs, "RemoveRosterItem", Json, true, "User");
			}
		}

		private Task XmppClient_OnRosterItemAdded(object Sender, RosterItem Item)
		{
			return this.XmppClient_OnRosterItemUpdated(Sender, Item);
		}

		private string[] GetTabIDs()
		{
			if (Gateway.Configuring)
				return ClientEvents.GetTabIDs();
			else
				return ClientEvents.GetTabIDsForLocation("/Settings/Roster.md");
		}

		private async Task<string> RosterItemsHtml(RosterItem[] Contacts, PresenceEventArgs[] SubscriptionRequests)
		{
			string FileName = Path.Combine(Gateway.RootFolder, "Settings", "RosterItems.md");
			string Markdown = await Resources.ReadAllTextAsync(FileName);
			Variables v = HttpServer.CreateVariables();
			v["Contacts"] = Contacts;
			v["Requests"] = SubscriptionRequests;

			MarkdownSettings Settings = new MarkdownSettings(Gateway.Emoji1_24x24, true, v);
			MarkdownDocument Doc = await MarkdownDocument.CreateAsync(Markdown, Settings, FileName, string.Empty, string.Empty);
			string Html = await Doc.GenerateHTML();

			Html = HtmlDocument.GetBody(Html);

			return Html;
		}

		/// <summary>
		/// Sets the static instance of the configuration.
		/// </summary>
		/// <param name="Configuration">Configuration object</param>
		public override void SetStaticInstance(ISystemConfiguration Configuration)
		{
			instance = Configuration as RosterConfiguration;
		}

		/// <summary>
		/// Minimum required privilege for a user to be allowed to change the configuration defined by the class.
		/// </summary>
		protected override string ConfigPrivilege => "Admin.Communication.Roster";

		/// <summary>
		/// Initializes the setup object.
		/// </summary>
		/// <param name="WebServer">Current Web Server object.</param>
		public override Task InitSetup(HttpServer WebServer)
		{
			HttpAuthenticationScheme Auth = Gateway.LoggedIn(new string[] { this.ConfigPrivilege });

			this.connectToJID = WebServer.Register("/Settings/ConnectToJID", null, this.ConnectToJID, true, false, true, Auth);
			this.removeContact = WebServer.Register("/Settings/RemoveContact", null, this.RemoveContact, true, false, true, Auth);
			this.unsubscribeContact = WebServer.Register("/Settings/UnsubscribeContact", null, this.UnsubscribeContact, true, false, true, Auth);
			this.subscribeToContact = WebServer.Register("/Settings/SubscribeToContact", null, this.SubscribeToContact, true, false, true, Auth);
			this.renameContact = WebServer.Register("/Settings/RenameContact", null, this.RenameContact, true, false, true, Auth);
			this.updateContactGroups = WebServer.Register("/Settings/UpdateContactGroups", null, this.UpdateContactGroups, true, false, true, Auth);
			this.getGroups = WebServer.Register("/Settings/GetGroups", null, this.GetGroups, true, false, true, Auth);
			this.acceptRequest = WebServer.Register("/Settings/AcceptRequest", null, this.AcceptRequest, true, false, true, Auth);
			this.declineRequest = WebServer.Register("/Settings/DeclineRequest", null, this.DeclineRequest, true, false, true, Auth);

			return base.InitSetup(WebServer);
		}

		/// <summary>
		/// Unregisters the setup object.
		/// </summary>
		/// <param name="WebServer">Current Web Server object.</param>
		public override Task UnregisterSetup(HttpServer WebServer)
		{
			WebServer.Unregister(this.connectToJID);
			WebServer.Unregister(this.removeContact);
			WebServer.Unregister(this.unsubscribeContact);
			WebServer.Unregister(this.subscribeToContact);
			WebServer.Unregister(this.renameContact);
			WebServer.Unregister(this.updateContactGroups);
			WebServer.Unregister(this.getGroups);
			WebServer.Unregister(this.acceptRequest);
			WebServer.Unregister(this.declineRequest);

			return base.UnregisterSetup(WebServer);
		}

		/// <summary>
		/// Waits for the user to provide configuration.
		/// </summary>
		/// <param name="WebServer">Current Web Server object.</param>
		/// <returns>If all system configuration objects must be reloaded from the database.</returns>
		public override async Task<bool> SetupConfiguration(HttpServer WebServer)
		{
			if (!this.Complete &&
				Gateway.XmppClient.State == XmppState.Offline &&
				!(Gateway.XmppClient is null))
			{
				await Gateway.XmppClient.Connect();
			}

			this.AddHandlers();

			return await base.SetupConfiguration(WebServer);
		}

		private async Task ConnectToJID(HttpRequest Request, HttpResponse Response)
		{
			Gateway.AssertUserAuthenticated(Request, this.ConfigPrivilege);

			if (!Request.HasData)
				throw new BadRequestException();

			object Obj = await Request.DecodeDataAsync();
			if (!(Obj is string JID))
				throw new BadRequestException();

			Response.ContentType = PlainTextCodec.DefaultContentType;

			string JidToValidate = JID;
			if (CaseInsensitiveString.IsNullOrEmpty(Gateway.Domain) && JidToValidate.EndsWith("@"))
				JidToValidate += "example.org";

			if (!XmppClient.BareJidRegEx.IsMatch(JidToValidate))
				await Response.Write("0");
			else
			{
				RosterItem Item = Gateway.XmppClient.GetRosterItem(JID);
				if (Item is null)
				{
					await Gateway.XmppClient.RequestPresenceSubscription(JID, await this.NickName());
					Log.Informational("Requesting presence subscription.", JID);
					await Response.Write("1");
				}
				else if (Item.State != SubscriptionState.Both && Item.State != SubscriptionState.To)
				{
					await Gateway.XmppClient.RequestPresenceSubscription(JID, await this.NickName());
					Log.Informational("Requesting presence subscription.", JID);
					await Response.Write("2");
				}
				else
					await Response.Write("3");
			}
		}

		private async Task<string> NickName()
		{
			SuggestionEventArgs e = new SuggestionEventArgs(string.Empty);
			await OnGetNickNameSuggestions.Raise(this, e);

			string[] Suggestions = e.ToArray();
			string NickName = Suggestions.Length > 0 ? Suggestions[0] : (string)Gateway.Domain;

			return XmppClient.EmbedNickName(NickName);
		}

		private async Task RemoveContact(HttpRequest Request, HttpResponse Response)
		{
			Gateway.AssertUserAuthenticated(Request, this.ConfigPrivilege);

			if (!Request.HasData)
				throw new BadRequestException();

			object Obj = await Request.DecodeDataAsync();
			if (!(Obj is string JID))
				throw new BadRequestException();

			Response.ContentType = PlainTextCodec.DefaultContentType;

			XmppClient Client = Gateway.XmppClient;
			RosterItem Contact = Client.GetRosterItem(JID);
			if (Contact is null)
				await Response.Write("0");
			else
			{
				await Client.RemoveRosterItem(Contact.BareJid);
				Log.Informational("Contact removed.", Contact.BareJid);
				await Response.Write("1");
			}
		}

		private async Task UnsubscribeContact(HttpRequest Request, HttpResponse Response)
		{
			Gateway.AssertUserAuthenticated(Request, this.ConfigPrivilege);

			if (!Request.HasData)
				throw new BadRequestException();

			object Obj = await Request.DecodeDataAsync();
			if (!(Obj is string JID))
				throw new BadRequestException();

			Response.ContentType = PlainTextCodec.DefaultContentType;

			XmppClient Client = Gateway.XmppClient;
			RosterItem Contact = Client.GetRosterItem(JID);
			if (Contact is null)
				await Response.Write("0");
			else
			{
				await Client.RequestPresenceUnsubscription(Contact.BareJid);
				Log.Informational("Unsubscribing from presence.", Contact.BareJid);
				await Response.Write("1");
			}
		}

		private async Task SubscribeToContact(HttpRequest Request, HttpResponse Response)
		{
			Gateway.AssertUserAuthenticated(Request, this.ConfigPrivilege);

			if (!Request.HasData)
				throw new BadRequestException();

			object Obj = await Request.DecodeDataAsync();
			if (!(Obj is string JID))
				throw new BadRequestException();

			Response.ContentType = PlainTextCodec.DefaultContentType;

			XmppClient Client = Gateway.XmppClient;
			RosterItem Contact = Client.GetRosterItem(JID);
			if (Contact is null)
				await Response.Write("0");
			else
			{
				await Client.RequestPresenceSubscription(Contact.BareJid, await this.NickName());
				Log.Informational("Requesting presence subscription.", Contact.BareJid);
				await Response.Write("1");
			}
		}

		private async Task RenameContact(HttpRequest Request, HttpResponse Response)
		{
			Gateway.AssertUserAuthenticated(Request, this.ConfigPrivilege);

			if (!Request.HasData)
				throw new BadRequestException();

			object Obj = await Request.DecodeDataAsync();
			string JID = Request.Header["X-BareJID"];
			string NewName = Obj as string;
			if (JID is null || string.IsNullOrEmpty(JID))
				throw new BadRequestException();

			Response.ContentType = PlainTextCodec.DefaultContentType;

			XmppClient Client = Gateway.XmppClient;
			RosterItem Contact = Client.GetRosterItem(JID);
			if (Contact is null)
				await Response.Write("0");
			else
			{
				await Client.UpdateRosterItem(Contact.BareJid, NewName, Contact.Groups);
				Log.Informational("Contact renamed.", Contact.BareJid, new KeyValuePair<string, object>("Name", NewName));
				await Response.Write("1");
			}
		}

		private async Task UpdateContactGroups(HttpRequest Request, HttpResponse Response)
		{
			Gateway.AssertUserAuthenticated(Request, this.ConfigPrivilege);

			if (!Request.HasData)
				throw new BadRequestException();

			object Obj = await Request.DecodeDataAsync();
			string BareJid = Obj as string;
			if (string.IsNullOrEmpty(BareJid))
				throw new BadRequestException();

			XmppClient Client = Gateway.XmppClient;
			RosterItem Contact = Client.GetRosterItem(BareJid)
				?? throw new NotFoundException();

			SortedDictionary<string, bool> Groups = new SortedDictionary<string, bool>(StringComparer.InvariantCultureIgnoreCase);
			string[] GroupsArray;
			int i = 0;
			string s;

			while (!string.IsNullOrEmpty(s = System.Web.HttpUtility.UrlDecode(Request.Header["X-Group-" + (++i).ToString()])))
				Groups[s] = true;

			if (Groups.Count > 0)
			{
				GroupsArray = new string[Groups.Count];
				Groups.Keys.CopyTo(GroupsArray, 0);
			}
			else
				GroupsArray = new string[0];

			StringBuilder sb = new StringBuilder();
			bool First = true;

			if (!(Groups is null))
			{
				foreach (string Group in GroupsArray)
				{
					if (First)
						First = false;
					else
						sb.Append(", ");

					sb.Append(Group);
				}
			}

			await Client.UpdateRosterItem(Contact.BareJid, Contact.Name, GroupsArray);
			Log.Informational("Contact groups updated.", Contact.BareJid, new KeyValuePair<string, object>("Groups", sb.ToString()));

			Response.ContentType = PlainTextCodec.DefaultContentType;
			await Response.Write("1");
		}

		private async Task GetGroups(HttpRequest Request, HttpResponse Response)
		{
			Gateway.AssertUserAuthenticated(Request, this.ConfigPrivilege);

			if (!Request.HasData)
				throw new BadRequestException();

			string StartsWith = await Request.DecodeDataAsync() as string;
			if (string.IsNullOrEmpty(StartsWith))
				throw new BadRequestException();

			SuggestionEventArgs e = new SuggestionEventArgs(StartsWith.Trim());

			foreach (RosterItem Item in Gateway.XmppClient.Roster)
			{
				foreach (string Group in Item.Groups)
					e.AddSuggestion(Group);
			}

			await OnGetGroupSuggestions.Raise(this, e);

			StringBuilder sb = new StringBuilder();
			string[] Groups = e.ToArray();
			bool First = true;
			int Nr = 0;

			sb.Append("{\"groups\":[");

			foreach (string Group in Groups)
			{
				if (First)
					First = false;
				else
					sb.Append(',');

				sb.Append('"');
				sb.Append(CommonTypes.JsonStringEncode(Group));
				sb.Append('"');

				Nr++;
			}

			sb.Append("],\"count\":");
			sb.Append(Nr.ToString());
			sb.Append("}");

			Response.ContentType = JsonCodec.DefaultContentType;
			await Response.Write(sb.ToString());
		}

		/// <summary>
		/// Event raised when list of group suggestions is populated.
		/// </summary>
		public static event EventHandlerAsync<SuggestionEventArgs> OnGetGroupSuggestions = null;

		/// <summary>
		/// Event raised when list of nickname suggestions is populated.
		/// </summary>
		public static event EventHandlerAsync<SuggestionEventArgs> OnGetNickNameSuggestions = null;

		private async Task AcceptRequest(HttpRequest Request, HttpResponse Response)
		{
			Gateway.AssertUserAuthenticated(Request, this.ConfigPrivilege);

			if (!Request.HasData)
				throw new BadRequestException();

			object Obj = await Request.DecodeDataAsync();
			if (!(Obj is string JID))
				throw new BadRequestException();

			Response.ContentType = PlainTextCodec.DefaultContentType;

			XmppClient Client = Gateway.XmppClient;

			PresenceEventArgs SubscriptionRequest = Client.GetSubscriptionRequest(JID);
			if (SubscriptionRequest is null)
				await Response.Write("0");
			else
			{
				await SubscriptionRequest.Accept();
				Log.Informational("Accepting presence subscription request.", SubscriptionRequest.FromBareJID);

				if (Gateway.XmppClient.LastSetPresenceAvailability != Availability.Offline)
				{
					await Gateway.XmppClient.SetPresence(
						Gateway.XmppClient.LastSetPresenceAvailability,
						Gateway.XmppClient.LastSetPresenceCustomStatus);
				}

				await Response.Write("1");
			}
		}

		private async Task DeclineRequest(HttpRequest Request, HttpResponse Response)
		{
			Gateway.AssertUserAuthenticated(Request, this.ConfigPrivilege);

			if (!Request.HasData)
				throw new BadRequestException();

			object Obj = await Request.DecodeDataAsync();
			if (!(Obj is string JID))
				throw new BadRequestException();

			Response.ContentType = PlainTextCodec.DefaultContentType;

			XmppClient Client = Gateway.XmppClient;

			PresenceEventArgs SubscriptionRequest = Client.GetSubscriptionRequest(JID);
			if (SubscriptionRequest is null)
				await Response.Write("0");
			else
			{
				await SubscriptionRequest.Decline();
				Log.Informational("Declining presence subscription request.", SubscriptionRequest.FromBareJID);

				this.RosterItemRemoved(JID);

				await Response.Write("1");
			}
		}

		/// <summary>
		/// Simplified configuration by configuring simple default values.
		/// </summary>
		/// <returns>If the configuration was changed, and can be considered completed.</returns>
		public override Task<bool> SimplifiedConfiguration()
		{
			return Task.FromResult(true);
		}

		/// <summary>
		/// Optional Comma-separated list of Bare JIDs to add to the roster.
		/// </summary>
		public const string GATEWAY_ROSTER_ADD = nameof(GATEWAY_ROSTER_ADD);

		/// <summary>
		/// Optional Comma-separated list of Bare JIDs to send presence subscription requests to.
		/// </summary>
		public const string GATEWAY_ROSTER_SUBSCRIBE = nameof(GATEWAY_ROSTER_SUBSCRIBE);

		/// <summary>
		/// Optional Comma-separated list of Bare JIDs to accept presence subscription requests from.
		/// </summary>
		public const string GATEWAY_ROSTER_ACCEPT = nameof(GATEWAY_ROSTER_ACCEPT);

		/// <summary>
		/// Optional Comma-separated list of groups to define.
		/// </summary>
		public const string GATEWAY_ROSTER_GROUPS = nameof(GATEWAY_ROSTER_GROUPS);

		/// <summary>
		/// Optional Comma-separated list of Bare JIDs in the roster to add to the group `[group]`.
		/// </summary>
		public const string GATEWAY_ROSTER_GRP_ = nameof(GATEWAY_ROSTER_GRP_);

		/// <summary>
		/// Optional human-readable name of a JID in the roster.
		/// </summary>
		public const string GATEWAY_ROSTER_NAME_ = nameof(GATEWAY_ROSTER_NAME_);

		/// <summary>
		/// Environment configuration by configuring values available in environment variables.
		/// </summary>
		/// <returns>If the configuration was changed, and can be considered completed.</returns>
		public override Task<bool> EnvironmentConfiguration()
		{
			Dictionary<string, string> ToAdd = GetDictionaryElementWithNames(GATEWAY_ROSTER_ADD);
			if (!(ToAdd is null) && !this.ValidateJids(ToAdd.Keys, GATEWAY_ROSTER_ADD))
				return Task.FromResult(false);

			Dictionary<string, bool> ToSubscribe = GetDictionaryElements(GATEWAY_ROSTER_SUBSCRIBE);
			if (!(ToSubscribe is null) && !this.ValidateJids(ToSubscribe.Keys, GATEWAY_ROSTER_SUBSCRIBE))
				return Task.FromResult(false);

			Dictionary<string, bool> ToAccept = GetDictionaryElements(GATEWAY_ROSTER_ACCEPT);
			if (!(ToAccept is null) && !this.ValidateJids(ToAccept.Keys, GATEWAY_ROSTER_ACCEPT))
				return Task.FromResult(false);

			string[] GroupNames = GetElements(GATEWAY_ROSTER_GROUPS);

			if (ToAdd is null && ToSubscribe is null && ToAccept is null && GroupNames is null)
				return Task.FromResult(true);

			Dictionary<string, SortedDictionary<string, bool>> GroupsByJid =
				new Dictionary<string, SortedDictionary<string, bool>>(StringComparer.InvariantCultureIgnoreCase);

			foreach (string Group in GroupNames)
			{
				string Name = GATEWAY_ROSTER_GRP_ + Group;
				string[] Jids = GetElements(Name);
				if (Jids is null)
				{
					this.LogEnvironmentVariableMissingError(Name, string.Empty);
					return Task.FromResult(false);
				}

				if (!this.ValidateJids(Jids, GATEWAY_ROSTER_GROUPS))
					return Task.FromResult(false);

				foreach (string Jid in Jids)
				{
					if (!GroupsByJid.TryGetValue(Jid, out SortedDictionary<string, bool> Groups))
					{
						Groups = new SortedDictionary<string, bool>();
						GroupsByJid[Jid] = Groups;
					}

					Groups[Group] = true;
				}
			}

			this.toAdd = ToAdd;
			this.toSubscribe = ToSubscribe;
			this.toAccept = ToAccept;
			this.groupsByJid = GroupsByJid;

			this.AddHandlers();

			return Task.FromResult(true);
		}

		private Dictionary<string, string> toAdd = null;
		private Dictionary<string, bool> toSubscribe = null;
		private Dictionary<string, bool> toAccept = null;
		private Dictionary<string, SortedDictionary<string, bool>> groupsByJid = null;

		private bool ValidateJids(IEnumerable<string> Jids, string ParameterName)
		{
			foreach (string Jid in Jids)
			{
				if (!XmppClient.BareJidRegEx.IsMatch(Jid))
				{
					this.LogEnvironmentError("Not a valid Bare JID.", ParameterName, Jid);
					return false;
				}
			}

			return true;
		}

		private (string, string, string[]) PopToAdd()
		{
			Dictionary<string, string> ToAdd = this.toAdd;
			if (ToAdd is null)
				return (null, null, null);

			string Jid = null;
			string Name = null;

			lock (ToAdd)
			{
				foreach (KeyValuePair<string, string> P in this.toAdd)
				{
					Jid = P.Key;
					Name = P.Value;
					this.toAdd.Remove(Jid);
					break;
				}
			}

			if (Jid is null)
			{
				this.toAdd = null;
				return (null, null, null);
			}

			Dictionary<string, SortedDictionary<string, bool>> GroupsByJid = this.groupsByJid;
			if (GroupsByJid is null)
				return (Jid, Name, null);

			string[] Groups;

			lock (GroupsByJid)
			{
				if (!GroupsByJid.TryGetValue(Jid, out SortedDictionary<string, bool> GroupsOrdered))
					return (Jid, Name, null);

				GroupsByJid.Remove(Jid);
				if (GroupsByJid.Count == 0)
					this.groupsByJid = null;

				Groups = new string[GroupsOrdered.Count];
				GroupsOrdered.Keys.CopyTo(Groups, 0);
			}

			return (Jid, Name, Groups);
		}

		private string PopToSubscribe()
		{
			Dictionary<string, bool> ToSubscribe = this.toSubscribe;
			if (ToSubscribe is null)
				return null;

			lock (ToSubscribe)
			{
				foreach (string Jid in this.toSubscribe.Keys)
				{
					this.toSubscribe.Remove(Jid);
					return Jid;
				}
			}

			this.toSubscribe = null;
			return null;
		}

		private bool AcceptSubscriptionRequest(string Jid)
		{
			Dictionary<string, bool> ToAccept = this.toAccept;
			if (ToAccept is null)
				return false;

			lock (ToAccept)
			{
				if (!ToAccept.ContainsKey(Jid))
					return false;

				ToAccept.Remove(Jid);
				if (ToAccept.Count == 0)
					this.toAccept = null;
			}

			return true;
		}

		private static string[] GetElements(string VariableName)
		{
			string Value = Environment.GetEnvironmentVariable(VariableName);
			if (string.IsNullOrEmpty(Value))
				return null;
			else
				return Value.Split(',');
		}

		private static Dictionary<string, bool> GetDictionaryElements(string VariableName)
		{
			string[] Elements = GetElements(VariableName);
			if (Elements is null)
				return null;

			Dictionary<string, bool> Result = new Dictionary<string, bool>(StringComparer.InvariantCultureIgnoreCase);

			foreach (string Element in Elements)
				Result[Element] = true;

			return Result;
		}

		private static Dictionary<string, string> GetDictionaryElementWithNames(string VariableName)
		{
			string[] Elements = GetElements(VariableName);
			if (Elements is null)
				return null;

			Dictionary<string, string> Result = new Dictionary<string, string>();

			foreach (string Element in Elements)
			{
				string Name = Environment.GetEnvironmentVariable(GATEWAY_ROSTER_NAME_ + Element);
				Result[Element] = Name ?? string.Empty;
			}

			return Result;
		}
	}
}
