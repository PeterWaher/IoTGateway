using System.Text;
using System.Threading.Tasks;

namespace Waher.Networking.XMPP.Contracts.HumanReadable.InlineElements
{
	/// <summary>
	/// Line-Break
	/// </summary>
	public class LineBreak : InlineElement
	{
		/// <summary>
		/// Serializes the element in normalized form.
		/// </summary>
		/// <param name="Xml">XML Output.</param>
		public override void Serialize(StringBuilder Xml)
		{
			Serialize(Xml, null, "lineBreak");
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
		public override void GenerateMarkdown(MarkdownOutput Markdown, int SectionLevel, int Indentation, MarkdownSettings Settings)
		{
			Markdown.Append("  ");
			Markdown.AppendLine();
		}
	}
}
