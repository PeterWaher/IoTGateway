using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;
using Waher.Runtime.Collections;
using Waher.Script.Abstraction.Elements;
using Waher.Script.Abstraction.Sets;
using Waher.Script.Exceptions;
using Waher.Script.Model;
using Waher.Script.Objects;

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
			this.isAsync = true;
		}

		/// <summary>
		/// If the node (or its decendants) include asynchronous evaluation. Asynchronous nodes should be evaluated using
		/// <see cref="ScriptNode.EvaluateAsync(Variables)"/>.
		/// </summary>
		public override bool IsAsynchronous => true;

		/// <summary>
		/// Evaluates the node, using the variables provided in the <paramref name="Variables"/> collection.
		/// </summary>
		/// <param name="Variables">Variables collection.</param>
		/// <returns>Result.</returns>
		public override IElement Evaluate(Variables Variables)
		{
			return this.EvaluateAsync(Variables).Result;
		}

		/// <summary>
		/// Evaluates the node, using the variables provided in the <paramref name="Variables"/> collection.
		/// </summary>
		/// <param name="Variables">Variables collection.</param>
		/// <returns>Result.</returns>
		public override async Task<IElement> EvaluateAsync(Variables Variables)
		{
			IElement Left = this.left.Evaluate(Variables);
			if (this.nullCheck && Left.AssociatedObjectValue is null)
				return Left;

			IElement Right = this.right.Evaluate(Variables);

			return await EvaluateIndex(Left, Right, this.nullCheck, this);
		}

		/// <summary>
		/// Evaluates the vector index operator.
		/// </summary>
		/// <param name="Vector">Vector</param>
		/// <param name="Index">Index</param>
		/// <param name="NullCheck">If null should be returned if left operand is null.</param>
		/// <param name="Node">Node performing the operation.</param>
		/// <returns>Result</returns>
		public static async Task<IElement> EvaluateIndex(IElement Vector, IElement Index, bool NullCheck, ScriptNode Node)
		{
			if (Vector is IVector V)
				return EvaluateIndex(V, Index, Node);
			else if (Vector is ISet S)
			{
				int i = (int)Expression.ToDouble(Index.AssociatedObjectValue);
				if (i < 0)
					throw new ScriptRuntimeException("Index must be a non-negative number.", Node);

				foreach (IElement E in S.ChildElements)
				{
					if (i-- == 0)
						return E;
				}

				throw new ScriptRuntimeException("Index out of range.", Node);
			}
			else if (Vector.IsScalar)
			{
				if (Vector is StringValue s)
					return EvaluateIndex(s, Index, Node);
				else
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
					if (!TryGetIndexProperty(T, true, false, out PropertyInfo ItemProperty,
						out ParameterInfo[] Parameters) ||
						(Parameters?.Length ?? 0) != 1)
					{
						throw new ScriptRuntimeException("The index operator operates on vectors.", Node);
					}

					return await EvaluateIndex(Object, T, ItemProperty, Parameters, Index, Node);
				}
			}
			else
			{
				ChunkedList<IElement> Elements = new ChunkedList<IElement>();

				foreach (IElement E in Vector.ChildElements)
					Elements.Add(await EvaluateIndex(E, Index, NullCheck, Node));

				return Vector.Encapsulate(Elements, Node);
			}
		}

		/// <summary>
		/// Tries to get a one-dimensional index property of a Type.
		/// </summary>
		/// <param name="T">Type</param>
		/// <param name="ForReading">If index property is for reading.</param>
		/// <param name="ForWriting">If index property is for writing.</param>
		/// <param name="PropertyInfo">Property information of index property.</param>
		/// <param name="Parameters">Parameter definitions of index property.</param>
		/// <returns>If a one-dimensional index property was found.</returns>
		public static bool TryGetIndexProperty(Type T, bool ForReading, bool ForWriting, 
			out PropertyInfo PropertyInfo, out ParameterInfo[] Parameters)
		{
			lock (indexProperties)
			{
				if (indexProperties.TryGetValue(T, out KeyValuePair<PropertyInfo, ParameterInfo[]> P))
				{
					PropertyInfo = P.Key;
					Parameters = P.Value;
					return true;
				}

				foreach (PropertyInfo P2 in T.GetRuntimeProperties())
				{
					if (P2.Name != "Item")
						continue;

					if (ForReading && (!P2.CanRead || !P2.GetMethod.IsPublic))
						continue;

					if (ForWriting && (!P2.CanWrite || !P2.SetMethod.IsPublic))
						continue;

					Parameters = P2.GetIndexParameters();
					if (Parameters is null || Parameters.Length != 1)
						continue;

					indexProperties[T] = new KeyValuePair<PropertyInfo, ParameterInfo[]>(P2, Parameters);
					PropertyInfo = P2;

					return true;
				}
			}

			PropertyInfo = null;
			Parameters = null;

			return false;
		}

		private static readonly Dictionary<Type, KeyValuePair<PropertyInfo, ParameterInfo[]>> indexProperties = new Dictionary<Type, KeyValuePair<PropertyInfo, ParameterInfo[]>>();

		internal static async Task<IElement> EvaluateIndex(object Object, Type T, PropertyInfo ItemProperty, ParameterInfo[] Parameters,
			IElement Index, ScriptNode Node)
		{
			if (Index.TryConvertTo(Parameters[0].ParameterType, out object IndexValue))
			{
				object Result = await WaitPossibleTask(ItemProperty.GetValue(Object, new object[] { IndexValue }));
				return Expression.Encapsulate(Result);
			}

			if (Index.IsScalar)
				throw new ScriptRuntimeException("Provided index value not compatible with expected index type.", Node);

			ChunkedList<IElement> Elements = new ChunkedList<IElement>();

			foreach (IElement E in Index.ChildElements)
				Elements.Add(await EvaluateIndex(Object, T, ItemProperty, Parameters, E, Node));

			return Index.Encapsulate(Elements, Node);
		}

		private static IElement EvaluateIndex(StringValue s, IElement Index, ScriptNode Node)
		{
			if (Index.IsScalar)
			{
				int i = (int)Expression.ToDouble(Index.AssociatedObjectValue);
				return new StringValue(new string(s.Value[i], 1), s.CaseInsensitive);
			}
			else
			{
				ChunkedList<IElement> Elements = new ChunkedList<IElement>();

				foreach (IElement E in Index.ChildElements)
					Elements.Add(EvaluateIndex(s, E, Node));

				return Index.Encapsulate(Elements, Node);
			}
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
			if (Index.AssociatedObjectValue is double d)
			{
				if (d < 0 || d > int.MaxValue || d != Math.Truncate(d))
					throw new ScriptRuntimeException("Index must be a non-negative integer.", Node);

				return Vector.GetElement((int)d);
			}

			if (Index.IsScalar)
				throw new ScriptRuntimeException("Index must be a non-negative integer.", Node);
			else
			{
				ChunkedList<IElement> Elements = new ChunkedList<IElement>();

				foreach (IElement E in Index.ChildElements)
					Elements.Add(EvaluateIndex(Vector, E, Node));

				return Index.Encapsulate(Elements, Node);
			}
		}

	}
}
