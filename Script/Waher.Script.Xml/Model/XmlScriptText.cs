using System;
using System.Collections.Generic;
using System.Xml;
using Waher.Script.Abstraction.Elements;
using Waher.Script.Model;

namespace Waher.Script.Xml.Model
{
	/// <summary>
	/// XML Script text node.
	/// </summary>
	public class XmlScriptText : XmlScriptLeafNode
	{
		private readonly string text;

		/// <summary>
		/// XML Script text node.
		/// </summary>
		/// <param name="Text">Text content.</param>
		/// <param name="Start">Start position in script expression.</param>
		/// <param name="Length">Length of expression covered by node.</param>
		/// <param name="Expression">Expression containing script.</param>
		public XmlScriptText(string Text, int Start, int Length, Expression Expression)
			: base(Start, Length, Expression)
		{
			this.text = Text;
		}

		/// <summary>
		/// Text represented by node.
		/// </summary>
		public string Text => this.text;

		/// <summary>
		/// Builds an XML Document object
		/// </summary>
		/// <param name="Document">Document being built.</param>
		/// <param name="Parent">Parent element.</param>
		/// <param name="Variables">Current set of variables.</param>
		internal override void Build(XmlDocument Document, XmlElement Parent, Variables Variables)
		{
			Parent.AppendChild(Document.CreateTextNode(this.text));
		}

		/// <summary>
		/// Performs a pattern match operation.
		/// </summary>
		/// <param name="CheckAgainst">Value to check against.</param>
		/// <param name="AlreadyFound">Variables already identified.</param>
		/// <returns>Pattern match result</returns>
		public override PatternMatchResult PatternMatch(XmlNode CheckAgainst, Dictionary<string, IElement> AlreadyFound)
		{
			if (CheckAgainst is XmlText ||
				CheckAgainst is XmlCDataSection ||
				CheckAgainst is XmlWhitespace ||
				CheckAgainst is XmlSignificantWhitespace)
			{
				return CheckAgainst.InnerText.Trim() == this.text.Trim() ? PatternMatchResult.Match : PatternMatchResult.NoMatch;
			}
			else if (CheckAgainst is null)
				return string.IsNullOrWhiteSpace(this.text) ? PatternMatchResult.Match : PatternMatchResult.NoMatch;
			else
				return PatternMatchResult.NoMatch;
		}

		/// <summary>
		/// If the node is applicable in pattern matching against <paramref name="CheckAgainst"/>.
		/// </summary>
		/// <param name="CheckAgainst">Value to check against.</param>
		/// <returns>If the node is applicable for pattern matching.</returns>
		public override bool IsApplicable(XmlNode CheckAgainst)
		{
			return (CheckAgainst is XmlText ||
				CheckAgainst is XmlCDataSection ||
				CheckAgainst is XmlWhitespace ||
				CheckAgainst is XmlSignificantWhitespace);
		}

	}
}
