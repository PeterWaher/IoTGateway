using System;
using System.Threading.Tasks;

namespace Waher.Networking.CoAP
{
	/// <summary>
	/// DELETE Interface for CoAP resources.
	/// </summary>
	public interface ICoapDeleteMethod
	{
		/// <summary>
		/// Executes the DELETE method on the resource.
		/// </summary>
		/// <param name="Request">CoAP Request</param>
		/// <param name="Response">CoAP Response</param>
		/// <exception cref="CoapException">If an error occurred when processing the method.</exception>
		Task DELETE(CoapMessage Request, CoapResponse Response);

		/// <summary>
		/// If the DELETE method is allowed.
		/// </summary>
		bool AllowsDELETE
		{
			get;
		}
	}
}
