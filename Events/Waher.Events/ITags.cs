using System;
using System.Collections.Generic;
using System.Text;

namespace Waher.Events
{
	/// <summary>
	/// Implement this interface on exception classes to allow the log to extract tags in corresponding events.
	/// Interface can also be used on other types of classes.
	/// </summary>
	public interface ITags
	{
		/// <summary>
		/// Tags related to the object.
		/// </summary>
		KeyValuePair<string, object>[] Tags
		{
			get;
		}
	}
}
