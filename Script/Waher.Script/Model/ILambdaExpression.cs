using System;
using System.Collections.Generic;
using System.Text;
using Waher.Script.Abstraction.Elements;

namespace Waher.Script.Model
{
	/// <summary>
	/// Base interface for lambda expressions.
	/// </summary>
	public interface ILambdaExpression
	{
		/// <summary>
		/// Number of arguments.
		/// </summary>
		int NrArguments
		{
			get;
		}

		/// <summary>
		/// Argument Names.
		/// </summary>
		string[] ArgumentNames
		{
			get;
		}

		/// <summary>
		/// Argument types.
		/// </summary>
		ArgumentType[] ArgumentTypes
		{
			get;
		}


		/// <summary>
		/// Evaluates the lambda expression.
		/// </summary>
		/// <param name="Arguments">Arguments.</param>
		/// <param name="Variables">Variables collection.</param>
		/// <returns>Result.</returns>
		IElement Evaluate(IElement[] Arguments, Variables Variables);
	}
}
