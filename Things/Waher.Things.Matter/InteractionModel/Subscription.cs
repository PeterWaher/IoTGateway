using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Waher.Events;
using Waher.Things.Matter.Messaging;

namespace Waher.Things.Matter.InteractionModel
{
	/// <summary>
	/// An active subscription (Matter Core Specification, 8.5).
	/// </summary>
	public class Subscription : IDisposable
	{
		private readonly InteractionClient client;
		private SubscriptionManager manager = null;
		private Timer livenessTimer = null;
		private DateTime lastReport;
		private bool active = true;

		/// <summary>
		/// An active subscription.
		/// </summary>
		/// <param name="Client">Interaction client.</param>
		/// <param name="SubscriptionId">Subscription ID.</param>
		/// <param name="MinInterval">Minimum interval, in seconds.</param>
		/// <param name="MaxInterval">Negotiated maximum interval, in seconds.</param>
		/// <param name="AttributePaths">Attribute paths.</param>
		/// <param name="EventPaths">Event paths.</param>
		/// <param name="PrimingReport">Priming report.</param>
		internal Subscription(InteractionClient Client, uint SubscriptionId, ushort MinInterval, ushort MaxInterval,
			AttributePath[] AttributePaths, EventPath[] EventPaths, ReadResult PrimingReport)
		{
			this.client = Client;
			this.SubscriptionId = SubscriptionId;
			this.MinInterval = MinInterval;
			this.MaxInterval = MaxInterval;
			this.AttributePaths = AttributePaths;
			this.EventPaths = EventPaths;
			this.PrimingReport = PrimingReport;
			this.lastReport = DateTime.UtcNow;

			this.ScheduleLivenessCheck();
		}

		/// <summary>
		/// Subscription ID.
		/// </summary>
		public uint SubscriptionId { get; }

		/// <summary>
		/// Minimum interval, in seconds.
		/// </summary>
		public ushort MinInterval { get; }

		/// <summary>
		/// Negotiated maximum interval, in seconds.
		/// </summary>
		public ushort MaxInterval { get; }

		/// <summary>
		/// Attribute paths.
		/// </summary>
		public AttributePath[] AttributePaths { get; }

		/// <summary>
		/// Event paths.
		/// </summary>
		public EventPath[] EventPaths { get; }

		/// <summary>
		/// Priming report.
		/// </summary>
		public ReadResult PrimingReport { get; }

		/// <summary>
		/// Session on which the subscription was established.
		/// </summary>
		public SecureSession Session => this.client.Session;

		/// <summary>
		/// If the subscription is active.
		/// </summary>
		public bool IsActive => this.active;

		/// <summary>
		/// Timestamp of the last report (including empty keep-alive reports).
		/// </summary>
		public DateTime LastReport => this.lastReport;

		/// <summary>
		/// Event raised when a report is received.
		/// </summary>
		public event EventHandlerAsync<ReadResult> OnReport = null;

		/// <summary>
		/// Event raised when the subscription is lost (no report within the maximum interval, or session closed).
		/// </summary>
		public event EventHandlerAsync OnLost = null;

		/// <summary>
		/// Maximum time to wait for reports, before considering the subscription lost.
		/// </summary>
		public TimeSpan LivenessTimeout
		{
			get
			{
				return TimeSpan.FromSeconds(this.MaxInterval) +
					ExchangeManager.MaxRetransmissionTime(this.client.Session.PeerParameters, false) +
					TimeSpan.FromSeconds(5);
			}
		}

		private void ScheduleLivenessCheck()
		{
			this.livenessTimer?.Dispose();
			this.livenessTimer = new Timer(this.LivenessCheck, null, this.LivenessTimeout, Timeout.InfiniteTimeSpan);
		}

		private async void LivenessCheck(object _)
		{
			try
			{
				if (!this.active)
					return;

				if (DateTime.UtcNow - this.lastReport >= this.LivenessTimeout)
					await this.Lost();
				else
					this.ScheduleLivenessCheck();
			}
			catch (Exception ex)
			{
				Log.Exception(ex);
			}
		}

		internal SubscriptionManager Manager
		{
			get => this.manager;
			set => this.manager = value;
		}

		internal async Task Lost()
		{
			if (!this.active)
				return;

			this.active = false;
			this.livenessTimer?.Dispose();
			this.livenessTimer = null;
			this.manager?.Remove(this);

			await this.OnLost.Raise(this, EventArgs.Empty);
		}

		internal async Task ReportReceived(ReadResult Report)
		{
			this.lastReport = DateTime.UtcNow;
			this.ScheduleLivenessCheck();

			if (Report.Attributes.Count > 0 || Report.Events.Count > 0)
				await this.OnReport.Raise(this, Report);
		}

		/// <summary>
		/// Cancels the subscription. Subsequent reports will be rejected.
		/// </summary>
		public void Cancel()
		{
			this.active = false;
			this.livenessTimer?.Dispose();
			this.livenessTimer = null;
			this.manager?.Remove(this);
		}

		/// <inheritdoc/>
		public void Dispose()
		{
			this.Cancel();
		}

		/// <inheritdoc/>
		public override string ToString()
		{
			return "Subscription " + this.SubscriptionId.ToString() + " (" + this.MinInterval.ToString() + "-" + this.MaxInterval.ToString() + " s)";
		}
	}

