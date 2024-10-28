using System.Threading.Tasks;
using Waher.Things.Ieee1451.Ieee1451_0.Messages;

namespace Waher.Things.Ieee1451.Ieee1451_0
{
	/// <summary>
	/// Interface for nodes that can return transducer information.
	/// </summary>
	public interface ITransducerNode : INode
	{
		/// <summary>
		/// A request for transducer data has been received.
		/// </summary>
		/// <param name="TransducerAccessMessage">Message</param>
		/// <param name="SamplingMode">Sampling mode.</param>
		/// <param name="TimeoutSeconds">Timeout, in seconds.</param>
		Task TransducerDataRequest(TransducerAccessMessage TransducerAccessMessage,
			SamplingMode SamplingMode, double TimeoutSeconds);
	}
}
