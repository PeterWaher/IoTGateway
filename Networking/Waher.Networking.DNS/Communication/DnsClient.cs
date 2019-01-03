using System;
using System.Collections.Generic;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.IO;
using System.Threading.Tasks;
using Waher.Events;
using Waher.Networking.DNS.Enumerations;
using Waher.Networking.DNS.ResourceRecords;
using Waher.Networking.Sniffers;
using Waher.Runtime.Cache;
using Waher.Runtime.Timing;

namespace Waher.Networking.DNS.Communication
{
	/// <summary>
	/// Abstract base class for DNS clients.
	/// </summary>
	public abstract class DnsClient : Sniffable, IDisposable
	{
		/// <summary>
		/// Default Timeout, in milliseconds (30000 ms)
		/// </summary>
		public const int DefaultTimeout = 30000;

		/// <summary>
		/// If the object has been disposed
		/// </summary>
		protected bool disposed = false;

		private readonly Dictionary<ushort, Rec> outgoingMessages = new Dictionary<ushort, Rec>();
		private readonly Random gen = new Random();
		private readonly LinkedList<byte[]> outputQueue = new LinkedList<byte[]>();
		private Scheduler scheduler;
		private bool isWriting = false;

		private class Rec
		{
			public ushort ID;
			public byte[] Output;
			public DnsMessageEventHandler Callback;
			public object State;
		}

		/// <summary>
		/// Abstract base class for DNS clients.
		/// </summary>
		public DnsClient()
		{
		}

		/// <summary>
		/// Called when DNS client is ready to be initialized.
		/// </summary>
		protected virtual void Init()
		{
			this.scheduler = new Scheduler();
		}

