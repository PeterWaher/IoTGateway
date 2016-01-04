using System;
using System.Collections.Generic;
using System.Text;

namespace Waher.Events
{
	/// <summary>
	/// Event level.
	/// </summary>
	public enum EventLevel
	{
		/// <summary>
		/// Minor events, concerning normal operating procedures.
		/// </summary>
		Minor,

		/// <summary>
		/// Medium events.
		/// </summary>
		Medium,

		/// <summary>
		/// More substantial events or events that are affecting larger parts of the system.
		/// </summary>
		Major
	}
}
