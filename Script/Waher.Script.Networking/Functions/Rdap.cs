using System;
using System.Net;
using System.Threading.Tasks;
using Waher.Content;
using Waher.Networking.WHOIS;
using Waher.Script.Abstraction.Elements;
using Waher.Script.Exceptions;
using Waher.Script.Model;
using Waher.Script.Objects;

namespace Waher.Script.Networking.Functions
{
	/// <summary>
	/// Makes an RDAP query regarding an IP address.
	/// </summary>
	public class Rdap : FunctionOneScalarVariable
	{
		/// <summary>
		/// Makes an RDAP query regarding an IP address.
		/// </summary>
		/// <param name="Argument">Argument.</param>
		/// <param name="Start">Start position in script expression.</param>
		/// <param name="Length">Length of expression covered by node.</param>
		/// <param name="Expression">Expression containing script.</param>
		public Rdap(ScriptNode Argument, int Start, int Length, Expression Expression)
			: base(Argument, Start, Length, Expression)
		{
		}

		/// <summary>
		/// Name of the function
		/// </summary>
		public override string FunctionName => nameof(Rdap);

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
			if (IPAddress.TryParse(Argument, out IPAddress IP))
				return await this.EvaluateScalarAsync(IP);
			else
				throw new ScriptRuntimeException("Not an IP address.", this);
		}

		private async Task<IElement> EvaluateScalarAsync(IPAddress IP)
		{
			Uri Uri = WhoIsClient.RdapUri(IP)
				?? throw new ScriptRuntimeException("RDAP URI not available for " + IP.ToString(), this);

			ContentResponse Content = await InternetContent.GetAsync(Uri);
			if (Content.HasError)
				throw new ScriptRuntimeException(Content.Error.Message, this, Content.Error);

			return new ObjectValue(Content.Decoded);
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
				return await this.EvaluateScalarAsync(IP);
			}
			else
				throw new ScriptRuntimeException("Not an IP address.", this);
		}
	}
}