	/// <summary>
	/// Routes incoming subscription reports to active subscriptions.
	/// </summary>
	public class SubscriptionManager : IDisposable
	{
		private readonly Dictionary<(SecureSession, uint), Subscription> subscriptions = new Dictionary<(SecureSession, uint), Subscription>();
		private readonly ExchangeManager manager;

		/// <summary>
		/// Routes incoming subscription reports to active subscriptions.
		/// </summary>
		/// <param name="Manager">Exchange manager.</param>
		public SubscriptionManager(ExchangeManager Manager)
		{
			this.manager = Manager;
			this.manager.OnUnsolicitedMessage += this.Manager_OnUnsolicitedMessage;
			this.manager.OnSessionClosed += this.Manager_OnSessionClosed;
		}

		private static (SecureSession, uint) GetKey(SecureSession Session, uint SubscriptionId)
		{
			return (Session, SubscriptionId);
		}

		internal void Register(Subscription Subscription)
		{
			Subscription.Manager = this;

			lock (this.subscriptions)
			{
				this.subscriptions[GetKey(Subscription.Session, Subscription.SubscriptionId)] = Subscription;
			}
		}

		internal void Remove(Subscription Subscription)
		{
			lock (this.subscriptions)
			{
				(SecureSession, uint) Key = GetKey(Subscription.Session, Subscription.SubscriptionId);

				if (this.subscriptions.TryGetValue(Key, out Subscription S) && S == Subscription)
					this.subscriptions.Remove(Key);
			}
		}

		/// <summary>
		/// Removes a subscription.
		/// </summary>
		/// <param name="Subscription">Subscription.</param>
		public void Unregister(Subscription Subscription)
		{
			Subscription.Cancel();
			this.Remove(Subscription);
		}

		/// <summary>
		/// Gets active subscriptions.
		/// </summary>
		/// <returns>Subscriptions.</returns>
		public Subscription[] GetSubscriptions()
		{
			lock (this.subscriptions)
			{
				Subscription[] Result = new Subscription[this.subscriptions.Count];
				this.subscriptions.Values.CopyTo(Result, 0);
				return Result;
			}
		}

		private async Task Manager_OnSessionClosed(object Sender, SecureSession Session)
		{
			List<Subscription> Lost = new List<Subscription>();

			lock (this.subscriptions)
			{
				foreach (KeyValuePair<(SecureSession, uint), Subscription> P in this.subscriptions)
				{
					if (P.Value.Session == Session)
						Lost.Add(P.Value);
				}

				foreach (Subscription S in Lost)
					this.subscriptions.Remove(GetKey(S.Session, S.SubscriptionId));
			}

			foreach (Subscription S in Lost)
				await S.Lost();
		}

		private async Task Manager_OnUnsolicitedMessage(object Sender, UnsolicitedMessageEventArgs e)
		{
			if (!e.Message.Is(ProtocolIds.InteractionModel, (byte)InteractionModelOpcode.ReportData))
				return;

			if (!(e.Exchange.Session is SecureSession Session))
				return;

			e.Handled = true;

			Exchange Exchange = e.Exchange;

			try
			{
				MatterMessage Message = await Exchange.ReceiveAsync(TimeSpan.FromSeconds(1));
				ReadResult Report = new ReadResult();
				Subscription Subscription = null;

				while (true)
				{
					InteractionClient.ParseReportData(Message.DecodeTlv(), Report, out bool MoreChunks, out bool SuppressResponse);

					if (Subscription is null && Report.SubscriptionId.HasValue)
					{
						lock (this.subscriptions)
						{
							if (!this.subscriptions.TryGetValue(GetKey(Session, Report.SubscriptionId.Value), out Subscription))
								Subscription = null;
						}
					}

					if (Subscription is null || !Subscription.IsActive)
					{
						await InteractionClient.SendStatusResponse(Exchange, ImStatusCode.InvalidSubscription);
						return;
					}

					if (MoreChunks)
					{
						await InteractionClient.SendStatusResponse(Exchange, ImStatusCode.Success);
						Message = await Exchange.ReceiveAsync(this.manager.GetResponseTimeout(Session, TimeSpan.FromSeconds(5)));
						InteractionClient.CheckStatusResponse(Message);
						continue;
					}

					if (!SuppressResponse)
						await InteractionClient.SendStatusResponse(Exchange, ImStatusCode.Success);

					break;
				}

				await Subscription.ReportReceived(Report);
			}
			catch (Exception ex)
			{
				Log.Exception(ex);
			}
			finally
			{
				await Exchange.CloseAsync();
			}
		}

		/// <inheritdoc/>
		public void Dispose()
		{
			this.manager.OnUnsolicitedMessage -= this.Manager_OnUnsolicitedMessage;
			this.manager.OnSessionClosed -= this.Manager_OnSessionClosed;

			Subscription[] ToCancel;

			lock (this.subscriptions)
			{
				ToCancel = new Subscription[this.subscriptions.Count];
				this.subscriptions.Values.CopyTo(ToCancel, 0);
				this.subscriptions.Clear();
			}

			foreach (Subscription S in ToCancel)
				S.Cancel();
		}
	}
}
