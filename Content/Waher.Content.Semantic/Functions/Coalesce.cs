using System;
using System.Runtime.ExceptionServices;
using System.Threading.Tasks;
using Waher.Script;
using Waher.Script.Abstraction.Elements;
using Waher.Script.Exceptions;
using Waher.Script.Model;

namespace Waher.Content.Semantic.Functions
{
	/// <summary>
	/// Coalesce(Expression, ...)
	/// </summary>
	public class Coalesce : FunctionMultiVariate
	{
		/// <summary>
		/// Coalesce(Exp1)
		/// </summary>
		/// <param name="Argument1">Argument 1.</param>
		/// <param name="Start">Start position in script expression.</param>
		/// <param name="Length">Length of expression covered by node.</param>
		/// <param name="Expression">Expression containing script.</param>
		public Coalesce(ScriptNode Argument1, int Start, int Length, Expression Expression)
			: this(new ScriptNode[] { Argument1 }, Start, Length, Expression)
		{
		}

		/// <summary>
		/// Coalesce(Exp1,Exp2)
		/// </summary>
		/// <param name="Argument1">Argument 1.</param>
		/// <param name="Argument2">Argument 2.</param>
		/// <param name="Start">Start position in script expression.</param>
		/// <param name="Length">Length of expression covered by node.</param>
		/// <param name="Expression">Expression containing script.</param>
		public Coalesce(ScriptNode Argument1, ScriptNode Argument2, int Start, int Length, Expression Expression)
			: this(new ScriptNode[] { Argument1, Argument2 }, Start, Length, Expression)
		{
		}

		/// <summary>
		/// Coalesce(Exp1,Exp2,Exp3)
		/// </summary>
		/// <param name="Argument1">Argument 1.</param>
		/// <param name="Argument2">Argument 2.</param>
		/// <param name="Argument3">Argument 3.</param>
		/// <param name="Start">Start position in script expression.</param>
		/// <param name="Length">Length of expression covered by node.</param>
		/// <param name="Expression">Expression containing script.</param>
		public Coalesce(ScriptNode Argument1, ScriptNode Argument2, ScriptNode Argument3, int Start, int Length, Expression Expression)
			: this(new ScriptNode[] { Argument1, Argument2, Argument3 }, Start, Length, Expression)
		{
		}

		/// <summary>
		/// Coalesce(Exp1,Exp2,Exp3,Exp4)
		/// </summary>
		/// <param name="Argument1">Argument 1.</param>
		/// <param name="Argument2">Argument 2.</param>
		/// <param name="Argument3">Argument 3.</param>
		/// <param name="Argument4">Argument 4.</param>
		/// <param name="Start">Start position in script expression.</param>
		/// <param name="Length">Length of expression covered by node.</param>
		/// <param name="Expression">Expression containing script.</param>
		public Coalesce(ScriptNode Argument1, ScriptNode Argument2, ScriptNode Argument3, 
			ScriptNode Argument4, int Start, int Length, Expression Expression)
			: this(new ScriptNode[] { Argument1, Argument2, Argument3, Argument4 }, Start, Length, Expression)
		{
		}

		/// <summary>
		/// Coalesce(Exp1,Exp2,Exp3,Exp4,Exp5)
		/// </summary>
		/// <param name="Argument1">Argument 1.</param>
		/// <param name="Argument2">Argument 2.</param>
		/// <param name="Argument3">Argument 3.</param>
		/// <param name="Argument4">Argument 4.</param>
		/// <param name="Argument5">Argument 5.</param>
		/// <param name="Start">Start position in script expression.</param>
		/// <param name="Length">Length of expression covered by node.</param>
		/// <param name="Expression">Expression containing script.</param>
		public Coalesce(ScriptNode Argument1, ScriptNode Argument2, ScriptNode Argument3,
			ScriptNode Argument4, ScriptNode Argument5, int Start, int Length, Expression Expression)
			: this(new ScriptNode[] { Argument1, Argument2, Argument3, Argument4, Argument5 }, Start, Length, Expression)
		{
		}

		/// <summary>
		/// Coalesce(Exp1,Exp2,Exp3,Exp4,Exp5,Exp6)
		/// </summary>
		/// <param name="Argument1">Argument 1.</param>
		/// <param name="Argument2">Argument 2.</param>
		/// <param name="Argument3">Argument 3.</param>
		/// <param name="Argument4">Argument 4.</param>
		/// <param name="Argument5">Argument 5.</param>
		/// <param name="Argument6">Argument 6.</param>
		/// <param name="Start">Start position in script expression.</param>
		/// <param name="Length">Length of expression covered by node.</param>
		/// <param name="Expression">Expression containing script.</param>
		public Coalesce(ScriptNode Argument1, ScriptNode Argument2, ScriptNode Argument3,
			ScriptNode Argument4, ScriptNode Argument5, ScriptNode Argument6, int Start, int Length, Expression Expression)
			: this(new ScriptNode[] { Argument1, Argument2, Argument3, Argument4, Argument5, Argument6 }, Start, Length, Expression)
		{
		}

