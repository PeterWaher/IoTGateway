using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using Waher.Events;
using Waher.Networking.Sniffers;
using Waher.Runtime.Timing;

namespace Waher.Networking.CoAP
{
	/// <summary>
	/// CoAP client. CoAP is defined in RFC7252:
	/// https://tools.ietf.org/html/rfc7252
	/// </summary>
	public class CoapClient : Sniffable, IDisposable
	{
		/// <summary>
		/// Default CoAP port = 5683
		/// </summary>
		public const int DefaultCoapPort = 5683;

		/// <summary>
		/// DEfault CoAP over DTLS port = 5684
		/// </summary>
		public const int DefaultCoapsPort = 5684;

		internal const int ACK_TIMEOUT = 2;   // seconds
		internal const double ACK_RANDOM_FACTOR = 1.5;
		internal const int MAX_RETRANSMIT = 4;
		internal const int NSTART = 1;
		internal const int DEFAULT_LEISURE = 5;   // seconds
		internal const int PROBING_RATE = 1;  // byte/second

		private static readonly CoapOptionComparer optionComparer = new CoapOptionComparer();

		private Scheduler scheduler;
		private UdpClient udpClient = null;
		private ushort msgId = 0;
		private ulong token = 0;

		/// <summary>
		/// CoAP client. CoAP is defined in RFC7252:
		/// https://tools.ietf.org/html/rfc7252
		/// </summary>
		public CoapClient()
		{
			this.scheduler = new Scheduler(ThreadPriority.BelowNormal, "CoAP tasks");
			this.udpClient = new UdpClient();
		}

		/// <summary>
		/// <see cref="IDisposable.Dispose"/>
		/// </summary>
		public void Dispose()
		{
			if (this.scheduler != null)
			{
				this.scheduler.Dispose();
				this.scheduler = null;
			}

			if (this.udpClient != null)
			{
				this.udpClient.Dispose();
				this.udpClient = null;
			}
		}

		private byte[] Encode(CoapMessageType Type, CoapCode Code, ulong Token, ushort MessageID, byte[] Payload, params CoapOption[] Options)
		{
			MemoryStream ms = new MemoryStream(128);
			ulong Temp;
			byte b, b2;

			b = 3;
			b |= (byte)((byte)Type << 2);

			Temp = Token;
			b2 = 0;

			while (Temp > 0)
			{
				Temp >>= 8;
				b2++;
			}

			b |= (byte)(b2 << 4);
			ms.WriteByte(b);
			ms.WriteByte((byte)Code);
			ms.WriteByte((byte)(MessageID >> 8));
			ms.WriteByte((byte)MessageID);

			while (b2 > 0)
			{
				ms.WriteByte((byte)Token);
				Token >>= 8;
				b2--;
			}

			if (Options != null && Options.Length > 0)
			{
				byte[] Value;
				int LastNumber = 0;
				int Delta;
				int Length;

				Array.Sort(Options, optionComparer);

				foreach (CoapOption Option in Options)
				{
					Delta = Option.OptionNumber - LastNumber;
					LastNumber += Delta;

					Value = Option.GetValue();
					Length = Value == null ? 0 : Value.Length;

					if (Delta < 13)
						b = (byte)Delta;
					else if (Delta < 269)
						b = 13;
					else
						b = 14;

					if (Length < 13)
						b |= (byte)(Length << 4);
					else if (Length < 269)
						b |= 13 << 4;
					else
						b |= 14 << 4;

					ms.WriteByte(b);

					if (Delta >= 13)
					{
						if (Delta < 269)
							ms.WriteByte((byte)(Delta - 13));
						else
						{
							Delta -= 269;
							ms.WriteByte((byte)(Delta >> 8));
							ms.WriteByte((byte)Delta);
						}
					}

					if (Length >= 13)
					{
						if (Length < 269)
							ms.WriteByte((byte)(Length - 13));
						else
						{
							Length -= 269;
							ms.WriteByte((byte)(Length >> 8));
							ms.WriteByte((byte)Length);
						}
					}

					if (Value != null)
						ms.Write(Value, 0, Value.Length);
				}
			}

			if (Payload != null && Payload.Length > 0)
			{
				ms.WriteByte(0xff);
				ms.Write(Payload, 0, Payload.Length);
			}

			byte[] Result = ms.ToArray();

			ms.Dispose();

			return Result;
		}

		private void Transmit(IPEndPoint Destination, CoapMessageType MessageType, CoapCode Code, byte[] Payload,
			CoapResponseEventHandler Callback, object State, params CoapOption[] Options)
		{
			Message Message;
			ushort MessageID;

			lock (outgoingMessages)
			{
				do
				{
					MessageID = this.msgId++;
				}
				while (this.outgoingMessages.ContainsKey(MessageID));

				Message = new Message()
				{
					client = this,
					messageType = MessageType,
					acknowledged = MessageType == CoapMessageType.CON,
					destination = Destination,
					messageID = MessageID,
					token = this.token++,
					callback = Callback,
					state = State
				};

				if (Message.acknowledged || Message.callback != null)
					this.outgoingMessages[MessageID] = Message;
			}

			if (Message.acknowledged)
			{
				lock (this.gen)
				{
					Message.timeoutMilliseconds = (int)Math.Round(1000 * (ACK_TIMEOUT + (ACK_RANDOM_FACTOR - 1) * gen.NextDouble()));
				}
			}
			else if (Message.callback != null)
			{
				Message.timeoutMilliseconds = 1000 * ACK_TIMEOUT;
				Message.retryCount = MAX_RETRANSMIT;
			}

			Message.encoded = this.Encode(Message.messageType, Code, Message.token, MessageID, Payload, Options);

			this.SendMessage(Message);
		}

