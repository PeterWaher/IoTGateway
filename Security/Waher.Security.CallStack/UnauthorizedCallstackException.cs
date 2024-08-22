using System;

namespace Waher.Security.CallStack
{
	/// <summary>
	/// Exception raised when code tries to access protected code while having a callstack out of the permitted call stacks.
	/// </summary>
	public class UnauthorizedCallstackException : UnauthorizedAccessException
	{
		/// <summary>
		/// Exception raised when code tries to access protected code while having a callstack out of the permitted call stacks.
		/// </summary>
		/// <param name="Message">Exception message.</param>
		public UnauthorizedCallstackException(string Message)
			: base(Message)
		{
		}
	}
}
