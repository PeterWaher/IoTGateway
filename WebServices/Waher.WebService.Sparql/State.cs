using System.Diagnostics;
using System.Threading.Tasks;
using Waher.IoTGateway;
using Waher.Networking.HTTP;
using Waher.Things;

namespace Waher.WebService.Sparql
{
	/// <summary>
	/// Contains the internal state of a query.
	/// </summary>
	internal class State
	{
		private readonly HttpRequest request;
		private readonly Stopwatch watch;
		private RequestOrigin origin;
		private long lastTicks = 0;
		private long parsingTicks = 0;
		private long loadDefaultTicks = 0;
		private long evaluatingTicks = 0;
		private long returningTicks = 0;

		/// <summary>
		/// Contains the internal state of a query.
		/// </summary>
		/// <param name="Request">Request performing query.</param>
		public State(HttpRequest Request)
		{
			this.request = Request;
			this.watch = new Stopwatch();
		}

		/// <summary>
		/// Starts the processing of the query.
		/// </summary>
		public void Start()
		{
			this.watch.Start();
		}

		/// <summary>
		/// Stops the processing of the query.
		/// </summary>
		public void Stop()
		{
			this.watch.Stop();
		}

		/// <summary>
		/// Total number of milliseconds since start of processing.
		/// </summary>
		public double TotalMilliseconds => ToMilliseconds(this.watch.ElapsedTicks);

		private static double ToMilliseconds(long Ticks)
		{
			return (Ticks * 1000.0) / Stopwatch.Frequency;
		}

		/// <summary>
		/// Time dedicated to parsing query.
		/// </summary>
		public double ParsingTimeMs => ToMilliseconds(this.parsingTicks);

		/// <summary>
		/// Mark query as parsed.
		/// </summary>
		public void Parsed()
		{
			long l = this.watch.ElapsedTicks;
			this.parsingTicks += (l - this.lastTicks);
			this.lastTicks = l;
		}

		/// <summary>
		/// Time dedicated to loading default data set.
		/// </summary>
		public double LoadDefaultMs => ToMilliseconds(this.loadDefaultTicks);

		/// <summary>
		/// Mark default dataset as loaded.
		/// </summary>
		public void DefaultLoaded()
		{
			long l = this.watch.ElapsedTicks;
			this.loadDefaultTicks += (l - this.lastTicks);
			this.lastTicks = l;
		}

		/// <summary>
		/// Time dedicated to evaluating query
		/// </summary>
		public double EvaluatingMs => ToMilliseconds(this.evaluatingTicks);

		/// <summary>
		/// Mark query as evaluated
		/// </summary>
		public void Evaluated()
		{
			long l = this.watch.ElapsedTicks;
			this.evaluatingTicks += (l - this.lastTicks);
			this.lastTicks = l;
		}

		/// <summary>
		/// Time dedicated to returning result
		/// </summary>
		public double ReturningMs => ToMilliseconds(this.returningTicks);

		/// <summary>
		/// Mark response as returned
		/// </summary>
		public void Returned()
		{
			long l = this.watch.ElapsedTicks;
			this.returningTicks += (l - this.lastTicks);
			this.lastTicks = l;
		}

		/// <summary>
		/// Origin of request.
		/// </summary>
		public Task<RequestOrigin> GetOrigin()
		{
			if (this.origin is null)
				this.origin = new RequestOrigin(this.request.RemoteEndPoint, null, null, null);

			return Task.FromResult(this.origin);
		}
	}
}
