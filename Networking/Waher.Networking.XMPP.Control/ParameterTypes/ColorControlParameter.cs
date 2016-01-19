using System;
using System.Collections.Generic;
using System.Text;
using Waher.Events;
using Waher.Things;
using Waher.Content;

namespace Waher.Networking.XMPP.Control.ParameterTypes
{
	/// <summary>
	/// Set handler delegate for color control parameters.
	/// </summary>
	/// <param name="Sender">Sender of event.</param>
	/// <param name="Value">Value set.</param>
	public delegate void ColorSetHandler(ThingReference Node, ColorReference Value);

	/// <summary>
	/// Get handler delegate for color control parameters.
	/// </summary>
	/// <param name="Sender">Sender of event.</param>
	/// <returns>Current value, or null if not available.</returns>
	public delegate ColorReference ColorGetHandler(ThingReference Node);

	/// <summary>
	/// Color control parameter.
	/// </summary>
	public class ColorControlParameter : ControlParameter
	{
		private ColorGetHandler getHandler;
		private ColorSetHandler setHandler;

		/// <summary>
		/// Color control parameter.
		/// </summary>
		/// <param name="Name">Parameter name.</param>
		public ColorControlParameter(string Name, ColorGetHandler GetHandler, ColorSetHandler SetHandler)
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
		public void Set(ThingReference Node, ColorReference Value)
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
		public ColorReference Get(ThingReference Node)
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
