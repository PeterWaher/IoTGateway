﻿using System;

namespace Waher.Networking.CoAP
{
	/// <summary>
	/// CoAP Message Type
	/// </summary>
	public enum CoapMessageType
	{
		/// <summary>
		/// Confirmable
		/// </summary>
		CON = 0,

		/// <summary>
		/// Non-confirmable
		/// </summary>
		NON = 1,

		/// <summary>
		/// Acknowledgement
		/// </summary>
		ACK = 2,

		/// <summary>
		/// Reset
		/// </summary>
		RST = 3
	}
}
