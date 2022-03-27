using System;
using System.Collections.Generic;
using Waher.Script.Abstraction.Elements;
using Waher.Script.Abstraction.Sets;
using Waher.Script.Model;
using Waher.Script.Objects;

namespace Waher.Script.Operators.Vectors
{
	/// <summary>
	/// To-Vector operator.
	/// </summary>
	public class ToVector : NullCheckUnaryOperator
	{
		/// <summary>
		/// To-Vector operator.
		/// </summary>
		/// <param name="Operand">Operand.</param>
		/// <param name="NullCheck">If null should be returned if left operand is null.</param>
		/// <param name="Start">Start position in script expression.</param>
		/// <param name="Length">Length of expression covered by node.</param>
		/// <param name="Expression">Expression containing script.</param>
		public ToVector(ScriptNode Operand, bool NullCheck, int Start, int Length, Expression Expression)
			: base(Operand, NullCheck, Start, Length, Expression)
		{
		}

		/// <summary>
		/// Evaluates the node, using the variables provided in the <paramref name="Variables"/> collection.
		/// </summary>
		/// <param name="Operand">Operand.</param>
		/// <param name="Variables">Variables collection.</param>
		/// <returns>Result.</returns>
		public override IElement Evaluate(IElement Operand, Variables Variables)
		{
			return ConvertToVector(Operand);
		}

		private IElement ConvertToVector(IElement E)
		{
			if (this.nullCheck && E.AssociatedObjectValue is null)
				return E;

			if (E is IVectorSpaceElement)
				return E;

			if (E is IVector V)
				return VectorDefinition.Encapsulate(V.VectorElements, false, this);

			if (E is ISet S)
				return VectorDefinition.Encapsulate(S.ChildElements, false, this);

			if (this.nullCheck && E.AssociatedObjectValue is null)
				return E;

			return VectorDefinition.Encapsulate(new IElement[] { E }, false, this);
		}

		/// <summary>
		/// Performs a pattern match operation.
		/// </summary>
		/// <param name="CheckAgainst">Value to check against.</param>
		/// <param name="AlreadyFound">Variables already identified.</param>
		/// <returns>Pattern match result</returns>
		public override PatternMatchResult PatternMatch(IElement CheckAgainst, Dictionary<string, IElement> AlreadyFound)
		{
			bool VectorOfObjects = this.op is ObjectExNihilo;
			string VariableName = null;

			if (!VectorOfObjects)
			{
				if (!this.ForAllChildNodes((ScriptNode Node, out ScriptNode NewNode, object State) =>
				{
					NewNode = null;

					if (Node is VariableReference Ref)
					{
						if (VariableName is null)
							VariableName = Ref.VariableName;
						else if (VariableName != Ref.VariableName)
							return false;
					}

					return true;
				}, null, true))
				{
					return PatternMatchResult.Unknown;
				}
			}

			if (!(CheckAgainst is IVector Vector))
			{
				Vector = this.ConvertToVector(CheckAgainst) as IVector;
				if (Vector is null)
					return PatternMatchResult.Unknown;
			}

			bool HasVariable = !string.IsNullOrEmpty(VariableName);

			if (HasVariable && AlreadyFound.ContainsKey(VariableName))
				return this.op.PatternMatch(CheckAgainst, AlreadyFound);

			List<IElement> Elements = HasVariable || VectorOfObjects ? new List<IElement>() : null;

			foreach (IElement Element in Vector.VectorElements)
			{
				if (VectorOfObjects)
				{
					Dictionary<string, IElement> ObjProperties = new Dictionary<string, IElement>();

					PatternMatchResult Result = this.op.PatternMatch(Element, ObjProperties);
					if (Result != PatternMatchResult.Match)
						return Result;

					Elements.Add(new ObjectValue(ObjProperties));
				}
				else
				{
					switch (this.op.PatternMatch(Element, AlreadyFound))
					{
						case PatternMatchResult.Match:
							if (HasVariable)
							{
								if (AlreadyFound.TryGetValue(VariableName, out IElement Item))
								{
									Elements.Add(Item);
									AlreadyFound.Remove(VariableName);
								}
								else
									Elements.Add(ObjectValue.Null);
							}
							break;

						case PatternMatchResult.NoMatch:
							return PatternMatchResult.NoMatch;

						case PatternMatchResult.Unknown:
						default:
							return PatternMatchResult.Unknown;
					}
				}
			}

			if (HasVariable)
				AlreadyFound[VariableName] = VectorDefinition.Encapsulate(Elements, false, this);
			else if (VectorOfObjects)
			{
				int i = 1;
				string s = "v1";

				while (AlreadyFound.ContainsKey(s))
				{
					i++;
					s = "v" + i.ToString();
				}

				AlreadyFound[s]= VectorDefinition.Encapsulate(Elements, false, this);
			}

			return PatternMatchResult.Match;
		}

	}
}
