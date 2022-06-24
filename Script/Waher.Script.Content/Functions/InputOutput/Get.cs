using System;
using System.Collections.Generic;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;
using Waher.Content;
using Waher.Script.Abstraction.Elements;
using Waher.Script.Exceptions;
using Waher.Script.Model;

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
		/// Get(Url,Headers,Certificate)
		/// </summary>
		/// <param name="Url">URL.</param>
		/// <param name="Headers">Request headers.</param>
		/// <param name="Certificate">Client certificate to use in a Mutual TLS session.</param>
		/// <param name="Start">Start position in script expression.</param>
		/// <param name="Length">Length of expression covered by node.</param>
		/// <param name="Expression">Expression containing script.</param>
		public Get(ScriptNode Url, ScriptNode Headers, ScriptNode Certificate, int Start, int Length, Expression Expression)
			: base(new ScriptNode[] { Url, Headers, Certificate }, new ArgumentType[] { ArgumentType.Scalar, ArgumentType.Normal, ArgumentType.Normal }, Start, Length, Expression)
		{
		}

		/// <summary>
		/// Name of the function
		/// </summary>
		public override string FunctionName => nameof(Get);

		/// <summary>
		/// Default Argument names
		/// </summary>
		public override string[] DefaultArgumentNames => new string[] { "URL", "Headers", "Certificate" };

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
			object Result;

			if (Arguments.Length > 1)
				HeaderList = GetHeaders(Arguments[1].AssociatedObjectValue, this);

			if (Arguments.Length > 2)
			{
				if (!(Arguments[2].AssociatedObjectValue is X509Certificate Certificate))
					throw new ScriptRuntimeException("Expected X.509 certificate in third argument.", this);

				Result = await InternetContent.GetAsync(Url, Certificate, HeaderList?.ToArray() ?? new KeyValuePair<string, string>[0]);
			}
			else
				Result = await InternetContent.GetAsync(Url, HeaderList?.ToArray() ?? new KeyValuePair<string, string>[0]);

			return Expression.Encapsulate(Result);
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
				throw new ScriptRuntimeException("Invalid header parameter. Should be either an accept string, or an object with protocol-specific headers or options.", Node);

			return HeaderList;
		}
	}
}
