﻿using System;
using System.Threading.Tasks;

namespace Waher.Networking.CoAP
{
	/// <summary>
	/// PUT Interface for CoAP resources.
	/// </summary>
	public interface ICoapPutMethod
	{
		/// <summary>
		/// Executes the PUT method on the resource.
		/// </summary>
		/// <param name="Request">CoAP Request</param>
		/// <param name="Response">CoAP Response</param>
		/// <exception cref="CoapException">If an error occurred when processing the method.</exception>
		Task PUT(CoapMessage Request, CoapResponse Response);

		/// <summary>
		/// If the PUT method is allowed.
		/// </summary>
		bool AllowsPUT
		{
			get;
		}
	}
}
