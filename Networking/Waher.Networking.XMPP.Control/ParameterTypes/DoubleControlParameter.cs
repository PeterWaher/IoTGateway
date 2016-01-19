using System;
using System.Collections.Generic;
using System.Text;
using Waher.Events;
using Waher.Things;

namespace Waher.Networking.XMPP.Control.ParameterTypes
{
	/// <summary>
	/// Set handler delegate for double control parameters.
	/// </summary>
	/// <param name="Sender">Sender of event.</param>
	/// <param name="Value">Value set.</param>
	public delegate void DoubleSetHandler(ThingReference Node, double Value);

	/// <summary>
	/// Get handler delegate for double control parameters.
	/// </summary>
	/// <param name="Sender">Sender of event.</param>
	/// <returns>Current value, or null if not available.</returns>
	public delegate double? DoubleGetHandler(ThingReference Node);

	/// <summary>
	/// Double control parameter.
	/// </summary>
	public class DoubleControlParameter : ControlParameter
	{
		private DoubleGetHandler getHandler;
		private DoubleSetHandler setHandler;

		/// <summary>
		/// Double control parameter.
		/// </summary>
		/// <param name="Name">Parameter name.</param>
		public DoubleControlParameter(string Name, DoubleGetHandler GetHandler, DoubleSetHandler SetHandler)
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
		public void Set(ThingReference Node, double Value)
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
		public double? Get(ThingReference Node)
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
