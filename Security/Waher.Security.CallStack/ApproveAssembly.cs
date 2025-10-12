using System.Reflection;

namespace Waher.Security.CallStack
{
	/// <summary>
	/// Checks for an approved assembly in the call stack.
	/// </summary>
	public class ApproveAssembly : CallStackCheck
	{
		private readonly Assembly assembly;

		/// <summary>
		/// Checks for an approved assembly in the call stack.
		/// </summary>
		public ApproveAssembly(Assembly Assembly)
		{
			this.assembly = Assembly;
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
			if (Frame.Assembly == this.assembly)
				return true;
			else
				return null;
		}
	}
}
