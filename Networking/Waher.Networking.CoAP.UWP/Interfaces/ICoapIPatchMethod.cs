using System;
using System.Threading.Tasks;

namespace Waher.Networking.CoAP
{
	/// <summary>
	/// iPATCH Interface for CoAP resources.
	/// </summary>
	public interface ICoapIPatchMethod
	{
		/// <summary>
		/// Executes the iPATCH method on the resource.
		/// </summary>
		/// <param name="Request">CoAP Request</param>
		/// <param name="Response">CoAP Response</param>
		/// <exception cref="CoapException">If an error occurred when processing the method.</exception>
		Task IPATCH(CoapMessage Request, CoapResponse Response);

		/// <summary>
		/// If the iPATCH method is allowed.
		/// </summary>
		bool AllowsIPATCH
		{
			get;
		}
	}
}