		/// <summary>
		/// Sends a message to a DNS server.
		/// </summary>
		/// <param name="ID">Message ID</param>
		/// <param name="Message">Encoded message</param>
		/// <param name="Callback">method to call when a response is returned.</param>
		/// <param name="State">State object to pass on to callback method.</param>
		protected async void BeginTransmit(ushort ID, byte[] Message, DnsMessageEventHandler Callback, object State)
		{
			if (this.disposed)
				return;

			if (!(Callback is null))
			{
				Rec Rec = new Rec()
				{
					ID = ID,
					Output = Message,
					Callback = Callback,
					State = State
				};

				lock (this.outgoingMessages)
				{
					this.outgoingMessages[ID] = Rec;
				}

				this.scheduler.Add(DateTime.Now.AddSeconds(2), this.CheckRetry, Rec);
			}

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

			try
			{
				while (Message != null)
				{
					this.TransmitBinary(Message);

					await this.SendAsync(Message, null);

					if (this.disposed)
						return;

					lock (this.outputQueue)
					{
						if (this.outputQueue.First is null)
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
				}
			}
			catch (Exception ex)
			{
				this.Error(ex.Message);
			}
		}

		/// <summary>
		/// Sends a message to a destination.
		/// </summary>
		/// <param name="Message">Message</param>
		/// <param name="Destination">Destination. If null, default destination
		/// is assumed.</param>
		protected abstract Task SendAsync(byte[] Message, IPEndPoint Destination);

		private void CheckRetry(object P)
		{
			Rec Rec = (Rec)P;

			lock (this.outgoingMessages)
			{
				if (!this.outgoingMessages.ContainsKey(Rec.ID))
					return;
			}

			this.BeginTransmit(Rec.ID, Rec.Output, Rec.Callback, Rec.State);
		}

		/// <summary>
		/// Processes an incoming message.
		/// </summary>
		/// <param name="Message">DNS Message</param>
		protected virtual void ProcessIncomingMessage(DnsMessage Message)
		{
			if (Message.Response)
			{
				Rec Rec;

				lock (this.outgoingMessages)
				{
					if (this.outgoingMessages.TryGetValue(Message.ID, out Rec))
						this.outgoingMessages.Remove(Message.ID);
					else
						return;
				}

				try
				{
					Rec.Callback?.Invoke(this, new DnsMessageEventArgs(Message, Rec.State));
				}
				catch (Exception ex)
				{
					Log.Critical(ex);
				}
			}
		}

		/// <summary>
		/// <see cref="IDisposable.Dispose"/>
		/// </summary>
		public virtual void Dispose()
		{
			this.disposed = true;

			this.scheduler?.Dispose();
			this.scheduler = null;
		}

		/// <summary>
		/// Sends a DNS Request
		/// </summary>
		/// <param name="OpCode">OpCode</param>
		/// <param name="Recursive">If recursive evaluation is desired.
		/// (Recursive evaluation is optional on behalf of the DNS server.</param>
		/// <param name="Questions">Questions</param>
		/// <param name="Callback">Method to call when response is returned.</param>
		/// <param name="State">State object to pass on to callback method.</param>
		public void SendRequest(OpCode OpCode, bool Recursive, Question[] Questions,
			DnsMessageEventHandler Callback, object State)
		{
			using (MemoryStream Request = new MemoryStream())
			{
				ushort ID = DnsResolver.NextID;

				DnsResolver.WriteUInt16(ID, Request);

				byte b = (byte)((int)OpCode.Query << 3);
				if (Recursive)
					b |= 1;

				Request.WriteByte(b);
				Request.WriteByte((byte)RCode.NoError);

				int c = Questions.Length;
				if (c == 0)
					throw new ArgumentException("No questions included in request.", nameof(Questions));

				if (c > ushort.MaxValue)
					throw new ArgumentException("Too many questions in request.", nameof(Questions));

				DnsResolver.WriteUInt16((ushort)c, Request);    // Query Count
				DnsResolver.WriteUInt16(0, Request);            // Answer Count
				DnsResolver.WriteUInt16(0, Request);            // Authoritative Count
				DnsResolver.WriteUInt16(0, Request);            // Additional Count

				Dictionary<string, ushort> NamePositions = new Dictionary<string, ushort>();

				foreach (Question Q in Questions)
				{
					DnsResolver.WriteName(Q.QNAME, Request, NamePositions);
					DnsResolver.WriteUInt16((ushort)Q.QTYPE, Request);
					DnsResolver.WriteUInt16((ushort)Q.QCLASS, Request);
				}

				byte[] Packet = Request.ToArray();

				this.BeginTransmit(ID, Packet, Callback, State);

			}
		}

		/// <summary>
		/// Sends a DNS Request
		/// </summary>
		/// <param name="OpCode">OpCode</param>
		/// <param name="Recursive">If recursive evaluation is desired.
		/// (Recursive evaluation is optional on behalf of the DNS server.</param>
		/// <param name="Questions">Questions</param>
		/// <param name="Timeout">Timeout, in milliseconds.</param>
		public Task<DnsMessage> SendRequestAsync(OpCode OpCode, bool Recursive, 
			Question[] Questions, int Timeout)
		{
			TaskCompletionSource<DnsMessage> Result = new TaskCompletionSource<DnsMessage>();
			DateTime TP;

			TP = this.scheduler.Add(DateTime.Now.AddMilliseconds(Timeout), (P) =>
			{
				((TaskCompletionSource<DnsMessage>)P).TrySetException(
					new TimeoutException("No response returned within the given time."));
			}, Result);


			this.SendRequest(OpCode, Recursive, Questions, (sender, e) =>
			{
				this.scheduler?.Remove(TP);

				((TaskCompletionSource<DnsMessage>)e.State).TrySetResult(e.Message);
			}, Result);

			return Result.Task;
		}

		/// <summary>
		/// Execute a DNS query.
		/// </summary>
		/// <param name="QNAME">Query Name</param>
		/// <param name="QTYPE">Query Type</param>
		/// <param name="QCLASS">Query Class</param>
		/// <param name="Callback">Method to call when response is returned.</param>
		/// <param name="State">State object to pass on to callback method.</param>
		public void Query(string QNAME, QTYPE QTYPE, QCLASS QCLASS, DnsMessageEventHandler Callback, object State)
		{
			this.Query(new Question[] { new Question(QNAME, QTYPE, QCLASS) }, Callback, State);
		}

		/// <summary>
		/// Execute a DNS query.
		/// </summary>
		/// <param name="QNAME">Query Name</param>
		/// <param name="QTYPEs">Query Types</param>
		/// <param name="QCLASS">Query Class</param>
		/// <param name="Callback">Method to call when response is returned.</param>
		/// <param name="State">State object to pass on to callback method.</param>
		public void Query(string QNAME, QTYPE[] QTYPEs, QCLASS QCLASS, DnsMessageEventHandler Callback, object State)
		{
			this.Query(ToQuestions(QNAME, QTYPEs, QCLASS), Callback, State);
		}

		/// <summary>
		/// Execute a DNS query.
		/// </summary>
		/// <param name="Questions">Questions</param>
		/// <param name="Callback">Method to call when response is returned.</param>
		/// <param name="State">State object to pass on to callback method.</param>
		public void Query(Question[] Questions, DnsMessageEventHandler Callback, object State)
		{
			this.SendRequest(OpCode.Query, false, Questions, Callback, State);
		}

		/// <summary>
		/// Execute a DNS query.
		/// </summary>
		/// <param name="QNAME">Query Name</param>
		/// <param name="QTYPE">Query Type</param>
		/// <param name="QCLASS">Query Class</param>
		public Task<DnsMessage> QueryAsync(string QNAME, QTYPE QTYPE, QCLASS QCLASS)
		{
			return this.QueryAsync(new Question[] { new Question(QNAME, QTYPE, QCLASS) });
		}

		/// <summary>
		/// Execute a DNS query.
		/// </summary>
		/// <param name="QNAME">Query Name</param>
		/// <param name="QTYPEs">Query Type</param>
		/// <param name="QCLASS">Query Class</param>
		public Task<DnsMessage> QueryAsync(string QNAME, QTYPE[] QTYPEs, QCLASS QCLASS)
		{
			return this.QueryAsync(ToQuestions(QNAME, QTYPEs, QCLASS));
		}

		/// <summary>
		/// Execute a DNS query.
		/// </summary>
		/// <param name="Questions">Questions</param>
		public Task<DnsMessage> QueryAsync(Question[] Questions)
		{
			return this.QueryAsync(Questions, DefaultTimeout);
		}

		/// <summary>
		/// Execute a DNS query.
		/// </summary>
		/// <param name="Questions">Questions</param>
		/// <param name="Timeout">Timeout, in milliseconds.</param>
		public Task<DnsMessage> QueryAsync(Question[] Questions, int Timeout)
		{
			return this.SendRequestAsync(OpCode.Query, false, Questions, Timeout);
		}

		private static Question[] ToQuestions(string QNAME, QTYPE[] QTYPEs, QCLASS QCLASS)
		{
			int i, c = QTYPEs.Length;
			Question[] Questions = new Question[c];

			for (i = 0; i < c; i++)
				Questions[i] = new Question(QNAME, QTYPEs[i], QCLASS);

			return Questions;
		}

	}
}
