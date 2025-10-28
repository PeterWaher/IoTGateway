using System.Threading.Tasks;
using Waher.Runtime.Inventory;

namespace Waher.Networking.XMPP.Contracts.HumanReadable.InlineElements.ValueRendering
{
	/// <summary>
	/// Converts a parameter value to a human-readable string.
	/// </summary>
	public interface IParameterValueRenderer : IProcessingSupport<object>
	{
		/// <summary>
		/// Generates a Markdown string from the parameter value.
		/// </summary>
		/// <param name="Value">Parameter value.</param>
		/// <param name="Language">Desired language.</param>
		/// <param name="Settings">Markdown settings.</param>
		/// <returns>Markdown string.</returns>
		Task<string> ToString(object Value, string Language, MarkdownSettings Settings);

		/// <summary>
		/// If the <see cref="ToString(object, string, MarkdownSettings)"/> method returns
		/// Markdown (true), or plain text (false).
		/// </summary>
		bool IsMarkdownOutput { get; }
	}
}
