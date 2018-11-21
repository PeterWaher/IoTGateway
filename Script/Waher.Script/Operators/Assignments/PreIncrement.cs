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
	/// Pre-Increment operator.
	/// </summary>
	public class PreIncrement : ScriptLeafNode
	{
		private readonly string variableName;
		
		/// <summary>
		/// Pre-Increment operator.
		/// </summary>
		/// <param name="VariableName">Variable name..</param>
		/// <param name="Start">Start position in script expression.</param>
		/// <param name="Length">Length of expression covered by node.</param>
		/// <param name="Expression">Expression containing script.</param>
		public PreIncrement(string VariableName, int Start, int Length, Expression Expression)
			: base(Start, Length, Expression)
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
				Value = new DoubleNumber(n.Value + 1);
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
			ICommutativeRingWithIdentityElement e = Value as ICommutativeRingWithIdentityElement;
			if (e != null)
				return Operators.Arithmetics.Add.EvaluateAddition(Value, e.One, Node);
			else if (Value.IsScalar)
				throw new ScriptRuntimeException("Unable to increment variable.", Node);
			else
			{
				LinkedList<IElement> Elements = new LinkedList<IElement>();

				foreach (IElement Element in Value.ChildElements)
					Elements.AddLast(Increment(Element, Node));

				return Value.Encapsulate(Elements, Node);
			}
		}

	}
}
