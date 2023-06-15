using System;
using System.Numerics;
using System.Threading.Tasks;
using Waher.Events;
using Waher.Runtime.Inventory;
using Waher.Script.Abstraction.Elements;
using Waher.Script.Model;
using Waher.Script.Objects;

namespace Waher.Script.Functions.Runtime
{
	/// <summary>
	/// Checks if an expression exists, or has a valid value.
	/// </summary>
	public class Exists : FunctionOneVariable
	{
		private readonly VariableReference varRef;

		/// <summary>
		/// Checks if an expression exists, or has a valid value.
		/// </summary>
		/// <param name="Argument">Argument.</param>
		/// <param name="Start">Start position in script expression.</param>
		/// <param name="Length">Length of expression covered by node.</param>
		/// <param name="Expression">Expression containing script.</param>
		public Exists(ScriptNode Argument, int Start, int Length, Expression Expression)
			: base(Argument, Start, Length, Expression)
		{
			this.varRef = Argument as VariableReference;
		}

		/// <summary>
		/// Name of the function
		/// </summary>
		public override string FunctionName => nameof(Exists);

		/// <summary>
		/// Evaluates the node, using the variables provided in the <paramref name="Variables"/> collection.
		/// </summary>
		/// <param name="Variables">Variables collection.</param>
		/// <returns>Result.</returns>
		public override IElement Evaluate(Variables Variables)
		{
			if (!(this.varRef is null))
			{
				string s = this.varRef.VariableName;

				if (Variables.TryGetVariable(s, out Variable v))
					return v.ValueObject is null ? BooleanValue.False : BooleanValue.True;

				if (Expression.TryGetConstant(s, Variables, out IElement ConstantValue))
					return ConstantValue.AssociatedObjectValue is null ? BooleanValue.False : BooleanValue.True;

				if (Types.IsRootNamespace(s))
					return BooleanValue.True;

				IElement E = Expression.GetFunctionLambdaDefinition(s, this.Start, this.Length, this.Expression);
				if (!(E is null))
					return BooleanValue.True;

				return BooleanValue.False;
			}

			try
			{
				IElement E = this.Argument.Evaluate(Variables);
				object Obj = E.AssociatedObjectValue;

				if (Obj is null)
					return BooleanValue.False;

				if (Obj is double d)
				{
					if (double.IsNaN(d))
						return BooleanValue.False;
					else
						return BooleanValue.True;
				}

				if (Obj is Complex z)
				{
					if (double.IsNaN(z.Real) || double.IsNaN(z.Imaginary))
						return BooleanValue.False;
					else
						return BooleanValue.True;
				}

				return BooleanValue.True;
			}
			catch (Exception)
			{
				return BooleanValue.False;
			}
		}

		/// <summary>
		/// Evaluates the node, using the variables provided in the <paramref name="Variables"/> collection.
		/// </summary>
		/// <param name="Variables">Variables collection.</param>
		/// <returns>Result.</returns>
		public override async Task<IElement> EvaluateAsync(Variables Variables)
		{
			if (!(this.varRef is null))
			{
				string s = this.varRef.VariableName;

				if (Variables.TryGetVariable(s, out Variable v))
					return v.ValueObject is null ? BooleanValue.False : BooleanValue.True;

				if (Expression.TryGetConstant(s, Variables, out IElement ConstantValue))
					return ConstantValue.AssociatedObjectValue is null ? BooleanValue.False : BooleanValue.True;

				if (Types.IsRootNamespace(s))
					return BooleanValue.True;

				IElement E = Expression.GetFunctionLambdaDefinition(s, this.Start, this.Length, this.Expression);
				if (!(E is null))
					return BooleanValue.True;

				return BooleanValue.False;
			}

			try
			{
				IElement E = await this.Argument.EvaluateAsync(Variables);
				object Obj = E.AssociatedObjectValue;

				if (Obj is null)
					return BooleanValue.False;

				if (Obj is double d)
				{
					if (double.IsNaN(d))
						return BooleanValue.False;
					else
						return BooleanValue.True;
				}

				if (Obj is Complex z)
				{
					if (double.IsNaN(z.Real) || double.IsNaN(z.Imaginary))
						return BooleanValue.False;
					else
						return BooleanValue.True;
				}

				return BooleanValue.True;
			}
			catch (Exception)
			{
				return BooleanValue.False;
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
			return BooleanValue.True;
		}
	}
}
