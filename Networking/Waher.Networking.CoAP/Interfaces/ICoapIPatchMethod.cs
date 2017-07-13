using System;
using System.Collections.Generic;
using System.Text;

namespace Waher.Networking.CoAP
{
	/// <summary>
	/// iPATCH Interface for CoAP resources.
	/// </summary>
	public interface ICoapIPatchMethod
	{
		/// <summary>
		/// Executes the GET method on the resource.
		/// </summary>
		/// <param name="Request">CoAP Request</param>
		/// <param name="Response">CoAP Response</param>
		/// <exception cref="CoapException">If an error occurred when processing the method.</exception>
		void iPATCH(CoapMessage Request, CoapResponse Response);

		/// <summary>
		/// If the iPATCH method is allowed.
		/// </summary>
		bool AllowsiPATCH
		{
			get;
		}
	}
}
