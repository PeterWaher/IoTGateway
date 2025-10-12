using System.Diagnostics;

namespace Waher.Security.CallStack
{
	/// <summary>
	/// Abstract base class for call stack checks.
	/// </summary>
	public abstract class CallStackCheck : ICallStackCheck
	{
		/// <summary>
		/// Abstract base class for call stack checks.
		/// </summary>
		public CallStackCheck()
		{
		}

		/// <summary>
		/// Performs a check of a stack frame.
		/// </summary>
		/// <param name="Frame">Stack frame.</param>
		/// <returns>
		/// true, if frame represents a valid frame, and the check can conclude positively.
		/// false, if frame represents a prohibited frame, and the check can conclude negatively.
		/// null, if the frame is not conclusive, and the check should continue with the next frame.
		/// </returns>
		public abstract bool? Check(FrameInformation Frame);
	}
}
