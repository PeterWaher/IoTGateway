using System;
using System.Collections.Generic;
using System.Text;

namespace Waher.Events
{
	/// <summary>
	/// Type of event.
	/// </summary>
	public enum EventType
	{
		/// <summary>
		/// Developers can ask applications to send debug messages during development or testing to more easily see what happens in a system.
		/// </summary>
		Debug,

		/// <summary>
		/// An informational message describing a normal event.
		/// </summary>
		Informational,

		/// <summary>
		/// Represents a significant condition or change that administrators should be aware of.
		/// </summary>
		Notice,

		/// <summary>
		/// A warning condition. If not taken into account, the condition could turn into an error.
		/// </summary>
		Warning,

		/// <summary>
		/// An error condition. A condition has been detected that is considered to be an error or a fault.
		/// </summary>
		Error,

		/// <summary>
		/// A critical condition. An error so great that it could escalate into something graver if not addressed.
		/// </summary>
		Critical,

		/// <summary>
		/// An alert condition. Action must be taken immediately.
		/// </summary>
		Alert,

		/// <summary>
		/// System is unusable.
		/// </summary>
		Emergency
	}
}
