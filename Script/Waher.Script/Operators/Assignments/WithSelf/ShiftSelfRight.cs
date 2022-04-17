using Waher.Script.Model;
using Waher.Script.Operators.Binary;

namespace Waher.Script.Operators.Assignments.WithSelf
{
	/// <summary>
	/// Shift self right operator.
	/// </summary>
	public class ShiftSelfRight : Assignment 
	{
        /// <summary>
        /// Shift self right operator.
        /// </summary>
        /// <param name="VariableName">Variable name..</param>
        /// <param name="Operand">Operand.</param>
        /// <param name="Start">Start position in script expression.</param>
        /// <param name="Length">Length of expression covered by node.</param>
		/// <param name="Expression">Expression containing script.</param>
        public ShiftSelfRight(string VariableName, ScriptNode Operand, int Start, int Length, Expression Expression)
			: base(VariableName, new ShiftRight(new VariableReference(VariableName, true, Start, Length, Expression), Operand, Start, Length, Expression), Start, Length, Expression)
		{
        }
    }
}
