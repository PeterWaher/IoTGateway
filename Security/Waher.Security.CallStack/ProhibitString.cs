namespace Waher.Security.CallStack
{
	/// <summary>
	/// Checks for prohibited sources in the call stack using a string.
	/// </summary>
	public class ProhibitString : CallStackCheck
	{
		private readonly string s;

		/// <summary>
		/// Checks for prohibited sources in the call stack using a string.
		/// </summary>
		public ProhibitString(string String)
		{
			this.s = String;
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
			if (Frame.TypeName + "." + Frame.Method.Name == this.s ||
				Frame.TypeName == this.s ||
				Frame.AssemblyName == this.s)
			{
				return false;
			}
			else
				return null;
		}
	}
}
