using System;
using System.Collections;
using System.Xml;
using Waher.Script.Abstraction.Elements;
using Waher.Script.Model;
using Waher.Script.Objects;

namespace Waher.Script.Xml.Model
{
	/// <summary>
	/// XML Script value node.
	/// </summary>
	public class XmlScriptValue : XmlScriptNode
	{
		private ScriptNode node;

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
		}

		/// <summary>
		/// Calls the callback method for all child nodes.
		/// </summary>
		/// <param name="Callback">Callback method to call.</param>
		/// <param name="State">State object to pass on to the callback method.</param>
		/// <param name="DepthFirst">If calls are made depth first (true) or on each node and then its leaves (false).</param>
		/// <returns>If the process was completed.</returns>
		public override bool ForAllChildNodes(ScriptNodeEventHandler Callback, object State, bool DepthFirst)
		{
			if (DepthFirst)
			{
				if (!this.node.ForAllChildNodes(Callback, State, DepthFirst))
					return false;
			}

			if (!Callback(ref this.node, State))
				return false;

			if (!DepthFirst)
			{
				if (!this.node.ForAllChildNodes(Callback, State, DepthFirst))
					return false;
			}

			return true;
		}

		/// <summary>
		/// Builds an XML Document objcet
		/// </summary>
		/// <param name="Document">Document being built.</param>
		/// <param name="Parent">Parent element.</param>
		/// <param name="Variables">Current set of variables.</param>
		internal override void Build(XmlDocument Document, XmlElement Parent, Variables Variables)
		{
			IElement Element = this.node.Evaluate(Variables);
			this.AppendChild(Document, Parent, Element.AssociatedObjectValue);
		}

		private void AppendChild(XmlDocument Document, XmlElement Parent, object Value)
		{
			if (Value is null)
				return;

			if (Value is string s)
				Parent.AppendChild(Document.CreateTextNode(s));
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

	}
}
