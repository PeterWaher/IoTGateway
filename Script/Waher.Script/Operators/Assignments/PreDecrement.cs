using System;
using System.Collections.Generic;
using System.Text;
using Waher.Script.Abstraction.Elements;
using Waher.Script.Exceptions;
using Waher.Script.Model;
using Waher.Script.Objects;

namespace Waher.Script.Operators.Assignments
{
	/// <summary>
	/// Pre-Decrement operator.
	/// </summary>
	public class PreDecrement : ScriptNode
	{
		private string variableName;
		
		/// <summary>
		/// Pre-Decrement operator.
		/// </summary>
		/// <param name="VariableName">Variable name..</param>
		/// <param name="Start">Start position in script expression.</param>
		/// <param name="Length">Length of expression covered by node.</param>
		public PreDecrement(string VariableName, int Start, int Length)
			: base(Start, Length)
		{
			this.variableName = VariableName;
		}

		/// <summary>
		/// Name of variable
		/// </summary>
		public string VariableName
		{
			get { return this.variableName; }
		}

		/// <summary>
		/// Evaluates the node, using the variables provided in the <paramref name="Variables"/> collection.
		/// </summary>
		/// <param name="Variables">Variables collection.</param>
		/// <returns>Result.</returns>
		public override IElement Evaluate(Variables Variables)
		{
			Variable v;

			if (!Variables.TryGetVariable(this.variableName, out v))
				throw new ScriptRuntimeException("Variable not found: " + this.variableName, this);

			IElement Value = v.ValueElement;
			DoubleNumber n = Value as DoubleNumber;

			if (n != null)
				Value = new DoubleNumber(n.Value - 1);
			else
				Value = Decrement(Value, this);

			v.ValueElement = Value;

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
			ICommutativeRingWithIdentityElement e = Value as ICommutativeRingWithIdentityElement;
			if (e != null)
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
