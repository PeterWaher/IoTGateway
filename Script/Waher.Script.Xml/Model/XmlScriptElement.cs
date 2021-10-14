using System;
using System.Collections.Generic;
using System.Xml;
using Waher.Script.Abstraction.Elements;
using Waher.Script.Exceptions;
using Waher.Script.Model;
using Waher.Script.Objects;

namespace Waher.Script.Xml.Model
{
	/// <summary>
	/// XML Script element node.
	/// </summary>
	public class XmlScriptElement : XmlScriptNode
	{
		private XmlScriptAttribute xmlns;
		private readonly XmlScriptAttribute[] attributes;
		private LinkedList<XmlScriptNode> children = null;
		private readonly string name;

		/// <summary>
		/// XML Script element node.
		/// </summary>
		/// <param name="Name">Element name.</param>
		/// <param name="Xmlns">XML Namespace attribute.</param>
		/// <param name="Attributes">Attributes</param>
		/// <param name="Start">Start position in script expression.</param>
		/// <param name="Length">Length of expression covered by node.</param>
		/// <param name="Expression">Expression containing script.</param>
		public XmlScriptElement(string Name, XmlScriptAttribute Xmlns, XmlScriptAttribute[] Attributes,
			int Start, int Length, Expression Expression)
			: base(Start, Length, Expression)
		{
			this.name = Name;
			this.xmlns = Xmlns;
			this.attributes = Attributes;
		}

		/// <summary>
		/// Adds a XML Script node to the element.
		/// </summary>
		/// <param name="Node">Node to add.</param>
		public void Add(XmlScriptNode Node)
		{
			if (this.children is null)
				this.children = new LinkedList<XmlScriptNode>();

			this.children.AddLast(Node);
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
			int i, c = this.attributes.Length;
			LinkedListNode<XmlScriptNode> Loop;
			ScriptNode Node;

			if (DepthFirst)
			{
				if (!(this.xmlns?.ForAllChildNodes(Callback, State, DepthFirst) ?? true))
					return false;

				for (i = 0; i < c; i++)
				{
					if (!this.attributes[i].ForAllChildNodes(Callback, State, DepthFirst))
						return false;
				}

				Loop = this.children?.First;
				while (!(Loop is null))
				{
					if (!Loop.Value.ForAllChildNodes(Callback, State, DepthFirst))
						return false;

					Loop = Loop.Next;
				}
			}

			if (!(this.xmlns is null))
			{
				Node = this.xmlns;

				if (!Callback(ref Node, State))
					return false;

				if (Node != this.xmlns)
				{
					if (Node is XmlScriptAttribute Attr)
						this.xmlns = Attr;
					else
						throw new ScriptRuntimeException("Incompatible node change.", this);
				}
			}

			for (i = 0; i < c; i++)
			{
				Node = this.attributes[i];

				if (!Callback(ref Node, State))
					return false;

				if (Node != this.attributes[i])
				{
					if (Node is XmlScriptAttribute Attr)
						this.attributes[i] = Attr;
					else
						throw new ScriptRuntimeException("Incompatible node change.", this);
				}
			}

			Loop = this.children?.First;
			while (!(Loop is null))
			{
				Node = Loop.Value;

				if (!Callback(ref Node, State))
					return false;

				if (Node != Loop.Value)
				{
					if (Node is XmlScriptNode Node2)
						Loop.Value = Node2;
					else
						throw new ScriptRuntimeException("Incompatible node change.", this);
				}

				Loop = Loop.Next;
			}

			if (!DepthFirst)
			{
				if (!(this.xmlns?.ForAllChildNodes(Callback, State, DepthFirst) ?? true))
					return false;

				for (i = 0; i < c; i++)
				{
					if (!this.attributes[i].ForAllChildNodes(Callback, State, DepthFirst))
						return false;
				}

				Loop = this.children?.First;
				while (!(Loop is null))
				{
					if (!Loop.Value.ForAllChildNodes(Callback, State, DepthFirst))
						return false;

					Loop = Loop.Next;
				}
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
			string ns = this.xmlns?.GetValue(Variables) ?? null;
			XmlElement E;

			if (ns is null)
			{
				if (Parent is null || string.IsNullOrEmpty(ns = Parent.NamespaceURI))
					E = Document.CreateElement(this.name);
				else
					E = Document.CreateElement(this.name, ns);
			}
			else
				E = Document.CreateElement(this.name, ns);

			if (Parent is null)
				Document.AppendChild(E);
			else
				Parent.AppendChild(E);

			foreach (XmlScriptAttribute Attr in this.attributes)
				Attr.Build(Document, E, Variables);

			if (!(this.children is null))
			{
				foreach (XmlScriptNode Node in this.children)
					Node.Build(Document, E, Variables);
			}
		}

		/// <summary>
		/// Performs a pattern match operation.
		/// </summary>
		/// <param name="CheckAgainst">Value to check against.</param>
		/// <param name="AlreadyFound">Variables already identified.</param>
		/// <returns>Pattern match result</returns>
		public override PatternMatchResult PatternMatch(XmlNode CheckAgainst, Dictionary<string, IElement> AlreadyFound)
		{
			if (!(CheckAgainst is XmlElement E))
				return PatternMatchResult.NoMatch;

			if (E.LocalName != this.name)
				return PatternMatchResult.NoMatch;

			PatternMatchResult Result;

			if (!(this.xmlns is null))
			{
				Result = this.xmlns.PatternMatch(CheckAgainst, AlreadyFound);
				if (Result != PatternMatchResult.Match)
					return Result;
			}

			foreach (XmlScriptAttribute Attr in this.attributes)
			{
				XmlAttribute Attr2 = E.Attributes[Attr.Name];

				Result = Attr.PatternMatch(Attr2, AlreadyFound);
				if (Result != PatternMatchResult.Match)
					return Result;
			}

			if (this.children is null)
			{
				if (!(E.FirstChild is null))
					return PatternMatchResult.NoMatch;
			}
			else
			{
				XmlNode N = E.FirstChild;

				foreach (XmlScriptNode N2 in this.children)
				{
					Result = N2.PatternMatch(N, AlreadyFound);
					if (Result != PatternMatchResult.Match)
						return Result;

					N = N?.NextSibling;
				}

				if (!(N is null))
					return PatternMatchResult.NoMatch;
			}

			return PatternMatchResult.Match;
		}
	}
}
