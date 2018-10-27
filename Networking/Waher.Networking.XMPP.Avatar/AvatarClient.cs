using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using Waher.Content;
using Waher.Content.Xml;
using Waher.Events;
using Waher.Networking.XMPP.PEP;
using Waher.Persistence;
using Waher.Persistence.Filters;
using Waher.Runtime.Settings;
using Waher.Security;

namespace Waher.Networking.XMPP.Avatar
{
	/// <summary>
	/// Provides help with managing avatars.
	/// </summary>
	public class AvatarClient : XmppExtension
	{
		private readonly Dictionary<string, Avatar> contactAvatars = new Dictionary<string, Avatar>(StringComparer.CurrentCultureIgnoreCase);
		private readonly PepClient pep;
		private IEndToEndEncryption e2e;
		private Avatar localAvatar = null;
		private Avatar defaultAvatar = null;
		private bool includeAvatarInPresence = true;

		/// <summary>
		/// Provides help with managing avatars.
		/// </summary>
		/// <param name="Client">XMPP Client</param>
		public AvatarClient(XmppClient Client)
			: this(Client, null, null)
		{
		}

		/// <summary>
		/// Provides help with managing avatars.
		/// </summary>
		/// <param name="Client">XMPP Client</param>
		/// <param name="E2E">End-to-End encryption</param>
		public AvatarClient(XmppClient Client, IEndToEndEncryption E2E)
			: this(Client, null, E2E)
		{
		}

		/// <summary>
		/// Provides help with managing avatars.
		/// </summary>
		/// <param name="Client">XMPP Client</param>
		/// <param name="PepClient">Personal Eventing Protocol Client</param>
		public AvatarClient(XmppClient Client, PepClient PepClient)
			: this(Client, PepClient, null)
		{
		}

		/// <summary>
		/// Provides help with managing avatars.
		/// </summary>
		/// <param name="Client">XMPP Client</param>
		/// <param name="PepClient">Personal Eventing Protocol Client</param>
		/// <param name="E2E">End-to-End encryption</param>
		public AvatarClient(XmppClient Client, PepClient PepClient, IEndToEndEncryption E2E)
			: base(Client)
		{
			this.pep = PepClient;
			this.e2e = E2E;

			// XEP-0008: IQ-Based Avatars: http://xmpp.org/extensions/xep-0008.html
			Client.RegisterIqGetHandler("query", "jabber:iq:avatar", this.QueryAvatarHandler, false);

			Client.OnStateChanged += Client_OnStateChanged;
			Client.OnPresence += Client_OnPresence;
			Client.OnRosterItemRemoved += Client_OnRosterItemRemoved;
			Client.OnRosterItemAdded += Client_OnRosterItemAdded;
			Client.CustomPresenceXml += Client_CustomPresenceXml;

			byte[] Bin = Resources.LoadResource(typeof(AvatarClient).Namespace + ".Images.DefaultAvatar.png",
				typeof(AvatarClient).GetTypeInfo().Assembly);

			this.defaultAvatar = new Avatar(Client.BareJID.ToLower(), "image/png", Bin);

			Task.Run(() => this.LoadAvatar());
		}

		/// <summary>
		/// Disposes of the extension.
		/// </summary>
		public override void Dispose()
		{
			this.client.UnregisterIqGetHandler("query", "jabber:iq:avatar", this.QueryAvatarHandler, false);

			this.client.OnStateChanged -= Client_OnStateChanged;
			this.client.OnPresence -= Client_OnPresence;
			this.client.OnRosterItemRemoved -= Client_OnRosterItemRemoved;
			this.client.OnRosterItemAdded -= Client_OnRosterItemAdded;
			this.client.CustomPresenceXml -= Client_CustomPresenceXml;

			this.localAvatar = null;
			this.defaultAvatar = null;

			base.Dispose();
		}

		/// <summary>
		/// Implemented extensions.
		/// </summary>
		public override string[] Extensions => new string[] { "XEP-0008" };

