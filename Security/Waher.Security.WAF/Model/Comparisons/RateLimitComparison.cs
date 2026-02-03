using System;
using System.Threading.Tasks;
using System.Xml;
using Waher.Content;
using Waher.Content.Xml.Attributes;
using Waher.Networking.HTTP.Interfaces;
using Waher.Runtime.IO;
using Waher.Runtime.Threading;

namespace Waher.Security.WAF.Model.Comparisons
{
	/// <summary>
	/// Abstract base class for rate limit comparisons.
	/// </summary>
	public abstract class RateLimitComparison : LimitComparison
	{
		private readonly DurationAttribute duration;

		/// <summary>
		/// Abstract base class for rate limit comparisons.
		/// </summary>
		public RateLimitComparison()
			: base()
		{
		}

		/// <summary>
		/// Abstract base class for rate limit comparisons.
		/// </summary>
		/// <param name="Xml">XML definition.</param>
		/// <param name="Parent">Parent node.</param>
		/// <param name="Document">Document hosting the Web Application Firewall action.</param>
		public RateLimitComparison(XmlElement Xml, WafAction Parent, WebApplicationFirewall Document)
			: base(Xml, Parent, Document)
		{
			this.duration = new DurationAttribute(Xml, "duration");
		}

		/// <summary>
		/// Evaluates the duration of the rate limit.
		/// </summary>
		/// <param name="State">Current state.</param>
		/// <returns>Duration</returns>
		protected Task<Duration> EvaluateDurationAsync(ProcessingState State)
		{
			return this.duration.EvaluateAsync(State.Variables, Duration.Zero);
		}

		/// <summary>
		/// Reviews the processing state, and returns a WAF result, if any.
		/// </summary>
		/// <param name="State">Current state.</param>
		/// <param name="Delta">Increment of counter being reviewed.</param>
		/// <returns>Result to return, if any.</returns>
		public async Task<WafResult?> ReviewIncrement(ProcessingState State, long Delta)
		{
			string Key = State.Request.RemoteEndPoint.RemovePortNumber() + "|Count";
			long Count;

			using (Semaphore Semaphore = await Semaphores.BeginWrite(Key))
			{
				if (State.TryGetCachedObject(Key, out RateCounter Counter))
					Counter.Count += Delta;
				else
				{
					Duration Duration = await this.EvaluateDurationAsync(State);

					Counter = new RateCounter()
					{
						Count = Delta
					};

					State.AddToCache(Key, Counter, DateTime.UtcNow + Duration);
				}

				Count = Counter.Count;
			}

			return await this.Review(State, Count);
		}

		private class RateCounter
		{
			public long Count = 0;
		}
	}
}