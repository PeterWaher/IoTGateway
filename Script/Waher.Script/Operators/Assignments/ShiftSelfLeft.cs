using System;
using System.Collections.Generic;
using System.Text;
using Waher.Script.Abstraction.Elements;
using Waher.Script.Exceptions;
using Waher.Script.Model;
using Waher.Script.Operators.Binary;

namespace Waher.Script.Operators.Assignments
{
	/// <summary>
	/// Shift self left operator.
	/// </summary>
	public class ShiftSelfLeft : Assignment 
	{
        private ShiftLeft shiftLeft;

		/// <summary>
		/// Shift self left operator.
		/// </summary>
		/// <param name="VariableName">Variable name..</param>
		/// <param name="Operand">Operand.</param>
		/// <param name="Start">Start position in script expression.</param>
		/// <param name="Length">Length of expression covered by node.</param>
		public ShiftSelfLeft(string VariableName, ScriptNode Operand, int Start, int Length)
			: base(VariableName, Operand, Start, Length)
		{
            this.shiftLeft = new ShiftLeft(new VariableReference(VariableName, true, Start, Length), Operand, Start, Length);
		}

		/// <summary>
		/// Evaluates the node, using the variables provided in the <paramref name="Variables"/> collection.
		/// </summary>
		/// <param name="Variables">Variables collection.</param>
		/// <returns>Result.</returns>
		public override IElement Evaluate(Variables Variables)
		{
            IElement Result = this.shiftLeft.Evaluate(Variables);
            Variables[this.VariableName] = Result;
            return Result;
        }
    }
}
