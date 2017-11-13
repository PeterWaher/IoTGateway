using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;
using Waher.Content.Xml;
using Waher.Events;
using Waher.Networking.XMPP.StanzaErrors;
using Waher.Runtime.Cache;

namespace Waher.Networking.XMPP.InBandBytestreams
{
	/// <summary>
	/// Class sending and receiving binary streams over XMPP using XEP-0047: In-band Bytestreams:
	/// https://xmpp.org/extensions/xep-0047.html
	/// </summary>
	public class IbbClient : XmppExtension
	{
		/// <summary>
		/// http://jabber.org/protocol/ibb
		/// </summary>
		public const string Namespace = "http://jabber.org/protocol/ibb";

		private Cache<string, IncomingStream> cache = null;
		private Dictionary<string, OutgoingStream> output = new Dictionary<string, OutgoingStream>();
		private object synchObject = new object();
		private int maxBlockSize;

		/// <summary>
		/// Class sending and receiving binary streams over XMPP using XEP-0047: In-band Bytestreams:
		/// https://xmpp.org/extensions/xep-0047.html
		/// </summary>
		/// <param name="Client">XMPP Client</param>
		/// <param name="MaxBlockSize">Maximum block size in transfer.</param>
		public IbbClient(XmppClient Client, int MaxBlockSize)
			: base(Client)
		{
			this.maxBlockSize = MaxBlockSize;

			this.client.RegisterIqSetHandler("open", Namespace, this.OpenHandler, true);
			this.client.RegisterIqSetHandler("close", Namespace, this.CloseHandler, false);
			this.client.RegisterIqSetHandler("data", Namespace, this.DataHandler, false);
			this.client.RegisterMessageHandler("data", Namespace, this.DataHandler, false);
		}

		/// <summary>
		/// <see cref="IDisposable.Dispose"/>
		/// </summary>
		public override void Dispose()
		{
			base.Dispose();

			this.client.UnregisterIqSetHandler("open", Namespace, this.OpenHandler, true);
			this.client.UnregisterIqSetHandler("close", Namespace, this.CloseHandler, false);
			this.client.UnregisterIqSetHandler("data", Namespace, this.DataHandler, false);
			this.client.UnregisterMessageHandler("data", Namespace, this.DataHandler, false);

			if (this.cache != null)
			{
				this.cache.Dispose();
				this.cache = null;
			}

			if (this.output != null)
			{
				lock (this.output)
				{
					foreach (OutgoingStream Output in this.output.Values)
						Output.Dispose();

					this.output.Clear();
				}

				this.output = null;
			}
		}

		/// <summary>
		/// Implemented extensions.
		/// </summary>
		public override string[] Extensions => new string[] { "XEP-0047" };

		/// <summary>
		/// Opens a new binary stream.
		/// </summary>
		/// <param name="To">Recipient of stream.</param>
		/// <param name="BlockSize">Block size.</param>
		/// <param name="E2E">End-to-end encryption interface, if such is to be used.</param>
		/// <returns>Outgoing stream.</returns>
		public OutgoingStream OpenStream(string To, int BlockSize, IEndToEndEncryption E2E)
		{
			return this.OpenStream(To, BlockSize, null, E2E);
		}

		/// <summary>
		/// Opens a new binary stream.
		/// </summary>
		/// <param name="To">Recipient of stream.</param>
		/// <param name="BlockSize">Block size.</param>
		/// <param name="StreamId">Desired stream ID. If null or empty, one will be created.</param>
		/// <param name="E2E">End-to-end encryption interface, if such is to be used.</param>
		/// <returns>Outgoing stream.</returns>
		public OutgoingStream OpenStream(string To, int BlockSize, string StreamId, IEndToEndEncryption E2E)
		{
			if (string.IsNullOrEmpty(StreamId))
				StreamId = Guid.NewGuid().ToString().Replace("-", string.Empty);

			OutgoingStream Result = new OutgoingStream(this.client, To, StreamId, BlockSize, E2E);

			StringBuilder Xml = new StringBuilder();

			Xml.Append("<open xmlns='");
			Xml.Append(Namespace);
			Xml.Append("' block-size='");
			Xml.Append(BlockSize.ToString());
			Xml.Append("' sid='");
			Xml.Append(StreamId);
			Xml.Append("' stanza='iq'/>");

			this.client.SendIqSet(To, Xml.ToString(), (sender, e) =>
			{
				if (e.Ok)
				{
					Result.Opened(e);

					lock (this.output)
					{
						this.output[StreamId] = Result;
					}
				}
				else
					Result.Abort();

			}, null);

			return Result;
		}

