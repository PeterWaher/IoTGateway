using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using Waher.Content.Images;
using Waher.Content.Xml;
using Waher.Events;
using Waher.Networking.XMPP.Events;
using Waher.Networking.XMPP.PEP;
using Waher.Networking.XMPP.PEP.Events;
using Waher.Persistence;
using Waher.Persistence.Filters;
using Waher.Runtime.IO;
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
		private readonly Dictionary<string, bool> requestingAvatar = new Dictionary<string, bool>();
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

			Client.OnStateChanged += this.Client_OnStateChanged;
			Client.OnPresence += this.Client_OnPresence;
			Client.OnRosterItemRemoved += this.Client_OnRosterItemRemoved;
			Client.OnRosterItemAdded += this.Client_OnRosterItemAdded;
			Client.CustomPresenceXml += this.Client_CustomPresenceXml;

			if (!(this.pep is null))
				this.pep.OnUserAvatarMetaData += this.Pep_OnUserAvatarMetaData;

			byte[] Bin = Resources.LoadResource(typeof(AvatarClient).Namespace + ".Images.DefaultAvatar.png",
				typeof(AvatarClient).Assembly);

			this.defaultAvatar = new Avatar(Client.BareJID.ToLower(), ImageCodec.ContentTypePng, Bin, 64, 64);

			Task.Run(() => this.LoadAvatar());
		}

		/// <summary>
		/// Disposes of the extension.
		/// </summary>
		public override void Dispose()
		{
			this.client.UnregisterIqGetHandler("query", "jabber:iq:avatar", this.QueryAvatarHandler, false);

			this.client.OnStateChanged -= this.Client_OnStateChanged;
			this.client.OnPresence -= this.Client_OnPresence;
			this.client.OnRosterItemRemoved -= this.Client_OnRosterItemRemoved;
			this.client.OnRosterItemAdded -= this.Client_OnRosterItemAdded;
			this.client.CustomPresenceXml -= this.Client_CustomPresenceXml;

			if (!(this.pep is null))
				this.pep.OnUserAvatarMetaData -= this.Pep_OnUserAvatarMetaData;

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
							Log.Exception(ex);
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
		/// <param name="Width">Width of avatar, in pixels. 0 = Not known.</param>
		/// <param name="Height">Height of avatar, in pixels. 0 = Not known.</param>
		/// <param name="StoreOnBroker">If the avatar should be stored on the broker.</param>
		public async Task UpdateLocalAvatarAsync(string ContentType, byte[] Binary, int Width, int Height, bool StoreOnBroker)
		{
			if (this.localAvatar is null)
			{
				this.localAvatar = new Avatar(this.client.BareJID.ToLower(), ContentType, Binary, Width, Height);
				await Database.Insert(this.localAvatar);
			}
			else
			{
				this.localAvatar.Binary = Binary;
				this.localAvatar.ContentType = ContentType;
				this.localAvatar.Hash = Binary is null ? string.Empty : Hashes.ComputeSHA1HashString(Binary);

				await Database.Update(this.localAvatar);
			}

			if (!(this.client is null) && this.client.State == XmppState.Connected)
			{
				if (StoreOnBroker)
				{
					StringBuilder Request = new StringBuilder();
					Request.Append("<query xmlns='storage:client:avatar'><data mimetype='");
					Request.Append(XML.Encode(ContentType));
					Request.Append("'>");

					if (!(Binary is null))
						Request.Append(Convert.ToBase64String(Binary));

					Request.Append("</data></query>");

					await this.client.SendIqSet(this.client.BareJID, Request.ToString(), null, null);
				}

				this.pep?.Publish(new UserAvatarImage()
				{
					ContentType = ContentType,
					Data = Binary,
					Width = Width,
					Height = Height
				});
			}
		}

		/// <summary>
		/// Event raised when somebody wants to access the avatar of the local client.
		/// If the remote endpoint is not authoized, the corresponding stanza exception
		/// should be thrown.
		/// </summary>
		public event EventHandlerAsync<IqEventArgs> ValidateAccess = null;

		private async Task QueryAvatarHandler(object Sender, IqEventArgs e)
		{
			if (!await this.ValidateAccess.Raise(this, e, false))
				return;

			Avatar Avatar = this.localAvatar ?? this.defaultAvatar;

			StringBuilder Response = new StringBuilder();
			Response.Append("<query xmlns='jabber:iq:avatar'><data mimetype='");
			Response.Append(XML.Encode(Avatar.ContentType));
			Response.Append("'>");
			Response.Append(Convert.ToBase64String(Avatar.Binary));
			Response.Append("</data></query>");

			await e.IqResult(Response.ToString());
		}

		private async Task Client_OnStateChanged(object Sender, XmppState NewState)
		{
			if (NewState == XmppState.StreamOpened &&
				!(this.localAvatar is null) &&
				this.localAvatar.BareJid != this.client.BareJID)
			{
				this.localAvatar.BareJid = this.client.BareJID;
				await Database.Update(this.localAvatar);
			}
		}

		private enum AvatarType
		{
			/// <summary>
			/// No avatar
			/// </summary>
			None,

			/// <summary>
			/// IQ-based avatars (XEP-0008)
			/// </summary>
			Xep0008,

			/// <summary>
			/// vCard avatar (XEP-0153)
			/// </summary>
			Xep0153
		}

		private async Task Client_OnPresence(object Sender, PresenceEventArgs e)
		{
			if (e.Type == PresenceType.Available && e.Presence.HasChildNodes)
			{
				AvatarType Type = AvatarType.None;
				string Hash = null;
				bool InMuc = false;

				foreach (XmlNode N in e.Presence.ChildNodes)
				{
					if (!(N is XmlElement E))
						continue;

					switch (E.LocalName)
					{
						case "x":
							switch (E.NamespaceURI)
							{
								case "jabber:x:avatar":
									Hash = E.InnerText;
									Type = AvatarType.Xep0008;
									break;

								case "vcard-temp:x:update":
									XmlElement E2 = E["photo"];
									if (!(E2 is null))
									{
										Hash = E2.InnerText;
										Type = AvatarType.Xep0153;
									}
									break;

								default:
									if (E.NamespaceURI.StartsWith("http://jabber.org/protocol/muc"))
										InMuc = true;
									break;
							}
							break;

						case "occupant-id":
							InMuc = true;
							break;
					}
				}

				if (Type != AvatarType.None)
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
							if (string.IsNullOrEmpty(Hash))
								this.contactAvatars.Remove(BareJID);
							else if (Avatar is null || Avatar.Hash != Hash)
							{
								if (!(Avatar is null))
									this.contactAvatars[BareJID] = null;

								LoadAvatar = true;
							}
						}
						else
						{
							if (!string.IsNullOrEmpty(Hash))
								LoadAvatar = CheckDatabase = true;
						}
					}

					if (LoadAvatar)
					{
						try
						{
							if (!(Avatar is null))
								await Database.Delete(Avatar);
							else
							{
								if (CheckDatabase)
								{
									IEnumerable<Avatar> Avatars = await Database.Find<Avatar>(new FilterFieldEqualTo("BareJid", BareJID.ToLower()));

									foreach (Avatar Avatar2 in Avatars)
									{
										if (Avatar is null && (Hash is null || Hash == Avatar2.Hash))
											Avatar = Avatar2;
										else
											await Database.Delete(Avatar2);
									}

									if (!(Avatar is null))
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
								lock (this.requestingAvatar)
								{
									if (this.requestingAvatar.ContainsKey(BareJID))
										LoadAvatar = false;
									else
										this.requestingAvatar[BareJID] = true;
								}
							}

							if (LoadAvatar)
							{
								switch (Type)
								{
									case AvatarType.Xep0008:
										if (this.e2e is null)
										{
											await this.client.SendIqGet(FullJID, "<query xmlns='jabber:iq:avatar'/>",
												this.AvatarResponse, new object[] { BareJID, Hash });
										}
										else
										{
											await this.e2e.SendIqGet(this.client, E2ETransmission.NormalIfNotE2E,
												FullJID, "<query xmlns='jabber:iq:avatar'/>", this.AvatarResponse,
												new object[] { BareJID, Hash });
										}
										break;

									case AvatarType.Xep0153:
										await this.client.SendIqGet(InMuc ? FullJID : BareJID, "<vCard xmlns='vcard-temp'/>",
											(Sender2, e2) =>
											{
												lock (this.requestingAvatar)
												{
													this.requestingAvatar.Remove(BareJID);
												}

												Task.Run(() => this.ParseVCard(e2, InMuc));

												return Task.CompletedTask;
											}, null);
										break;

									default:
										lock (this.requestingAvatar)
										{
											this.requestingAvatar.Remove(BareJID);
										}
										break;
								}
							}
						}
						catch (Exception ex)
						{
							Log.Exception(ex);
						}
					}
				}
			}
		}

		private async Task AvatarResponse(object Sender, IqResultEventArgs e2)
		{
			object[] P = (object[])e2.State;
			string BareJID = (string)P[0];
			string Hash = (string)P[1];

			lock (this.requestingAvatar)
			{
				this.requestingAvatar.Remove(BareJID);
			}

			if (e2.Ok)
				await this.ParseAvatar(BareJID, Hash, e2.FirstElement);
			else
			{
				await this.Client.SendIqGet(BareJID, "<query xmlns='storage:client:avatar'/>",
					async (sender3, e3) =>
					{
						if (e3.Ok)
							await this.ParseAvatar(BareJID, Hash, e3.FirstElement);
						else
							await this.ParseAvatar(BareJID, Hash, null);
					}, null);
			}
		}

		private async Task ParseAvatar(string BareJid, string Hash, XmlElement E)
		{
			if (!(E is null) && E.LocalName == "query" && (E.NamespaceURI == "jabber:iq:avatar" || E.NamespaceURI == "storage:client:avatar"))
			{
				XmlElement E2;
				byte[] Bin = null;
				string ContentType = null;

				foreach (XmlNode N in E.ChildNodes)
				{
					E2 = N as XmlElement;
					if (E2 is null)
						continue;

					if (E2.LocalName == "data")
					{
						ContentType = XML.Attribute(E2, "mimetype");
						try
						{
							Bin = Convert.FromBase64String(E2.InnerText.Trim());
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
				bool IsNew = true;

				lock (this.contactAvatars)
				{
					if (this.contactAvatars.TryGetValue(BareJid, out OldAvatar))
					{
						if (!(OldAvatar is null) && AreEqual(OldAvatar.Binary, Bin))
							IsNew = false;
						else
							this.contactAvatars.Remove(BareJid);
					}

					if (IsNew)
					{
						if (!(Bin is null))
						{
							if (string.IsNullOrEmpty(Hash))
								NewAvatar = new Avatar(BareJid.ToLower(), ContentType, Bin, 0, 0);
							else
								NewAvatar = new Avatar(BareJid.ToLower(), ContentType, Hash, Bin, 0, 0);

							this.contactAvatars[BareJid] = NewAvatar;
						}
						else
							this.contactAvatars[BareJid] = NewAvatar = null;
					}
					else
						NewAvatar = null;
				}

				if (!(OldAvatar is null))
				{
					if (!IsNew)
					{
						if (OldAvatar.Hash != Hash)
						{
							OldAvatar.Hash = Hash;
							await Database.Update(OldAvatar);
							return;
						}
					}

					await Database.Delete(OldAvatar);

					await this.AvatarRemoved.Raise(this, new AvatarEventArgs(BareJid, OldAvatar));
				}

				if (!(NewAvatar is null))
				{
					await Database.Insert(NewAvatar);

					await this.AvatarAdded.Raise(this, new AvatarEventArgs(BareJid, NewAvatar));
				}

				await this.AvatarUpdated.Raise(this, new AvatarEventArgs(BareJid, NewAvatar));
			}
		}

		private static bool AreEqual(byte[] Bin1, byte[] Bin2)
		{
			int i, c = Bin1.Length;
			if (Bin2.Length != c)
				return false;

			for (i = 0; i < c; i++)
			{
				if (Bin1[i] != Bin2[i])
					return false;
			}

			return true;
		}

		/// <summary>
		/// Event raised when an avatar has been removed.
		/// </summary>
		public event EventHandlerAsync<AvatarEventArgs> AvatarRemoved = null;

		/// <summary>
		/// Event raised, when an avatar has been added.
		/// </summary>
		public event EventHandlerAsync<AvatarEventArgs> AvatarAdded = null;

		/// <summary>
		/// Event raised, when an avatar has been updated (added or removed).
		/// </summary>
		public event EventHandlerAsync<AvatarEventArgs> AvatarUpdated = null;

		/// <summary>
		/// Gets an avatar.
		/// </summary>
		/// <param name="BareJid">Bare JID of avatar. If empty, the local avatar is returned. If no avatar is found, the
		/// default avatar is returned.</param>
		/// <returns>Avatar</returns>
		public async Task<Avatar> GetAvatarAsync(string BareJid)
		{
			Avatar Result;

			if (string.IsNullOrEmpty(BareJid) || string.Compare(BareJid, this.client.BareJID, true) == 0)
			{
				if (this.localAvatar is null)
					return this.defaultAvatar;
				else
					return this.localAvatar;
			}

			lock (this.contactAvatars)
			{
				if (this.contactAvatars.TryGetValue(BareJid, out Result))
					return Result ?? this.defaultAvatar;
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

		private async Task Client_OnRosterItemRemoved(object _, RosterItem Item)
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

			if (!(Avatar is null) && !string.IsNullOrEmpty(Avatar.ObjectId))
				await Database.Delete(Avatar);
			else
				await Database.Delete<Avatar>(new FilterFieldEqualTo("BareJid", BareJid.ToLower()));
		}

		/// <summary>
		/// Event raised when a vCard has been received. vCards can be requested to get access to embedded Avatars.
		/// </summary>
		public event EventHandlerAsync<IqResultEventArgs> VCardReceived = null;

		private async Task Client_OnRosterItemAdded(object _, RosterItem Item)
		{
			Avatar Avatar;

			lock (this.contactAvatars)
			{
				if (!this.contactAvatars.TryGetValue(Item.BareJid, out Avatar))
					Avatar = null;
			}

			if (!(Avatar is null) && string.IsNullOrEmpty(Avatar.ObjectId))
			{
				await Database.Insert(Avatar);

				await this.AvatarAdded.Raise(this, new AvatarEventArgs(Item.BareJid, Avatar));
			}
			else
			{
				await this.client.SendIqGet(Item.BareJid, "<vCard xmlns='vcard-temp'/>",
					(Sender2, e2) => Task.Run(() => this.ParseVCard(e2, false)), null);
			}
		}

		/// <summary>
		/// Parses a vCard for Avatar information.
		/// </summary>
		/// <param name="e">vCard response.</param>
		/// <param name="InMuc">If avatar relates to an occupant in a Multi-user chat room.</param>
		public Task ParseVCard(IqResultEventArgs e, bool InMuc)
		{
			return this.ParseVCard(e, false, InMuc);
		}

		private async Task ParseVCard(IqResultEventArgs e, bool RaiseEvent, bool InMuc)
		{
			XmlElement E;

			try
			{
				if (e.Ok && !((E = e.FirstElement) is null) && E.LocalName == "vCard" && E.NamespaceURI == "vcard-temp")
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
										Data = Convert.FromBase64String(N2.InnerText);
										break;
								}
							}

							if (!string.IsNullOrEmpty(ContentType) && !(Data is null))
							{
								string Jid = InMuc ? e.From : XmppClient.GetBareJID(e.From).ToLower();
								Avatar = new Avatar(Jid, ContentType, Data, 0, 0);

								lock (this.contactAvatars)
								{
									this.contactAvatars[Jid] = Avatar;
								}

								await Database.Insert(Avatar);

								await this.AvatarAdded.Raise(this, new AvatarEventArgs(Jid, Avatar));
							}
						}
					}

					if (RaiseEvent)
						await this.VCardReceived.Raise(this, e);
				}
			}
			catch (Exception ex)
			{
				Log.Exception(ex);
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

		private Task Client_CustomPresenceXml(object Sender, CustomPresenceEventArgs e)
		{
			if (this.includeAvatarInPresence)
			{
				e.Stanza.Append("<x xmlns='jabber:x:avatar'><hash>");

				if (!(this.localAvatar is null))
					e.Stanza.Append(this.localAvatar.Hash);

				e.Stanza.Append("</hash></x>");
			}

			return Task.CompletedTask;
		}

		private async Task Pep_OnUserAvatarMetaData(object Sender, UserAvatarMetaDataEventArgs e)
		{
			UserAvatarReference Best = null;

			foreach (UserAvatarReference Ref in e.AvatarMetaData.References)
			{
				if (Best is null ||
					Best.Type != ImageCodec.ContentTypePng ||
					Ref.Bytes > Best.Bytes)
				{
					Best = Ref;
				}
			}

			if (!(Best is null))
			{
				Avatar Avatar = await this.GetAvatarAsync(e.FromBareJID);

				if (Avatar is null || Avatar.Hash != Best.Id)
				{
					e.GetUserAvatarData(Best, async (sender2, e2) =>
					{
						if (e2.Ok && !(e2.AvatarImage is null))
						{
							EventHandlerAsync<AvatarEventArgs> h;

							if (Avatar is null || Avatar?.ObjectId is null)
							{
								Avatar = new Avatar(e.FromBareJID, e2.AvatarImage.ContentType, e2.AvatarImage.Data,
									e2.AvatarImage.Width, e2.AvatarImage.Height)
								{
									Hash = Best.Id
								};

								await Database.Insert(Avatar);

								h = this.AvatarAdded;
							}
							else
							{
								Avatar.Hash = Best.Id;
								Avatar.ContentType = e2.AvatarImage.ContentType;
								Avatar.Binary = e2.AvatarImage.Data;

								await Database.Update(Avatar);

								h = this.AvatarUpdated;
							}

							await h.Raise(this, new AvatarEventArgs(e.FromBareJID, Avatar));
						}
					}, null);
				}
			}
		}

	}
}
