using System;
using System.Collections;
using System.Collections.Generic;
using System.Numerics;
using System.Threading.Tasks;
using System.Xml;
using Waher.Content;
using Waher.Content.Xml;
using Waher.Persistence;
using Waher.Script.Abstraction.Elements;
using Waher.Script.Model;
using Waher.Script.Objects;
using Waher.Script.Objects.VectorSpaces;
using Waher.Script.Operators.Vectors;

namespace Waher.Script.Xml.Model
{
	/// <summary>
	/// XML Script value node.
	/// </summary>
	public class XmlScriptValue : XmlScriptNode
	{
		private ScriptNode node;
		private bool isAsync;

		/// <summary>
		/// XML Script value node.
		/// </summary>
		/// <param name="Node">Script value.</param>
		/// <param name="Start">Start position in script expression.</param>
		/// <param name="Length">Length of expression covered by node.</param>
		/// <param name="Expression">Expression containing script.</param>
		public XmlScriptValue(ScriptNode Node, int Start, int Length, Expression Expression)
			: base(Start, Length, Expression)
		{
			this.node = Node;
			this.node?.SetParent(this);

			this.isAsync = Node?.IsAsynchronous ?? false;
		}

		/// <summary>
		/// If the node (or its decendants) include asynchronous evaluation. Asynchronous nodes should be evaluated using
		/// <see cref="ScriptNode.EvaluateAsync(Variables)"/>.
		/// </summary>
		public override bool IsAsynchronous => this.isAsync;

		/// <summary>
		/// Calls the callback method for all child nodes.
		/// </summary>
		/// <param name="Callback">Callback method to call.</param>
		/// <param name="State">State object to pass on to the callback method.</param>
		/// <param name="Order">Order to traverse the nodes.</param>
		/// <returns>If the process was completed.</returns>
		public override bool ForAllChildNodes(ScriptNodeEventHandler Callback, object State, SearchMethod Order)
		{
			if (Order == SearchMethod.DepthFirst)
			{
				if (!this.node.ForAllChildNodes(Callback, State, Order))
					return false;
			}

			bool b = !Callback(this.node, out ScriptNode NewNode, State);
			if (!(NewNode is null))
			{
				this.node = NewNode;
				this.node.SetParent(this);

				this.isAsync = NewNode.IsAsynchronous;
			}

			if (b)
				return false;

			if (Order != SearchMethod.DepthFirst)
			{
				if (!this.node.ForAllChildNodes(Callback, State, Order))
					return false;
			}

			return true;
		}

		/// <summary>
		/// Builds an XML Document object
		/// </summary>
		/// <param name="Document">Document being built.</param>
		/// <param name="Parent">Parent element.</param>
		/// <param name="Variables">Current set of variables.</param>
		internal override void Build(XmlDocument Document, XmlElement Parent, Variables Variables)
		{
			IElement Bak = Variables.TryGetVariable(ParentNamespaceVariableName, out Variable v) ? v.ValueElement : null;
			Variables[ParentNamespaceVariableName] = Parent.NamespaceURI;

			try
			{
				IElement Element = this.node.Evaluate(Variables);
				this.AppendChild(Document, Parent, Element.AssociatedObjectValue);
			}
			finally
			{
				if (Bak is null)
					Variables.Remove(ParentNamespaceVariableName);
				else
					Variables[ParentNamespaceVariableName] = Bak;
			}
		}

		internal const string ParentNamespaceVariableName = " Parent NS ";

		/// <summary>
		/// Builds an XML Document object
		/// </summary>
		/// <param name="Document">Document being built.</param>
		/// <param name="Parent">Parent element.</param>
		/// <param name="Variables">Current set of variables.</param>
		internal override async Task BuildAsync(XmlDocument Document, XmlElement Parent, Variables Variables)
		{
			IElement Bak = Variables.TryGetVariable(ParentNamespaceVariableName, out Variable v) ? v.ValueElement : null;
			Variables[ParentNamespaceVariableName] = Parent.NamespaceURI;

			try
			{
				IElement Element = await this.node.EvaluateAsync(Variables);
				this.AppendChild(Document, Parent, Element.AssociatedObjectValue);
			}
			finally
			{
				if (Bak is null)
					Variables.Remove(ParentNamespaceVariableName);
				else
					Variables[ParentNamespaceVariableName] = Bak;
			}
		}

		private void AppendChild(XmlDocument Document, XmlElement Parent, object Value)
		{
			if (Value is null)
				return;

			if (Value is string s)
				Parent.AppendChild(Document.CreateTextNode(s));
			else if (Value is bool b)
				Parent.AppendChild(Document.CreateTextNode(CommonTypes.Encode(b)));
			else if (Value is double d)
				Parent.AppendChild(Document.CreateTextNode(CommonTypes.Encode(d)));
			else if (Value is DateTime TP)
				Parent.AppendChild(Document.CreateTextNode(XML.Encode(TP, TP.TimeOfDay == TimeSpan.Zero)));
			else if (Value is CaseInsensitiveString cis)
				Parent.AppendChild(Document.CreateTextNode(cis.Value));
			else if (Value is BigInteger I)
				Parent.AppendChild(Document.CreateTextNode(I.ToString()));
			else if (Value is XmlDocument Doc)
				Parent.AppendChild(Document.ImportNode(Doc.DocumentElement, true));
			else if (Value is XmlElement E)
				Parent.AppendChild(Document.ImportNode(E, true));
			else if (Value is IEnumerable A)
			{
				IEnumerator e = A.GetEnumerator();

				while (e.MoveNext())
					this.AppendChild(Document, Parent, e.Current);
			}
			else
				Parent.AppendChild(Document.CreateTextNode(Expression.ToString(Value)));
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
				CheckAgainst is XmlWhitespace ||
				CheckAgainst is XmlSignificantWhitespace ||
				CheckAgainst is XmlCDataSection)
			{
				return this.node.PatternMatch(new StringValue(CheckAgainst.InnerText), AlreadyFound);
			}
			else if (CheckAgainst is XmlElement)
				return this.node.PatternMatch(new ObjectValue(CheckAgainst), AlreadyFound);
			else if (CheckAgainst is null)
			{
				if (this.node is ToVector)
					return this.node.PatternMatch(new ObjectVector(Array.Empty<object>()), AlreadyFound);
				else
					return this.node.PatternMatch(ObjectValue.Null, AlreadyFound);
			}
			else
				return PatternMatchResult.NoMatch;
		}

		/// <summary>
		/// Performs a pattern match operation.
		/// </summary>
		/// <param name="CheckAgainst">Value to check against.</param>
		/// <param name="AlreadyFound">Variables already identified.</param>
		/// <returns>Pattern match result</returns>
		public override PatternMatchResult PatternMatch(IElement CheckAgainst, Dictionary<string, IElement> AlreadyFound)
		{
			return this.node.PatternMatch(CheckAgainst, AlreadyFound);
		}

		/// <summary>
		/// If the node is applicable in pattern matching against <paramref name="CheckAgainst"/>.
		/// </summary>
		/// <param name="CheckAgainst">Value to check against.</param>
		/// <returns>If the node is applicable for pattern matching.</returns>
		public override bool IsApplicable(XmlNode CheckAgainst)
		{
			return (CheckAgainst is XmlText ||
				CheckAgainst is XmlWhitespace ||
				CheckAgainst is XmlSignificantWhitespace ||
				CheckAgainst is XmlCDataSection ||
				CheckAgainst is XmlElement);
		}

		/// <summary>
		/// If the node represents a vector of nodes.
		/// </summary>
		public override bool IsVector
		{
			get => this.node is ToVector;
		}

	}
}