		private async Task LoadAvatar()
		{
			this.includeAvatarInPresence = await RuntimeSettings.GetAsync("IncludeAvatarInPresence", true);

			this.localAvatar = await Database.FindFirstDeleteRest<Avatar>(
				new FilterFieldEqualTo("BareJid", this.client.BareJID.ToLower()));
		}

		/// <summary>
		/// If the local avatar should be included in presence stanzas
		/// </summary>
		public bool IncludeAvatarInPresence
		{
			get => this.includeAvatarInPresence;

			set
			{
				if (this.includeAvatarInPresence != value)
				{
					this.includeAvatarInPresence = value;

					Task.Run(async () =>
					{
						try
						{
							await RuntimeSettings.SetAsync("IncludeAvatarInPresence", this.includeAvatarInPresence);
						}
						catch (Exception ex)
						{
							Log.Critical(ex);
						}
					});
				}
			}
		}

		/// <summary>
		/// Updates the local avatar.
		/// </summary>
		/// <param name="ContentType">Content-Type of the avatar image.</param>
		/// <param name="Binary">Binary encoding of the image.</param>
		/// <param name="StoreOnBroker">If the avatar should be stored on the broker.</param>
		public async Task UpdateLocalAvatarAsync(string ContentType, byte[] Binary, bool StoreOnBroker)
		{
			if (this.localAvatar == null)
			{
				this.localAvatar = new Avatar(this.client.BareJID.ToLower(), ContentType, Binary);
				await Database.Insert(this.localAvatar);
			}
			else
			{
				this.localAvatar.Binary = Binary;
				this.localAvatar.ContentType = ContentType;
				this.localAvatar.Hash = Binary == null ? string.Empty : Hashes.ComputeSHA1HashString(Binary);

				await Database.Update(this.localAvatar);
			}

			if (StoreOnBroker && this.client != null && this.client.State == XmppState.Connected)
			{
				StringBuilder Request = new StringBuilder();
				Request.Append("<query xmlns='storage:client:avatar'><data mimetype='");
				Request.Append(XML.Encode(ContentType));
				Request.Append("'>");

				if (Binary != null)
					Request.Append(Convert.ToBase64String(Binary));

				Request.Append("</data></query>");

				this.client.SendIqSet(this.client.BareJID, Request.ToString(), null, null);
			}
		}

		/// <summary>
		/// Event raised when somebody wants to access the avatar of the local client.
		/// If the remote endpoint is not authoized, the corresponding stanza exception
		/// should be thrown.
		/// </summary>
		public event IqEventHandler ValidateAccess = null;

		private void QueryAvatarHandler(object Sender, IqEventArgs e)
		{
			this.ValidateAccess?.Invoke(this, e);

			Avatar Avatar = this.localAvatar;

			if (Avatar == null)
				Avatar = this.defaultAvatar;

			StringBuilder Response = new StringBuilder();
			Response.Append("<query xmlns='jabber:iq:avatar'><data mimetype='");
			Response.Append(XML.Encode(Avatar.ContentType));
			Response.Append("'>");
			Response.Append(System.Convert.ToBase64String(Avatar.Binary));
			Response.Append("</data></query>");

			e.IqResult(Response.ToString());
		}

		private async void Client_OnStateChanged(object Sender, XmppState NewState)
		{
			try
			{
				if (NewState == XmppState.StreamOpened &&
					this.localAvatar != null &&
					this.localAvatar.BareJid != this.client.BareJID)
				{
					this.localAvatar.BareJid = this.client.BareJID;
					await Database.Update(this.localAvatar);
				}
			}
			catch (Exception ex)
			{
				Log.Critical(ex);
			}
		}

