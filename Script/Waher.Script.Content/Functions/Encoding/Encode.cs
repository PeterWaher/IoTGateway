using System;
using System.Collections.Generic;
using Waher.Content;
using Waher.Content.Xml;
using Waher.Script.Abstraction.Elements;
using Waher.Script.Exceptions;
using Waher.Script.Model;
using Waher.Script.Objects;
using Waher.Script.Objects.VectorSpaces;

namespace Waher.Script.Content.Functions.Encoding
{
	/// <summary>
	/// Encode(Object)
	/// </summary>
	public class Encode : FunctionMultiVariate
	{
		/// <summary>
		/// Encode(Object)
		/// </summary>
		/// <param name="Object">Object to encode</param>
		/// <param name="Start">Start position in script expression.</param>
		/// <param name="Length">Length of expression covered by node.</param>
		/// <param name="Expression">Expression containing script.</param>
		public Encode(ScriptNode Object, int Start, int Length, Expression Expression)
			: base(new ScriptNode[] { Object }, argumentTypes1Normal, Start, Length, Expression)
		{
		}

		/// <summary>
		/// Encode(Object,AcceptedTypes)
		/// </summary>
		/// <param name="Object">Object to encode</param>
		/// <param name="AcceptedTypes">Accepted content types.</param>
		/// <param name="Start">Start position in script expression.</param>
		/// <param name="Length">Length of expression covered by node.</param>
		/// <param name="Expression">Expression containing script.</param>
		public Encode(ScriptNode Object, ScriptNode AcceptedTypes, int Start, int Length, Expression Expression)
			: base(new ScriptNode[] { Object, AcceptedTypes }, new ArgumentType[] { ArgumentType.Normal, ArgumentType.Vector },
				  Start, Length, Expression)
		{
		}

		/// <summary>
		/// Name of the function
		/// </summary>
		public override string FunctionName
		{
			get { return "encode"; }
		}

		/// <summary>
		/// Default Argument names
		/// </summary>
		public override string[] DefaultArgumentNames => new string[] { "Object" };

		/// <summary>
		/// Evaluates the function.
		/// </summary>
		/// <param name="Arguments">Function arguments.</param>
		/// <param name="Variables">Variables collection.</param>
		/// <returns>Function result.</returns>
		public override IElement Evaluate(IElement[] Arguments, Variables Variables)
		{
			string ContentType;
			byte[] Bin;

			if (Arguments.Length > 1)
			{
				if (!(Arguments[1].AssociatedObjectValue is Array A))
					throw new ScriptRuntimeException("Second parameter to Encode should be an array of acceptable content types.", this);

				int i, c = A.Length;
				string[] AcceptedTypes = new string[c];

				for (i = 0; i < c; i++)
					AcceptedTypes[i] = A.GetValue(i)?.ToString();

				Bin = InternetContent.Encode(Arguments[0].AssociatedObjectValue, System.Text.Encoding.UTF8, out ContentType, AcceptedTypes);
			}
			else
				Bin = InternetContent.Encode(Arguments[0].AssociatedObjectValue, System.Text.Encoding.UTF8, out ContentType);

			return new ObjectVector(new ObjectValue(Bin), new StringValue(ContentType));
		}

	}
}
