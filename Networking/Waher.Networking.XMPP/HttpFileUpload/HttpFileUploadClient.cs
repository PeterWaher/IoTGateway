﻿using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using Waher.Content.Xml;
using Waher.Events;
using Waher.Networking.XMPP.DataForms;
using Waher.Networking.XMPP.ServiceDiscovery;

namespace Waher.Networking.XMPP.HttpFileUpload
{
	/// <summary>
	/// Class managing HTTP File uploads, as defined in XEP-0363.
	/// </summary>
	public class HttpFileUploadClient : XmppExtension
	{
		/// <summary>
		/// urn:xmpp:http:upload:0
		/// </summary>
		public const string Namespace = "urn:xmpp:http:upload:0";

		private string fileUploadJid = null;
		private long? maxFileSize = null;
		private bool hasSupport = false;

		/// <summary>
		/// Class managing HTTP File uploads, as defined in XEP-0363.
		/// </summary>
		/// <param name="Client">XMPP Client.</param>
		public HttpFileUploadClient(XmppClient Client)
			: base(Client)
		{
		}

		/// <summary>
		/// Class managing HTTP File uploads, as defined in XEP-0363.
		/// </summary>
		/// <param name="Client">XMPP Client.</param>
		/// <param name="FileUploadJid">JID to upload files to.</param>
		/// <param name="MaxFileSize">Maximum file size.</param>
		public HttpFileUploadClient(XmppClient Client, string FileUploadJid, long? MaxFileSize)
			: base(Client)
		{
			this.fileUploadJid = FileUploadJid;
			this.maxFileSize = MaxFileSize;
			this.hasSupport = !string.IsNullOrEmpty(this.fileUploadJid) && MaxFileSize.HasValue && MaxFileSize.Value > 0;
		}

		/// <summary>
		/// Implemented extensions.
		/// </summary>
		public override string[] Extensions => new string[] { "XEP-0363" };

		/// <summary>
		/// Searches for HTTP File Upload support on the current broker.
		/// </summary>
		/// <param name="Callback">Callback method to call when discovery procedure is complete.</param>
		public Task Discover(EventHandlerAsync Callback)
		{
			return this.Discover(this.client.Domain, Callback);
		}

		/// <summary>
		/// Searches for HTTP File Upload support on the current broker.
		/// </summary>
		/// <param name="Domain">Domain name of host.</param>
		/// <param name="Callback">Callback method to call when discovery procedure is complete.</param>
		public Task Discover(string Domain, EventHandlerAsync Callback)
		{
			Dictionary<string, bool> Jids = new Dictionary<string, bool>();

			return this.client.SendServiceItemsDiscoveryRequest(Domain, async (Sender, e) =>
			{
				if (e.Ok)
				{
					foreach (Item Item in e.Items)
					{
						lock (Jids)
						{
							Jids[Item.JID] = true;
						}

						await this.client.SendServiceDiscoveryRequest(Item.JID, async (sender2, e2) =>
						{
							Item Item2 = (Item)e2.State;
							bool Last;

							lock (Jids)
							{
								Jids.Remove(Item2.JID);
								Last = Jids.Count == 0;
							}

							if (e2.Ok && e2.HasFeature(Namespace))
							{
								this.fileUploadJid = Item2.JID;
								this.maxFileSize = FindMaxFileSize(this.client, e2);
								this.hasSupport = this.maxFileSize.HasValue;

								EventHandlerAsync h = Callback;
								Callback = null;
								await this.RaiseEvent(h);
							}
							else if (Last)
							{
								EventHandlerAsync h = Callback;
								Callback = null;
								await this.RaiseEvent(h);
							}
						}, Item);
					}
				}
				else
				{
					EventHandlerAsync h = Callback;
					Callback = null;
					await this.RaiseEvent(h);
				}
			}, null);
		}

