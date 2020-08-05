using System;
using System.Collections.Generic;
using System.Text;

namespace Waher.Layout.Layout2D.Exceptions
{
	/// <summary>
	/// Base class for all layout-related exceptions.
	/// </summary>
	public class LayoutException : Exception
	{
		/// <summary>
		/// Base class for all layout-related exceptions.
		/// </summary>
		/// <param name="Message">Message</param>
		public LayoutException(string Message)
			: base(Message)
		{
		}
	}
}
