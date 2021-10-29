using System;
using System.Text;
using Waher.Content.Markdown;
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
		/// <param name="Contract">Contract, of which the human-readable text is part.</param>
		public override void GenerateMarkdown(StringBuilder Markdown, int SectionLevel, Contract Contract)
		{
			object Value = Contract[this.name];
			string s;

			if (Value is null)
			{
				string Guide = null;

				foreach (Contracts.Parameter P in Contract.Parameters)
				{
					if (P.Name == this.name)
					{
						Guide = P.Guide;
						break;
					}
				}

				if (string.IsNullOrEmpty(Guide))
					Guide = this.name;

				s = "`" + Guide + "`";
			}
			else if (Value is bool BooleanValue)
				s = BooleanValue ? "[X]" : "[ ]";
			else
				s = Value.ToString();

			Markdown.Append(MarkdownDocument.Encode(s));
		}
	}
}
