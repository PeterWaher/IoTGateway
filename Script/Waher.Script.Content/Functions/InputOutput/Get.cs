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
	/// Get(Url)
	/// </summary>
	public class Get : FunctionMultiVariate
	{
		/// <summary>
		/// Get(Url)
		/// </summary>
		/// <param name="Url">URL.</param>
		/// <param name="Start">Start position in script expression.</param>
		/// <param name="Length">Length of expression covered by node.</param>
		/// <param name="Expression">Expression containing script.</param>
		public Get(ScriptNode Url, int Start, int Length, Expression Expression)
			: base(new ScriptNode[] { Url }, new ArgumentType[] { ArgumentType.Scalar }, Start, Length, Expression)
		{
		}

		/// <summary>
		/// Get(Url,Headers)
		/// </summary>
		/// <param name="Url">URL.</param>
		/// <param name="Headers">Request headers.</param>
		/// <param name="Start">Start position in script expression.</param>
		/// <param name="Length">Length of expression covered by node.</param>
		/// <param name="Expression">Expression containing script.</param>
		public Get(ScriptNode Url, ScriptNode Headers, int Start, int Length, Expression Expression)
			: base(new ScriptNode[] { Url, Headers }, new ArgumentType[] { ArgumentType.Scalar, ArgumentType.Normal }, Start, Length, Expression)
		{
		}

		/// <summary>
		/// Name of the function
		/// </summary>
		public override string FunctionName
		{
			get { return "get"; }
		}

		/// <summary>
		/// Default Argument names
		/// </summary>
		public override string[] DefaultArgumentNames => new string[] { "URL" };

		/// <summary>
		/// Evaluates the node, using the variables provided in the <paramref name="Variables"/> collection.
		/// </summary>
		/// <param name="Arguments">Function arguments.</param>
		/// <param name="Variables">Variables collection.</param>
		/// <returns>Result.</returns>
		public override IElement Evaluate(IElement[] Arguments, Variables Variables)
		{
			Uri Url = new Uri(Arguments[0].AssociatedObjectValue?.ToString());
			List<KeyValuePair<string, string>> HeaderList = null;

			if (Arguments.Length > 1)
				HeaderList = GetHeaders(Arguments[1].AssociatedObjectValue, this);

			object Result = InternetContent.GetAsync(Url, HeaderList?.ToArray() ?? new KeyValuePair<string, string>[0]).Result;

			return new ObjectValue(Result);
		}

		internal static List<KeyValuePair<string, string>> GetHeaders(object Arg, ScriptNode Node)
		{
			List<KeyValuePair<string, string>> HeaderList;

			if (Arg is IDictionary<string, IElement> Headers)
			{
				HeaderList = new List<KeyValuePair<string, string>>();

				foreach (KeyValuePair<string, IElement> P in Headers)
					HeaderList.Add(new KeyValuePair<string, string>(P.Key, P.Value.AssociatedObjectValue?.ToString()));
			}
			else if (Arg is string Accept)
			{
				HeaderList = new List<KeyValuePair<string, string>>()
				{
					new KeyValuePair<string, string>("Accept", Accept)
				};
			}
			else
				throw new ScriptRuntimeException("Invalid second parameter to Get. Should be either an accept string, or an object with protocol-specific headers or options.", Node);

			return HeaderList;
		}
	}
}
