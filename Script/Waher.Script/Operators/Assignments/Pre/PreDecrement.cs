using System;
using System.Collections.Generic;
using Waher.Script.Abstraction.Elements;
using Waher.Script.Exceptions;
using Waher.Script.Model;
using Waher.Script.Objects;

namespace Waher.Script.Operators.Assignments.Pre
{
	/// <summary>
	/// Pre-Decrement operator.
	/// </summary>
	public class PreDecrement : ScriptLeafNodeVariableReference
	{
		/// <summary>
		/// Pre-Decrement operator.
		/// </summary>
		/// <param name="VariableName">Variable name..</param>
		/// <param name="Start">Start position in script expression.</param>
		/// <param name="Length">Length of expression covered by node.</param>
		/// <param name="Expression">Expression containing script.</param>
		public PreDecrement(string VariableName, int Start, int Length, Expression Expression)
			: base(VariableName, Start, Length, Expression)
		{
		}

		/// <summary>
		/// Evaluates the node, using the variables provided in the <paramref name="Variables"/> collection.
		/// </summary>
		/// <param name="Variables">Variables collection.</param>
		/// <returns>Result.</returns>
		public override IElement Evaluate(Variables Variables)
		{
			if (!Variables.TryGetVariable(this.variableName, out Variable v))
				throw new ScriptRuntimeException("Variable not found: " + this.variableName, this);

			IElement Value = v.ValueElement;

			if (Value is DoubleNumber n)
				Value = new DoubleNumber(n.Value - 1);
			else
				Value = Decrement(Value, this);

            Variables[this.variableName] = Value;

            return Value;
		}

		/// <summary>
		/// Decrements a value.
		/// </summary>
		/// <param name="Value">Value to decrement.</param>
		/// <param name="Node">Node performing the evaluation.</param>
		/// <returns>Decremented value.</returns>
		public static IElement Decrement(IElement Value, ScriptNode Node)
		{
			if (Value is ICommutativeRingWithIdentityElement e)
				return Operators.Arithmetics.Subtract.EvaluateSubtraction(Value, e.One, Node);
			else if (Value.IsScalar)
				throw new ScriptRuntimeException("Unable to increment variable.", Node);
			else
			{
				LinkedList<IElement> Elements = new LinkedList<IElement>();

				foreach (IElement Element in Value.ChildElements)
					Elements.AddLast(Decrement(Element, Node));

				return Value.Encapsulate(Elements, Node);
			}
		}

	}
}