		/// <summary>
		/// Searches for HTTP File Upload support on the current broker.
		/// </summary>
		public Task DiscoverAsync()
		{
			return this.DiscoverAsync(this.client.Domain);
		}

		/// <summary>
		/// Searches for HTTP File Upload support on the current broker.
		/// </summary>
		/// <param name="Domain">Domain name of host.</param>
		public async Task DiscoverAsync(string Domain)
		{
			TaskCompletionSource<bool> Wait = new TaskCompletionSource<bool>();

			await this.Discover(Domain, (Sender, e) =>
			{
				Wait.TrySetResult(true);
				return Task.CompletedTask;
			});

			await Wait.Task;
		}

		private Task RaiseEvent(EventHandlerAsync Callback)
		{
			return Callback.Raise(this, EventArgs.Empty);
		}

		/// <summary>
		/// Finds the maximum file size supported by the file upload service.
		/// </summary>
		/// <param name="Client">XMPP Client</param>
		/// <param name="e">Event arguments.</param>
		/// <returns>Maximum file size, if available in response.</returns>
		public static long? FindMaxFileSize(XmppClient Client, ServiceDiscoveryEventArgs e)
		{
			foreach (XmlNode N in e.FirstElement.ChildNodes)
			{
				if (N is XmlElement E &&
					E.LocalName == "x" &&
					E.NamespaceURI == XmppClient.NamespaceData)
				{
					DataForm Form = new DataForm(Client, E, null, null, e.From, e.To);
					Field F = Form["FORM_TYPE"];
					if (!(F is null) && F.ValueString == Namespace)
					{
						F = Form["max-file-size"];
						if (!(F is null) && long.TryParse(F.ValueString, out long l))
							return l;
					}
				}
			}

			return null;
		}

		/// <summary>
		/// If support has been found.
		/// </summary>
		public bool HasSupport => this.hasSupport;

		/// <summary>
		/// JID of HTTP File Upload component.
		/// </summary>
		public string FileUploadJid => this.fileUploadJid;

		/// <summary>
		/// Maximum file size.
		/// </summary>
		public long? MaxFileSize => this.maxFileSize;

		/// <summary>
		/// Uploads a file to the upload component.
		/// </summary>
		/// <param name="FileName">Name of file.</param>
		/// <param name="ContentType">Internet content type.</param>
		/// <param name="ContentSize">Size of content.</param>
		/// <param name="Callback">Callback method to call after process completes or fails.</param>
		/// <param name="State">State object to pass to callback method.</param>
		public Task RequestUploadSlot(string FileName, string ContentType, long ContentSize,
			EventHandlerAsync<HttpFileUploadEventArgs> Callback, object State)
		{
			return this.RequestUploadSlot(FileName, ContentType, ContentSize, true, Callback, State);
		}

