using Waher.Runtime.Collections;
using Waher.Script.Abstraction.Elements;
using Waher.Script.Exceptions;
using Waher.Script.Model;
using Waher.Script.Objects;

namespace Waher.Script.Operators.Assignments.Pre
{
	/// <summary>
	/// Pre-Increment operator.
	/// </summary>
	public class PreIncrement : ScriptLeafNodeVariableReference
	{
		/// <summary>
		/// Pre-Increment operator.
		/// </summary>
		/// <param name="VariableName">Variable name..</param>
		/// <param name="Start">Start position in script expression.</param>
		/// <param name="Length">Length of expression covered by node.</param>
		/// <param name="Expression">Expression containing script.</param>
		public PreIncrement(string VariableName, int Start, int Length, Expression Expression)
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
				throw new VariableNotFoundScriptException(this.variableName, this);

			IElement Value = v.ValueElement;

			if (Value.AssociatedObjectValue is double d)
				Value = new DoubleNumber(d + 1);
			else
				Value = Increment(Value, this);

			Variables[this.variableName] = Value;

            return Value;
		}

		/// <summary>
		/// Increments a value.
		/// </summary>
		/// <param name="Value">Value to increment.</param>
		/// <param name="Node">Node performing the evaluation.</param>
		/// <returns>Incremented value.</returns>
		public static IElement Increment(IElement Value, ScriptNode Node)
		{
			if (Value is ICommutativeRingWithIdentityElement e)
				return Operators.Arithmetics.Add.EvaluateAddition(Value, e.One, Node);
			else if (Value.IsScalar)
				throw new ScriptRuntimeException("Unable to increment variable.", Node);
			else
			{
				ChunkedList<IElement> Elements = new ChunkedList<IElement>();

				foreach (IElement Element in Value.ChildElements)
					Elements.Add(Increment(Element, Node));

				return Value.Encapsulate(Elements, Node);
			}
		}

	}
}
