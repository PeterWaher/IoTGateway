using System;

namespace Waher.Security.CallStack
{
	/// <summary>
	/// Checks for an approved type in the call stack.
	/// </summary>
	public class ApproveType : CallStackCheck
	{
		private readonly Type type;

		/// <summary>
		/// Checks for an approved type in the call stack.
		/// </summary>
		public ApproveType(Type Type)
		{
			this.type = Type;
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
		public override bool? Check(FrameInformation Frame)
		{
			if (Frame.Type == this.type)
				return true;
			else
				return null;
		}
	}
}