		private async void Client_OnPresence(object Sender, PresenceEventArgs e)
		{
			try
			{
				if (e.Type == PresenceType.Available)
				{
					bool HasAvatar;

					lock (this.contactAvatars)
					{
						HasAvatar = this.contactAvatars.ContainsKey(e.FromBareJID);
					}

					if (!HasAvatar)
					{
						string Hash = null;

						foreach (XmlNode N in e.Presence.ChildNodes)
						{
							if (N.LocalName == "x" && N.NamespaceURI == "jabber:x:avatar")
							{
								Hash = N.InnerText;
								break;
							}
						}

						if (Hash != null)
						{
							Avatar Avatar;
							string FullJID = e.From;
							string BareJID = e.FromBareJID;
							bool LoadAvatar = false;
							bool CheckDatabase = false;

							lock (this.contactAvatars)
							{
								if (this.contactAvatars.TryGetValue(BareJID, out Avatar))
								{
									if (Hash != null)
									{
										if (string.IsNullOrEmpty(Hash))
											this.contactAvatars.Remove(BareJID);
										else if (Avatar == null || Avatar.Hash != Hash)
										{
											if (Avatar != null)
												this.contactAvatars[BareJID] = null;

											LoadAvatar = true;
										}
									}
								}
								else
								{
									if (Hash == null || !string.IsNullOrEmpty(Hash))
										LoadAvatar = CheckDatabase = true;
								}
							}

							if (LoadAvatar)
							{
								try
								{
									if (Avatar != null)
										await Database.Delete(Avatar);
									else
									{
										if (CheckDatabase)
										{
											IEnumerable<Avatar> Avatars = await Database.Find<Avatar>(new FilterFieldEqualTo("BareJid", BareJID.ToLower()));

											foreach (Avatar Avatar2 in Avatars)
											{
												if (Avatar == null && (Hash == null || Hash == Avatar2.Hash))
													Avatar = Avatar2;
												else
													await Database.Delete(Avatar2);
											}

											if (Avatar != null)
											{
												lock (this.contactAvatars)
												{
													this.contactAvatars[BareJID] = Avatar;
												}

												LoadAvatar = false;
											}
										}
									}

									if (LoadAvatar)
									{
										this.e2e.SendIqGet(this.client, E2ETransmission.NormalIfNotE2E,
											FullJID, "<query xmlns='jabber:iq:avatar'/>", async (sender2, e2) =>
											{
												try
												{
													if (e2.Ok)
														await this.ParseAvatar(BareJID, Hash, e2.FirstElement);
													else
													{
														this.Client.SendIqGet(BareJID, "<query xmlns='storage:client:avatar'/>",
															async (sender3, e3) =>
															{
																try
																{
																	if (e3.Ok)
																		await this.ParseAvatar(BareJID, Hash, e3.FirstElement);
																	else
																		await this.ParseAvatar(BareJID, Hash, null);
																}
																catch (Exception ex3)
																{
																	Log.Critical(ex3);
																}
															}, null);
													}
												}
												catch (Exception ex2)
												{
													Log.Critical(ex2);
												}
											}, null);
									}
								}
								catch (Exception ex)
								{
									Log.Critical(ex);
								}
							}
						}
					}
				}
			}
			catch (Exception ex)
			{
				Log.Critical(ex);
			}
		}

