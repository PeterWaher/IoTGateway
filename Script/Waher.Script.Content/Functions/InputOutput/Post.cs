using System;
using System.Collections.Generic;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;
using Waher.Content;
using Waher.Runtime.Collections;
using Waher.Script.Abstraction.Elements;
using Waher.Script.Exceptions;
using Waher.Script.Model;

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
		/// Post(Url,Data,Headers,Certificate)
		/// </summary>
		/// <param name="Url">URL.</param>
		/// <param name="Data">Data</param>
		/// <param name="Headers">Request headers.</param>
		/// <param name="Certificate">Client certificate to use in a Mutual TLS session.</param>
		/// <param name="Start">Start position in script expression.</param>
		/// <param name="Length">Length of expression covered by node.</param>
		/// <param name="Expression">Expression containing script.</param>
		public Post(ScriptNode Url, ScriptNode Data, ScriptNode Headers, ScriptNode Certificate, int Start, int Length, Expression Expression)
			: base(new ScriptNode[] { Url, Data, Headers, Certificate }, new ArgumentType[] { ArgumentType.Scalar, ArgumentType.Normal, ArgumentType.Normal, ArgumentType.Normal }, Start, Length, Expression)
		{
		}

		/// <summary>
		/// Name of the function
		/// </summary>
		public override string FunctionName => nameof(Post);

		/// <summary>
		/// Default Argument names
		/// </summary>
		public override string[] DefaultArgumentNames => new string[] { "URL", "Data", "Headers", "Certificate" };

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
			object Data = Arguments[1].AssociatedObjectValue;
			ChunkedList<KeyValuePair<string, string>> HeaderList = null;
			ContentResponse Content;

			if (Arguments.Length > 2)
				HeaderList = Get.GetHeaders(Arguments[2].AssociatedObjectValue, this);

			if (Arguments.Length > 3)
			{
				if (!(Arguments[3].AssociatedObjectValue is X509Certificate Certificate))
					throw new ScriptRuntimeException("Expected X.509 certificate in fourth argument.", this);

				Content = await InternetContent.PostAsync(Url, Data, Certificate, HeaderList?.ToArray() ?? new KeyValuePair<string, string>[0]);
			}
			else
				Content = await InternetContent.PostAsync(Url, Data, HeaderList?.ToArray() ?? new KeyValuePair<string, string>[0]);

			if (Content.HasError)
				throw new ScriptRuntimeException(Content.Error.Message, this, Content.Error);

			return Expression.Encapsulate(Content.Decoded);
		}
	}
}
