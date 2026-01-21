using System.Threading.Tasks;
using Waher.Script.Abstraction.Elements;
using Waher.Script.Exceptions;
using Waher.Script.Model;

namespace Waher.Script.Operators.Assignments.WithSelf
{
	/// <summary>
	/// Add to self operator.
	/// </summary>
	public class AddToSelf : Assignment 
	{
		/// <summary>
		/// Add to self operator.
		/// </summary>
		/// <param name="VariableName">Variable name..</param>
		/// <param name="Operand">Operand.</param>
		/// <param name="Start">Start position in script expression.</param>
		/// <param name="Length">Length of expression covered by node.</param>
		/// <param name="Expression">Expression containing script.</param>
		public AddToSelf(string VariableName, ScriptNode Operand, int Start, int Length, Expression Expression)
			: base(VariableName, Operand, Start, Length, Expression)
		{
		}

		/// <summary>
		/// Evaluates the node, using the variables provided in the <paramref name="Variables"/> collection.
		/// </summary>
		/// <param name="Variables">Variables collection.</param>
		/// <returns>Result.</returns>
		public override IElement Evaluate(Variables Variables)
		{
			if (!Variables.TryGetVariable(this.VariableName, out Variable v))
				new VariableNotFoundScriptException(this.VariableName, this);

			IElement E = this.op.Evaluate(Variables);
            E = Arithmetics.Add.EvaluateAddition(v.ValueElement, E, this);

            Variables[this.VariableName] = E;

            return E;
		}

		/// <summary>
		/// Evaluates the node, using the variables provided in the <paramref name="Variables"/> collection.
		/// </summary>
		/// <param name="Variables">Variables collection.</param>
		/// <returns>Result.</returns>
		public override async Task<IElement> EvaluateAsync(Variables Variables)
		{
			if (!this.isAsync)
				return this.Evaluate(Variables);

			if (!Variables.TryGetVariable(this.VariableName, out Variable v))
				new VariableNotFoundScriptException(this.VariableName, this);

			IElement E = await this.op.EvaluateAsync(Variables);
			E = Arithmetics.Add.EvaluateAddition(v.ValueElement, E, this);

			Variables[this.VariableName] = E;

			return E;
		}
	}
}
