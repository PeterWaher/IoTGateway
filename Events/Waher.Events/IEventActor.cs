using System;
using System.Collections.Generic;
using System.Text;

namespace Waher.Events
{
	/// <summary>
	/// Implement this interface on exception classes to allow the log to extract actor information in corresponding events.
	/// Interface can also be used on other types of classes.
	/// </summary>
	public interface IEventActor
	{
		/// <summary>
		/// Actor identifier related to the object.
		/// </summary>
		string Actor
		{
			get;
		}
	}
}
