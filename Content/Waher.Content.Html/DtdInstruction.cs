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
		private readonly string instruction;

		/// <summary>
		/// Represents a DTD instruction inside the document.
		/// </summary>
		/// <param name="Document">HTML Document.</param>
		/// <param name="Parent">Parent element. Can be null for root elements.</param>
		/// <param name="StartPosition">Start position.</param>
		/// <param name="EndPosition">End position.</param>
		/// <param name="Instruction">Instruction</param>
		public DtdInstruction(HtmlDocument Document, HtmlElement Parent, int StartPosition, 
			int EndPosition, string Instruction)
			: base(Document, Parent, StartPosition, EndPosition)
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

        /// <summary>
        /// Exports the HTML document to XML.
        /// </summary>
        /// <param name="Output">XML Output</param>
        public override void Export(StringBuilder Output)
        {
            Output.Append("<!");
            Output.Append(this.instruction);
            Output.Append(">");
        }
    }
}
