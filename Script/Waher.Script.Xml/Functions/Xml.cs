using System;
using System.Xml;
using System.Threading.Tasks;
using Waher.Script.Abstraction.Elements;
using Waher.Script.Model;
using Waher.Script.Objects;
using System.Collections.Generic;
using Waher.Content.Xml;

namespace Waher.Script.Xml.Functions
{
	/// <summary>
	/// Xml(s)
	/// </summary>
	public class Xml : FunctionOneScalarVariable
	{
		/// <summary>
		/// Xml(x)
		/// </summary>
		/// <param name="Argument">Argument.</param>
		/// <param name="Start">Start position in script expression.</param>
		/// <param name="Length">Length of expression covered by node.</param>
		/// <param name="Expression">Expression containing script.</param>
		public Xml(ScriptNode Argument, int Start, int Length, Expression Expression)
			: base(Argument, Start, Length, Expression)
		{
		}

		/// <summary>
		/// Name of the function
		/// </summary>
		public override string FunctionName => nameof(Xml);

		/// <summary>
		/// Evaluates the function on a scalar argument.
		/// </summary>
		/// <param name="Argument">Function argument.</param>
		/// <param name="Variables">Variables collection.</param>
		/// <returns>Function result.</returns>
		public override IElement EvaluateScalar(string Argument, Variables Variables)
		{
			return new ObjectValue(ToXml(Argument));
		}

		private static XmlDocument ToXml(string s)
		{
			return XML.ParseXml(s, true);
		}

		/// <summary>
		/// Evaluates the function on a scalar argument.
		/// </summary>
		/// <param name="Argument">Function argument.</param>
		/// <param name="Variables">Variables collection.</param>
		/// <returns>Function result.</returns>
		public override IElement EvaluateScalar(IElement Argument, Variables Variables)
		{
			object Obj = Argument.AssociatedObjectValue;

			if (Obj is null)
				return Argument;
			else if (Obj is string s)
				return this.EvaluateScalar(s, Variables);
			else if (Obj is XmlDocument)
				return Argument;
			else if (Obj is XmlNode N)
				return this.EvaluateScalar(N.OuterXml, Variables);
			else
				return base.EvaluateScalar(Argument, Variables);
		}

		/// <summary>
		/// Evaluates the function on a scalar argument.
		/// </summary>
		/// <param name="Argument">Function argument.</param>
		/// <param name="Variables">Variables collection.</param>
		/// <returns>Function result.</returns>
		public override Task<IElement> EvaluateScalarAsync(IElement Argument, Variables Variables)
		{
			return Task.FromResult(this.EvaluateScalar(Argument, Variables));
		}

		/// <summary>
		/// Performs a pattern match operation.
		/// </summary>
		/// <param name="CheckAgainst">Value to check against.</param>
		/// <param name="AlreadyFound">Variables already identified.</param>
		/// <returns>Pattern match result</returns>
		public override PatternMatchResult PatternMatch(IElement CheckAgainst, Dictionary<string, IElement> AlreadyFound)
		{
			object Obj = CheckAgainst.AssociatedObjectValue;

			if (Obj is null)
				return PatternMatchResult.NoMatch;
			else if (Obj is string s)
			{
				try
				{
					CheckAgainst = new ObjectValue(ToXml(s));
				}
				catch (Exception)
				{
					return PatternMatchResult.NoMatch;
				}

				return this.Argument.PatternMatch(CheckAgainst, AlreadyFound);
			}
			else if (Obj is XmlDocument)
				return this.Argument.PatternMatch(CheckAgainst, AlreadyFound);
			else if (Obj is XmlNode N)
			{
				CheckAgainst = new ObjectValue(ToXml(N.OuterXml));
				return this.PatternMatch(CheckAgainst, AlreadyFound);
			}
			else
				return PatternMatchResult.NoMatch;
		}
	}
}
