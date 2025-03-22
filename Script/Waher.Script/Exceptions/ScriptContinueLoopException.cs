using System.Collections.Generic;
using System.Runtime.ExceptionServices;
using Waher.Script.Abstraction.Elements;

namespace Waher.Script.Exceptions
{
	/// <summary>
	/// Exception used to continue a loop.
	/// </summary>
	public class ScriptContinueLoopException : ScriptException 
	{
		private static readonly LinkedList<ScriptContinueLoopException> reused = new LinkedList<ScriptContinueLoopException>();

		private IElement loopValue;
		private bool hasLoopValue;

		/// <summary>
		/// Exception used to continue a loop.
		/// </summary>
		/// <param name="LoopValue">Value to include in the loop's result.</param>
		public ScriptContinueLoopException(IElement LoopValue)
			: base(string.Empty)
		{
            this.loopValue = LoopValue;
			this.hasLoopValue = true;
		}

		/// <summary>
		/// Exception used to continue a loop.
		/// </summary>
		public ScriptContinueLoopException()
			: base(string.Empty)
		{
			this.loopValue = null;
			this.hasLoopValue = false;
		}

		/// <summary>
		/// Value to include in the loop's result.
		/// </summary>
		public IElement LoopValue => this.loopValue;

		/// <summary>
		/// If <see cref="LoopValue"/> contains a value that should be included in the loop's result.
		/// </summary>
		public bool HasLoopValue => this.hasLoopValue;

		/// <summary>
		/// The <see cref="ScriptBreakLoopException"/> does not capture the stack trace.
		/// </summary>
		public override string StackTrace => string.Empty;

		/// <summary>
		/// Tries to throw a reused exception, to avoid the capture of a new stack trace.
		/// </summary>
		/// <returns></returns>
		public static bool TryThrowReused()
		{
			lock (reused)
			{
				if (reused.First is null)
					return false;

				ScriptContinueLoopException ex = reused.First.Value;
				reused.RemoveFirst();

				ex.loopValue = null;
				ex.hasLoopValue = false;

				ExceptionDispatchInfo.Capture(ex).Throw();
				return true;
			}
		}

		/// <summary>
		/// Tries to throw a reused exception, to avoid the capture of a new stack trace.
		/// </summary>
		/// <param name="LoopValue">Value to include in the loop's result.</param>
		/// <returns>If not able to throw a reused object, false will be returned.</returns>
		public static bool TryThrowReused(IElement LoopValue)
		{
			lock (reused)
			{
				if (reused.First is null)
					return false;

				ScriptContinueLoopException ex = reused.First.Value;
				reused.RemoveFirst();

				ex.loopValue = LoopValue;
				ex.hasLoopValue = true;

				ExceptionDispatchInfo.Capture(ex).Throw();
				return true;
			}
		}

		/// <summary>
		/// Reuses an exception object, to avoid capture of stack traces
		/// </summary>
		/// <param name="ex">Exception object.</param>
		public static void Reuse(ScriptContinueLoopException ex)
		{
			lock (reused)
			{
				reused.AddFirst(ex);
			}
		}
	}
}
