using System;
using System.Collections.Generic;
using System.Text;
using Waher.Events;
using Waher.Things;

namespace Waher.Networking.XMPP.Control.ParameterTypes
{
	/// <summary>
	/// Set handler delegate for 32-bit integer control parameters.
	/// </summary>
	/// <param name="Sender">Sender of event.</param>
	/// <param name="Value">Value set.</param>
	public delegate void Int32SetHandler(ThingReference Node, int Value);

	/// <summary>
	/// Get handler delegate for 32-bit integer control parameters.
	/// </summary>
	/// <param name="Sender">Sender of event.</param>
	/// <returns>Current value, or null if not available.</returns>
	public delegate int? Int32GetHandler(ThingReference Node);

	/// <summary>
	/// Int32 control parameter.
	/// </summary>
	public class Int32ControlParameter : ControlParameter
	{
		private Int32GetHandler getHandler;
		private Int32SetHandler setHandler;

		/// <summary>
		/// Int32 control parameter.
		/// </summary>
		/// <param name="Name">Parameter name.</param>
		public Int32ControlParameter(string Name, Int32GetHandler GetHandler, Int32SetHandler SetHandler)
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
		public void Set(ThingReference Node, int Value)
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
		public int? Get(ThingReference Node)
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
