﻿using System.Collections.Generic;
using System.Text;
using System.Xml;

namespace Waher.Content.Html
{
	/// <summary>
	/// Represents a Processing instruction inside the document.
	/// </summary>
    public class ProcessingInstruction : HtmlNode
    {
		private readonly string instruction;

		/// <summary>
		/// Represents a Processing instruction inside the document.
		/// </summary>
		/// <param name="Document">HTML Document.</param>
		/// <param name="Parent">Parent node. Can be null for root elements.</param>
		/// <param name="StartPosition">Start position.</param>
		/// <param name="EndPosition">End position.</param>
		/// <param name="Instruction">Instruction</param>
		public ProcessingInstruction(HtmlDocument Document, HtmlElement Parent, int StartPosition,
			int EndPosition, string Instruction)
			: base(Document, Parent, StartPosition, EndPosition)
		{
			this.instruction = Instruction;
		}

		/// <summary>
		/// Unparsed Processing instruction.
		/// </summary>
		public string Instruction => this.instruction;

		/// <summary>
		/// Exports the HTML document to XML.
		/// </summary>
		/// <param name="Namespaces">Namespaces defined, by prefix.</param>
		/// <param name="Output">XML Output</param>
		public override void Export(XmlWriter Output, Dictionary<string, string> Namespaces)
		{
			Output.WriteRaw("<?");
			Output.WriteRaw(this.instruction);
			Output.WriteRaw("?>");
		}

        /// <summary>
        /// Exports the HTML document to XML.
        /// </summary>
        /// <param name="Output">XML Output</param>
        public override void Export(StringBuilder Output)
        {
            Output.Append("<?");
            Output.Append(this.instruction);
            Output.Append("?>");
        }
    }
}
