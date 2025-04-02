using System.Collections.Generic;
using System.Threading.Tasks;
using System.Xml;
using Waher.Script.Abstraction.Elements;
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
		private readonly int nrInstructions;
		private bool isAsync;

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
			this.root?.SetParent(this);
		
			this.processingInstructions = ProcessingInstructions;
			this.processingInstructions?.SetParent(this);
			
			this.nrInstructions = ProcessingInstructions.Length;

			this.CalcIsAsync();
		}

		private void CalcIsAsync()
		{
			this.isAsync = this.root?.IsAsynchronous ?? false;
			if (this.isAsync)
				return;

			for (int i = 0; i < this.nrInstructions; i++)
			{
				if (this.processingInstructions[i]?.IsAsynchronous ?? false)
				{
					this.isAsync = true;
					break;
				}
			}
		}


		/// <summary>
		/// Root element.
		/// </summary>
		public XmlScriptElement Root => this.root;

		/// <summary>
		/// If the node (or its decendants) include asynchronous evaluation. Asynchronous nodes should be evaluated using
		/// <see cref="EvaluateAsync(Variables)"/>.
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
			int i, c = this.processingInstructions.Length;

			if (Order == SearchMethod.DepthFirst)
			{
				for (i = 0; i < c; i++)
				{
					if (!this.processingInstructions[i].ForAllChildNodes(Callback, State, Order))
						return false;
				}

				if (!this.root.ForAllChildNodes(Callback, State, Order))
					return false;
			}

			ScriptNode NewNode;
			bool RecalcIsAsync = false;
			bool b;

			for (i = 0; i < this.nrInstructions; i++)
			{
				b = !Callback(this.processingInstructions[i], out NewNode, State);
				if (!(NewNode is null) && NewNode is XmlScriptProcessingInstruction Instruction)
				{
					this.processingInstructions[i] = Instruction;
					Instruction.SetParent(this);

					RecalcIsAsync = true;
				}

				if (b || (Order == SearchMethod.TreeOrder && !this.processingInstructions[i].ForAllChildNodes(Callback, State, Order)))
				{
					if (RecalcIsAsync)
						this.CalcIsAsync();

					return false;
				}
			}

			if (!(this.root is null))
			{
				b = !Callback(this.root, out NewNode, State);
				if (!(NewNode is null) && NewNode is XmlScriptElement NewRoot)
				{
					this.root = NewRoot;
					this.root.SetParent(this);

					RecalcIsAsync = true;
				}

				if (b || (Order == SearchMethod.TreeOrder && !this.root.ForAllChildNodes(Callback, State, Order)))
				{
					if (RecalcIsAsync)
						this.CalcIsAsync();

					return false;
				}
			}

			if (Order == SearchMethod.BreadthFirst)
			{
				for (i = 0; i < c; i++)
				{
					if (!this.processingInstructions[i].ForAllChildNodes(Callback, State, Order))
						return false;
				}

				if (!this.root.ForAllChildNodes(Callback, State, Order))
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
		/// Evaluates the node, using the variables provided in the <paramref name="Variables"/> collection.
		/// </summary>
		/// <param name="Variables">Variables collection.</param>
		/// <returns>Result.</returns>
		public override async Task<IElement> EvaluateAsync(Variables Variables)
		{
			if (!this.isAsync)
				return this.Evaluate(Variables);

			XmlDocument Doc = new XmlDocument()
			{
				PreserveWhitespace = true
			};

			await this.BuildAsync(Doc, null, Variables);

			return new ObjectValue(Doc);
		}

		/// <summary>
		/// Builds an XML Document object
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

		/// <summary>
		/// Builds an XML Document object
		/// </summary>
		/// <param name="Document">Document being built.</param>
		/// <param name="Parent">Parent element.</param>
		/// <param name="Variables">Current set of variables.</param>
		internal override async Task BuildAsync(XmlDocument Document, XmlElement Parent, Variables Variables)
		{
			foreach (XmlScriptProcessingInstruction PI in this.processingInstructions)
				await PI.BuildAsync(Document, Parent, Variables);

			if (!(this.root is null))
				await this.root.BuildAsync(Document, Parent, Variables);
		}

		/// <summary>
		/// Performs a pattern match operation.
		/// </summary>
		/// <param name="CheckAgainst">Value to check against.</param>
		/// <param name="AlreadyFound">Variables already identified.</param>
		/// <returns>Pattern match result</returns>
		public override PatternMatchResult PatternMatch(IElement CheckAgainst, Dictionary<string, IElement> AlreadyFound)
		{
			if (!(CheckAgainst?.AssociatedObjectValue is XmlNode Node))
				return PatternMatchResult.NoMatch;

			return this.PatternMatch(Node, AlreadyFound);
		}

		/// <summary>
		/// Performs a pattern match operation.
		/// </summary>
		/// <param name="CheckAgainst">Value to check against.</param>
		/// <param name="AlreadyFound">Variables already identified.</param>
		/// <returns>Pattern match result</returns>
		public override PatternMatchResult PatternMatch(XmlNode CheckAgainst, Dictionary<string, IElement> AlreadyFound)
		{
			System.Collections.IEnumerable Nodes;

			if (CheckAgainst is XmlDocument Doc)
				Nodes = Doc.ChildNodes;
			else if (CheckAgainst is XmlElement E)
				Nodes = new XmlElement[] { E };
			else
				return PatternMatchResult.NoMatch;

			int PiIndex = 0;
			bool RootMatched = false;
			PatternMatchResult Result;

			foreach (XmlNode N in Nodes)
			{
				if (N is XmlElement)
				{
					if (RootMatched || this.root is null)
						return PatternMatchResult.NoMatch;

					if ((Result = this.root.PatternMatch(N, AlreadyFound)) != PatternMatchResult.Match)
						return Result;

					RootMatched = true;
				}
				else if (N is XmlDeclaration)
				{
					if (PiIndex < this.processingInstructions.Length &&
						(Result = this.processingInstructions[PiIndex++].PatternMatch(N, AlreadyFound)) != PatternMatchResult.Match)
					{
						return Result;
					}
				}
				else if (N is XmlProcessingInstruction)
				{
					if (PiIndex >= this.processingInstructions.Length)
						return PatternMatchResult.NoMatch;
					else if ((Result = this.processingInstructions[PiIndex++].PatternMatch(N, AlreadyFound)) != PatternMatchResult.Match)
						return Result;
				}
				else if (N is XmlComment || N is XmlSignificantWhitespace || N is XmlWhitespace)
					continue;
				else
					return PatternMatchResult.NoMatch;
			}

			if (!RootMatched && !(this.root is null))
				return PatternMatchResult.NoMatch;

			if (PiIndex < this.processingInstructions.Length)
				return PatternMatchResult.NoMatch;

			return PatternMatchResult.Match;
		}

		/// <summary>
		/// If the node is applicable in pattern matching against <paramref name="CheckAgainst"/>.
		/// </summary>
		/// <param name="CheckAgainst">Value to check against.</param>
		/// <param name="First">First element</param>
		/// <returns>If the node is applicable for pattern matching.</returns>
		public override bool IsApplicable(XmlNode CheckAgainst, XmlElement First)
		{
			return CheckAgainst is XmlDocument;
		}

	}
}
