using System.Threading.Tasks;
using Waher.Runtime.Inventory;

namespace Waher.Networking.XMPP.Contracts.HumanReadable.InlineElements.ValueRendering
{
	/// <summary>
	/// Converts a Boolean parameter value to a human-readable string.
	/// </summary>
	public class BooleanParameterValueRenderer : ParameterValueRenderer
	{
		/// <summary>
		/// Converts a Boolean parameter value to a human-readable string.
		/// </summary>
		public BooleanParameterValueRenderer()
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
			return Value is bool ? Grade.Ok : Grade.NotAtAll;
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
			if (Value is bool b)
				return Task.FromResult(b ? "[X]" : "[ ]");
			else
				return base.ToString(Value, Language, Settings);
		}
	}
}
