using System;
using System.Collections.Generic;
using Waher.Script.Abstraction.Elements;
using Waher.Script.Model;

namespace Waher.Script.Order
{
	/// <summary>
	/// Orders elements based logic defined in a lambda expression.
	/// </summary>
	public class LambdaOrder : IComparer<IElement>
	{
		private readonly ILambdaExpression lambda;
		private readonly IElement[] arguments;
		private readonly Variables variables;

		/// <summary>
		/// Orders elements based logic defined in a lambda expression.
		/// </summary>
		/// <param name="Lambda">Lambda expression-</param>
		/// <param name="Variables">Variables to use during evaluation.</param>
		public LambdaOrder(ILambdaExpression Lambda, Variables Variables)
		{
			this.lambda = Lambda;
			this.arguments = new IElement[2];
			this.variables = Variables;
		}

		/// <summary>
		/// Compares two elements.
		/// </summary>
		/// <param name="x">First element</param>
		/// <param name="y">Second element</param>
		/// <returns>Ordinal difference between elements.</returns>
		public int Compare(IElement x, IElement y)
		{
			this.arguments[0] = x;
			this.arguments[1] = y;

			IElement Result = this.lambda.Evaluate(this.arguments, this.variables);

			return Math.Sign(Expression.ToDouble(Result));
		}
	}
}
