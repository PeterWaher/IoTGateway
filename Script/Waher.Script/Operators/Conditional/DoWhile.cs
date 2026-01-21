using System.Threading.Tasks;
using Waher.Script.Abstraction.Elements;
using Waher.Script.Exceptions;
using Waher.Script.Model;
using Waher.Script.Objects;

namespace Waher.Script.Operators.Conditional
{
	/// <summary>
	/// DO-WHILE operator.
	/// </summary>
	public class DoWhile : BinaryOperator
	{
		/// <summary>
		/// DO-WHILE operator.
		/// </summary>
		/// <param name="Statement">Statement.</param>
		/// <param name="Condition">Condition.</param>
		/// <param name="Start">Start position in script expression.</param>
		/// <param name="Length">Length of expression covered by node.</param>
		/// <param name="Expression">Expression containing script.</param>
		public DoWhile(ScriptNode Statement, ScriptNode Condition, int Start, int Length, Expression Expression)
			: base(Statement, Condition, Start, Length, Expression)
		{
		}

		/// <summary>
		/// Evaluates the node, using the variables provided in the <paramref name="Variables"/> collection.
		/// </summary>
		/// <param name="Variables">Variables collection.</param>
		/// <returns>Result.</returns>
		public override IElement Evaluate(Variables Variables)
		{
            IElement Last = ObjectValue.Null;
            BooleanValue Condition;

            do
            {
				try
				{
					Last = this.left.Evaluate(Variables);
				}
				catch (ScriptBreakLoopException ex)
				{
					if (ex.HasLoopValue)
						Last = ex.LoopValue;

					//ScriptBreakLoopException.Reuse(ex);
					break;
				}
				catch (ScriptContinueLoopException ex)
				{
					if (ex.HasLoopValue)
						Last = ex.LoopValue;

					//ScriptContinueLoopException.Reuse(ex);
				}

				Condition = this.right.Evaluate(Variables) as BooleanValue;
                if (Condition is null)
                    throw new ConditionNotBooleanScriptException(this);
            }
            while (Condition.Value);

            return Last;
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

			IElement Last = ObjectValue.Null;
			BooleanValue Condition;

			do
			{
				try
				{
					Last = await this.left.EvaluateAsync(Variables);
				}
				catch (ScriptBreakLoopException ex)
				{
					if (ex.HasLoopValue)
						Last = ex.LoopValue;

					//ScriptBreakLoopException.Reuse(ex);
					break;
				}
				catch (ScriptContinueLoopException ex)
				{
					if (ex.HasLoopValue)
						Last = ex.LoopValue;
				
					//ScriptContinueLoopException.Reuse(ex);
				}

				Condition = await this.right.EvaluateAsync(Variables) as BooleanValue;
				if (Condition is null)
					throw new ConditionNotBooleanScriptException(this);
			}
			while (Condition.Value);

			return Last;
		}
	}
}
