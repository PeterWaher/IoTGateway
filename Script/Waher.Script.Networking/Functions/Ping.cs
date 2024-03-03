using System.Net;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading.Tasks;
using Waher.Script.Abstraction.Elements;
using Waher.Script.Exceptions;
using Waher.Script.Model;
using Waher.Script.Objects;
using Waher.Script.Units;

namespace Waher.Script.Networking.Functions
{
	/// <summary>
	/// Performs an ICMP Echo to ping a remote endpoint, and return the roundtrip time.
	/// </summary>
	public class Ping : FunctionOneScalarVariable
	{
		/// <summary>
		/// Performs an ICMP Echo to ping a remote endpoint, and return the roundtrip time.
		/// </summary>
		/// <param name="Argument">Argument.</param>
		/// <param name="Start">Start position in script expression.</param>
		/// <param name="Length">Length of expression covered by node.</param>
		/// <param name="Expression">Expression containing script.</param>
		public Ping(ScriptNode Argument, int Start, int Length, Expression Expression)
			: base(Argument, Start, Length, Expression)
		{
		}

		/// <summary>
		/// Name of the function
		/// </summary>
		public override string FunctionName => nameof(Ping);

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
				PingReply Response = await Icmp.SendPingAsync(Argument, 2000, data, options);
				return this.GetResult(Response);
			}
		}

		private IElement GetResult(PingReply Response)
		{
			if (Response.Status == IPStatus.Success)
				return new PhysicalQuantity(Response.RoundtripTime, Unit.Parse("ms"));
			else
				throw new ScriptRuntimeException(GetErrorMessage(Response), this);
		}

		internal static string GetErrorMessage(PingReply Response)
		{
			switch (Response.Status)
			{
				case IPStatus.Success: return null;
				case IPStatus.BadDestination: return "Bad destination";
				case IPStatus.BadHeader: return "Bad header";
				case IPStatus.BadOption: return "Bad option";
				case IPStatus.BadRoute: return "Bad route";
				case IPStatus.DestinationHostUnreachable: return "Destination host unreachable";
				case IPStatus.DestinationNetworkUnreachable: return "Destination network unreachable";
				case IPStatus.DestinationPortUnreachable: return "Destination port unreachable";
				case IPStatus.DestinationProhibited: return "Destination prohibited";
				//case IPStatus.DestinationProtocolUnreachable: return "Destination protocol unreachable";
				case IPStatus.DestinationScopeMismatch: return "Destination scope mismatch";
				case IPStatus.DestinationUnreachable: return "Destination unreachable";
				case IPStatus.HardwareError: return "Hardware error";
				case IPStatus.IcmpError: return "ICMP error";
				case IPStatus.NoResources: return "No resources";
				case IPStatus.PacketTooBig: return "Packet too big";
				case IPStatus.ParameterProblem: return "Parameter problem";
				case IPStatus.SourceQuench: return "Source quench";
				case IPStatus.TimedOut: return "Timed out";
				case IPStatus.TimeExceeded: return "Time exceeded";
				case IPStatus.TtlExpired: return "TTL expired";
				case IPStatus.TtlReassemblyTimeExceeded: return "TTL reassembly time exceeded";
				case IPStatus.UnrecognizedNextHeader: return "Unrecognized next header";
				case IPStatus.Unknown:
				default:
					return "Unknown error";
			}
		}

		internal static readonly byte[] data = Encoding.ASCII.GetBytes("aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa");
		internal static readonly PingOptions options = new PingOptions(64, true);

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
				using (System.Net.NetworkInformation.Ping Icmp = new System.Net.NetworkInformation.Ping())
				{
					PingReply Response = await Icmp.SendPingAsync(IP, 2000, data, options);
					return this.GetResult(Response);
				}
			}
			else
				throw new ScriptRuntimeException("Not an IP address.", this);
		}
	}
}