		private async Task ParseAvatar(string BareJid, string Hash, XmlElement E)
		{
			if (E != null && E.LocalName == "query" && (E.NamespaceURI == "jabber:iq:avatar" || E.NamespaceURI == "storage:client:avatar"))
			{
				XmlElement E2;
				byte[] Bin = null;
				string ContentType = null;

				foreach (XmlNode N in E.ChildNodes)
				{
					E2 = N as XmlElement;
					if (E2 == null)
						continue;

					if (E2.LocalName == "data")
					{
						ContentType = XML.Attribute(E2, "mimetype");
						try
						{
							Bin = System.Convert.FromBase64String(E2.InnerText.Trim());
						}
						catch (Exception)
						{
							Bin = null;
							ContentType = null;
						}

						break;
					}
				}

				Avatar OldAvatar;
				Avatar NewAvatar;

				lock (this.contactAvatars)
				{
					if (this.contactAvatars.TryGetValue(BareJid, out OldAvatar))
						this.contactAvatars.Remove(BareJid);

					if (Bin != null)
					{
						if (string.IsNullOrEmpty(Hash))
							NewAvatar = new Avatar(BareJid.ToLower(), ContentType, Bin);
						else
							NewAvatar = new Avatar(BareJid.ToLower(), ContentType, Hash, Bin);

						this.contactAvatars[BareJid] = NewAvatar;
					}
					else
						this.contactAvatars[BareJid] = NewAvatar = null;
				}

				AvatarEventHandler h;

				if (OldAvatar != null)
				{
					await Database.Delete(OldAvatar);

					h = this.AvatarRemoved;
					if (h != null)
					{
						try
						{
							h(this, new AvatarEventArgs(BareJid, OldAvatar));
						}
						catch (Exception ex)
						{
							Log.Critical(ex);
						}
					}
				}

				if (NewAvatar != null)
				{
					await Database.Insert(NewAvatar);

					h = this.AvatarAdded;
					if (h != null)
					{
						try
						{
							h(this, new AvatarEventArgs(BareJid, NewAvatar));
						}
						catch (Exception ex)
						{
							Log.Critical(ex);
						}
					}
				}

				h = this.AvatarUpdated;
				if (h != null)
				{
					try
					{
						h(this, new AvatarEventArgs(BareJid, NewAvatar));
					}
					catch (Exception ex)
					{
						Log.Critical(ex);
					}
				}
			}
		}

		/// <summary>
		/// Event raised when an avatar has been removed.
		/// </summary>
		public event AvatarEventHandler AvatarRemoved = null;

		/// <summary>
		/// Event raised, when an avatar has been added.
		/// </summary>
		public event AvatarEventHandler AvatarAdded = null;

		/// <summary>
		/// Event raised, when an avatar has been updated (added or removed).
		/// </summary>
		public event AvatarEventHandler AvatarUpdated = null;

		/// <summary>
		/// Gets an avatar.
		/// </summary>
		/// <param name="BareJid">Bare JID of avatar. If empty, the local avatar is returned. If no avatar is found, the
		/// default avatar is returned.</param>
		/// <returns>Avatar</returns>
		public async Task<Avatar> GetAvatarAsync(string BareJid)
		{
			Avatar Result;

			if (string.IsNullOrEmpty(BareJid) || BareJid == this.client.BareJID)
			{
				if (this.localAvatar == null)
					return this.defaultAvatar;
				else
					return this.localAvatar;
			}

			lock (this.contactAvatars)
			{
				if (this.contactAvatars.TryGetValue(BareJid, out Result))
					return Result;
			}

			IEnumerable<Avatar> Avatars = await Database.Find<Avatar>(new FilterFieldEqualTo("BareJid", BareJid.ToLower()));

			foreach (Avatar Avatar in Avatars)
			{
				lock (this.contactAvatars)
				{
					if (this.contactAvatars.TryGetValue(BareJid, out Result))
						return Result;
					else
					{
						this.contactAvatars[BareJid] = Avatar;
						return Avatar;
					}
				}
			}

			return this.defaultAvatar;
		}

		/// <summary>
		/// Local avatar. Update by calling <see cref="UpdateLocalAvatarAsync"/>
		/// </summary>
		public Avatar LocalAvatar
		{
			get => this.localAvatar ?? this.defaultAvatar;
		}

		/// <summary>
		/// Default avatar.
		/// </summary>
		public Avatar DefaultAvatar
		{
			get => this.defaultAvatar;
			set => this.defaultAvatar = value;
		}

		/// <summary>
		/// End-to-End encryption interface.
		/// </summary>
		public IEndToEndEncryption E2E
		{
			get => this.e2e;
			set => this.e2e = value;
		}

