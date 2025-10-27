using System;
using System.Threading.Tasks;
using Waher.Runtime.Inventory;

namespace Waher.Networking.XMPP.Contracts.HumanReadable.InlineElements.ValueRendering
{
	/// <summary>
	/// Converts a date and time parameter value to a human-readable string.
	/// </summary>
	public class DateTimeParameterValueRenderer : ParameterValueRenderer
	{
		/// <summary>
		/// Converts a date and time parameter value to a human-readable string.
		/// </summary>
		public DateTimeParameterValueRenderer()
			: base()
		{
		}

		/// <summary>
		/// How well a parameter value is supported.
		/// </summary>
		/// <param name="Value">Parameter value.</param>
		/// <returns>How well values of this type are supported.</returns>
		public override Grade Supports(object Value)
		{
			return Value is DateTime ? Grade.Ok : Grade.NotAtAll;
		}

		/// <summary>
		/// Generates a Markdown string from the parameter value.
		/// </summary>
		/// <param name="Value">Parameter value.</param>
		/// <param name="Language">Desired language.</param>
		/// <param name="Settings">Markdown settings.</param>
		/// <returns>Markdown string.</returns>
		public override Task<string> ToString(object Value, string Language,
			MarkdownSettings Settings)
		{
			if (Value is DateTime TP && TP.TimeOfDay == TimeSpan.Zero)
				return Task.FromResult(TP.ToShortDateString());
			else
				return base.ToString(Value, Language, Settings);
		}
	}
}