		/// <summary>
		/// Uploads a file to the upload component.
		/// </summary>
		/// <param name="FileName">Name of file.</param>
		/// <param name="ContentType">Internet content type.</param>
		/// <param name="ContentSize">Size of content.</param>
		/// <param name="CheckFileSize">If the size of the file should be checked before the slot is requested.</param>
		/// <param name="Callback">Callback method to call after process completes or fails.</param>
		/// <param name="State">State object to pass to callback method.</param>
		public Task RequestUploadSlot(string FileName, string ContentType, long ContentSize, bool CheckFileSize,
			EventHandlerAsync<HttpFileUploadEventArgs> Callback, object State)
		{
			if (!this.hasSupport)
				throw new Exception("HTTP File Upload not supported.");

			if (CheckFileSize && this.maxFileSize.HasValue && ContentSize > this.maxFileSize.Value)
				throw new Exception("File too large.");

			StringBuilder Xml = new StringBuilder();

			Xml.Append("<request xmlns='");
			Xml.Append(Namespace);
			Xml.Append("' filename='");
			Xml.Append(XML.Encode(FileName));
			Xml.Append("' size='");
			Xml.Append(ContentSize.ToString());
			Xml.Append("' content-type='");
			Xml.Append(XML.Encode(ContentType));
			Xml.Append("' />");

			KeyValuePair<string, string>[] PutHeaders = null;
			string PutUrl = null;
			string GetUrl = null;

			return this.client.SendIqGet(this.fileUploadJid, Xml.ToString(), async (Sender, e) =>
			{
				XmlElement E;

				if (e.Ok && !((E = e.FirstElement) is null) && E.LocalName == "slot" && E.NamespaceURI == Namespace)
				{
					foreach (XmlNode N in E.ChildNodes)
					{
						if (N is XmlElement E2)
						{
							switch (E2.LocalName)
							{
								case "put":
									PutUrl = this.CheckEmptyDomain(XML.Attribute(E2, "url"));

									List<KeyValuePair<string, string>> Headers = new List<KeyValuePair<string, string>>();

									foreach (XmlNode N2 in E2.ChildNodes)
									{
										if (N2 is XmlElement E3 && E3.LocalName == "header")
										{
											string Name = XML.Attribute(E3, "name");
											string Value = E3.InnerText;

											Headers.Add(new KeyValuePair<string, string>(Name, Value));
										}
									}

									PutHeaders = Headers.ToArray();
									break;

								case "get":
									GetUrl = this.CheckEmptyDomain(XML.Attribute(E2, "url"));
									break;
							}
						}
					}

					if (!string.IsNullOrEmpty(PutUrl) && !string.IsNullOrEmpty(GetUrl))
						e.Ok = true;
					else
						e.Ok = false;
				}
				else
					e.Ok = false;

				HttpFileUploadEventArgs e2;

				if (this.maxFileSize.HasValue)
				{
					e2 = new HttpFileUploadEventArgs(e, this, GetUrl, PutUrl, PutHeaders,
						(int)Math.Min(this.maxFileSize.Value, HttpFileUploadEventArgs.DefaultMaxChunkSize),
						this.client.Sniffers);
				}
				else
				{
					e2 = new HttpFileUploadEventArgs(e, this, GetUrl, PutUrl, PutHeaders,
						this.client.Sniffers);
				}

				await Callback.Raise(this, e2);

			}, State);
		}

		private string CheckEmptyDomain(string Url)
		{
			int i;

			if ((i = Url.IndexOf(":///")) > 0 || (i = Url.IndexOf("://:")) > 0)
				Url = Url.Insert(i + 3, this.client.Host);

			return Url;
		}

		/// <summary>
		/// Uploads a file to the upload component.
		/// </summary>
		/// <param name="FileName">Name of file.</param>
		/// <param name="ContentType">Internet content type.</param>
		/// <param name="ContentSize">Size of content.</param>
		public Task<HttpFileUploadEventArgs> RequestUploadSlotAsync(string FileName, string ContentType, long ContentSize)
		{
			return this.RequestUploadSlotAsync(FileName, ContentType, ContentSize, true);
		}

		/// <summary>
		/// Uploads a file to the upload component.
		/// </summary>
		/// <param name="FileName">Name of file.</param>
		/// <param name="ContentType">Internet content type.</param>
		/// <param name="ContentSize">Size of content.</param>
		/// <param name="CheckFileSize">If the size of the file should be checked before the slot is requested.</param>
		public async Task<HttpFileUploadEventArgs> RequestUploadSlotAsync(string FileName, string ContentType, long ContentSize,
			bool CheckFileSize)
		{
			TaskCompletionSource<HttpFileUploadEventArgs> Result = new TaskCompletionSource<HttpFileUploadEventArgs>();

			await this.RequestUploadSlot(FileName, ContentType, ContentSize, CheckFileSize, (Sender, e) =>
			{
				Result.TrySetResult(e);
				return Task.CompletedTask;
			}, null);

			return await Result.Task;
		}
	}
}
