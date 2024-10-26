using System.Threading.Tasks;
using Waher.Things.Ieee1451.Ieee1451_0.Messages;

namespace Waher.Things.Ieee1451.Ieee1451_0
{
	/// <summary>
	/// Interface for nodes that can return TEDS.
	/// </summary>
	public interface ITedsNode : INode
	{
		/// <summary>
		/// A request for TEDS data has been received.
		/// </summary>
		/// <param name="TedsAccessMessage">Message</param>
		/// <param name="TedsAccessCode">TEDS access code.</param>
		/// <param name="TedsOffset">TEDS offset.</param>
		/// <param name="TimeoutSeconds">Timeout, in seconds.</param>
		Task TedsRequest(TedsAccessMessage TedsAccessMessage,
			TedsAccessCode TedsAccessCode, uint TedsOffset, double TimeoutSeconds);
	}
}
