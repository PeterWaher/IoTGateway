using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Waher.Things.Matter.Messaging
{
	/// <summary>
	/// A Matter message exchange (Matter Core Specification, 4.10), providing reliable request/response
	/// semantics over the Message Reliability Protocol (MRP).
	/// </summary>
	public class Exchange : IDisposable
	{
		private readonly ExchangeManager manager;
		private readonly LinkedList<MatterMessage> inbox = new LinkedList<MatterMessage>();
		private TaskCompletionSource<bool> inboxSignal = null;
		private Exception failure = null;
		private PendingMessage pending = null;
		private uint? pendingAck = null;
		private CancellationTokenSource ackTimer = null;
		private bool closed = false;

		/// <summary>
		/// A Matter message exchange.
		/// </summary>
		/// <param name="Manager">Exchange manager.</param>
		/// <param name="Session">Session.</param>
		/// <param name="Id">Exchange ID.</param>
		/// <param name="IsInitiator">If the local node is the initiator of the exchange.</param>
		internal Exchange(ExchangeManager Manager, Session Session, ushort Id, bool IsInitiator)
		{
			this.manager = Manager;
			this.Session = Session;
			this.Id = Id;
			this.IsInitiator = IsInitiator;
		}

		/// <summary>
		/// Session of exchange.
		/// </summary>
		public Session Session { get; }

		/// <summary>
		/// Exchange ID.
		/// </summary>
		public ushort Id { get; }

		/// <summary>
		/// If the local node is the initiator of the exchange.
		/// </summary>
		public bool IsInitiator { get; }

		/// <summary>
		/// If the exchange has been closed.
		/// </summary>
		public bool IsClosed => this.closed;

		/// <summary>
		/// Exchange manager.
		/// </summary>
		public ExchangeManager Manager => this.manager;

		internal PendingMessage Pending
		{
			get => this.pending;
			set => this.pending = value;
		}

		/// <summary>
		/// Sends a message on the exchange.
		/// </summary>
		/// <param name="ProtocolId">Protocol ID.</param>
		/// <param name="Opcode">Opcode.</param>
		/// <param name="Payload">Application payload.</param>
		/// <param name="Reliable">If the message should be sent reliably (requiring acknowledgement).</param>
		public Task SendAsync(ushort ProtocolId, byte Opcode, byte[] Payload, bool Reliable = true)
		{
			if (this.closed)
				throw new InvalidOperationException("Exchange closed.");

			return this.manager.SendAsync(this, ProtocolId, Opcode, Payload, Reliable);
		}

		/// <summary>
		/// Waits until the last reliable message sent on the exchange has been acknowledged.
		/// </summary>
		/// <returns>If the message was acknowledged.</returns>
		public async Task<bool> WaitForAcknowledgementAsync()
		{
			PendingMessage Msg = this.pending;
			if (Msg is null)
				return true;

			try
			{
				return await Msg.Acked.Task;
			}
			catch (Exception)
			{
				return false;
			}
		}

		/// <summary>
		/// Sends a request and waits for the response.
		/// </summary>
		/// <param name="ProtocolId">Protocol ID.</param>
		/// <param name="Opcode">Opcode.</param>
		/// <param name="Payload">Application payload.</param>
		/// <param name="ProcessingTime">Expected processing time of the peer.</param>
		/// <returns>Response message.</returns>
		public async Task<MatterMessage> RequestAsync(ushort ProtocolId, byte Opcode, byte[] Payload, TimeSpan ProcessingTime)
		{
			await this.SendAsync(ProtocolId, Opcode, Payload, true);
			return await this.ReceiveAsync(this.manager.GetResponseTimeout(this.Session, ProcessingTime));
		}

		/// <summary>
		/// Sends a request and waits for the response, using a default expected processing time.
		/// </summary>
		/// <param name="ProtocolId">Protocol ID.</param>
		/// <param name="Opcode">Opcode.</param>
		/// <param name="Payload">Application payload.</param>
		/// <returns>Response message.</returns>
		public Task<MatterMessage> RequestAsync(ushort ProtocolId, byte Opcode, byte[] Payload)
		{
			return this.RequestAsync(ProtocolId, Opcode, Payload, ExchangeManager.DefaultProcessingTime);
		}

		/// <summary>
		/// Waits for the next message on the exchange.
		/// </summary>
		/// <param name="Timeout">Maximum time to wait.</param>
		/// <returns>Received message.</returns>
		/// <exception cref="MatterTimeoutException">If no message is received in time.</exception>
		public async Task<MatterMessage> ReceiveAsync(TimeSpan Timeout)
		{
			DateTime Limit = DateTime.UtcNow + Timeout;

			while (true)
			{
				TaskCompletionSource<bool> Signal;

				lock (this.inbox)
				{
					if (!(this.inbox.First is null))
					{
						MatterMessage Result = this.inbox.First.Value;
						this.inbox.RemoveFirst();
						return Result;
					}

					if (!(this.failure is null))
						throw this.failure;

					if (this.closed)
						throw new MatterException("Exchange closed.");

					Signal = this.inboxSignal;
					if (Signal is null || Signal.Task.IsCompleted)
						this.inboxSignal = Signal = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
				}

				TimeSpan Left = Limit - DateTime.UtcNow;
				if (Left <= TimeSpan.Zero)
					throw new MatterTimeoutException("No response received from peer within the expected time.");

				Task Delay = Task.Delay(Left);
				if (await Task.WhenAny(Signal.Task, Delay) == Delay)
				{
					lock (this.inbox)
					{
						if (this.inbox.First is null && this.failure is null)
							throw new MatterTimeoutException("No response received from peer within the expected time.");
					}
				}
			}
		}

		internal void Enqueue(MatterMessage Message)
		{
			TaskCompletionSource<bool> Signal;

			lock (this.inbox)
			{
				this.inbox.AddLast(Message);
				Signal = this.inboxSignal;
			}

			Signal?.TrySetResult(true);
		}

		internal void Fail(Exception ex)
		{
			TaskCompletionSource<bool> Signal;

			lock (this.inbox)
			{
				this.failure ??= ex;
				Signal = this.inboxSignal;
			}

			Signal?.TrySetResult(false);
		}

		internal void SetPendingAck(uint Counter)
		{
			CancellationTokenSource Prev;
			CancellationTokenSource Timer = new CancellationTokenSource();
			uint? PrevAck;

			lock (this.inbox)
			{
				PrevAck = this.pendingAck;
				Prev = this.ackTimer;
				this.pendingAck = Counter;
				this.ackTimer = Timer;
			}

			Prev?.Cancel();

			if (PrevAck.HasValue && PrevAck.Value != Counter)
				this.manager.SendStandaloneAckNoWait(this.Session, this.Id, this.IsInitiator, PrevAck.Value);

			Task.Delay(ExchangeManager.StandaloneAckTimeout, Timer.Token).ContinueWith(async T =>
			{
				if (T.IsCanceled)
					return;

				uint? Ack = this.TakePendingAck(Timer);
				if (Ack.HasValue)
					await this.manager.SendStandaloneAck(this.Session, this.Id, this.IsInitiator, Ack.Value);
			}, TaskScheduler.Default);
		}

		internal uint? TakePendingAck()
		{
			return this.TakePendingAck(null);
		}

		private uint? TakePendingAck(CancellationTokenSource ExpectedTimer)
		{
			CancellationTokenSource Timer;
			uint? Result;

			lock (this.inbox)
			{
				if (!(ExpectedTimer is null) && ExpectedTimer != this.ackTimer)
					return null;

				Result = this.pendingAck;
				Timer = this.ackTimer;
				this.pendingAck = null;
				this.ackTimer = null;
			}

			if (ExpectedTimer is null)
				Timer?.Cancel();

			return Result;
		}

		/// <summary>
		/// Closes the exchange, flushing any pending acknowledgement.
		/// </summary>
		public async Task CloseAsync()
		{
			if (this.closed)
				return;

			this.closed = true;

			uint? Ack = this.TakePendingAck();
			if (Ack.HasValue)
			{
				try
				{
					await this.manager.SendStandaloneAck(this.Session, this.Id, this.IsInitiator, Ack.Value);
				}
				catch (Exception)
				{
					// Ignore
				}
			}

			this.manager.ExchangeClosed(this);
			this.Fail(new MatterException("Exchange closed."));
		}

		/// <inheritdoc/>
		public void Dispose()
		{
			Task _ = this.CloseAsync();
		}

		/// <inheritdoc/>
		public override string ToString()
		{
			return "Exchange " + this.Id.ToString() + (this.IsInitiator ? "i" : "r");
		}
	}

	/// <summary>
	/// A reliable message awaiting acknowledgement.
	/// </summary>
	internal class PendingMessage
	{
		public PendingMessage(uint Counter, byte[] Datagram)
		{
			this.Counter = Counter;
			this.Datagram = Datagram;
		}

		public uint Counter { get; }
		public byte[] Datagram { get; }
		public TaskCompletionSource<bool> Acked { get; } = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
		public CancellationTokenSource Cancel { get; } = new CancellationTokenSource();
	}
}
