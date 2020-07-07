using System;

namespace Waher.Persistence
{
	/// <summary>
	/// Source of code flagging a collection for repair.
	/// </summary>
	public class FlagSource
	{
		private readonly string reason;
		private readonly string stackTrace;
		private int count;

		/// <summary>
		/// Source of code flagging a collection for repair.
		/// </summary>
		/// <param name="Reason">Reason for flagging collection.</param>
		/// <param name="StackTrace">Stack trace of source flagging the collection.</param>
		/// <param name="Count">Number of times the collection has been flagged from this source.</param>
		public FlagSource(string Reason, string StackTrace, int Count)
		{
			this.reason = Reason;
			this.stackTrace = StackTrace;
			this.count = Count;
		}

		/// <summary>
		/// Reason for flagging collection.
		/// </summary>
		public string Reason => this.reason;

		/// <summary>
		/// Stack trace of source flagging the collection.
		/// </summary>
		public string StackTrace => this.stackTrace;

		/// <summary>
		/// Number of times the collection has been flagged from this source.
		/// </summary>
		public int Count
		{
			get => this.count;
			internal set => this.count = value;
		}

		/// <summary>
		/// <see cref="Object.GetHashCode()"/>
		/// </summary>
		public override int GetHashCode()
		{
			int Result = this.reason.GetHashCode();
			Result ^= Result << 5 ^ this.stackTrace.GetHashCode();
			Result ^= Result << 5 ^ this.count.GetHashCode();
			return Result;
		}

		/// <summary>
		/// <see cref="Object.Equals(object)"/>
		/// </summary>
		public override bool Equals(object obj)
		{
			return (obj is FlagSource S &&
				this.reason == S.reason &&
				this.stackTrace == S.stackTrace &&
				this.count == S.count);
		}
	}
}
