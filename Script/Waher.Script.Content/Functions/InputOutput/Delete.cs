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
	/// Delete(Url,Headers)
	/// </summary>
	public class Delete : FunctionMultiVariate
	{
		/// <summary>
		/// Delete(Url,Headers)
		/// </summary>
		/// <param name="Url">URL.</param>
		/// <param name="Headers">Request headers.</param>
		/// <param name="Start">Start position in script expression.</param>
		/// <param name="Length">Length of expression covered by node.</param>
		/// <param name="Expression">Expression containing script.</param>
		public Delete(ScriptNode Url, ScriptNode Headers, int Start, int Length, Expression Expression)
			: base(new ScriptNode[] { Url, Headers }, new ArgumentType[] { ArgumentType.Scalar, ArgumentType.Normal }, Start, Length, Expression)
		{
		}

		/// <summary>
		/// Delete(Url,Headers,Certificate)
		/// </summary>
		/// <param name="Url">URL.</param>
		/// <param name="Headers">Request headers.</param>
		/// <param name="Certificate">Client certificate to use in a Mutual TLS session.</param>
		/// <param name="Start">Start position in script expression.</param>
		/// <param name="Length">Length of expression covered by node.</param>
		/// <param name="Expression">Expression containing script.</param>
		public Delete(ScriptNode Url, ScriptNode Headers, ScriptNode Certificate, int Start, int Length, Expression Expression)
			: base(new ScriptNode[] { Url, Headers, Certificate }, new ArgumentType[] { ArgumentType.Scalar, ArgumentType.Normal, ArgumentType.Normal }, Start, Length, Expression)
		{
		}

		/// <summary>
		/// Name of the function
		/// </summary>
		public override string FunctionName => nameof(Delete);

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
			ChunkedList<KeyValuePair<string, string>> HeaderList = null;
			ContentResponse Content;

			if (Arguments.Length > 1)
				HeaderList = Get.GetHeaders(Arguments[1].AssociatedObjectValue, this);

			if (Arguments.Length > 2)
			{
				if (!(Arguments[2].AssociatedObjectValue is X509Certificate Certificate))
					throw new ScriptRuntimeException("Expected X.509 certificate in third argument.", this);

				Content = await InternetContent.DeleteAsync(Url, Certificate, HeaderList?.ToArray() ?? Array.Empty<KeyValuePair<string, string>>());
			}
			else
				Content = await InternetContent.DeleteAsync(Url, HeaderList?.ToArray() ?? Array.Empty<KeyValuePair<string, string>>());

			if (Content.HasError)
				throw new ScriptRuntimeException(Content.Error.Message, this, Content.Error);

			return Expression.Encapsulate(Content.Decoded);
		}
	}
}
