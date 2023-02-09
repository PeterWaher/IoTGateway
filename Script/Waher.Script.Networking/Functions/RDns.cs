using System.Net;
using System.Threading.Tasks;
using Waher.Networking.DNS;
using Waher.Script.Abstraction.Elements;
using Waher.Script.Exceptions;
using Waher.Script.Model;
using Waher.Script.Objects.VectorSpaces;

namespace Waher.Script.Networking.Functions
{
	/// <summary>
	/// Makes a Reverse DNS query regarding an IP Address.
	/// </summary>
	public class RDns : FunctionOneScalarVariable
	{
		/// <summary>
		/// Makes a Reverse DNS query regarding an IP Address.
		/// </summary>
		/// <param name="IpAddress">IP Address.</param>
		/// <param name="Start">Start position in script expression.</param>
		/// <param name="Length">Length of expression covered by node.</param>
		/// <param name="Expression">Expression containing script.</param>
		public RDns(ScriptNode IpAddress, int Start, int Length, Expression Expression)
			: base(IpAddress, Start, Length, Expression)
		{
		}

		/// <summary>
		/// Name of the function
		/// </summary>
		public override string FunctionName => nameof(RDns);

		/// <summary>
		/// Default Argument names
		/// </summary>
		public override string[] DefaultArgumentNames => new string[] { "IpAddress" };
		
		/// <summary>
		/// If the node (or its decendants) include asynchronous evaluation. Asynchronous nodes should be evaluated using
		/// <see cref="ScriptNode.EvaluateAsync(Variables)"/>.
		/// </summary>
		public override bool IsAsynchronous => true;

		/// <summary>
		/// Evaluates the function on a scalar argument.
		/// </summary>
		/// <param name="Argument">Function argument.</param>
		/// <param name="Variables">Variables collection.</param>
		/// <returns>Function result.</returns>
		public override IElement EvaluateScalar(string Argument, Variables Variables)
		{
			return this.EvaluateScalarAsync(Argument, Variables).Result;
		}

		/// <summary>
		/// Evaluates the function on a scalar argument.
		/// </summary>
		/// <param name="Argument">Function argument.</param>
		/// <param name="Variables">Variables collection.</param>
		/// <returns>Function result.</returns>
		public override async Task<IElement> EvaluateScalarAsync(string Argument, Variables Variables)
		{
			if (IPAddress.TryParse(Argument, out IPAddress Address))
				return new ObjectVector(await DnsResolver.ReverseDns(Address));
			else
				throw new ScriptRuntimeException("Not an IP address.", this);
		}

		/// <summary>
		/// Evaluates the function on a scalar argument.
		/// </summary>
		/// <param name="Argument">Function argument.</param>
		/// <param name="Variables">Variables collection.</param>
		/// <returns>Function result.</returns>
		public override IElement EvaluateScalar(IElement Argument, Variables Variables)
		{
			return this.EvaluateScalarAsync(Argument, Variables).Result;
		}

		/// <summary>
		/// Evaluates the function on a scalar argument.
		/// </summary>
		/// <param name="Argument">Function argument.</param>
		/// <param name="Variables">Variables collection.</param>
		/// <returns>Function result.</returns>
		public override async Task<IElement> EvaluateScalarAsync(IElement Argument, Variables Variables)
		{
			if (Argument.AssociatedObjectValue is IPAddress IP ||
				IPAddress.TryParse(Argument.AssociatedObjectValue?.ToString() ?? string.Empty, out IP))
			{
				return new ObjectVector(await DnsResolver.ReverseDns(IP));
			}
			else
				throw new ScriptRuntimeException("Not an IP address.", this);
		}
	}
}
