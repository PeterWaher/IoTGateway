﻿using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Waher.Content.Xml;
using Waher.Events;
using Waher.Networking.XMPP.Events;
using Waher.Networking.XMPP.StanzaErrors;
using Waher.Runtime.IO;

namespace Waher.Networking.XMPP.BitsOfBinary
{
	/// <summary>
	/// Client managing bits of binary (XEP-0231):
	/// https://xmpp.org/extensions/xep-0231.html
	/// </summary>
	public class BobClient : XmppExtension
	{
		/// <summary>
		/// urn:xmpp:bob
		/// </summary>
		public const string Namespace = "urn:xmpp:bob";

		private readonly string folder;

		/// <summary>
		/// Client managing bits of binary (XEP-0231):
		/// https://xmpp.org/extensions/xep-0231.html
		/// </summary>
		/// <param name="Client">XMPP Client to use.</param>
		/// <param name="Folder">Folder to persist bits of binary data.</param>
		public BobClient(XmppClient Client, string Folder)
			: base(Client)
		{
			this.folder = Folder;

			if (!Directory.Exists(Folder))
				Directory.CreateDirectory(Folder);

			this.client.RegisterIqGetHandler("data", Namespace, this.GetData, true);
		}

		/// <inheritdoc/>
		public override void Dispose()
		{
			this.client.UnregisterIqGetHandler("data", Namespace, this.GetData, true);
			this.DeleteAll();
			base.Dispose();
		}

		/// <summary>
		/// Implemented extensions.
		/// </summary>
		public override string[] Extensions => new string[] { "XEP-0231" };

		/// <summary>
		/// Stores data for access with the Bits of binary protocol.
		/// </summary>
		/// <param name="Data">Binary data.</param>
		/// <param name="ContentType">Internet content type.</param>
		/// <returns>Content ID</returns>
		public Task<string> StoreData(byte[] Data, string ContentType)
		{
			return this.StoreData(Data, ContentType, null);
		}

		/// <summary>
		/// Stores data for access with the Bits of binary protocol.
		/// </summary>
		/// <param name="Data">Binary data.</param>
		/// <param name="ContentType">Internet content type.</param>
		/// <param name="Expires">When the bits of binary expires.
		/// 
		/// Note: The caller is responsible for removing the data at the corresponding point in time.
		/// Call <see cref="DeleteData(string)"/> or <see cref="DeleteAll"/> to delete bits of binary.</param>
		/// <returns>Content ID</returns>
		public async Task<string> StoreData(byte[] Data, string ContentType, DateTime? Expires)
		{
			string Hash = Security.Hashes.ComputeSHA256HashString(Data);
			StringBuilder Xml = new StringBuilder();

			Xml.Append("<data xmlns='");
			Xml.Append(Namespace);
			Xml.Append("' cid='");
			Xml.Append(Hash);
			Xml.Append("' type='");
			Xml.Append(XML.Encode(ContentType));

			if (Expires.HasValue)
			{
				double Seconds = (Expires.Value - DateTime.Now).TotalSeconds;
				if (Seconds > 0 && Seconds <= int.MaxValue)
				{
					Xml.Append("' max-age='");
					Xml.Append(((int)Seconds).ToString());
				}
			}

			Xml.Append("'>");
			Xml.Append(Convert.ToBase64String(Data));
			Xml.Append("</data>");

			await Files.WriteAllTextAsync(this.GetFileName(Hash), Xml.ToString(), Encoding.ASCII);

			return Hash;
		}

		private string GetFileName(string ContentId)
		{
			return Path.Combine(this.folder, ContentId + ".xml");
		}

		/// <summary>
		/// Deletes a bits of binary.
		/// </summary>
		/// <param name="ContentId">Content ID</param>
		/// <returns>If content was found and deleted.</returns>
		public bool DeleteData(string ContentId)
		{
			string FileName = this.GetFileName(ContentId);

			if (File.Exists(FileName))
			{
				File.Delete(FileName);
				return true;
			}
			else
				return false;
		}

		/// <summary>
		/// Deletes all bits of binary.
		/// </summary>
		public void DeleteAll()
		{
			Directory.Delete(this.folder, true);
		}

		private async Task GetData(object Sender, IqEventArgs e)
		{
			string ContentId = XML.Attribute(e.Query, "cid");
			string FileName = this.GetFileName(ContentId);

			if (!File.Exists(FileName))
				throw new ItemNotFoundException("Content not found.", e.Query);

			using (FileStream f = File.OpenRead(FileName))
			{
				StreamReader r = new StreamReader(f, Encoding.ASCII);
				string Xml = await r.ReadToEndAsync();
			
				await e.IqResult(Xml);
			}
		}

		/// <summary>
		/// Gets bits-of-binary data from remote endpoint.
		/// </summary>
		/// <param name="To">Address of remote endpoint.</param>
		/// <param name="ContentId">Content ID</param>
		/// <param name="Callback">Method to call when response has been returned.</param>
		/// <param name="State">State object to pass on to the callback method.</param>
		public Task GetData(string To, string ContentId, EventHandlerAsync<BitsOfBinaryEventArgs> Callback, object State)
		{
			StringBuilder Xml = new StringBuilder();

			Xml.Append("<data xmlns='");
			Xml.Append(Namespace);
			Xml.Append("' cid='");
			Xml.Append(ContentId);
			Xml.Append("'/>");

			return this.client.SendIqGet(To, Xml.ToString(), async (Sender, e) =>
			{
				await Callback.Raise(this, new BitsOfBinaryEventArgs(e));
			}, State);
		}
	}
}
