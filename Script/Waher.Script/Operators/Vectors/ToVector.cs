using System.Collections.Generic;
using Waher.Runtime.Collections;
using Waher.Script.Abstraction.Elements;
using Waher.Script.Abstraction.Sets;
using Waher.Script.Functions.Vectors;
using Waher.Script.Model;
using Waher.Script.Objects;

namespace Waher.Script.Operators.Vectors
{
	/// <summary>
	/// To-Vector operator.
	/// </summary>
	public class ToVector : NullCheckUnaryOperator, IIterativeEvaluation
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
			return this.ConvertToVector(Operand);
		}

		private IElement ConvertToVector(IElement Operand)
		{
			if (this.nullCheck && Operand.AssociatedObjectValue is null)
				return Operand;

			if (Operand is IVectorSpaceElement)
				return Operand;

			if (Operand is IVector V)
				return VectorDefinition.Encapsulate(V.VectorElements, false, this);

			if (Operand is ISet S)
				return VectorDefinition.Encapsulate(S.ChildElements, false, this);

			if (Operand is IToVector ToVector)
				return ToVector.ToVector();

			if (Operand?.AssociatedObjectValue is IToVector ToVector2)
				return ToVector2.ToVector();

			return VectorDefinition.Encapsulate(new IElement[] { Operand }, false, this);
		}

		/// <summary>
		/// Performs a pattern match operation.
		/// </summary>
		/// <param name="CheckAgainst">Value to check against.</param>
		/// <param name="AlreadyFound">Variables already identified.</param>
		/// <returns>Pattern match result</returns>
		public override PatternMatchResult PatternMatch(IElement CheckAgainst, Dictionary<string, IElement> AlreadyFound)
		{
			if (!(CheckAgainst is IVector Vector))
			{
				Vector = this.ConvertToVector(CheckAgainst) as IVector;
				if (Vector is null)
					return PatternMatchResult.Match;
			}

			if (this.op is VariableReference Ref)
				return this.op.PatternMatch(Vector, AlreadyFound);

			Dictionary<string, PatternRec> VariableNames = null;

			this.ForAllChildNodes((ScriptNode Node, out ScriptNode NewNode, object State) =>
			{
				NewNode = null;

				if (Node is VariableReference Ref2)
				{
					if (VariableNames is null)
						VariableNames = new Dictionary<string, PatternRec>();

					if (!VariableNames.ContainsKey(Ref2.VariableName))
					{
						if (AlreadyFound.TryGetValue(Ref2.VariableName, out IElement E))
						{
							VariableNames[Ref2.VariableName] = new PatternRec()
							{
								New = null,
								Prev = E
							};
						}
						else
						{
							VariableNames[Ref2.VariableName] = new PatternRec()
							{
								New = new ChunkedList<IElement>(),
								Prev = null
							};
						}
					}
				}

				return true;
			}, null, SearchMethod.TreeOrder);

			foreach (IElement Element in Vector.VectorElements)
			{
				switch (this.op.PatternMatch(Element, AlreadyFound))
				{
					case PatternMatchResult.Match:
						if (!(VariableNames is null))
						{
							foreach (KeyValuePair<string, PatternRec> P in VariableNames)
							{
								if (!(P.Value.New is null))
								{
									if (AlreadyFound.TryGetValue(P.Key, out IElement E))
									{
										P.Value.New.Add(E);
										AlreadyFound.Remove(P.Key);
									}
									else
										P.Value.New.Add(ObjectValue.Null);
								}
							}
						}
						break;

					case PatternMatchResult.NoMatch:
						return PatternMatchResult.NoMatch;

					case PatternMatchResult.Unknown:
					default:
						return PatternMatchResult.Unknown;
				}
			}

			if (!(VariableNames is null))
			{
				foreach (KeyValuePair<string, PatternRec> P in VariableNames)
				{
					if (!(P.Value.New is null))
						AlreadyFound[P.Key] = VectorDefinition.Encapsulate(P.Value.New, false, this);
				}
			}

			return PatternMatchResult.Match;
		}

		private class PatternRec
		{
			public ChunkedList<IElement> New;
			public IElement Prev;
		}

		#region IIterativeEvaluation

		/// <summary>
		/// If the node can be evaluated iteratively.
		/// </summary>
		public bool CanEvaluateIteratively => true;

		/// <summary>
		/// Creates an iterative evaluator for the node.
		/// </summary>
		/// <returns>Iterative evaluator reference.</returns>
		public IIterativeEvaluator CreateEvaluator() => new ToVectorEvaluator(this);

		#endregion
	}
}
