﻿using System;
using System.Collections.Generic;
using System.Text;
using Waher.Script.Abstraction.Elements;
using Waher.Script.Model;
using Waher.Script.Operators.Logical;

namespace Waher.Script.Operators.Assignments
{
	/// <summary>
	/// Logical Or with self operator.
	/// </summary>
	public class LogicalOrWithSelf : Assignment 
	{
        private Or or;

        /// <summary>
        /// Logical Or with self operator.
        /// </summary>
        /// <param name="VariableName">Variable name..</param>
        /// <param name="Operand">Operand.</param>
        /// <param name="Start">Start position in script expression.</param>
        /// <param name="Length">Length of expression covered by node.</param>
        public LogicalOrWithSelf(string VariableName, ScriptNode Operand, int Start, int Length)
			: base(VariableName, Operand, Start, Length)
		{
            this.or = new Or(new VariableReference(VariableName, true, Start, Length), Operand, Start, Length);
        }

        /// <summary>
        /// Evaluates the node, using the variables provided in the <paramref name="Variables"/> collection.
        /// </summary>
        /// <param name="Variables">Variables collection.</param>
        /// <returns>Result.</returns>
        public override IElement Evaluate(Variables Variables)
		{
            IElement Result = this.or.Evaluate(Variables);
            Variables[this.VariableName] = Result;
            return Result;
        }
    }
}
