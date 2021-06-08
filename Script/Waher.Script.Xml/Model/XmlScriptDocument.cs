using System;
using System.Xml;
using Waher.Script.Abstraction.Elements;
using Waher.Script.Exceptions;
using Waher.Script.Model;
using Waher.Script.Objects;

namespace Waher.Script.Xml.Model
{
	/// <summary>
	/// Represents an script-based XML document.
	/// </summary>
	public class XmlScriptDocument : XmlScriptNode
	{
		private readonly XmlScriptProcessingInstruction[] processingInstructions;
		private XmlScriptElement root;

		/// <summary>
		/// Represents an script-based XML document.
		/// </summary>
		/// <param name="Root">Root element of document.</param>
		/// <param name="ProcessingInstructions">Processing Instructions.</param>
		/// <param name="Start">Start position in script expression.</param>
		/// <param name="Length">Length of expression covered by node.</param>
		/// <param name="Expression">Expression containing script.</param>
		public XmlScriptDocument(XmlScriptElement Root, XmlScriptProcessingInstruction[] ProcessingInstructions,
			int Start, int Length, Expression Expression)
			: base(Start, Length, Expression)
		{
			this.root = Root;
			this.processingInstructions = ProcessingInstructions;
		}

		/// <summary>
		/// Root element.
		/// </summary>
		public XmlScriptElement Root => this.root;

		/// <summary>
		/// Calls the callback method for all child nodes.
		/// </summary>
		/// <param name="Callback">Callback method to call.</param>
		/// <param name="State">State object to pass on to the callback method.</param>
		/// <param name="DepthFirst">If calls are made depth first (true) or on each node and then its leaves (false).</param>
		/// <returns>If the process was completed.</returns>
		public override bool ForAllChildNodes(ScriptNodeEventHandler Callback, object State, bool DepthFirst)
		{
			int i, c = this.processingInstructions.Length;
			ScriptNode Node;

			if (DepthFirst)
			{
				for (i = 0; i < c; i++)
				{
					if (!this.processingInstructions[i].ForAllChildNodes(Callback, State, DepthFirst))
						return false;
				}

				if (!this.root.ForAllChildNodes(Callback, State, DepthFirst))
					return false;
			}

			for (i = 0; i < c; i++)
			{
				Node = this.processingInstructions[i];

				if (!Callback(ref Node, State))
					return false;

				if (Node != this.processingInstructions[i])
				{
					if (Node is XmlScriptProcessingInstruction PI)
						this.processingInstructions[i] = PI;
					else
						throw new ScriptRuntimeException("Incompatible node change.", this);
				}
			}

			Node = this.root;

			if (!Callback(ref Node, State))
				return false;
			
			if (Node != this.root)
			{
				if (Node is XmlScriptElement Root)
					this.root = Root;
				else
					throw new ScriptRuntimeException("Incompatible node change.", this);
			}

			if (!DepthFirst)
			{
				for (i = 0; i < c; i++)
				{
					if (!this.processingInstructions[i].ForAllChildNodes(Callback, State, DepthFirst))
						return false;
				}

				if (!this.root.ForAllChildNodes(Callback, State, DepthFirst))
					return false;
			}

			return true;
		}

		/// <summary>
		/// Evaluates the node, using the variables provided in the <paramref name="Variables"/> collection.
		/// </summary>
		/// <param name="Variables">Variables collection.</param>
		/// <returns>Result.</returns>
		public override IElement Evaluate(Variables Variables)
		{
			XmlDocument Doc = new XmlDocument()
			{
				PreserveWhitespace = true
			};

			this.Build(Doc, null, Variables);	

			return new ObjectValue(Doc);
		}

		/// <summary>
		/// Builds an XML Document objcet
		/// </summary>
		/// <param name="Document">Document being built.</param>
		/// <param name="Parent">Parent element.</param>
		/// <param name="Variables">Current set of variables.</param>
		internal override void Build(XmlDocument Document, XmlElement Parent, Variables Variables)
		{
			foreach (XmlScriptProcessingInstruction PI in this.processingInstructions)
				PI.Build(Document, Parent, Variables);

			this.root?.Build(Document, Parent, Variables);
		}
	}
}
