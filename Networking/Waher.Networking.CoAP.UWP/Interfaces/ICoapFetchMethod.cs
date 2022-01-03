using System;
using System.Threading.Tasks;

namespace Waher.Networking.CoAP
{
	/// <summary>
	/// FETCH Interface for CoAP resources.
	/// </summary>
	public interface ICoapFetchMethod
	{
		/// <summary>
		/// Executes the FETCH method on the resource.
		/// </summary>
		/// <param name="Request">CoAP Request</param>
		/// <param name="Response">CoAP Response</param>
		/// <exception cref="CoapException">If an error occurred when processing the method.</exception>
		Task FETCH(CoapMessage Request, CoapResponse Response);

		/// <summary>
		/// If the FETCH method is allowed.
		/// </summary>
		bool AllowsFETCH
		{
			get;
		}
	}
}
