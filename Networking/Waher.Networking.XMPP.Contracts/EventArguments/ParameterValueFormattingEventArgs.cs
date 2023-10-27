using System;

namespace Waher.Networking.XMPP.Contracts.EventArguments
{
	/// <summary>
	/// Delegate for parameter value formatting event handlers.
	/// </summary>
	/// <param name="Sender">Sender of event.</param>
	/// <param name="e">Event arguments.</param>
	/// <returns>Formatted value.</returns>
	public delegate void ParameterValueFormattingEventHandler(object Sender, ParameterValueFormattingEventArgs e);

	/// <summary>
	/// Event arguments for parameter value formatting events.
	/// </summary>
	public class ParameterValueFormattingEventArgs : EventArgs
	{
		/// <summary>
		/// Event arguments for parameter value formatting events.
		/// </summary>
		/// <param name="Name">Name of parameter.</param>
		/// <param name="Value">Value of parameter.</param>
		public ParameterValueFormattingEventArgs(string Name, object Value)
		{
			this.Name = Name;
			this.Value = Value;
		}

		/// <summary>
		/// Name of parameter.
		/// </summary>
		public string Name { get; }

		/// <summary>
		/// Value of parameter. Can be set by event handler, to format the value before it is being displayed.
		/// </summary>
		public object Value { get; set; }
	}
}
