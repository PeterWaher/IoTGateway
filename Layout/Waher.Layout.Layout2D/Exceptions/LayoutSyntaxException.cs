using System;
using System.Collections.Generic;
using System.Text;

namespace Waher.Layout.Layout2D.Exceptions
{
	/// <summary>
	/// Syntax-related layout exception
	/// </summary>
	public class LayoutSyntaxException : LayoutException
	{
		/// <summary>
		/// Syntax-related layout exception
		/// </summary>
		/// <param name="Message">Message</param>
		public LayoutSyntaxException(string Message)
			: base(Message)
		{
		}
	}
}
