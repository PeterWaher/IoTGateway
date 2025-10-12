using System;
using System.Reflection;
using System.Text.RegularExpressions;

namespace Waher.Security.CallStack
{
	/// <summary>
	/// Checks for prohibited sources in the call stack using a regular expression.
	/// </summary>
	public class ProhibitRegex : CallStackCheck
	{
		private readonly Regex regex;

		/// <summary>
		/// Checks for prohibited sources in the call stack using a regular expression.
		/// </summary>
		public ProhibitRegex(Regex Regex)
		{
			this.regex = Regex;
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
			if (ApproveRegex.IsMatch(this.regex, Frame.TypeName + "." + Frame.Method.Name) ||
				ApproveRegex.IsMatch(this.regex, Frame.TypeName) ||
				ApproveRegex.IsMatch(this.regex, Frame.AssemblyName))
			{
				return false;
			}

			Type T = Frame.Type;
			while (!(T is null) && T.Attributes.HasFlag(TypeAttributes.NestedPrivate))
			{
				T = T.DeclaringType;

				if (ApproveRegex.IsMatch(this.regex, T.FullName + "." + Frame.Method.Name) ||
					ApproveRegex.IsMatch(this.regex, T.FullName))
				{
					return false;
				}
			}

			return null;
		}
	}
}
