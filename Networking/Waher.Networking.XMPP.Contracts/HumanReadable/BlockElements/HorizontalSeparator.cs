using System.Text;
using System.Threading.Tasks;

namespace Waher.Networking.XMPP.Contracts.HumanReadable.BlockElements
{
	/// <summary>
	/// Horizontal separator.
	/// </summary>
	public class HorizontalSeparator : BlockElement
	{
		/// <summary>
		/// Serializes the element in normalized form.
		/// </summary>
		/// <param name="Xml">XML Output.</param>
		public override void Serialize(StringBuilder Xml)
		{
			Serialize(Xml, null, "separator");
		}

		/// <summary>
		/// Checks if the element is well-defined.
		/// </summary>
		/// <returns>Returns first failing element, if found.</returns>
		public override Task<HumanReadableElement> IsWellDefined() => Task.FromResult<HumanReadableElement>(null);

		/// <summary>
		/// Generates markdown for the human-readable text.
		/// </summary>
		/// <param name="Markdown">Markdown output.</param>
		/// <param name="SectionLevel">Current section level.</param>
		/// <param name="Indentation">Current indentation.</param>
		/// <param name="Settings">Settings used for Markdown generation of human-readable text.</param>
		public override Task GenerateMarkdown(MarkdownOutput Markdown, int SectionLevel, int Indentation, MarkdownSettings Settings)
		{
			Markdown.Indent(Indentation);
			Markdown.Append(new string('*', 80));
			Markdown.AppendLine();

			Markdown.Indent(Indentation);
			Markdown.AppendLine();
		
			return Task.CompletedTask;
		}
	}
}
