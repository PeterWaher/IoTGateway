using System;
using Waher.Script;
using Waher.Script.Abstraction.Elements;
using Waher.Script.Model;

namespace Waher.Networking.HTTP.ScriptExtensions.Functions
{
	/// <summary>
	/// Throws an <see cref="HttpException"/>
	/// </summary>
	public class HttpError : FunctionMultiVariate
	{
		/// <summary>
		/// Throws an <see cref="HttpException"/>
		/// </summary>
		/// <param name="Code">HTTP Status Code</param>
		/// <param name="Message">HTTP Status Message</param>
		/// <param name="Content">Content to return</param>
		/// <param name="Start">Start position in script expression.</param>
		/// <param name="Length">Length of expression covered by node.</param>
		/// <param name="Expression">Expression containing script.</param>
		public HttpError(ScriptNode Code, ScriptNode Message, ScriptNode Content, int Start, int Length, Expression Expression)
			: base(new ScriptNode[] { Code, Message, Content }, 
				  new ArgumentType[] { ArgumentType.Normal, ArgumentType.Normal, ArgumentType.Normal },
				  Start, Length, Expression)
		{
		}

		/// <summary>
		/// Name of the function
		/// </summary>
		public override string FunctionName => "HttpError";

		/// <summary>
		/// Default Argument names
		/// </summary>
		public override string[] DefaultArgumentNames => new string[] { "Code", "Message", "Content" };

		/// <summary>
		/// Evaluates the function.
		/// </summary>
		/// <param name="Arguments">Function arguments.</param>
		/// <param name="Variables">Variables collection.</param>
		/// <returns>Function result.</returns>
		public override IElement Evaluate(IElement[] Arguments, Variables Variables)
		{
			int Code = (int)Expression.ToDouble(Arguments[0].AssociatedObjectValue);
			string Message = Arguments[1].AssociatedObjectValue?.ToString();
			object Content = Arguments[2].AssociatedObjectValue;

			throw new HttpException(Code, Message, Content);
		}
	}
}
