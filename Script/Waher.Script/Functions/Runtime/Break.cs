using System.Threading.Tasks;
using Waher.Script.Abstraction.Elements;
using Waher.Script.Exceptions;
using Waher.Script.Model;
using Waher.Script.Objects;

namespace Waher.Script.Functions.Runtime
{
	/// <summary>
	/// Breaks a loop, with or without providing a loop value.
	/// </summary>
	public class Break : FunctionOneVariable
	{
		private readonly bool hasValue;

		/// <summary>
		/// Breaks a loop, without providing a loop value.
		/// </summary>
		/// <param name="Start">Start position in script expression.</param>
		/// <param name="Length">Length of expression covered by node.</param>
		/// <param name="Expression">Expression containing script.</param>
		public Break(int Start, int Length, Expression Expression)
			: base(new ConstantElement(ObjectValue.Null, Start, Length, Expression),
				  Start, Length, Expression)
		{
			this.hasValue = false;
		}

		/// <summary>
		/// Breaks a loop, providing a loop value.
		/// </summary>
		/// <param name="Argument">Argument.</param>
		/// <param name="Start">Start position in script expression.</param>
		/// <param name="Length">Length of expression covered by node.</param>
		/// <param name="Expression">Expression containing script.</param>
		public Break(ScriptNode Argument, int Start, int Length, Expression Expression)
			: base(Argument, Start, Length, Expression)
		{
			this.hasValue = true;
		}

		/// <summary>
		/// Name of the function
		/// </summary>
		public override string FunctionName => nameof(Break);

		/// <summary>
		/// Evaluates the node, using the variables provided in the <paramref name="Variables"/> collection.
		/// </summary>
		/// <param name="Variables">Variables collection.</param>
		/// <returns>Result.</returns>
		public override IElement Evaluate(Variables Variables)
		{
			if (this.hasValue)
				return base.Evaluate(Variables);
			else
			{
				//ScriptBreakLoopException.TryThrowReused();
				throw new ScriptBreakLoopException();
			}
		}

		/// <summary>
		/// Evaluates the node, using the variables provided in the <paramref name="Variables"/> collection.
		/// </summary>
		/// <param name="Variables">Variables collection.</param>
		/// <returns>Result.</returns>
		public override Task<IElement> EvaluateAsync(Variables Variables)
		{
			if (this.hasValue)
				return base.EvaluateAsync(Variables);
			else
			{
				//ScriptBreakLoopException.TryThrowReused();
				throw new ScriptBreakLoopException();
			}
		}

		/// <summary>
		/// Evaluates the function.
		/// </summary>
		/// <param name="Argument">Function argument.</param>
		/// <param name="Variables">Variables collection.</param>
		/// <returns>Function result.</returns>
		public override IElement Evaluate(IElement Argument, Variables Variables)
		{
			//ScriptBreakLoopException.TryThrowReused(Argument);
			throw new ScriptBreakLoopException(Argument);
		}
	}
}
