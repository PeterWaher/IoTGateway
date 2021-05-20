using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Waher.Events;
using Waher.Networking.DNS.Enumerations;
using Waher.Networking.DNS.ResourceRecords;
using Waher.Networking.Sniffers;
using Waher.Runtime.Inventory;
using Waher.Runtime.Profiling;
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

		private static object idnMapping = null;
		private static MethodInfo getAscii = null;
		private static MethodInfo getUnicode = null;
		private static bool initialized = false;

		private readonly Dictionary<ushort, Rec> outgoingMessages = new Dictionary<ushort, Rec>();
		private readonly LinkedList<KeyValuePair<byte[], IPEndPoint>> outputQueue = new LinkedList<KeyValuePair<byte[], IPEndPoint>>();
		private ProfilerThread thread = null;
		private Scheduler scheduler;
		private bool isWriting = false;

		private class Rec
		{
			public ushort ID;
			public byte[] Output;
			public IPEndPoint Destination;
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
		/// Optional thread for profiling.
		/// </summary>
		public ProfilerThread Thread
		{
			get => this.thread;
			set => this.thread = value;
		}

		/// <summary>
		/// Sends a message to a DNS server.
		/// </summary>
		/// <param name="ID">Message ID</param>
		/// <param name="Message">Encoded message</param>
		/// <param name="Destination">Destination. If null, default destination is assumed.</param>
		/// <param name="Callback">method to call when a response is returned.</param>
		/// <param name="State">State object to pass on to callback method.</param>
		protected async void BeginTransmit(ushort ID, byte[] Message, IPEndPoint Destination, DnsMessageEventHandler Callback, object State)
		{
			if (this.disposed)
				return;

			if (!(Callback is null))
			{
				Rec Rec = new Rec()
				{
					ID = ID,
					Output = Message,
					Destination = Destination,
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
					this.outputQueue.AddLast(new KeyValuePair<byte[], IPEndPoint>(Message, Destination));
					return;
				}
				else
					this.isWriting = true;
			}

			try
			{
				while (!(Message is null))
				{
					this.thread?.Event("Tx");
					this.TransmitBinary(Message);

					await this.SendAsync(Message, Destination);

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
							Message = this.outputQueue.First.Value.Key;
							Destination = this.outputQueue.First.Value.Value;
							this.outputQueue.RemoveFirst();
						}
					}
				}
			}
			catch (Exception ex)
			{
				ex = Log.UnnestException(ex);
				this.thread?.Exception(ex);
				this.Exception(ex);
			}
		}

		/// <summary>
		/// Sends a message to a destination.
		/// </summary>
		/// <param name="Message">Message</param>
		/// <param name="Destination">Destination. If null, default destination is assumed.</param>
		protected abstract Task SendAsync(byte[] Message, IPEndPoint Destination);

		private void CheckRetry(object P)
		{
			Rec Rec = (Rec)P;

			lock (this.outgoingMessages)
			{
				if (!this.outgoingMessages.ContainsKey(Rec.ID))
					return;
			}

			this.BeginTransmit(Rec.ID, Rec.Output, Rec.Destination, Rec.Callback, Rec.State);
		}

		/// <summary>
		/// Processes an incoming message.
		/// </summary>
		/// <param name="Message">DNS Message</param>
		protected virtual void ProcessIncomingMessage(DnsMessage Message)
		{
			this.thread?.Event("Rx");

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
					ex = Log.UnnestException(ex);
					this.thread?.Exception(ex);
					Log.Critical(ex);
				}
			}
		}

		/// <summary>
		/// Request resulted in a failure.
		/// </summary>
		/// <param name="ID">Original request ID.</param>
		protected virtual void ProcessMessageFailure(ushort ID)
		{
			Rec Rec;

			lock (this.outgoingMessages)
			{
				if (this.outgoingMessages.TryGetValue(ID, out Rec))
					this.outgoingMessages.Remove(ID);
				else
					return;
			}

			try
			{
				DnsMessage Message = new DnsMessage(new byte[]
				{
					(byte)(ID >> 8),
					(byte)(ID & 255),
					0x80,	// Response
					(byte)RCode.ServFail,
					0, 0,	// QDCOUNT
					0, 0,	// ANCOUNT
					0, 0,	// NSCOUNT
					0, 0	// ARCOUNT
				});

				Rec.Callback?.Invoke(this, new DnsMessageEventArgs(Message, Rec.State));
			}
			catch (Exception ex)
			{
				Log.Critical(ex);
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
		/// <param name="Destination">Destination. If null, default destination is assumed.</param>
		/// <param name="Callback">Method to call when response is returned.</param>
		/// <param name="State">State object to pass on to callback method.</param>
		public void SendRequest(OpCode OpCode, bool Recursive, Question[] Questions,
			IPEndPoint Destination, DnsMessageEventHandler Callback, object State)
		{
			using (MemoryStream Request = new MemoryStream())
			{
				ushort ID = DnsResolver.NextID;

				DnsClient.WriteUInt16(ID, Request);

				byte b = (byte)((int)OpCode << 3);
				if (Recursive)
					b |= 1;

				Request.WriteByte(b);
				Request.WriteByte((byte)RCode.NoError);

				int c = Questions.Length;
				if (c == 0)
					throw new ArgumentException("No questions included in request.", nameof(Questions));

				if (c > ushort.MaxValue)
					throw new ArgumentException("Too many questions in request.", nameof(Questions));

				DnsClient.WriteUInt16((ushort)c, Request);    // Query Count
				DnsClient.WriteUInt16(0, Request);            // Answer Count
				DnsClient.WriteUInt16(0, Request);            // Authoritative Count
				DnsClient.WriteUInt16(0, Request);            // Additional Count

				Dictionary<string, ushort> NamePositions = new Dictionary<string, ushort>();

				foreach (Question Q in Questions)
				{
					DnsClient.WriteName(Q.QNAME, Request, NamePositions);
					DnsClient.WriteUInt16((ushort)Q.QTYPE, Request);
					DnsClient.WriteUInt16((ushort)Q.QCLASS, Request);
				}

				byte[] Packet = Request.ToArray();

				this.BeginTransmit(ID, Packet, Destination, Callback, State);
			}
		}

		/// <summary>
		/// Sends a DNS Request
		/// </summary>
		/// <param name="OpCode">OpCode</param>
		/// <param name="Recursive">If recursive evaluation is desired.
		/// (Recursive evaluation is optional on behalf of the DNS server.</param>
		/// <param name="Questions">Questions</param>
		/// <param name="Destination">Destination. If null, default destination is assumed.</param>
		/// <param name="Timeout">Timeout, in milliseconds.</param>
		public Task<DnsMessage> SendRequestAsync(OpCode OpCode, bool Recursive,
			Question[] Questions, IPEndPoint Destination, int Timeout)
		{
			TaskCompletionSource<DnsMessage> Result = new TaskCompletionSource<DnsMessage>();
			DateTime TP = DateTime.MinValue;

			this.SendRequest(OpCode, Recursive, Questions, Destination, (sender, e) =>
			{
				this.scheduler?.Remove(TP);

				((TaskCompletionSource<DnsMessage>)e.State).TrySetResult(e.Message);
			}, Result);

			TP = DateTime.Now.AddMilliseconds(Timeout);
			TP = this.scheduler.Add(TP, (P) =>
			{
				((TaskCompletionSource<DnsMessage>)P).TrySetException(
					new TimeoutException("No DNS response returned within the given time."));
			}, Result);

			return Result.Task;
		}

		/// <summary>
		/// Execute a DNS query.
		/// </summary>
		/// <param name="QNAME">Query Name</param>
		/// <param name="QTYPE">Query Type</param>
		/// <param name="QCLASS">Query Class</param>
		/// <param name="Destination">Destination. If null, default destination is assumed.</param>
		/// <param name="Callback">Method to call when response is returned.</param>
		/// <param name="State">State object to pass on to callback method.</param>
		public void Query(string QNAME, QTYPE QTYPE, QCLASS QCLASS, IPEndPoint Destination, DnsMessageEventHandler Callback, object State)
		{
			this.Query(new Question[] { new Question(QNAME, QTYPE, QCLASS) }, Destination, Callback, State);
		}

		/// <summary>
		/// Execute a DNS query.
		/// </summary>
		/// <param name="QNAME">Query Name</param>
		/// <param name="QTYPEs">Query Types</param>
		/// <param name="QCLASS">Query Class</param>
		/// <param name="Destination">Destination. If null, default destination is assumed.</param>
		/// <param name="Callback">Method to call when response is returned.</param>
		/// <param name="State">State object to pass on to callback method.</param>
		public void Query(string QNAME, QTYPE[] QTYPEs, QCLASS QCLASS, IPEndPoint Destination, DnsMessageEventHandler Callback, object State)
		{
			this.Query(ToQuestions(QNAME, QTYPEs, QCLASS), Destination, Callback, State);
		}

		/// <summary>
		/// Execute a DNS query.
		/// </summary>
		/// <param name="Questions">Questions</param>
		/// <param name="Destination">Destination. If null, default destination is assumed.</param>
		/// <param name="Callback">Method to call when response is returned.</param>
		/// <param name="State">State object to pass on to callback method.</param>
		public void Query(Question[] Questions, IPEndPoint Destination, DnsMessageEventHandler Callback, object State)
		{
			this.SendRequest(OpCode.Query, false, Questions, Destination, Callback, State);
		}

		/// <summary>
		/// Execute a DNS query.
		/// </summary>
		/// <param name="QNAME">Query Name</param>
		/// <param name="QTYPE">Query Type</param>
		/// <param name="QCLASS">Query Class</param>
		/// <param name="Destination">Destination. If null, default destination is assumed.</param>
		public Task<DnsMessage> QueryAsync(string QNAME, QTYPE QTYPE, QCLASS QCLASS, IPEndPoint Destination)
		{
			return this.QueryAsync(new Question[] { new Question(QNAME, QTYPE, QCLASS) }, Destination);
		}

		/// <summary>
		/// Execute a DNS query.
		/// </summary>
		/// <param name="QNAME">Query Name</param>
		/// <param name="QTYPEs">Query Type</param>
		/// <param name="QCLASS">Query Class</param>
		/// <param name="Destination">Destination. If null, default destination is assumed.</param>
		public Task<DnsMessage> QueryAsync(string QNAME, QTYPE[] QTYPEs, QCLASS QCLASS, IPEndPoint Destination)
		{
			return this.QueryAsync(ToQuestions(QNAME, QTYPEs, QCLASS), Destination);
		}

		/// <summary>
		/// Execute a DNS query.
		/// </summary>
		/// <param name="Questions">Questions</param>
		/// <param name="Destination">Destination. If null, default destination is assumed.</param>
		public Task<DnsMessage> QueryAsync(Question[] Questions, IPEndPoint Destination)
		{
			return this.QueryAsync(Questions, Destination, DefaultTimeout);
		}

		/// <summary>
		/// Execute a DNS query.
		/// </summary>
		/// <param name="Questions">Questions</param>
		/// <param name="Destination">Destination. If null, default destination is assumed.</param>
		/// <param name="Timeout">Timeout, in milliseconds.</param>
		public Task<DnsMessage> QueryAsync(Question[] Questions, IPEndPoint Destination, int Timeout)
		{
			return this.SendRequestAsync(OpCode.Query, false, Questions, Destination, Timeout);
		}

		private static Question[] ToQuestions(string QNAME, QTYPE[] QTYPEs, QCLASS QCLASS)
		{
			int i, c = QTYPEs.Length;
			Question[] Questions = new Question[c];

			for (i = 0; i < c; i++)
				Questions[i] = new Question(QNAME, QTYPEs[i], QCLASS);

			return Questions;
		}

		internal static uint ReadUInt32(Stream Data)
		{
			ushort Result = ReadUInt16(Data);
			Result <<= 16;
			Result |= ReadUInt16(Data);

			return Result;
		}

		internal static ushort ReadUInt16(Stream Data)
		{
			ushort Result = (byte)Data.ReadByte();
			Result <<= 8;
			Result |= (byte)Data.ReadByte();

			return Result;
		}

		internal static string ReadName(Stream Data)
		{
			if (!initialized)
				Initialize();

			StringBuilder sb = null;
			string s;
			bool Continue = true;

			while (Continue)
			{
				int Len = Data.ReadByte();
				if (Len == 0)
					break;

				switch (Len & 192)
				{
					case 0:
						byte[] Bin = new byte[Len];
						Data.Read(Bin, 0, Len);

						s = Encoding.ASCII.GetString(Bin);

						if (!(getUnicode is null))
							s = (string)getUnicode.Invoke(idnMapping, new object[] { s });

						break;

					case 192:
						ushort Offset = (byte)(Len & 63);
						Offset <<= 8;
						Offset |= (byte)(Data.ReadByte());

						long Bak = Data.Position;

						Data.Position = Offset;

						s = ReadName(Data);

						Data.Position = Bak;
						Continue = false;
						break;

					default:
						throw new NotSupportedException("Unsupported Label Type.");
				}

				if (sb is null)
					sb = new StringBuilder();
				else
					sb.Append('.');

				sb.Append(s);
			}

			return sb?.ToString() ?? string.Empty;
		}

		internal static string ReadString(Stream Data)
		{
			int Len = Data.ReadByte();
			if (Len == 0)
				return string.Empty;

			byte[] Bin = new byte[Len];
			Data.Read(Bin, 0, Len);

			return Encoding.ASCII.GetString(Bin);
		}

		internal static ResourceRecord[] ReadResourceRecords(Stream Data, ushort Count)
		{
			List<ResourceRecord> Result = new List<ResourceRecord>();
			ResourceRecord Rec;

			while (Count > 0)
			{
				Count--;
				Rec = ResourceRecord.Create(Data);

				if (!(Rec is null))
					Result.Add(Rec);
			}

			return Result.ToArray();
		}

		internal static void WriteName(string Name, Stream Output,
			Dictionary<string, ushort> NamePositions)
		{
			if (!initialized)
				Initialize();

			while (!string.IsNullOrEmpty(Name))
			{
				if (NamePositions.TryGetValue(Name, out ushort Pos))
				{
					byte b = (byte)(Pos >> 8);
					b |= 0xc0;

					Output.WriteByte(b);
					Output.WriteByte((byte)(Pos & 0xff));
					return;
				}
				else
				{
					NamePositions[Name] = (ushort)Output.Position;

					int i = Name.IndexOf('.');
					string Label;

					if (i < 0)
					{
						Label = Name;
						Name = string.Empty;
					}
					else
					{
						Label = Name.Substring(0, i);
						Name = Name.Substring(i + 1);
					}

					if (!(getAscii is null))
						Label = (string)getAscii.Invoke(idnMapping, new object[] { Label });

					Output.WriteByte((byte)Label.Length);

					byte[] Bin = Encoding.ASCII.GetBytes(Label);
					Output.Write(Bin, 0, Bin.Length);
				}
			}

			Output.WriteByte(0);
		}

		internal static void WriteUInt16(ushort Value, Stream Output)
		{
			Output.WriteByte((byte)(Value >> 8));
			Output.WriteByte((byte)Value);
		}

		private static void Initialize()
		{
			initialized = true;
			Type T = Types.GetType("System.Globalization.IdnMapping");
			if (T is null)
			{
				idnMapping = null;
				getAscii = null;
				getUnicode = null;
			}
			else
			{
				Type[] Parameters = new Type[] { typeof(string) };

				idnMapping = Activator.CreateInstance(T);
				getAscii = T.GetRuntimeMethod("GetAscii", Parameters);
				getUnicode = T.GetRuntimeMethod("GetUnicode", Parameters);
			}
		}

	}
}
