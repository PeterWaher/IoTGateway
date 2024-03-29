﻿using Waher.Script.Model;
using Waher.Script.Operators.Logical;

namespace Waher.Script.Operators.Assignments.WithSelf
{
	/// <summary>
	/// Logical And with self operator.
	/// </summary>
	public class LogicalAndWithSelf : Assignment 
	{
        /// <summary>
        /// Logical And with self operator.
        /// </summary>
        /// <param name="VariableName">Variable name..</param>
        /// <param name="Operand">Operand.</param>
        /// <param name="Start">Start position in script expression.</param>
        /// <param name="Length">Length of expression covered by node.</param>
		/// <param name="Expression">Expression containing script.</param>
        public LogicalAndWithSelf(string VariableName, ScriptNode Operand, int Start, int Length, Expression Expression)
			: base(VariableName, new And(new VariableReference(VariableName, true, Start, Length, Expression), Operand, Start, Length, Expression), Start, Length, Expression)
		{
        }
    }
}
