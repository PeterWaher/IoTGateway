using System.Collections.Generic;
using System.Threading.Tasks;
using System.Xml;
using Waher.Script.Abstraction.Elements;
using Waher.Script.Model;
using Waher.Script.Objects;

namespace Waher.Script.Xml.Model
{
	/// <summary>
	/// XML Script attribute node, whose value is defined by script.
	/// </summary>
	public class XmlScriptAttributeScript : XmlScriptAttribute
	{
		private ScriptNode node;
		private bool isAsync;

		/// <summary>
		/// XML Script attribute node, whose value is defined by script.
		/// </summary>
		/// <param name="Name">Element name.</param>
		/// <param name="Node">Script node.</param>
		/// <param name="Start">Start position in script expression.</param>
		/// <param name="Length">Length of expression covered by node.</param>
		/// <param name="Expression">Expression containing script.</param>
		public XmlScriptAttributeScript(string Name, ScriptNode Node, int Start, int Length, Expression Expression)
			: base(Name, Start, Length, Expression)
		{
			this.node = Node;
			this.node?.SetParent(this);

			this.isAsync = Node?.IsAsynchronous ?? false;
		}

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
		/// <param name="DepthFirst">If calls are made depth first (true) or on each node and then its leaves (false).</param>
		/// <returns>If the process was completed.</returns>
		public override bool ForAllChildNodes(ScriptNodeEventHandler Callback, object State, bool DepthFirst)
		{
			if (DepthFirst)
			{
				if (!this.node.ForAllChildNodes(Callback, State, DepthFirst))
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

			if (!DepthFirst)
			{
				if (!this.node.ForAllChildNodes(Callback, State, DepthFirst))
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
			string s = EvaluateString(this.node, Variables);
			if (!(s is null))
				Parent.SetAttribute(this.Name, s);
		}

		/// <summary>
		/// Builds an XML Document object
		/// </summary>
		/// <param name="Document">Document being built.</param>
		/// <param name="Parent">Parent element.</param>
		/// <param name="Variables">Current set of variables.</param>
		internal override async Task BuildAsync(XmlDocument Document, XmlElement Parent, Variables Variables)
		{
			string s = await EvaluateStringAsync(this.node, Variables);
			if (!(s is null))
				Parent.SetAttribute(this.Name, s);
		}

		/// <summary>
		/// Gets the attribute value.
		/// </summary>
		/// <param name="Variables">Current set of variables.</param>
		internal override string GetValue(Variables Variables)
		{
			return EvaluateString(this.node, Variables);
		}

		/// <summary>
		/// Gets the attribute value.
		/// </summary>
		/// <param name="Variables">Current set of variables.</param>
		internal override Task<string> GetValueAsync(Variables Variables)
		{
			return EvaluateStringAsync(this.node, Variables);
		}

		/// <summary>
		/// Performs a pattern match operation.
		/// </summary>
		/// <param name="CheckAgainst">Value to check against.</param>
		/// <param name="AlreadyFound">Variables already identified.</param>
		/// <returns>Pattern match result</returns>
		public override PatternMatchResult PatternMatch(XmlNode CheckAgainst, Dictionary<string, IElement> AlreadyFound)
		{
			if (CheckAgainst is XmlAttribute)
				return this.node.PatternMatch(new StringValue(CheckAgainst.Value), AlreadyFound);
			else if (CheckAgainst is null)
				return this.node.PatternMatch(ObjectValue.Null, AlreadyFound);
			else
				return PatternMatchResult.NoMatch;
		}

		/// <summary>
		/// Performs a pattern match operation.
		/// </summary>
		/// <param name="CheckAgainst">Value to check against.</param>
		/// <param name="AlreadyFound">Variables already identified.</param>
		/// <returns>Pattern match result</returns>
		public override PatternMatchResult PatternMatch(string CheckAgainst, Dictionary<string, IElement> AlreadyFound)
		{
			if (CheckAgainst is null)
				return this.node.PatternMatch(ObjectValue.Null, AlreadyFound);
			else
				return this.node.PatternMatch(new StringValue(CheckAgainst), AlreadyFound);
		}

		/// <summary>
		/// If the node is applicable in pattern matching against <paramref name="CheckAgainst"/>.
		/// </summary>
		/// <param name="CheckAgainst">Value to check against.</param>
		/// <returns>If the node is applicable for pattern matching.</returns>
		public override bool IsApplicable(string CheckAgainst)
		{
			return true;
		}
	}
}
