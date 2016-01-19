using System;
using System.Collections.Generic;
using System.Text;
using Waher.Events;
using Waher.Content;
using Waher.Things;

namespace Waher.Networking.XMPP.Control.ParameterTypes
{
	/// <summary>
	/// Set handler delegate for duration control parameters.
	/// </summary>
	/// <param name="Sender">Sender of event.</param>
	/// <param name="Value">Value set.</param>
	public delegate void DurationSetHandler(ThingReference Node, Duration Value);

	/// <summary>
	/// Get handler delegate for duration control parameters.
	/// </summary>
	/// <param name="Sender">Sender of event.</param>
	/// <returns>Current value, or null if not available.</returns>
	public delegate Duration DurationGetHandler(ThingReference Node);

	/// <summary>
	/// Duration control parameter.
	/// </summary>
	public class DurationControlParameter : ControlParameter
	{
		private DurationGetHandler getHandler;
		private DurationSetHandler setHandler;

		/// <summary>
		/// Duration control parameter.
		/// </summary>
		/// <param name="Name">Parameter name.</param>
		public DurationControlParameter(string Name, DurationGetHandler GetHandler, DurationSetHandler SetHandler)
			: base(Name)
		{
			this.getHandler = GetHandler;
			this.setHandler = SetHandler;
		}

		/// <summary>
		/// Sets the value of the control parameter.
		/// </summary>
		/// <param name="Node">Node reference, if available.</param>
		/// <param name="Value">Value to set.</param>
		public void Set(ThingReference Node, Duration Value)
		{
			try
			{
				this.setHandler(Node, Value);
			}
			catch (Exception ex)
			{
				Log.Critical(ex);
			}
		}

		/// <summary>
		/// Gets the value of the control parameter.
		/// </summary>
		/// <returns>Current value, or null if not available.</returns>
		public Duration Get(ThingReference Node)
		{
			try
			{
				return this.getHandler(Node);
			}
			catch (Exception ex)
			{
				Log.Critical(ex);
				return null;
			}
		}
	}
}
