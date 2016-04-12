using System;
using System.Collections.Generic;
using System.Text;

namespace Waher.Networking.XMPP
{
	/// <summary>
	/// Contains information about a pending IQ request.
	/// </summary>
	internal class PendingRequest
	{
		private IqResultEventHandler iqCallback;
		private PresenceEventHandler presenceCallback;
		private DateTime timeout;
		private string to;
		private string xml;
		private object state;
		private int retryTimeout;
		private int nrRetries;
		private int maxRetryTimeout;
		private uint seqNr;
		private bool dropOff;

		internal PendingRequest(uint SeqNr, IqResultEventHandler Callback, object State, int RetryTimeout, int NrRetries, bool DropOff, int MaxRetryTimeout, 
			string To)
		{
			this.seqNr = SeqNr;
			this.iqCallback = Callback;
			this.presenceCallback = null;
			this.state = State;
			this.retryTimeout = RetryTimeout;
			this.nrRetries = NrRetries;
			this.maxRetryTimeout = MaxRetryTimeout;
			this.dropOff = DropOff;
			this.to = To;

			this.timeout = DateTime.Now.AddMilliseconds(RetryTimeout);
		}

		internal PendingRequest(uint SeqNr, PresenceEventHandler Callback, object State, int RetryTimeout, int NrRetries, bool DropOff, int MaxRetryTimeout,
			string To)
		{
			this.seqNr = SeqNr;
			this.iqCallback = null;
			this.presenceCallback = Callback;
			this.state = State;
			this.retryTimeout = RetryTimeout;
			this.nrRetries = NrRetries;
			this.maxRetryTimeout = MaxRetryTimeout;
			this.dropOff = DropOff;
			this.to = To;

			this.timeout = DateTime.Now.AddMilliseconds(RetryTimeout);
		}

		/// <summary>
		/// Sequence number.
		/// </summary>
		public uint SeqNr { get { return this.seqNr; } }

		/// <summary>
		/// To
		/// </summary>
		public string To { get { return this.to; } }

		/// <summary>
		/// Request XML
		/// </summary>
		public string Xml
		{
			get { return this.xml; }
			internal set { this.xml = value; }
		}

		/// <summary>
		/// Callback method (for IQ stanzas) to call when a result or error is returned.
		/// </summary>
		public IqResultEventHandler IqCallback { get { return this.iqCallback; } }

		/// <summary>
		/// Callback method (for Presence stanzas) to call when a result or error is returned.
		/// </summary>
		public PresenceEventHandler PresenceCallback { get { return this.presenceCallback; } }

		/// <summary>
		/// State object passed in the original request.
		/// </summary>
		public object State { get { return this.state; } }

		/// <summary>
		/// Retry Timeout, in milliseconds.
		/// </summary>
		public int RetryTimeout { get { return this.retryTimeout; } }

		/// <summary>
		/// Number of retries (left).
		/// </summary>
		public int NrRetries { get { return this.nrRetries; } }

		/// <summary>
		/// Maximum retry timeout. Used if <see cref="DropOff"/> is true.
		/// </summary>
		public int MaxRetryTimeout { get { return this.maxRetryTimeout; } }

		/// <summary>
		/// If the retry timeout should be doubled between retries (true), or if the same retry timeout should be used for all retries.
		/// The retry timeout will never exceed <see cref="MaxRetryTieout"/>.
		/// </summary>
		public bool DropOff { get { return this.dropOff; } }

		/// <summary>
		/// When the requests times out.
		/// </summary>
		public DateTime Timeout 
		{
			get { return this.timeout; }
			internal set { this.timeout = value; } 
		}

		/// <summary>
		/// Checks if the request can be retried.
		/// </summary>
		/// <returns>If the request can be retried.</returns>
		public bool CanRetry()
		{
			if (this.nrRetries-- <= 0)
				return false;

			if (this.dropOff)
			{
				int i = this.retryTimeout * 2;
				if (i < this.retryTimeout || this.retryTimeout > this.maxRetryTimeout)
					this.retryTimeout = this.maxRetryTimeout;
				else
					this.retryTimeout = i;
			}

			this.timeout = this.timeout.AddMilliseconds(this.retryTimeout);

			return true;
		}

	}
}
