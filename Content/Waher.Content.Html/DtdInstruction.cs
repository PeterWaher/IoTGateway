using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;

namespace Waher.Content.Html
{
	/// <summary>
	/// Represents a DTD instruction inside the document.
	/// </summary>
    public class DtdInstruction : HtmlNode
    {
		private string instruction;

		/// <summary>
		/// Represents a DTD instruction inside the document.
		/// </summary>
		/// <param name="Parent">Parent node. Can be null for root elements.</param>
		/// <param name="Instruction">Instruction</param>
		public DtdInstruction(HtmlNode Parent, string Instruction)
			: base(Parent)
		{
			this.instruction = Instruction;
		}

		/// <summary>
		/// Unparsed DTD instruction.
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
			Output.WriteRaw("<!");
			Output.WriteRaw(this.instruction);
			Output.WriteRaw(">");
		}
	}
}
