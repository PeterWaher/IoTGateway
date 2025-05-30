using System;
using System.Collections.Generic;
using System.Numerics;
using System.Threading.Tasks;
using Waher.Runtime.Inventory;
using Waher.Script.Abstraction.Elements;
using Waher.Script.Model;
using Waher.Script.Objects;
using Waher.Script.Operators.Vectors;

namespace Waher.Script.Functions.Runtime
{
	/// <summary>
	/// Checks if an expression exists, or has a valid value.
	/// </summary>
	public class Exists : FunctionOneVariable
	{
		private readonly VariableReference varRef;
		private readonly ScriptNode vectorIndex;

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

			if (this.varRef is null &&
				Argument is VectorIndex VectorIndex &&
				VectorIndex.LeftOperand is VariableReference VRef)
			{
				this.varRef = VRef;
				this.vectorIndex = VectorIndex.RightOperand;
			}
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
			if (this.varRef is null)
			{
				try
				{
					IElement E = this.Argument.Evaluate(Variables);
					return this.IsWellDefined(E) ? BooleanValue.True : BooleanValue.False;
				}
				catch (Exception)
				{
					return BooleanValue.False;
				}
			}
			else
			{
				string s = this.varRef.VariableName;
				IElement E;

				if (Variables.TryGetVariable(s, out Variable v))
					E = v.ValueElement;
				else if (!Expression.TryGetConstant(s, Variables, out E))
				{
					if (Types.IsRootNamespace(s))
					{
						if (this.vectorIndex is null)
							return BooleanValue.True;
						else
							E = new Namespace(s);
					}
					else
					{
						ILambdaExpression Lambda = Expression.GetFunctionLambdaDefinition(s, this.Start, this.Length, this.Expression);
						if (Lambda is null)
							return BooleanValue.False;

						if (this.vectorIndex is null)
							return BooleanValue.True;
						else
							E = new ObjectValue(Lambda);
					}
				}

				if (this.vectorIndex is null)
					return this.IsWellDefined(E) ? BooleanValue.True : BooleanValue.False;

				try
				{
					IElement Index = this.vectorIndex.Evaluate(Variables);
					return this.IsWellDefined(E, Index) ? BooleanValue.True : BooleanValue.False;
				}
				catch (Exception)
				{
					return BooleanValue.False;
				}
			}
		}

		/// <summary>
		/// Checks if an element is well-defined, i.e., not null or NaN, etc.
		/// </summary>
		/// <param name="Element">Element</param>
		/// <returns>If element is well-defined.</returns>
		public bool IsWellDefined(IElement Element)
		{
			return this.IsWellDefined(Element?.AssociatedObjectValue);
		}

		/// <summary>
		/// Checks if an object is well-defined, i.e., not null or NaN, etc.
		/// </summary>
		/// <param name="Obj">Element</param>
		/// <returns>If element is well-defined.</returns>
		public bool IsWellDefined(object Obj)
		{
			if (Obj is null)
				return false;

			if (Obj is double d)
				return !double.IsNaN(d);

			if (Obj is Complex z)
				return !double.IsNaN(z.Real) && !double.IsNaN(z.Imaginary);

			return true;
		}

		/// <summary>
		/// Checks if an element index is well-defined, i.e., not null or NaN, etc.
		/// </summary>
		/// <param name="Element">Element</param>
		/// <param name="Index">Index into element.</param>
		/// <returns>If element index is well-defined.</returns>
		public bool IsWellDefined(IElement Element, IElement Index)
		{
			if (!this.IsWellDefined(Element) || !this.IsWellDefined(Index))
				return false;

			if (Index.AssociatedObjectValue is string StringIndex)
			{
				if (Element.AssociatedObjectValue is IDictionary<string, IElement> Obj)
				{
					if (Obj.TryGetValue(StringIndex, out IElement Value))
						return this.IsWellDefined(Value);
					else
						return false;
				}
				else if (Element.AssociatedObjectValue is IDictionary<string, object> Obj2)
				{
					if (Obj2.TryGetValue(StringIndex, out object Value))
						return this.IsWellDefined(Value);
					else
						return false;
				}
			}
			
			return this.IsWellDefined(VectorIndex.EvaluateIndex(Element, Index, false, this));
		}

		/// <summary>
		/// Evaluates the node, using the variables provided in the <paramref name="Variables"/> collection.
		/// </summary>
		/// <param name="Variables">Variables collection.</param>
		/// <returns>Result.</returns>
		public override async Task<IElement> EvaluateAsync(Variables Variables)
		{
			if (this.varRef is null)
			{
				try
				{
					IElement E = await this.Argument.EvaluateAsync(Variables);
					return this.IsWellDefined(E) ? BooleanValue.True : BooleanValue.False;
				}
				catch (Exception)
				{
					return BooleanValue.False;
				}
			}
			else
			{
				string s = this.varRef.VariableName;
				IElement E;

				if (Variables.TryGetVariable(s, out Variable v))
					E = v.ValueElement;
				else if (!Expression.TryGetConstant(s, Variables, out E))
				{
					if (Types.IsRootNamespace(s))
					{
						if (this.vectorIndex is null)
							return BooleanValue.True;
						else
							E = new Namespace(s);
					}
					else
					{
						ILambdaExpression Lambda = Expression.GetFunctionLambdaDefinition(s, this.Start, this.Length, this.Expression);
						if (Lambda is null)
							return BooleanValue.False;

						if (this.vectorIndex is null)
							return BooleanValue.True;
						else
							E = new ObjectValue(Lambda);
					}
				}

				if (this.vectorIndex is null)
					return this.IsWellDefined(E) ? BooleanValue.True : BooleanValue.False;

				try
				{
					IElement Index = await this.vectorIndex.EvaluateAsync(Variables);
					return this.IsWellDefined(E, Index) ? BooleanValue.True : BooleanValue.False;
				}
				catch (Exception)
				{
					return BooleanValue.False;
				}
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
