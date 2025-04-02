﻿using System;
using System.Collections.Generic;
using System.Xml;
using Waher.Script.Abstraction.Elements;
using Waher.Script.Model;

namespace Waher.Script.Xml.Model
{
	/// <summary>
	/// XML Script CDATA node.
	/// </summary>
	public class XmlScriptCData : XmlScriptLeafNode
	{
		private readonly string text;

		/// <summary>
		/// XML Script CDATA node.
		/// </summary>
		/// <param name="Text">Text content.</param>
		/// <param name="Start">Start position in script expression.</param>
		/// <param name="Length">Length of expression covered by node.</param>
		/// <param name="Expression">Expression containing script.</param>
		public XmlScriptCData(string Text, int Start, int Length, Expression Expression)
			: base(Start, Length, Expression)
		{
			this.text = Text;
		}

		/// <summary>
		/// Builds an XML Document object
		/// </summary>
		/// <param name="Document">Document being built.</param>
		/// <param name="Parent">Parent element.</param>
		/// <param name="Variables">Current set of variables.</param>
		internal override void Build(XmlDocument Document, XmlElement Parent, Variables Variables)
		{
			Parent.AppendChild(Document.CreateCDataSection(this.text));
		}

		/// <summary>
		/// Performs a pattern match operation.
		/// </summary>
		/// <param name="CheckAgainst">Value to check against.</param>
		/// <param name="AlreadyFound">Variables already identified.</param>
		/// <returns>Pattern match result</returns>
		public override PatternMatchResult PatternMatch(XmlNode CheckAgainst, Dictionary<string, IElement> AlreadyFound)
		{
			if (CheckAgainst is XmlCDataSection ||
				CheckAgainst is XmlText ||
				CheckAgainst is XmlWhitespace ||
				CheckAgainst is XmlSignificantWhitespace)
			{
				return CheckAgainst.InnerText.Trim() == this.text.Trim() ? PatternMatchResult.Match : PatternMatchResult.NoMatch;
			}
			else
				return PatternMatchResult.NoMatch;
		}

		/// <summary>
		/// If the node is applicable in pattern matching against <paramref name="CheckAgainst"/>.
		/// </summary>
		/// <param name="CheckAgainst">Value to check against.</param>
		/// <param name="First">First element</param>
		/// <returns>If the node is applicable for pattern matching.</returns>
		public override bool IsApplicable(XmlNode CheckAgainst, XmlElement First)
		{
			return CheckAgainst is XmlCDataSection ||
				CheckAgainst is XmlText ||
				CheckAgainst is XmlWhitespace ||
				CheckAgainst is XmlSignificantWhitespace;
		}
	}
}
