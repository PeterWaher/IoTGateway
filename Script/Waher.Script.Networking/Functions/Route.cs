using System;
using System.Collections.Generic;
using System.Net.NetworkInformation;
using System.Threading.Tasks;
using Waher.Networking.DNS;
using Waher.Script.Abstraction.Elements;
using Waher.Script.Model;
using Waher.Script.Objects;
using Waher.Script.Objects.Matrices;
using Waher.Script.Objects.VectorSpaces;
using Waher.Script.Units;

namespace Waher.Script.Networking.Functions
{
	/// <summary>
	/// Uses the ICMP Echo protocol to find the route in the IP network to a given host.
	/// </summary>
	public class Route : FunctionOneScalarVariable
	{
		/// <summary>
		/// Uses the ICMP Echo protocol to find the route in the IP network to a given host.
		/// </summary>
		/// <param name="Argument">Argument.</param>
		/// <param name="Start">Start position in script expression.</param>
		/// <param name="Length">Length of expression covered by node.</param>
		/// <param name="Expression">Expression containing script.</param>
		public Route(ScriptNode Argument, int Start, int Length, Expression Expression)
			: base(Argument, Start, Length, Expression)
		{
		}

		/// <summary>
		/// Name of the function
		/// </summary>
		public override string FunctionName => nameof(Route);

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
			using (System.Net.NetworkInformation.Ping Icmp = new System.Net.NetworkInformation.Ping())
			{
				PingReply Response;
				PingOptions Options = new PingOptions()
				{
					Ttl = 1
				};
				LinkedList<IElement> Elements = new LinkedList<IElement>();
				IElement E;

				do
				{
					Response = await Icmp.SendPingAsync(Argument, 2000, Ping.data, Options);

					Elements.AddLast(new DoubleNumber(Options.Ttl));
					Elements.AddLast(new ObjectValue(Response.Address));

					if (Response.Status == IPStatus.Success)
						E = new PhysicalQuantity(Response.RoundtripTime, Unit.Parse("ms"));
					else if (Response.Status == IPStatus.TimedOut ||
						Response.Status == IPStatus.TimeExceeded ||
						Response.Status == IPStatus.TtlExpired)
					{
						E = ObjectValue.Null;
					}
					else
						E = new StringValue(Ping.GetErrorMessage(Response));

					Elements.AddLast(E);

					try
					{
						string[] Names = await DnsResolver.ReverseDns(Response.Address);

						if (Names.Length == 1)
							E = new StringValue(Names[0]);
						else
							E = new ObjectVector(Names);
					}
					catch (Exception)
					{
						E = ObjectValue.Null;
					}

					Elements.AddLast(E);
					Options.Ttl++;
				}
				while (Response.Status != IPStatus.Success && Options.Ttl < 256);

				return new ObjectMatrix(Elements.Count / 4, 4, Elements)
				{
					ColumnNames = new string[] { "TTL", "IP", "Roundtrip", "Name" }
				};
			}
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
		public override Task<IElement> EvaluateScalarAsync(IElement Argument, Variables Variables)
		{
			return this.EvaluateScalarAsync(Argument.AssociatedObjectValue?.ToString() ?? string.Empty, Variables);
		}
	}
}
