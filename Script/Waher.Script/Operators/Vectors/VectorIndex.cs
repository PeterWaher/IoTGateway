using System;
using System.Collections.Generic;
using System.Reflection;
using Waher.Script.Abstraction.Elements;
using Waher.Script.Exceptions;
using Waher.Script.Model;
using Waher.Script.Objects;
using Waher.Script.Operators.Matrices;

namespace Waher.Script.Operators.Vectors
{
	/// <summary>
	/// Vector Index operator.
	/// </summary>
	public class VectorIndex : NullCheckBinaryOperator
	{
		/// <summary>
		/// Vector Index operator.
		/// </summary>
		/// <param name="Left">Left operand.</param>
		/// <param name="Right">Right operand.</param>
		/// <param name="NullCheck">If null should be returned if left operand is null.</param>
		/// <param name="Start">Start position in script expression.</param>
		/// <param name="Length">Length of expression covered by node.</param>
		/// <param name="Expression">Expression containing script.</param>
		public VectorIndex(ScriptNode Left, ScriptNode Right, bool NullCheck, int Start, int Length, Expression Expression)
			: base(Left, Right, NullCheck, Start, Length, Expression)
		{
		}

		/// <summary>
		/// Evaluates the node, using the variables provided in the <paramref name="Variables"/> collection.
		/// </summary>
		/// <param name="Variables">Variables collection.</param>
		/// <returns>Result.</returns>
		public override IElement Evaluate(Variables Variables)
		{
			IElement Left = this.left.Evaluate(Variables);
			if (this.nullCheck && Left.AssociatedObjectValue is null)
				return Left;

			IElement Right = this.right.Evaluate(Variables);

			return EvaluateIndex(Left, Right, this.nullCheck, this);
		}

		/// <summary>
		/// Evaluates the vector index operator.
		/// </summary>
		/// <param name="Vector">Vector</param>
		/// <param name="Index">Index</param>
		/// <param name="NullCheck">If null should be returned if left operand is null.</param>
		/// <param name="Node">Node performing the operation.</param>
		/// <returns>Result</returns>
		public static IElement EvaluateIndex(IElement Vector, IElement Index, bool NullCheck, ScriptNode Node)
		{
			if (Vector is IVector V)
				return EvaluateIndex(V, Index, Node);
			else if (Vector.IsScalar)
			{
				object Object = Vector.AssociatedObjectValue;
				if (Object is null)
				{
					if (NullCheck)
						return Vector;
					else
						throw new ScriptRuntimeException("Vector is null.", Node);
				}

				Type T = Object.GetType();
				PropertyInfo ItemProperty = T.GetRuntimeProperty("Item");
				ParameterInfo[] Parameters = ItemProperty?.GetIndexParameters();

				if (Parameters is null || Parameters.Length != 1)
					throw new ScriptRuntimeException("The index operator operates on vectors.", Node);

				return EvaluateIndex(Object, T, ItemProperty, Parameters, Index, Node);
			}
			else
			{
				LinkedList<IElement> Elements = new LinkedList<IElement>();

				foreach (IElement E in Vector.ChildElements)
					Elements.AddLast(EvaluateIndex(E, Index, NullCheck, Node));

				return Vector.Encapsulate(Elements, Node);
			}
		}

		private static IElement EvaluateIndex(object Object, Type T, PropertyInfo ItemProperty, ParameterInfo[] Parameters,
			IElement Index, ScriptNode Node)
		{
			if (Index.TryConvertTo(Parameters[0].ParameterType, out object IndexValue))
			{
				object Result = ItemProperty.GetValue(Object, new object[] { IndexValue });
				return Expression.Encapsulate(Result);
			}

			if (Index.IsScalar)
				throw new ScriptRuntimeException("Provided index value not compatible with expected index type.", Node);

			LinkedList<IElement> Elements = new LinkedList<IElement>();

			foreach (IElement E in Index.ChildElements)
				Elements.AddLast(EvaluateIndex(Object, T, ItemProperty, Parameters, E, Node));

			return Index.Encapsulate(Elements, Node);
		}

		/// <summary>
		/// Evaluates the vector index operator.
		/// </summary>
		/// <param name="Vector">Vector</param>
		/// <param name="Index">Index</param>
		/// <param name="Node">Node performing the operation.</param>
		/// <returns>Result</returns>
		public static IElement EvaluateIndex(IVector Vector, IElement Index, ScriptNode Node)
		{
			if (Index is DoubleNumber RE)
			{
				double d = RE.Value;
				if (d < 0 || d > int.MaxValue || d != Math.Truncate(d))
					throw new ScriptRuntimeException("Index must be a non-negative integer.", Node);

				return Vector.GetElement((int)d);
			}

			if (Index.IsScalar)
				throw new ScriptRuntimeException("Index must be a non-negative integer.", Node);
			else
			{
				LinkedList<IElement> Elements = new LinkedList<IElement>();

				foreach (IElement E in Index.ChildElements)
					Elements.AddLast(EvaluateIndex(Vector, E, Node));

				return Index.Encapsulate(Elements, Node);
			}
		}

	}
}
