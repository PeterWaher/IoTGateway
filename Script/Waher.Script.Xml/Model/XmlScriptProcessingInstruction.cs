using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Xml;
using Waher.Script.Model;

namespace Waher.Script.Xml.Model
{
	/// <summary>
	/// XML Script processing instruction node.
	/// </summary>
	public class XmlScriptProcessingInstruction : XmlScriptLeafNode
	{
		private readonly string text;

		/// <summary>
		/// XML Script processing instruction node.
		/// </summary>
		/// <param name="Text">Processing instruction text.</param>
		/// <param name="Start">Start position in script expression.</param>
		/// <param name="Length">Length of expression covered by node.</param>
		/// <param name="Expression">Expression containing script.</param>
		public XmlScriptProcessingInstruction(string Text, int Start, int Length, Expression Expression)
			: base(Start, Length, Expression)
		{
			this.text = Text;
		}

		/// <summary>
		/// Builds an XML Document objcet
		/// </summary>
		/// <param name="Document">Document being built.</param>
		/// <param name="Parent">Parent element.</param>
		/// <param name="Variables">Current set of variables.</param>
		internal override void Build(XmlDocument Document, XmlElement Parent, Variables Variables)
		{
			int i = this.text.IndexOf(' ');

			if (i < 0)
				Document.CreateProcessingInstruction(this.text, string.Empty);
			else
			{
				Match M = xmlDeclaration.Match(this.text);

				if (M.Success)
				{
					string Version = M.Groups["Version"].Value;
					string Encoding = M.Groups["Encoding"].Value;
					string Standalone = M.Groups["Standalone"].Value;

					Document.AppendChild(Document.CreateXmlDeclaration(Version, Encoding, Standalone));
				}
				else
				{
					string Target = this.text.Substring(0, i);
					string Data = this.text.Substring(i + 1).Trim();

					Document.AppendChild(Document.CreateProcessingInstruction(Target, Data));
				}
			}
		}

		private static readonly Regex xmlDeclaration = new Regex("\\s*xml\\s*(((version=['\"](?'Version'[^'\"]*)['\"])|(encoding=['\"](?'Encoding'[^'\"]*)['\"])|(standalone=['\"](?'Standalone'[^'\"]*)['\"]))\\s*)*", RegexOptions.Compiled | RegexOptions.Singleline | RegexOptions.IgnoreCase);

	}
}
