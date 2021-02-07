using System;
using System.Collections.Generic;
using Waher.Content;
using Waher.Script.Abstraction.Elements;
using Waher.Script.Exceptions;
using Waher.Script.Model;
using Waher.Script.Objects;

namespace Waher.Script.Content.Functions.InputOutput
{
	/// <summary>
	/// Post(Url,Data)
	/// </summary>
	public class Post : FunctionMultiVariate
	{
		/// <summary>
		/// Post(Url,Data)
		/// </summary>
		/// <param name="Url">URL.</param>
		/// <param name="Data">Data</param>
		/// <param name="Start">Start position in script expression.</param>
		/// <param name="Length">Length of expression covered by node.</param>
		/// <param name="Expression">Expression containing script.</param>
		public Post(ScriptNode Url, ScriptNode Data, int Start, int Length, Expression Expression)
			: base(new ScriptNode[] { Url, Data }, new ArgumentType[] { ArgumentType.Scalar, ArgumentType.Normal }, Start, Length, Expression)
		{
		}

		/// <summary>
		/// Post(Url,Data,Headers)
		/// </summary>
		/// <param name="Url">URL.</param>
		/// <param name="Data">Data</param>
		/// <param name="Headers">Request headers.</param>
		/// <param name="Start">Start position in script expression.</param>
		/// <param name="Length">Length of expression covered by node.</param>
		/// <param name="Expression">Expression containing script.</param>
		public Post(ScriptNode Url, ScriptNode Data, ScriptNode Headers, int Start, int Length, Expression Expression)
			: base(new ScriptNode[] { Url, Data, Headers }, new ArgumentType[] { ArgumentType.Scalar, ArgumentType.Normal, ArgumentType.Normal }, Start, Length, Expression)
		{
		}

		/// <summary>
		/// Name of the function
		/// </summary>
		public override string FunctionName
		{
			get { return "post"; }
		}

		/// <summary>
		/// Default Argument names
		/// </summary>
		public override string[] DefaultArgumentNames => new string[] { "URL", "Data" };

		/// <summary>
		/// Evaluates the node, using the variables provided in the <paramref name="Variables"/> collection.
		/// </summary>
		/// <param name="Arguments">Function arguments.</param>
		/// <param name="Variables">Variables collection.</param>
		/// <returns>Result.</returns>
		public override IElement Evaluate(IElement[] Arguments, Variables Variables)
		{
			Uri Url = new Uri(Arguments[0].AssociatedObjectValue?.ToString());
			object Data = Arguments[1].AssociatedObjectValue;
			List<KeyValuePair<string, string>> HeaderList = null;

			if (Arguments.Length > 2)
				HeaderList = Get.GetHeaders(Arguments[2].AssociatedObjectValue, this);

			object Result = InternetContent.PostAsync(Url, Data, HeaderList?.ToArray() ?? new KeyValuePair<string, string>[0]).Result;

			return Expression.Encapsulate(Result);
		}
	}
}
