using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Waher.Content;
using Waher.Script.Abstraction.Elements;
using Waher.Script.Model;

namespace Waher.Script.Content.Functions.InputOutput
{
	/// <summary>
	/// Head(Url)
	/// </summary>
	public class Head : FunctionMultiVariate
	{
		/// <summary>
		/// Head(Url)
		/// </summary>
		/// <param name="Url">URL.</param>
		/// <param name="Start">Start position in script expression.</param>
		/// <param name="Length">Length of expression covered by node.</param>
		/// <param name="Expression">Expression containing script.</param>
		public Head(ScriptNode Url, int Start, int Length, Expression Expression)
			: base(new ScriptNode[] { Url }, new ArgumentType[] { ArgumentType.Scalar }, Start, Length, Expression)
		{
		}

		/// <summary>
		/// Head(Url,Headers)
		/// </summary>
		/// <param name="Url">URL.</param>
		/// <param name="Headers">Request headers.</param>
		/// <param name="Start">Start position in script expression.</param>
		/// <param name="Length">Length of expression covered by node.</param>
		/// <param name="Expression">Expression containing script.</param>
		public Head(ScriptNode Url, ScriptNode Headers, int Start, int Length, Expression Expression)
			: base(new ScriptNode[] { Url, Headers }, new ArgumentType[] { ArgumentType.Scalar, ArgumentType.Normal }, Start, Length, Expression)
		{
		}

		/// <summary>
		/// Name of the function
		/// </summary>
		public override string FunctionName => nameof(Head);

		/// <summary>
		/// Default Argument names
		/// </summary>
		public override string[] DefaultArgumentNames => new string[] { "URL" };

		/// <summary>
		/// If the node (or its decendants) include asynchronous evaluation. Asynchronous nodes should be evaluated using
		/// <see cref="ScriptNode.EvaluateAsync(Variables)"/>.
		/// </summary>
		public override bool IsAsynchronous => true;

		/// <summary>
		/// Evaluates the node, using the variables provided in the <paramref name="Variables"/> collection.
		/// </summary>
		/// <param name="Arguments">Function arguments.</param>
		/// <param name="Variables">Variables collection.</param>
		/// <returns>Result.</returns>
		public override IElement Evaluate(IElement[] Arguments, Variables Variables)
		{
			return this.EvaluateAsync(Arguments, Variables).Result;
		}

		/// <summary>
		/// Evaluates the node, using the variables provided in the <paramref name="Variables"/> collection.
		/// </summary>
		/// <param name="Arguments">Function arguments.</param>
		/// <param name="Variables">Variables collection.</param>
		/// <returns>Result.</returns>
		public override async Task<IElement> EvaluateAsync(IElement[] Arguments, Variables Variables)
		{
			Uri Url = new Uri(Arguments[0].AssociatedObjectValue?.ToString());
			List<KeyValuePair<string, string>> HeaderList = null;

			if (Arguments.Length > 1)
				HeaderList = Get.GetHeaders(Arguments[1].AssociatedObjectValue, this);

			object Result = await InternetContent.HeadAsync(Url, HeaderList?.ToArray() ?? new KeyValuePair<string, string>[0]);

			return Expression.Encapsulate(Result);
		}
	}
}
