﻿using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using Waher.Content.Xml;
using Waher.Events;
using Waher.Networking.XMPP.Events;
using Waher.Networking.XMPP.StanzaErrors;
using Waher.Runtime.Cache;
using Waher.Security;

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
		private readonly object synchObject = new object();
		private readonly int maxBlockSize;

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

		/// <inheritdoc/>
		public override void Dispose()
		{
			this.client.UnregisterIqSetHandler("open", Namespace, this.OpenHandler, true);
			this.client.UnregisterIqSetHandler("close", Namespace, this.CloseHandler, false);
			this.client.UnregisterIqSetHandler("data", Namespace, this.DataHandler, false);
			this.client.UnregisterMessageHandler("data", Namespace, this.DataHandler, false);

			if (!(this.cache is null))
			{
				this.cache.Dispose();
				this.cache = null;
			}

			if (!(this.output is null))
			{
				OutgoingStream[] ToDispose;

				lock (this.synchObject)
				{
					ToDispose = new OutgoingStream[this.output.Count];
					this.output.Values.CopyTo(ToDispose, 0);
					this.output.Clear();
				}

				foreach (OutgoingStream Output in ToDispose)
					Output.Dispose();

				this.output = null;
			}

			base.Dispose();
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
		public Task<OutgoingStream> OpenStream(string To, int BlockSize, IEndToEndEncryption E2E)
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
		public async Task<OutgoingStream> OpenStream(string To, int BlockSize, string StreamId, IEndToEndEncryption E2E)
		{
			if (string.IsNullOrEmpty(StreamId))
				StreamId = Hashes.BinaryToString(XmppClient.GetRandomBytes(16));

			OutgoingStream Result = new OutgoingStream(this.client, To, StreamId, BlockSize, E2E);

			StringBuilder Xml = new StringBuilder();

			Xml.Append("<open xmlns='");
			Xml.Append(Namespace);
			Xml.Append("' block-size='");
			Xml.Append(BlockSize.ToString());
			Xml.Append("' sid='");
			Xml.Append(StreamId);
			Xml.Append("' stanza='iq'/>");

			await this.client.SendIqSet(To, Xml.ToString(), async (Sender, e) =>
			{
				if (e.Ok)
				{
					await Result.Opened(e);

					lock (this.synchObject)
					{
						this.output[StreamId] = Result;
					}
				}
				else
					await Result.Abort();

			}, null);

			return Result;
		}

		private async Task OpenHandler(object Sender, IqEventArgs e)
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

			ValidateStreamEventArgs e2 = new ValidateStreamEventArgs(this.client, e, StreamId, BlockSize);
			
			await this.OnOpen.Raise(this, e2, false);

			if (e2.DataCallback is null || e2.CloseCallback is null)
				throw new NotAcceptableException("Stream not expected.", e.IQ);

			this.AssertCacheCreated();

			string Key = e.From + " " + StreamId;

			if (this.cache.ContainsKey(Key))
				throw new NotAcceptableException("Stream already open.", e.IQ);

			IncomingStream Input = new IncomingStream(e2.DataCallback, e2.CloseCallback, e2.State, BlockSize);

			this.cache[Key] = Input;

			await e.IqResult(string.Empty);
		}

		private async Task CloseHandler(object Sender, IqEventArgs e)
		{
			string StreamId = XML.Attribute(e.Query, "sid");
			string Key = e.From + " " + StreamId;
			IncomingStream Input;
			OutgoingStream Output;

			lock (this.synchObject)
			{
				if (this.cache is null)
					Input = null;
				else if (this.cache.TryGetValue(Key, out Input))
					this.cache.Remove(Key);
				else
					Input = null;
			}

			if (Input is null)
			{
				lock (this.synchObject)
				{
					if (!this.output.TryGetValue(StreamId, out Output))
						throw new ItemNotFoundException("Stream not recognized.", e.Query);

					this.output.Remove(StreamId);
				}

				await Output.Abort();
				Output.Dispose();
			}
			else
			{
				if (Input.BlocksMissing)
					await Input.Closed(CloseReason.Aborted);
				else
					await Input.Closed(CloseReason.Done);

				Input.Dispose();
			}

			await e.IqResult(string.Empty);
		}

		private void AssertCacheCreated()
		{
			lock (this.synchObject)
			{
				if (this.cache is null)
				{
					this.cache = new Cache<string, IncomingStream>(1000, TimeSpan.MaxValue, TimeSpan.FromMinutes(1), true);
					this.cache.Removed += this.Cache_Removed;
				}
			}
		}

		private Task Cache_Removed(object Sender, CacheItemEventArgs<string, IncomingStream> e)
		{
			lock (this.synchObject)
			{
				if (!(this.cache is null) && this.cache.Count == 0)
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

			return Task.CompletedTask;
		}

		/// <summary>
		/// Event raised when a remote entity tries to open an in-band bytestream for transmission of data to the client.
		/// A stream has to be accepted before data can be successfully received.
		/// </summary>
		public event EventHandlerAsync<ValidateStreamEventArgs> OnOpen = null;

		private async Task DataHandler(object Sender, IqEventArgs e)
		{
			if (await this.HandleIncomingData(e.Query, e.From))
				await e.IqResult(string.Empty);
			else
				throw new ItemNotFoundException("Stream not recognized.", e.Query);
		}

		private Task DataHandler(object Sender, MessageEventArgs e)
		{
			return this.HandleIncomingData(e.Content, e.From);
		}

		private async Task<bool> HandleIncomingData(XmlElement Data, string From)
		{
			if (Data is null)
				return false;

			int Seq = XML.Attribute(Data, "seq", 0);
			string StreamId = XML.Attribute(Data, "sid");
			string Key = From + " " + StreamId;
			IncomingStream Input;

			lock (this.synchObject)
			{
				if (this.cache is null)
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

				await this.client.SendIqSet(From, Xml.ToString(), null, null);

				return false;
			}

			await Input.DataReceived(true, Bin, Seq);

			return true;
		}

	}
}
