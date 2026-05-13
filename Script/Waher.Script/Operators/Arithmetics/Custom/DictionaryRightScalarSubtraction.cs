using System.Collections.Generic;
using Waher.Runtime.Inventory;
using Waher.Script.Abstraction.CustomOperators;
using Waher.Script.Abstraction.Elements;
using Waher.Script.Model;
using Waher.Script.Objects;

namespace Waher.Script.Operators.Arithmetics.Custom
{
	/// <summary>
	/// Performs subtraction of a dictionary and a scalar.
	/// </summary>
	public class DictionaryRightScalarSubtraction : IBinaryOperator
	{
		/// <summary>
		/// Performs subtraction of a dictionary and a scalar.
		/// </summary>
		public DictionaryRightScalarSubtraction()
		{
		}

		/// <summary>
		/// How well the operator supports the given types of operands.
		/// </summary>
		/// <param name="Operation">Binary operation to evaluate.</param>
		/// <returns>A grade indicating how well the operator supports the operands.</returns>
		public Grade Supports(IBinaryOperation Operation)
		{
			if (Operation.Name != "op_Subtraction")
				return Grade.NotAtAll;

			object Left = Operation.Left.AssociatedObjectValue;
			object Right = Operation.Right.AssociatedObjectValue;

			if ((Left is IEnumerable<KeyValuePair<string, IElement>> ||
				Left is IEnumerable<KeyValuePair<string, object>>) &&
				Operation.Right.IsScalar &&
				!(Right is IEnumerable<KeyValuePair<string, IElement>> ||
				Right is IEnumerable<KeyValuePair<string, object>>))
			{
				return Grade.Excellent;
			}
			else
				return Grade.NotAtAll;
		}

		/// <summary>
		/// Evaluates the custom binary operator.
		/// </summary>
		/// <param name="Operation">Binary operation to evaluate.</param>
		/// <param name="Node">Node performing the evaluation.</param>
		/// <returns>Result of the evaluation.</returns>
		public IElement Evaluate(IBinaryOperation Operation, ScriptNode Node)
		{
			Dictionary<string, IElement> Result = new Dictionary<string, IElement>();
			object L = Operation.Left.AssociatedObjectValue;
			IElement Scalar = Operation.Right;

			if (L is IDictionary<string, IElement> LeftDictionary)
			{
				foreach (KeyValuePair<string, IElement> P in LeftDictionary)
					Result[P.Key] = Subtract.EvaluateSubtraction(P.Value, Scalar, Node);
			}
			else if (L is IDictionary<string, object> LeftDictionary2)
			{
				foreach (KeyValuePair<string, object> P in LeftDictionary2)
					Result[P.Key] = Subtract.EvaluateSubtraction(Expression.Encapsulate(P.Value), Scalar, Node);
			}
			else if (L is IEnumerable<KeyValuePair<string, IElement>> LeftEnumeration)
			{
				foreach (KeyValuePair<string, IElement> P in LeftEnumeration)
				{
					if (Result.TryGetValue(P.Key, out IElement E))
						Result[P.Key] = Subtract.EvaluateSubtraction(E, P.Value, Node);
					else
						Result[P.Key] = Subtract.EvaluateSubtraction(P.Value, Scalar, Node);
				}
			}
			else if (L is IEnumerable<KeyValuePair<string, object>> LeftEnumeration2)
			{
				foreach (KeyValuePair<string, object> P in LeftEnumeration2)
				{
					IElement Value = Expression.Encapsulate(P.Value);

					if (Result.TryGetValue(P.Key, out IElement E))
						Result[P.Key] = Subtract.EvaluateSubtraction(E, Value, Node);
					else
						Result[P.Key] = Subtract.EvaluateSubtraction(Value, Scalar, Node);
				}
			}

			return new ObjectValue(Result);
		}
	}
}
