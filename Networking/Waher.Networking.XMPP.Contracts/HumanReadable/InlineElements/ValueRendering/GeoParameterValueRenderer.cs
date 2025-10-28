using System.Threading.Tasks;
using Waher.Runtime.Geo;
using Waher.Runtime.Inventory;

namespace Waher.Networking.XMPP.Contracts.HumanReadable.InlineElements.ValueRendering
{
	/// <summary>
	/// Converts a geo parameter value to a human-readable string.
	/// </summary>
	public class GeoParameterValueRenderer : ParameterValueRenderer
	{
		/// <summary>
		/// Converts a geo parameter value to a human-readable string.
		/// </summary>
		public GeoParameterValueRenderer()
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
			return Value is GeoPosition ? Grade.Ok : Grade.NotAtAll;
		}

		/// <summary>
		/// Generates a Markdown string from the parameter value.
		/// </summary>
		/// <param name="Value">Parameter value.</param>
		/// <param name="Language">Desired language.</param>
		/// <param name="Settings">Markdown settings.</param>
		/// <returns>Markdown string.</returns>
		public override Task<string> ToString(object Value, string Language, MarkdownSettings Settings)
		{
			if (Value is GeoPosition Position)
				return Task.FromResult(Position.HumanReadable);
			else
				return base.ToString(Value, Language, Settings);
		}
	}
}
