using System.Text;

namespace Waher.Networking.XMPP.Contracts.HumanReadable.BlockElements
{
	/// <summary>
	/// Bullet list
	/// </summary>
	public class BulletList : ItemList 
	{
		/// <summary>
		/// Serializes the element in normalized form.
		/// </summary>
		/// <param name="Xml">XML Output.</param>
		public override void Serialize(StringBuilder Xml)
		{
			Serialize(Xml, this.Items, "bulletItems");
		}

		/// <summary>
		/// Generates markdown for the human-readable text.
		/// </summary>
		/// <param name="Markdown">Markdown output.</param>
		/// <param name="SectionLevel">Current section level.</param>
		/// <param name="Indentation">Current indentation.</param>
		/// <param name="Settings">Settings used for Markdown generation of human-readable text.</param>
		public override void GenerateMarkdown(StringBuilder Markdown, int SectionLevel, int Indentation, MarkdownSettings Settings)
		{
			if (!(this.Items is null))
			{
				bool First = true;

				foreach (Item Item in this.Items)
				{
					Indent(Markdown, Indentation);

					Markdown.Append("*\t");
					Item.GenerateMarkdown(Markdown, SectionLevel, First ? 0 : Indentation + 1, Settings);
					First = false;
				}

				Markdown.AppendLine();
			}
		}
	}
}
