using System;
using System.Reflection;
using System.Text.RegularExpressions;

namespace Waher.Security.CallStack
{
	/// <summary>
	/// Checks for approved sources in the call stack using a regular expression.
	/// </summary>
	public class ApproveRegex : CallStackCheck
	{
		private readonly Regex regex;

		/// <summary>
		/// Checks for approved sources in the call stack using a regular expression.
		/// </summary>
		public ApproveRegex(Regex Regex)
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
			if (IsMatch(this.regex, Frame.TypeName + "." + Frame.Method.Name) ||
				IsMatch(this.regex, Frame.TypeName) ||
				IsMatch(this.regex, Frame.AssemblyName))
			{
				return true;
			}

			Type T = Frame.Type;
			while (!(T is null) && T.Attributes.HasFlag(TypeAttributes.NestedPrivate))
			{
				T = T.DeclaringType;

				if (IsMatch(this.regex, T.FullName + "." + Frame.Method.Name) ||
					IsMatch(this.regex, T.FullName))
				{
					return true;
				}
			}

			return null;
		}

		internal static bool IsMatch(Regex Regex, string s)
		{
			Match M = Regex.Match(s);
			return M.Success && M.Index == 0 && M.Length == s.Length;
		}
	}
}