		/// <summary>
		/// Coalesce(Expression, ...)
		/// </summary>
		/// <param name="Arguments">Arguments.</param>
		/// <param name="Start">Start position in script expression.</param>
		/// <param name="Length">Length of expression covered by node.</param>
		/// <param name="Expression">Expression containing script.</param>
		public Coalesce(ScriptNode[] Arguments, int Start, int Length, Expression Expression)
			: base(Arguments, NormalTypes(Arguments), Start, Length, Expression)
		{
		}

		private static ArgumentType[] NormalTypes(ScriptNode[] Arguments)
		{
			int i, c = Arguments.Length;
			ArgumentType[] Types = new ArgumentType[c];

			for (i = 0; i < c; i++)
				Types[i] = ArgumentType.Normal;

			return Types;
		}

		/// <summary>
		/// Name of the function
		/// </summary>
		public override string FunctionName => nameof(Coalesce);

		/// <summary>
		/// Default Argument names
		/// </summary>
		public override string[] DefaultArgumentNames => new string[] { "Exp", "..." };

		/// <summary>
		/// Evaluates the node, using the variables provided in the <paramref name="Variables"/> collection.
		/// </summary>
		/// <param name="Variables">Variables collection.</param>
		/// <returns>Result.</returns>
		public override IElement Evaluate(Variables Variables)
		{
			Exception LastException = null;

			foreach (ScriptNode Argument in this.Arguments)
			{
				try
				{
					return Argument.Evaluate(Variables);
				}
				catch (ScriptReturnValueException ex)
				{
					ExceptionDispatchInfo.Capture(ex).Throw();
					throw new ScriptReturnValueException(ex.ReturnValue);
				}
				catch (ScriptBreakLoopException ex)
				{
					ExceptionDispatchInfo.Capture(ex).Throw();
					if (ex.HasLoopValue)
						throw new ScriptBreakLoopException(ex.LoopValue);
					else
						throw new ScriptBreakLoopException();
				}
				catch (ScriptContinueLoopException ex)
				{
					ExceptionDispatchInfo.Capture(ex).Throw();
					if (ex.HasLoopValue)
						throw new ScriptContinueLoopException(ex.LoopValue);
					else
						throw new ScriptContinueLoopException();
				}
				catch (Exception ex)
				{
					LastException = ex;
				}
			}

			if (!(LastException is null))
				ExceptionDispatchInfo.Capture(LastException).Throw();

			throw new ScriptRuntimeException("No valid result found.", this);
		}

		/// <summary>
		/// Evaluates the node, using the variables provided in the <paramref name="Variables"/> collection.
		/// </summary>
		/// <param name="Variables">Variables collection.</param>
		/// <returns>Result.</returns>
		public override async Task<IElement> EvaluateAsync(Variables Variables)
		{
			Exception LastException = null;

			foreach (ScriptNode Argument in this.Arguments)
			{
				try
				{
					return await Argument.EvaluateAsync(Variables);
				}
				catch (ScriptReturnValueException ex)
				{
					ExceptionDispatchInfo.Capture(ex).Throw();
					throw new ScriptReturnValueException(ex.ReturnValue);
				}
				catch (ScriptBreakLoopException ex)
				{
					ExceptionDispatchInfo.Capture(ex).Throw();
					if (ex.HasLoopValue)
						throw new ScriptBreakLoopException(ex.LoopValue);
					else
						throw new ScriptBreakLoopException();
				}
				catch (ScriptContinueLoopException ex)
				{
					ExceptionDispatchInfo.Capture(ex).Throw();
					if (ex.HasLoopValue)
						throw new ScriptContinueLoopException(ex.LoopValue);
					else
						throw new ScriptContinueLoopException();
				}
				catch (Exception ex)
				{
					LastException = ex;
				}
			}

			if (!(LastException is null))
				ExceptionDispatchInfo.Capture(LastException).Throw();

			throw new ScriptRuntimeException("No valid result found.", this);
		}

		/// <summary>
		/// Evaluates the function.
		/// </summary>
		/// <param name="Arguments">Function arguments.</param>
		/// <param name="Variables">Variables collection.</param>
		/// <returns>Function result.</returns>
		public override IElement Evaluate(IElement[] Arguments, Variables Variables)
		{
			return Arguments[0];
		}

		/// <summary>
		/// Evaluates the function.
		/// </summary>
		/// <param name="Arguments">Function arguments.</param>
		/// <param name="Variables">Variables collection.</param>
		/// <returns>Function result.</returns>
		public override Task<IElement> EvaluateAsync(IElement[] Arguments, Variables Variables)
		{
			return Task.FromResult(this.Evaluate(Arguments, Variables));
		}
	}
}
