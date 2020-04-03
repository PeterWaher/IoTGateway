using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Waher.Content;
using Waher.Events;
using Waher.Networking.HTTP;
using Waher.Networking.XMPP;
using Waher.Runtime.Language;

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

				Gateway.XmppClient.OnRosterItemAdded += XmppClient_OnRosterItemAdded;
				Gateway.XmppClient.OnRosterItemRemoved += XmppClient_OnRosterItemRemoved;
				Gateway.XmppClient.OnRosterItemUpdated += XmppClient_OnRosterItemUpdated;
				Gateway.XmppClient.OnPresence += XmppClient_OnPresence;
				Gateway.XmppClient.OnPresenceSubscribe += XmppClient_OnPresenceSubscribe;
				Gateway.XmppClient.OnStateChanged += XmppClient_OnStateChanged;
			}
		}

		private bool handlersAdded = false;
		private XmppClient prevClient = null;

		private void XmppClient_OnStateChanged(object Sender, XmppState NewState)
		{
			// TODO
		}

		private void XmppClient_OnPresenceSubscribe(object Sender, PresenceEventArgs e)
		{
			// TODO
		}

		private void XmppClient_OnPresence(object Sender, PresenceEventArgs e)
		{
			// TODO
		}

		private void XmppClient_OnRosterItemUpdated(object Sender, RosterItem Item)
		{
			// TODO
		}

		private void XmppClient_OnRosterItemRemoved(object Sender, RosterItem Item)
		{
			// TODO
		}

		private void XmppClient_OnRosterItemAdded(object Sender, RosterItem Item)
		{
			// TODO
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
		/// Initializes the setup object.
		/// </summary>
		/// <param name="WebServer">Current Web Server object.</param>
		public override Task InitSetup(HttpServer WebServer)
		{
			this.connectToJID = WebServer.Register("/Settings/ConnectToJID", null, this.ConnectToJID, true, false, true, Gateway.LoggedIn);
			this.removeContact = WebServer.Register("/Settings/RemoveContact", null, this.RemoveContact, true, false, true, Gateway.LoggedIn);
			this.unsubscribeContact = WebServer.Register("/Settings/UnsubscribeContact", null, this.UnsubscribeContact, true, false, true, Gateway.LoggedIn);
			this.subscribeToContact = WebServer.Register("/Settings/SubscribeToContact", null, this.SubscribeToContact, true, false, true, Gateway.LoggedIn);
			this.renameContact = WebServer.Register("/Settings/RenameContact", null, this.RenameContact, true, false, true, Gateway.LoggedIn);
			this.updateContactGroups = WebServer.Register("/Settings/UpdateContactGroups", null, this.UpdateContactGroups, true, false, true, Gateway.LoggedIn);
			this.getGroups = WebServer.Register("/Settings/GetGroups", null, this.GetGroups, true, false, true, Gateway.LoggedIn);

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

			return base.UnregisterSetup(WebServer);
		}

		/// <summary>
		/// Waits for the user to provide configuration.
		/// </summary>
		/// <param name="WebServer">Current Web Server object.</param>
		/// <returns>If all system configuration objects must be reloaded from the database.</returns>
		public override Task<bool> SetupConfiguration(HttpServer WebServer)
		{
			if (!this.Complete && Gateway.XmppClient.State == XmppState.Offline)
				Gateway.XmppClient?.Connect();

			this.AddHandlers();

			return base.SetupConfiguration(WebServer);
		}

		private void ConnectToJID(HttpRequest Request, HttpResponse Response)
		{
			Gateway.AssertUserAuthenticated(Request);

			if (!Request.HasData)
				throw new BadRequestException();

			object Obj = Request.DecodeData();
			if (!(Obj is string JID))
				throw new BadRequestException();

			Response.ContentType = "text/plain";

			if (!XmppClient.BareJidRegEx.IsMatch(JID))
				Response.Write("0");
			else
			{
				RosterItem Item = Gateway.XmppClient.GetRosterItem(JID);
				if (Item is null)
				{
					Gateway.XmppClient.RequestPresenceSubscription(JID, this.NickName());
					Log.Informational("Requesting presence subscription.", JID);
					Response.Write("1");
				}
				else if (Item.State != SubscriptionState.Both && Item.State != SubscriptionState.To)
				{
					Gateway.XmppClient.RequestPresenceSubscription(JID, this.NickName());
					Log.Informational("Requesting presence subscription.", JID);
					Response.Write("2");
				}
				else
					Response.Write("3");
			}
		}

		private string NickName()
		{
			if (string.IsNullOrEmpty(Gateway.Domain))
				return string.Empty;
			else
				return "<nick xmlns='http://jabber.org/protocol/nick'>" + Gateway.Domain + "</nick>";    // Nick name: XEP-0172.
		}

		private void RemoveContact(HttpRequest Request, HttpResponse Response)
		{
			Gateway.AssertUserAuthenticated(Request);

			if (!Request.HasData)
				throw new BadRequestException();

			object Obj = Request.DecodeData();
			if (!(Obj is string JID))
				throw new BadRequestException();

			Response.ContentType = "text/plain";

			XmppClient Client = Gateway.XmppClient;
			RosterItem Contact = Client.GetRosterItem(JID);
			if (Contact is null)
				Response.Write("0");
			else
			{
				Client.RemoveRosterItem(Contact.BareJid);
				Log.Informational("Contact removed.", Contact.BareJid);
				Response.Write("1");
			}
		}

		private void UnsubscribeContact(HttpRequest Request, HttpResponse Response)
		{
			Gateway.AssertUserAuthenticated(Request);

			if (!Request.HasData)
				throw new BadRequestException();

			object Obj = Request.DecodeData();
			if (!(Obj is string JID))
				throw new BadRequestException();

			Response.ContentType = "text/plain";

			XmppClient Client = Gateway.XmppClient;
			RosterItem Contact = Client.GetRosterItem(JID);
			if (Contact is null)
				Response.Write("0");
			else
			{
				Client.RequestPresenceUnsubscription(Contact.BareJid);
				Log.Informational("Unsubscribing from presence.", Contact.BareJid);
				Response.Write("1");
			}
		}

		private void SubscribeToContact(HttpRequest Request, HttpResponse Response)
		{
			Gateway.AssertUserAuthenticated(Request);

			if (!Request.HasData)
				throw new BadRequestException();

			object Obj = Request.DecodeData();
			if (!(Obj is string JID))
				throw new BadRequestException();

			Response.ContentType = "text/plain";

			XmppClient Client = Gateway.XmppClient;
			RosterItem Contact = Client.GetRosterItem(JID);
			if (Contact is null)
				Response.Write("0");
			else
			{
				Client.RequestPresenceSubscription(Contact.BareJid, this.NickName());
				Log.Informational("Requesting presence subscription.", Contact.BareJid);
				Response.Write("1");
			}
		}

		private void RenameContact(HttpRequest Request, HttpResponse Response)
		{
			Gateway.AssertUserAuthenticated(Request);

			if (!Request.HasData)
				throw new BadRequestException();

			object Obj = Request.DecodeData();
			string JID = Request.Header["X-BareJID"];
			string NewName = Obj as string;
			if (JID is null || string.IsNullOrEmpty(JID))
				throw new BadRequestException();

			Response.ContentType = "text/plain";

			XmppClient Client = Gateway.XmppClient;
			RosterItem Contact = Client.GetRosterItem(JID);
			if (Contact is null)
				Response.Write("0");
			else
			{
				Client.UpdateRosterItem(Contact.BareJid, NewName, Contact.Groups);
				Log.Informational("Contact renamed.", Contact.BareJid, new KeyValuePair<string, object>("Name", NewName));
				Response.Write("1");
			}
		}

		private void UpdateContactGroups(HttpRequest Request, HttpResponse Response)
		{
			try
			{
				Gateway.AssertUserAuthenticated(Request);

				if (!Request.HasData)
					throw new BadRequestException();

				object Obj = Request.DecodeData();
				string BareJid = Obj as string;
				if (string.IsNullOrEmpty(BareJid))
					throw new BadRequestException();

				XmppClient Client = Gateway.XmppClient;
				RosterItem Contact = Client.GetRosterItem(BareJid);
				if (Contact is null)
					throw new NotFoundException();

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

				if (Groups != null)
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

				Client.UpdateRosterItem(Contact.BareJid, Contact.Name, GroupsArray);
				Log.Informational("Contact groups updated.", Contact.BareJid, new KeyValuePair<string, object>("Groups", sb.ToString()));

				Response.ContentType = "text/plain";
				Response.Write("1");
			}
			catch (Exception ex)
			{
				Response.SendResponse(ex);
			}
			finally
			{
				Response.Dispose();
			}
		}

		private void GetGroups(HttpRequest Request, HttpResponse Response)
		{
			try
			{
				Gateway.AssertUserAuthenticated(Request);

				if (!Request.HasData)
					throw new BadRequestException();

				string StartsWith = Request.DecodeData() as string;
				if (string.IsNullOrEmpty(StartsWith))
					throw new BadRequestException();

				SuggestionEventArgs e = new SuggestionEventArgs(StartsWith.Trim());

				foreach (RosterItem Item in Gateway.XmppClient.Roster)
				{
					foreach (string Group in Item.Groups)
						e.AddSuggestion(Group);
				}

				SuggesstionsEventHandler h = OnGetGroupSuggestions;
				if (!(h is null))
				{
					try
					{
						h(this, e);
					}
					catch (Exception ex)
					{
						Log.Critical(ex);
					}
				}

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

				Response.ContentType = "application/json";
				Response.Write(sb.ToString());
				Response.SendResponse();
			}
			catch (Exception ex)
			{
				Response.SendResponse(ex);
			}
			finally
			{
				Response.Dispose();
			}
		}

		/// <summary>
		/// Event raised when list of group suggestions is populated.
		/// </summary>
		public static event SuggesstionsEventHandler OnGetGroupSuggestions = null;
	}
}