		private void SendMessage(Message Message)
		{
			lock (this.outputQueue)
			{
				if (this.isWriting)
				{
					this.outputQueue.AddLast(Message);
					return;
				}
				else
					this.isWriting = true;
			}

			this.udpClient.BeginSend(Message.encoded, Message.encoded.Length, Message.destination, this.MessageSent, Message);

			if (Message.acknowledged || Message.callback != null)
				this.scheduler.Add(DateTime.Now.AddMilliseconds(Message.timeoutMilliseconds), this.CheckRetry, Message);
		}

		private void MessageSent(IAsyncResult ar)
		{
			if (this.udpClient == null)
				return;

			try
			{
				this.udpClient.EndSend(ar);

				Message Message;

				lock (this.outputQueue)
				{
					if (this.outputQueue.First == null)
					{
						this.isWriting = false;
						Message = null;
					}
					else
					{
						Message = this.outputQueue.First.Value;
						this.outputQueue.RemoveFirst();
					}
				}

				if (Message != null)
				{
					this.udpClient.BeginSend(Message.encoded, Message.encoded.Length, Message.destination, this.MessageSent, Message);

					if (Message.acknowledged || Message.callback != null)
						this.scheduler.Add(DateTime.Now.AddMilliseconds(Message.timeoutMilliseconds), this.CheckRetry, Message);
				}
			}
			catch (Exception ex)
			{
				this.Error(ex.Message);
			}
		}

		private LinkedList<Message> outputQueue = new LinkedList<Message>();
		private Dictionary<ushort, Message> outgoingMessages = new Dictionary<ushort, Message>();
		private Random gen = new Random();
		private bool isWriting = false;

		private class Message
		{
			public CoapResponseEventHandler callback;
			public CoapClient client;
			public object state;
			public IPEndPoint destination;
			public CoapMessageType messageType;
			public ushort messageID;
			public ulong token;
			public byte[] encoded;
			public int timeoutMilliseconds;
			public int retryCount = 0;
			public bool acknowledged;
			public bool responseReceived = false;

			internal void NoResponse()
			{
				if (this.callback != null)
				{
					try
					{
						this.callback(this.client, new CoapResponseEventArgs(false, this.state));
					}
					catch (Exception ex)
					{
						Log.Critical(ex);
					}
				}
			}
		}

		private void CheckRetry(object State)
		{
			Message Message = (Message)State;
			Message Message2;
			bool Fail = false;

			if (Message.responseReceived)
				return;

			lock (this.outgoingMessages)
			{
				if (!this.outgoingMessages.TryGetValue(Message.messageID, out Message2) || Message != Message2)
					return;

				Message.retryCount++;
				if (Message.retryCount >= MAX_RETRANSMIT)
				{
					this.outgoingMessages.Remove(Message.messageID);
					Fail = true;
				}
			}

			if (Fail)
			{
				Message.NoResponse();
				return;
			}

			Message.timeoutMilliseconds *= 2;
			this.SendMessage(Message);
		}

		private void Request(IPEndPoint Destination, bool Acknowledged, CoapCode Code, byte[] Payload, 
			CoapResponseEventHandler Callback, object State, params CoapOption[] Options)
		{
			this.Transmit(Destination, Acknowledged ? CoapMessageType.CON : CoapMessageType.NON, Code, Payload, Callback, State, Options);
		}

		private void Request(IPEndPoint Destination, bool Acknowledged, CoapCode Code, byte[] Payload, params CoapOption[] Options)
		{
			this.Transmit(Destination, Acknowledged ? CoapMessageType.CON : CoapMessageType.NON, Code, Payload, null, null, Options);
		}

		private void Respond(IPEndPoint Destination, bool Acknowledged, CoapCode Code, byte[] Payload, params CoapOption[] Options)
		{
			this.Transmit(Destination, Acknowledged ? CoapMessageType.CON : CoapMessageType.NON, Code, Payload, null, null, Options);
		}

		private void ACK(IPEndPoint Destination)
		{
			this.Transmit(Destination, CoapMessageType.ACK, CoapCode.EmptyMessage, null, null, null);
		}

		private void Reset(IPEndPoint Destination)
		{
			this.Transmit(Destination, CoapMessageType.RST, CoapCode.EmptyMessage, null, null, null);
		}

		/// <summary>
		/// Performs a GET operation.
		/// </summary>
		/// <param name="Destination">Request resource from this locaton.</param>
		/// <param name="Acknowledged">If acknowledged message service is to be used.</param>
		/// <param name="Callback">Callback method to call when response is returned.</param>
		/// <param name="State">State object to pass on to callback method.</param>
		/// <param name="Options">CoAP options to include in the request.</param>
		public void GET(IPEndPoint Destination, bool Acknowledged, CoapResponseEventHandler Callback, object State, params CoapOption[] Options)
		{
			this.Request(Destination, Acknowledged, CoapCode.GET, null, Callback, State, Options);
		}

		private void PUT()
		{
		}

		private void POST()
		{
		}

		private void DELETE()
		{
		}

	}
}