		private void OpenHandler(object Sender, IqEventArgs e)
		{
			int BlockSize = XML.Attribute(e.Query, "block-size", 0);
			string StreamId = XML.Attribute(e.Query, "sid");
			string Stanza = XML.Attribute(e.Query, "stanza", "iq");

			if (Stanza != "message" && Stanza != "iq")
				throw new BadRequestException("Invalid stanza type.", e.IQ);

			if (BlockSize <= 0)
				throw new BadRequestException("Invalid block size.", e.IQ);

			if (BlockSize > this.maxBlockSize)
			{
				throw new ResourceConstraintException("Requested block size too large. Maximum acceptable is " +
					  this.maxBlockSize.ToString(), e.IQ);
			}

			ValidateStreamEventHandler h = this.OnOpen;
			ValidateStreamEventArgs e2 = new ValidateStreamEventArgs(this.client, e, StreamId, BlockSize);
			if (h != null)
			{
				try
				{
					h(this, e2);
				}
				catch (Exception ex)
				{
					Log.Critical(ex);
				}
			}

			if (e2.DataCallback == null || e2.CloseCallback == null)
				throw new NotAcceptableException("Stream not expected.", e.IQ);

			this.AssertCacheCreated();

			string Key = e.From + " " + StreamId;

			if (this.cache.ContainsKey(Key))
				throw new NotAcceptableException("Stream already open.", e.IQ);

			IncomingStream Input = new IncomingStream(e2.DataCallback, e2.CloseCallback, e2.State, BlockSize);

			this.cache[Key] = Input;

			e.IqResult(string.Empty);
		}

		private void CloseHandler(object Sender, IqEventArgs e)
		{
			string StreamId = XML.Attribute(e.Query, "sid");
			string Key = e.From + " " + StreamId;
			IncomingStream Input;
			OutgoingStream Output;

			lock (this.synchObject)
			{
				if (this.cache == null)
					Input = null;
				else if (this.cache.TryGetValue(Key, out Input))
					this.cache.Remove(Key);
				else
					Input = null;
			}

			if (Input == null)
			{
				lock (this.output)
				{
					if (!this.output.TryGetValue(StreamId, out Output))
						throw new ItemNotFoundException("Stream not recognized.", e.Query);

					this.output.Remove(StreamId);
				}

				Output.Abort();
				Output.Dispose();
			}
			else
			{
				if (Input.BlocksMissing)
					Input.Closed(CloseReason.Aborted);
				else
					Input.Closed(CloseReason.Done);

				Input.Dispose();
			}
		}

		private void AssertCacheCreated()
		{
			lock (this.synchObject)
			{
				if (this.cache == null)
				{
					this.cache = new Cache<string, IncomingStream>(1000, TimeSpan.MaxValue, new TimeSpan(0, 1, 0));
					this.cache.Removed += Cache_Removed;
				}
			}
		}

		private void Cache_Removed(object Sender, CacheItemEventArgs<string, IncomingStream> e)
		{
			lock (this.synchObject)
			{
				if (this.cache != null && this.cache.Count == 0)
				{
					this.cache.Dispose();
					this.cache = null;
				}
			}

			switch (e.Reason)
			{
				case RemovedReason.Manual:
					// Handled somewhere else.
					break;

				case RemovedReason.NotUsed:
				case RemovedReason.Old:
					e.Value.Closed(CloseReason.Timeout);
					break;

				case RemovedReason.Replaced:
				case RemovedReason.Space:
					e.Value.Closed(CloseReason.Aborted);
					break;
			}

			e.Value.Dispose();
		}

		/// <summary>
		/// Event raised when a remote entity tries to open an in-band bytestream for transmission of data to the client.
		/// A stream has to be accepted before data can be successfully received.
		/// </summary>
		public event ValidateStreamEventHandler OnOpen = null;

		private void DataHandler(object Sender, IqEventArgs e)
		{
			if (this.HandleIncomingData(e.Query, e.From))
				e.IqResult(string.Empty);
			else
				throw new ItemNotFoundException("Stream not recognized.", e.Query);
		}

		private void DataHandler(object Sender, MessageEventArgs e)
		{
			this.HandleIncomingData(e.Content, e.From);
		}

		private bool HandleIncomingData(XmlElement Data, string From)
		{
			if (Data == null)
				return false;

			int Seq = XML.Attribute(Data, "seq", 0);
			string StreamId = XML.Attribute(Data, "sid");
			string Key = From + " " + StreamId;
			IncomingStream Input;

			lock (this.synchObject)
			{
				if (this.cache == null)
					return false;

				if (!this.cache.TryGetValue(Key, out Input))
					return false;
			}

			if (Seq < 0x10000)
			{
				if (Seq < 0x8000)
					Seq += Input.BaseSeq;
				else if (Seq >= 0xf000)
				{
					if (!Input.UpperEnd)
					{
						Input.BaseSeq += 0x10000;
						Input.UpperEnd = true;
					}
				}
				else if (Seq < 0xa000)
					Input.UpperEnd = false;
			}

			byte[] Bin;

			try
			{
				Bin = Convert.FromBase64String(Data.InnerText);
				if (Bin.Length > Input.BlockSize)
					throw new Exception("Invalid block size");
			}
			catch (Exception)
			{
				this.cache.Remove(Key);

				StringBuilder Xml = new StringBuilder();

				Xml.Append("<close xmlns='");
				Xml.Append(Namespace);
				Xml.Append("' sid='");
				Xml.Append(StreamId);
				Xml.Append("'/>");

				this.client.SendIqSet(From, Xml.ToString(), null, null);

				return false;
			}

			Input.DataReceived(Bin, Seq);

			return true;
		}

	}
}
