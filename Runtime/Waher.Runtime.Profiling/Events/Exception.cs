using System;

namespace Waher.Runtime.Profiling.Events
{
	/// <summary>
	/// Exception occurred
	/// </summary>
	public class Exception : ProfilerEvent
	{
		private readonly System.Exception exception;

		/// <summary>
		/// Exception occurred
		/// </summary>
		/// <param name="Ticks">Elapsed ticks.</param>
		/// <param name="Exception">Exception object.</param>
		public Exception(long Ticks, System.Exception Exception)
			: base(Ticks)
		{
			this.exception = Exception;
		}

		/// <summary>
		/// Exception object.
		/// </summary>
		public System.Exception ExceptionObject => this.exception;
	}
}
