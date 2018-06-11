using System;
using System.Collections.Generic;
using System.Text;
using Waher.Script.Abstraction.Elements;
using Waher.Script.Exceptions;
using Waher.Script.Model;
using Waher.Script.Objects;

namespace Waher.Script.Operators.Calculus
{
	/// <summary>
	/// Default Differentiation operator.
	/// </summary>
	public class DefaultDifferentiation : UnaryOperator
	{
		private int nrDifferentiations;

		/// <summary>
		/// Default Differentiation operator.
		/// </summary>
		/// <param name="Operand">Operand.</param>
		/// <param name="NrDifferentiations">Number of differentiations.</param>
		/// <param name="Start">Start position in script expression.</param>
		/// <param name="Length">Length of expression covered by node.</param>
		/// <param name="Expression">Expression containing script.</param>
		public DefaultDifferentiation(ScriptNode Operand, int NrDifferentiations, int Start, int Length, Expression Expression)
			: base(Operand, Start, Length, Expression)
		{
			this.nrDifferentiations = NrDifferentiations;
		}

		/// <summary>
		/// Number of differentiations.
		/// </summary>
		public int NrDifferentiations
		{
			get { return this.nrDifferentiations; }
		}

		/// <summary>
		/// Evaluates the node, using the variables provided in the <paramref name="Variables"/> collection.
		/// </summary>
		/// <param name="Variables">Variables collection.</param>
		/// <returns>Result.</returns>
		public override IElement Evaluate(Variables Variables)
		{
			ScriptNode Node = this.op;
			IElement Result = null;
			int i;

			for (i = 0; i < this.nrDifferentiations; i++)
			{
				if (Node == null)
					return DoubleNumber.ZeroElement;

				Result = this.DifferentiateOnce(Node, Variables);
				Node = Result as ScriptNode;
			}

			return Result;
		}

		private IElement DifferentiateOnce(ScriptNode Node, Variables Variables)
		{
			if (Node is VariableReference VariableReference)
			{
				if (Variables.TryGetVariable(VariableReference.VariableName, out Variable v))
				{
					if (v.ValueObject is IDifferentiable Differentiable)
						return this.DifferentiateOnce(Differentiable, Variables);
					else if (v.ValueObject is ILambdaExpression)
						throw new ScriptRuntimeException(VariableReference.VariableName + " not differentiable.", this);
					else
						return DoubleNumber.ZeroElement;
				}
				else
				{
					LambdaDefinition f = Expression.GetFunctionLambdaDefinition(VariableReference.VariableName, this.Start, this.Length, this.Expression);
					if (f != null)
					{
						if (f is IDifferentiable Differentiable)
							return this.DifferentiateOnce(Differentiable, Variables);
						else
							throw new ScriptRuntimeException(VariableReference.VariableName + " not differentiable.", this);
					}
					else
						throw new ScriptRuntimeException(VariableReference.VariableName + " not defined.", this);
				}
			}
			else if (Node is IDifferentiable Differentiable)
				return this.DifferentiateOnce(Differentiable, Variables);
			else
				throw new ScriptRuntimeException("Not differentiable.", this);
		}

		private IElement DifferentiateOnce(IDifferentiable Differentiable, Variables Variables)
		{
			string VariableName = Differentiable.DefaultVariableName;
			if (string.IsNullOrEmpty(VariableName))
				throw new ScriptRuntimeException("No default variable available.", this);

			ScriptNode Result = Differentiable.Differentiate(VariableName, Variables);
			if (Result is IElement Element)
				return Element;

			return new LambdaDefinition(new string[] { VariableName }, new ArgumentType[] { ArgumentType.Normal },
				Result, this.Start, this.Length, this.Expression);
		}

	}
}
