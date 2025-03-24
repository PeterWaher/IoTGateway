using System;
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

namespace Waher.Script.Xml.Model
{
	/// <summary>
	/// Base class for all XML Script nodes in a parsed script tree.
	/// </summary>
	public abstract class XmlScriptNode : ScriptNode
	{
		/// <summary>
		/// Base class for all XML Script nodes in a parsed script tree.
		/// </summary>
		/// <param name="Start">Start position in script expression.</param>
		/// <param name="Length">Length of expression covered by node.</param>
		/// <param name="Expression">Expression containing script.</param>
		public XmlScriptNode(int Start, int Length, Expression Expression)
			: base(Start, Length, Expression)
		{
		}

		/// <summary>
		/// Evaluates the node, using the variables provided in the <paramref name="Variables"/> collection.
		/// </summary>
		/// <param name="Variables">Variables collection.</param>
		/// <returns>Result.</returns>
		public override IElement Evaluate(Variables Variables)
		{
			return ObjectValue.Null;
		}

		/// <summary>
		/// Builds an XML Document object
		/// </summary>
		/// <param name="Document">Document being built.</param>
		/// <param name="Parent">Parent element.</param>
		/// <param name="Variables">Current set of variables.</param>
		internal abstract void Build(XmlDocument Document, XmlElement Parent, Variables Variables);

		/// <summary>
		/// Builds an XML Document object
		/// </summary>
		/// <param name="Document">Document being built.</param>
		/// <param name="Parent">Parent element.</param>
		/// <param name="Variables">Current set of variables.</param>
		internal virtual Task BuildAsync(XmlDocument Document, XmlElement Parent, Variables Variables)
		{
			this.Build(Document, Parent, Variables);
			return Task.CompletedTask;
		}

		/// <summary>
		/// Evaluates a script node to a string.
		/// </summary>
		/// <param name="Node">Node to evaluate.</param>
		/// <param name="Variables">Variables.</param>
		/// <returns>String result.</returns>
		public static string EvaluateString(ScriptNode Node, Variables Variables)
		{
			return EvaluateString(Node.Evaluate(Variables));
		}

		/// <summary>
		/// Evaluates a script node to a string.
		/// </summary>
		/// <param name="Node">Node to evaluate.</param>
		/// <param name="Variables">Variables.</param>
		/// <returns>String result.</returns>
		public static async Task<string> EvaluateStringAsync(ScriptNode Node, Variables Variables)
		{
			return EvaluateString(await Node.EvaluateAsync(Variables));
		}

		/// <summary>
		/// Evaluates a script element to a string.
		/// </summary>
		/// <param name="Element">Element to convert to a string.</param>
		/// <returns>String result.</returns>
		public static string EvaluateString(IElement Element)
		{
			object Obj = Element.AssociatedObjectValue;

			if (Obj is string s)
				return s;
			else if (Obj is bool b)
				return CommonTypes.Encode(b);
			else if (Obj is double d)
				return CommonTypes.Encode(d);
			else if (Obj is DateTime TP)
				return XML.Encode(TP, TP.TimeOfDay == TimeSpan.Zero);
			else if (Obj is BigInteger I)
				return I.ToString();
			else
			{
				if (Obj is null)
					return null;
				else if (Obj is CaseInsensitiveString cis)
					return cis.Value;
				else
					return Expression.ToString(Obj);
			}
		}

		/// <summary>
		/// Performs a pattern match operation.
		/// </summary>
		/// <param name="CheckAgainst">Value to check against.</param>
		/// <param name="AlreadyFound">Variables already identified.</param>
		/// <returns>Pattern match result</returns>
		public abstract PatternMatchResult PatternMatch(XmlNode CheckAgainst, Dictionary<string, IElement> AlreadyFound);

		/// <summary>
		/// If the node is applicable in pattern matching against <paramref name="CheckAgainst"/>.
		/// </summary>
		/// <param name="CheckAgainst">Value to check against.</param>
		/// <param name="First">First element</param>
		/// <returns>If the node is applicable for pattern matching.</returns>
		public abstract bool IsApplicable(XmlNode CheckAgainst, XmlElement First);

		/// <summary>
		/// If the node represents a vector of nodes.
		/// </summary>
		public virtual bool IsVector
		{
			get => false;
		}

		/// <summary>
		/// If the node represents whitespace.
		/// </summary>
		public virtual bool IsWhitespace => false;
	}
}
