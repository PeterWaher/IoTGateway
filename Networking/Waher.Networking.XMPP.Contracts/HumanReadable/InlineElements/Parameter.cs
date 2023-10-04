using System.Text;
using Waher.Content.Xml;

namespace Waher.Networking.XMPP.Contracts.HumanReadable.InlineElements
{
	/// <summary>
	/// Is replaced by parameter value
	/// </summary>
	public class Parameter : InlineElement
	{
		private string name;

		/// <summary>
		/// Name of parameter
		/// </summary>
		public string Name
		{
			get => this.name;
			set => this.name = value;
		}

		/// <summary>
		/// Checks if the element is well-defined.
		/// </summary>
		public override bool IsWellDefined => !string.IsNullOrEmpty(this.name);

		/// <summary>
		/// Serializes the element in normalized form.
		/// </summary>
		/// <param name="Xml">XML Output.</param>
		public override void Serialize(StringBuilder Xml)
		{
			Xml.Append("<parameter name=\"");
			Xml.Append(XML.Encode(this.name));
			Xml.Append("\"/>");
		}

		/// <summary>
		/// Generates markdown for the human-readable text.
		/// </summary>
		/// <param name="Markdown">Markdown output.</param>
		/// <param name="SectionLevel">Current section level.</param>
		/// <param name="Indentation">Current indentation.</param>
		/// <param name="Settings">Settings used for Markdown generation of human-readable text.</param>
		public override void GenerateMarkdown(MarkdownOutput Markdown, int SectionLevel, int Indentation, MarkdownSettings Settings)
		{
			switch (Settings.Type)
			{
				case MarkdownType.ForEditing:
					Markdown.Append("[%");
					Markdown.Append(this.name);
					Markdown.Append(']');
					break;

				case MarkdownType.ForRendering:
					object Value = Settings.Contract[this.name];

					if (Value is null)
					{
						string Guide = null;

						foreach (Contracts.Parameter P in Settings.Contract.Parameters)
						{
							if (P.Name == this.name)
							{
								Guide = P.Guide;
								break;
							}
						}

						if (string.IsNullOrEmpty(Guide))
							Guide = this.name;

						Markdown.Append('`');
						Markdown.Append(Guide);
						Markdown.Append('`');
					}
					else
					{
						string s;

						if (Value is bool BooleanValue)
							s = BooleanValue ? "[X]" : "[ ]";
						else
							s = Value.ToString();

						Markdown.Append(MarkdownEncode(s, Settings.SimpleEscape));
					}
					break;
			}
		}
	}
}
