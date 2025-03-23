using System.Collections.Generic;
using System.Runtime.ExceptionServices;
using Waher.Script.Abstraction.Elements;

namespace Waher.Script.Exceptions
{
	/// <summary>
	/// Script runtime exception.
	/// </summary>
	public class ScriptReturnValueException : ScriptException 
	{
		private static readonly LinkedList<ScriptReturnValueException> reused = new LinkedList<ScriptReturnValueException>();

		private readonly IElement returnValue;

		/// <summary>
		/// Script runtime exception.
		/// </summary>
        /// <param name="ReturnValue">Return value.</param>
		public ScriptReturnValueException(IElement ReturnValue)
			: base(string.Empty)
		{
            this.returnValue = ReturnValue;
		}

		/// <summary>
		/// Return value.
		/// </summary>
		public IElement ReturnValue => this.returnValue;

		///// <summary>
		///// The <see cref="ScriptBreakLoopException"/> does not capture the stack trace.
		///// </summary>
		//public override string StackTrace => string.Empty;
		//
		///// <summary>
		///// Tries to throw a reused exception, to avoid the capture of a new stack trace.
		///// </summary>
		///// <param name="ReturnValue">Return value.</param>
		///// <returns>If not able to throw a reused object, false will be returned.</returns>
		//public static bool TryThrowReused(IElement ReturnValue)
		//{
		//	lock (reused)
		//	{
		//		if (reused.First is null)
		//			return false;
		//
		//		ScriptReturnValueException ex = reused.First.Value;
		//		reused.RemoveFirst();
		//
		//		ex.returnValue = ReturnValue;
		//
		//		ExceptionDispatchInfo.Capture(ex).Throw();
		//		return true;
		//	}
		//}
		//
		///// <summary>
		///// Reuses an exception object, to avoid capture of stack traces
		///// </summary>
		///// <param name="ex">Exception object.</param>
		//public static void Reuse(ScriptReturnValueException ex)
		//{
		//	lock (reused)
		//	{
		//		reused.AddFirst(ex);
		//	}
		//}
	}
}
