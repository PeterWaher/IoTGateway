using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Text;
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
	public class HttpFileUploadClient
	{
		/// <summary>
		/// urn:xmpp:http:upload:0
		/// </summary>
		public const string Namespace = "urn:xmpp:http:upload:0";

		private XmppClient client;
		private string fileUploadJid = null;
		private long? maxFileSize = null;
		private bool hasSupport = false;

		/// <summary>
		/// Class managing HTTP File uploads, as defined in XEP-0363.
		/// </summary>
		/// <param name="Client">XMPP Client.</param>
		public HttpFileUploadClient(XmppClient Client)
		{
			this.client = Client;
		}

		/// <summary>
		/// Class managing HTTP File uploads, as defined in XEP-0363.
		/// </summary>
		/// <param name="Client">XMPP Client.</param>
		/// <param name="FileUploadJid">JID to upload files to.</param>
		/// <param name="MaxFileSize">Maximum file size.</param>
		public HttpFileUploadClient(XmppClient Client, string FileUploadJid, long? MaxFileSize)
		{
			this.client = Client;
			this.fileUploadJid = FileUploadJid;
			this.maxFileSize = MaxFileSize;
			this.hasSupport = !string.IsNullOrEmpty(this.fileUploadJid);
		}

		/// <summary>
		/// Searches for HTTP File Upload support on the current broker.
		/// </summary>
		/// <param name="Callback">Callback method to call when discovery procedure is complete.</param>
		public void Discover(EventHandler Callback)
		{
			this.client.SendServiceItemsDiscoveryRequest(this.client.Domain, (sender, e) =>
			{
				foreach (Item Item in e.Items)
				{
					this.client.SendServiceDiscoveryRequest(Item.JID, (sender2, e2) =>
					{
						Item Item2 = (Item)e2.State;

						if (e2.Features.ContainsKey(Namespace))
						{
							foreach (XmlNode N in e2.FirstElement.ChildNodes)
							{
								if (N is XmlElement E &&
									E.LocalName == "x" &&
									E.NamespaceURI == XmppClient.NamespaceData)
								{
									DataForm Form = new DataForm(this.client, E, null, null, e.From, e.To);
									Field F = Form["FORM_TYPE"];
									if (F != null && F.ValueString == Namespace)
									{
										F = Form["max-file-size"];
										if (F != null && long.TryParse(F.ValueString, out long l))
										{
											this.fileUploadJid = Item2.JID;
											this.maxFileSize = l;
											this.hasSupport = true;

											if (Callback != null)
											{
												EventHandler h = Callback;
												Callback = null;

												try
												{
													h(this, new EventArgs());
												}
												catch (Exception ex)
												{
													Log.Critical(ex);
												}
											}

											break;
										}
									}
								}
							}

							if (!maxFileSize.HasValue)
							{
								this.hasSupport = false;

								if (Callback != null)
								{
									EventHandler h = Callback;
									Callback = null;

									try
									{
										h(this, new EventArgs());
									}
									catch (Exception ex)
									{
										Log.Critical(ex);
									}
								}
							}
						}
					}, Item);
				}
			}, null);
		}

		/// <summary>
		/// If support has been found.
		/// </summary>
		public bool HasSupport
		{
			get { return this.hasSupport; }
		}

		/// <summary>
		/// JID of HTTP File Upload component.
		/// </summary>
		public string FileUploadJid
		{
			get { return this.fileUploadJid; }
		}

		/// <summary>
		/// Maximum file size.
		/// </summary>
		public long? MaxFileSize
		{
			get { return this.maxFileSize; }
		}

		/// <summary>
		/// Uploads a file to the upload component.
		/// </summary>
		/// <param name="FileName">Name of file.</param>
		/// <param name="ContentType">Internet content type.</param>
		/// <param name="ContentSize">Size of content.</param>
		/// <param name="Callback">Callback method to call after process completes or fails.</param>
		/// <param name="State">State object to pass to callback method.</param>
		public void RequestUploadSlot(string FileName, string ContentType, long ContentSize,
			HttpFileUploadEventHandler Callback, object State)
		{
			if (!this.hasSupport)
				throw new Exception("HTTP File Upload not supported.");

			if (this.maxFileSize.HasValue && ContentSize > this.maxFileSize.Value)
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

			this.client.SendIqGet(this.fileUploadJid, Xml.ToString(), (sender, e) =>
			{
				XmlElement E;

				if (e.Ok && (E = e.FirstElement) != null && E.LocalName == "slot" && E.NamespaceURI == Namespace)
				{
					foreach (XmlNode N in E.ChildNodes)
					{
						if (N is XmlElement E2)
						{
							switch (E2.LocalName)
							{
								case "put":
									PutUrl = XML.Attribute(E2, "url");

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
									GetUrl = XML.Attribute(E2, "url");
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

				if (Callback != null)
				{
					try
					{
						Callback(this, new HttpFileUploadEventArgs(e, GetUrl, PutUrl, PutHeaders));
					}
					catch (Exception ex)
					{
						Log.Critical(ex);
					}
				}

			}, State);

		}

	}
}
