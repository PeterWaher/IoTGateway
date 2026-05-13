using System.Collections.Generic;
using Waher.Runtime.Inventory;
using Waher.Script.Abstraction.CustomOperators;
using Waher.Script.Abstraction.Elements;
using Waher.Script.Model;
using Waher.Script.Objects;

namespace Waher.Script.Operators.Arithmetics.Custom
{
	/// <summary>
	/// Performs addition of a scalar and a dictionary.
	/// </summary>
	public class DictionaryLeftScalarAddition : IBinaryOperator
	{
		/// <summary>
		/// Performs addition of a scalar and a dictionary.
		/// </summary>
		public DictionaryLeftScalarAddition()
		{
		}

		/// <summary>
		/// How well the operator supports the given types of operands.
		/// </summary>
		/// <param name="Operation">Binary operation to evaluate.</param>
		/// <returns>A grade indicating how well the operator supports the operands.</returns>
		public Grade Supports(IBinaryOperation Operation)
		{
			if (Operation.Name != "op_Addition")
				return Grade.NotAtAll;

			object Left = Operation.Left.AssociatedObjectValue;
			object Right = Operation.Right.AssociatedObjectValue;

			if ((Right is IEnumerable<KeyValuePair<string, IElement>> ||
				Right is IEnumerable<KeyValuePair<string, object>>) &&
				Operation.Left.IsScalar &&
				!(Left is IEnumerable<KeyValuePair<string, IElement>> ||
				Left is IEnumerable<KeyValuePair<string, object>>))
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
			object R = Operation.Right.AssociatedObjectValue;
			IElement Scalar = Operation.Left;

			if (R is IDictionary<string, IElement> RightDictionary)
			{
				foreach (KeyValuePair<string, IElement> P in RightDictionary)
					Result[P.Key] = Add.EvaluateAddition(Scalar, P.Value, Node);
			}
			else if (R is IDictionary<string, object> RightDictionary2)
			{
				foreach (KeyValuePair<string, object> P in RightDictionary2)
					Result[P.Key] = Add.EvaluateAddition(Scalar, Expression.Encapsulate(P.Value), Node);
			}
			else if (R is IEnumerable<KeyValuePair<string, IElement>> RightEnumeration)
			{
				foreach (KeyValuePair<string, IElement> P in RightEnumeration)
				{
					if (Result.TryGetValue(P.Key, out IElement E))
						Result[P.Key] = Add.EvaluateAddition(E, P.Value, Node);
					else
						Result[P.Key] = Add.EvaluateAddition(Scalar, P.Value, Node);
				}
			}
			else if (R is IEnumerable<KeyValuePair<string, object>> RightEnumeration2)
			{
				foreach (KeyValuePair<string, object> P in RightEnumeration2)
				{
					IElement Value = Expression.Encapsulate(P.Value);

					if (Result.TryGetValue(P.Key, out IElement E))
						Result[P.Key] = Add.EvaluateAddition(E, Value, Node);
					else
						Result[P.Key] = Add.EvaluateAddition(Scalar, Value, Node);
				}
			}

			return new ObjectValue(Result);
		}
	}
}