		private async void Client_OnRosterItemRemoved(object Sender, RosterItem Item)
		{
			try
			{
				string BareJid = Item.BareJid;
				Avatar Avatar;

				lock (this.contactAvatars)
				{
					if (this.contactAvatars.TryGetValue(BareJid, out Avatar))
						this.contactAvatars.Remove(BareJid);
					else
						Avatar = null;
				}

				if (Avatar != null && !string.IsNullOrEmpty(Avatar.ObjectId))
					await Database.Delete(Avatar);
				else
				{
					IEnumerable<Avatar> Avatars = await Database.Find<Avatar>(new FilterFieldEqualTo("BareJid", BareJid.ToLower()));
					await Database.Delete(Avatars);
				}
			}
			catch (Exception ex)
			{
				Log.Critical(ex);
			}
		}

		/// <summary>
		/// Event raised when a vCard has been received. vCards can be requested to get access to embedded Avatars.
		/// </summary>
		public event IqResultEventHandler VCardReceived = null;

		private async void Client_OnRosterItemAdded(object Sender, RosterItem Item)
		{
			try
			{
				Avatar Avatar;

				lock (this.contactAvatars)
				{
					if (!this.contactAvatars.TryGetValue(Item.BareJid, out Avatar))
						Avatar = null;
				}

				if (Avatar != null && string.IsNullOrEmpty(Avatar.ObjectId))
				{
					await Database.Insert(Avatar);

					this.AvatarAdded?.Invoke(this, new AvatarEventArgs(Item.BareJid, Avatar));
				}
				else
				{
					this.client.SendIqGet(Item.BareJid, "<vCard xmlns='vcard-temp'/>",
						(Sender2, e2) => Task.Run(() => this.ParseVCard(e2)), null);
				}
			}
			catch (Exception ex)
			{
				Log.Critical(ex);
			}
		}

		/// <summary>
		/// Parses a vCard for Avatar information.
		/// </summary>
		/// <param name="e">vCard response.</param>
		public Task ParseVCard(IqResultEventArgs e)
		{
			return this.ParseVCard(e, false);
		}

		private async Task ParseVCard(IqResultEventArgs e, bool RaiseEvent)
		{
			XmlElement E;

			try
			{
				if (e.Ok && (E = e.FirstElement) != null && E.LocalName == "vCard" && E.NamespaceURI == "vcard-temp")
				{
					Avatar Avatar = null;

					foreach (XmlNode N in E.ChildNodes)
					{
						if (N.LocalName == "PHOTO")
						{
							string ContentType = null;
							byte[] Data = null;

							foreach (XmlNode N2 in N.ChildNodes)
							{
								switch (N2.LocalName.ToUpper())
								{
									case "TYPE":
										ContentType = N2.InnerText;
										break;

									case "BINVAL":
										Data = System.Convert.FromBase64String(N2.InnerText);
										break;
								}
							}

							if (!string.IsNullOrEmpty(ContentType) && Data != null)
							{
								string BareJid = XmppClient.GetBareJID(e.From).ToLower();
								Avatar = new Avatar(BareJid, ContentType, Data);

								lock (this.contactAvatars)
								{
									this.contactAvatars[BareJid] = Avatar;
								}

								await Database.Insert(Avatar);

								this.AvatarAdded?.Invoke(this, new AvatarEventArgs(BareJid, Avatar));
							}
						}
					}

					if (RaiseEvent)
						this.VCardReceived?.Invoke(this, e);
				}
			}
			catch (Exception ex)
			{
				Log.Critical(ex);
			}
		}

		/// <summary>
		/// Checks if an avatar exists.
		/// </summary>
		/// <param name="BareJid">Bare JID of contact.</param>
		/// <returns>If an avatar with that bare JID is found.</returns>
		public bool ContainsAvatar(string BareJid)
		{
			lock (this.contactAvatars)
			{
				return this.contactAvatars.ContainsKey(BareJid.ToLower());
			}
		}

		private void Client_CustomPresenceXml(object Sender, CustomPresenceEventArgs e)
		{
			if (this.includeAvatarInPresence)
			{
				e.Stanza.Append("<x xmlns='jabber:x:avatar'><hash>");

				if (this.localAvatar != null)
					e.Stanza.Append(this.localAvatar.Hash);

				e.Stanza.Append("</hash></x>");
			}
		}

	}
}
