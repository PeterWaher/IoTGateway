﻿using System;
using System.Threading.Tasks;

namespace Waher.Networking.CoAP
{
	/// <summary>
	/// POST Interface for CoAP resources.
	/// </summary>
	public interface ICoapPostMethod
	{
		/// <summary>
		/// Executes the POST method on the resource.
		/// </summary>
		/// <param name="Request">CoAP Request</param>
		/// <param name="Response">CoAP Response</param>
		/// <exception cref="CoapException">If an error occurred when processing the method.</exception>
		Task POST(CoapMessage Request, CoapResponse Response);

		/// <summary>
		/// If the POST method is allowed.
		/// </summary>
		bool AllowsPOST
		{
			get;
		}
	}
}
