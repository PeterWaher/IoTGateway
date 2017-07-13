using System;
using System.Collections.Generic;
using System.Text;

namespace Waher.Networking.CoAP
{
	/// <summary>
	/// FETCH Interface for CoAP resources.
	/// </summary>
	public interface ICoapFetchMethod
	{
		/// <summary>
		/// Executes the GET method on the resource.
		/// </summary>
		/// <param name="Request">CoAP Request</param>
		/// <param name="Response">CoAP Response</param>
		/// <exception cref="CoapException">If an error occurred when processing the method.</exception>
		void FETCH(CoapMessage Request, CoapResponse Response);

		/// <summary>
		/// If the FETCH method is allowed.
		/// </summary>
		bool AllowsFETCH
		{
			get;
		}
	}
}
