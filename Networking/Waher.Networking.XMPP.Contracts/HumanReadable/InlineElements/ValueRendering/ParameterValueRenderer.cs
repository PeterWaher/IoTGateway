using System.Threading.Tasks;
using Waher.Runtime.Inventory;

namespace Waher.Networking.XMPP.Contracts.HumanReadable.InlineElements.ValueRendering
{
	/// <summary>
	/// Abstract base class for parameter value renderers.
	/// </summary>
	public abstract class ParameterValueRenderer : IParameterValueRenderer
	{
		/// <summary>
		/// Abstract base class for parameter value renderers.
		/// </summary>
		public ParameterValueRenderer()
		{
		}

		/// <summary>
		/// How well a parameter value is supported.
		/// </summary>
		/// <param name="Value">Parameter value.</param>
		/// <returns>How well values of this type are supported.</returns>
		public abstract Grade Supports(object Value);

		/// <summary>
		/// Generates a Markdown string from the parameter value.
		/// </summary>
		/// <param name="Value">Parameter value.</param>
		/// <param name="Language">Desired language.</param>
		/// <param name="Settings">Markdown settings.</param>
		/// <returns>Markdown string.</returns>
		public virtual Task<string> ToString(object Value, string Language, MarkdownSettings Settings)
		{
			return Task.FromResult(Value?.ToString() ?? string.Empty);
		}

		/// <summary>
		/// If the <see cref="ToString(object, string, MarkdownSettings)"/> method returns
		/// Markdown (true), or plain text (false).
		/// </summary>
		public virtual bool IsMarkdownOutput => false;
	}
}
