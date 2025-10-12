using System;
using System.Reflection;

namespace Waher.Security.CallStack
{
	/// <summary>
	/// Checks for approved sources in the call stack using a string.
	/// </summary>
	public class ApproveString : CallStackCheck
	{
		private readonly string s;

		/// <summary>
		/// Checks for approved sources in the call stack using a string.
		/// </summary>
		public ApproveString(string String)
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
				return true;
			}

			Type T = Frame.Type;
			while (!(T is null) && T.Attributes.HasFlag(TypeAttributes.NestedPrivate))
			{
				T = T.DeclaringType;

				if (T.FullName + "." + Frame.Method.Name == this.s ||
					T.FullName == this.s)
				{
					return true;
				}
			}

			return null;
		}
	}
}
