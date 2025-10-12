using System;
using System.Reflection;

namespace Waher.Security.CallStack
{
	/// <summary>
	/// Checks for an prohibited type in the call stack.
	/// </summary>
	public class ProhibitType : CallStackCheck
	{
		private readonly Type type;

		/// <summary>
		/// Checks for an prohibited type in the call stack.
		/// </summary>
		public ProhibitType(Type Type)
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
				return false;

			Type T = Frame.Type;
			while (!(T is null) && T.Attributes.HasFlag(TypeAttributes.NestedPrivate))
			{
				T = T.DeclaringType;
				if (T == this.type)
					return false;
			}

			return null;
		}
	}
}
