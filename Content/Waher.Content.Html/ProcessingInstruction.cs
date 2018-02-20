using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;

namespace Waher.Content.Html
{
	/// <summary>
	/// Represents a Processing instruction inside the document.
	/// </summary>
    public class ProcessingInstruction : HtmlNode
    {
		private string instruction;

		/// <summary>
		/// Represents a Processing instruction inside the document.
		/// </summary>
		/// <param name="Parent">Parent node. Can be null for root elements.</param>
		/// <param name="Instruction">Instruction</param>
		public ProcessingInstruction(HtmlNode Parent, string Instruction)
			: base(Parent)
		{
			this.instruction = Instruction;
		}

		/// <summary>
		/// Unparsed Processing instruction.
		/// </summary>
		public string Instruction
		{
			get { return this.instruction; }
		}

		/// <summary>
		/// Exports the HTML document to XML.
		/// </summary>
		/// <param name="Output">XML Output</param>
		public override void Export(XmlWriter Output)
		{
			Output.WriteRaw("<?");
			Output.WriteRaw(this.instruction);
			Output.WriteRaw("?>");
		}
	}
}
