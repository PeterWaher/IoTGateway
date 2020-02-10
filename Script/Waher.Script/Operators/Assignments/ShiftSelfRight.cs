using System;
using System.Collections.Generic;
using System.Text;
using Waher.Script.Abstraction.Elements;
using Waher.Script.Model;
using Waher.Script.Operators.Binary;

namespace Waher.Script.Operators.Assignments
{
	/// <summary>
	/// Shift self right operator.
	/// </summary>
	public class ShiftSelfRight : Assignment 
	{
        private readonly ShiftRight shiftRight;

        /// <summary>
        /// Shift self right operator.
        /// </summary>
        /// <param name="VariableName">Variable name..</param>
        /// <param name="Operand">Operand.</param>
        /// <param name="Start">Start position in script expression.</param>
        /// <param name="Length">Length of expression covered by node.</param>
		/// <param name="Expression">Expression containing script.</param>
        public ShiftSelfRight(string VariableName, ScriptNode Operand, int Start, int Length, Expression Expression)
			: base(VariableName, Operand, Start, Length, Expression)
		{
            this.shiftRight = new ShiftRight(new VariableReference(VariableName, true, Start, Length, Expression), Operand, Start, Length, Expression);
        }

        /// <summary>
        /// Evaluates the node, using the variables provided in the <paramref name="Variables"/> collection.
        /// </summary>
        /// <param name="Variables">Variables collection.</param>
        /// <returns>Result.</returns>
        public override IElement Evaluate(Variables Variables)
		{
            IElement Result = this.shiftRight.Evaluate(Variables);
            Variables[this.VariableName] = Result;
            return Result;
        }
    }
}
