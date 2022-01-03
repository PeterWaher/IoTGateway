using System;
using System.Threading.Tasks;

namespace Waher.Networking.CoAP
{
	/// <summary>
	/// GET Interface for CoAP resources.
	/// </summary>
	public interface ICoapGetMethod
	{
		/// <summary>
		/// Executes the GET method on the resource.
		/// </summary>
		/// <param name="Request">CoAP Request</param>
		/// <param name="Response">CoAP Response</param>
		/// <exception cref="CoapException">If an error occurred when processing the method.</exception>
		Task GET(CoapMessage Request, CoapResponse Response);

		/// <summary>
		/// If the GET method is allowed.
		/// </summary>
		bool AllowsGET
		{
			get;
		}
	}
}
