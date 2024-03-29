﻿using System;
using System.Threading.Tasks;
using Waher.Networking.DNS;
using Waher.Networking.DNS.Enumerations;
using Waher.Networking.DNS.ResourceRecords;
using Waher.Script.Abstraction.Elements;
using Waher.Script.Exceptions;
using Waher.Script.Model;
using Waher.Script.Objects;
using Waher.Script.Objects.VectorSpaces;

namespace Waher.Script.Networking.Functions
{
	/// <summary>
	/// Makes a DNS query regarding a name.
	/// </summary>
	public class Dns : FunctionMultiVariate
	{
		/// <summary>
		/// Makes a DNS query regarding a name.
		/// </summary>
		/// <param name="Name">Name to resolve.</param>
		/// <param name="Start">Start position in script expression.</param>
		/// <param name="Length">Length of expression covered by node.</param>
		/// <param name="Expression">Expression containing script.</param>
		public Dns(ScriptNode Name, int Start, int Length, Expression Expression)
			: base(new ScriptNode[] { Name }, argumentTypes1Scalar, Start, Length, Expression)
		{
		}

		/// <summary>
		/// Makes a DNS query regarding a name.
		/// </summary>
		/// <param name="Name">Name to resolve.</param>
		/// <param name="Type">TYPE parameter.</param>
		/// <param name="Start">Start position in script expression.</param>
		/// <param name="Length">Length of expression covered by node.</param>
		/// <param name="Expression">Expression containing script.</param>
		public Dns(ScriptNode Name, ScriptNode Type, int Start, int Length, Expression Expression)
			: base(new ScriptNode[] { Name, Type }, argumentTypes2Scalar, Start, Length, Expression)
		{
		}

		/// <summary>
		/// Makes a DNS query regarding a name.
		/// </summary>
		/// <param name="Name">Name to resolve.</param>
		/// <param name="Type">TYPE parameter.</param>
		/// <param name="Class">CLASS parameter.</param>
		/// <param name="Start">Start position in script expression.</param>
		/// <param name="Length">Length of expression covered by node.</param>
		/// <param name="Expression">Expression containing script.</param>
		public Dns(ScriptNode Name, ScriptNode Type, ScriptNode Class, int Start, int Length, Expression Expression)
			: base(new ScriptNode[] { Name, Type, Class }, argumentTypes3Scalar, Start, Length, Expression)
		{
		}

		/// <summary>
		/// Name of the function
		/// </summary>
		public override string FunctionName => nameof(Dns);

		/// <summary>
		/// Default Argument names
		/// </summary>
		public override string[] DefaultArgumentNames => new string[] { "Name", "QTYPE", "QCLASS" };

		/// <summary>
		/// If the node (or its decendants) include asynchronous evaluation. Asynchronous nodes should be evaluated using
		/// <see cref="ScriptNode.EvaluateAsync(Variables)"/>.
		/// </summary>
		public override bool IsAsynchronous => true;

		/// <summary>
		/// Evaluates the function.
		/// </summary>
		/// <param name="Arguments">Function arguments.</param>
		/// <param name="Variables">Variables collection.</param>
		/// <returns>Function result.</returns>
		public override IElement Evaluate(IElement[] Arguments, Variables Variables)
		{
			return this.EvaluateAsync(Arguments, Variables).Result;
		}

		/// <summary>
		/// Evaluates the function.
		/// </summary>
		/// <param name="Arguments">Function arguments.</param>
		/// <param name="Variables">Variables collection.</param>
		/// <returns>Function result.</returns>
		public override async Task<IElement> EvaluateAsync(IElement[] Arguments, Variables Variables)
		{
			string Name = Arguments[0].AssociatedObjectValue?.ToString() ?? string.Empty;
			int c = Arguments.Length;
			QTYPE TYPE = c < 2 ? QTYPE.A : this.ToEnum<QTYPE>(Arguments[1]);
			QCLASS CLASS = c < 3 ? QCLASS.IN : this.ToEnum<QCLASS>(Arguments[2]);

			ResourceRecord[] Records = await DnsResolver.Resolve(Name, TYPE, CLASS);

			if (Records.Length == 0)
				throw new ScriptRuntimeException("Unable to resolve name.", this);
			else if (Records.Length == 1)
				return new ObjectValue(Records[0]);
			else
				return new ObjectVector(Records);
		}
	}
}
