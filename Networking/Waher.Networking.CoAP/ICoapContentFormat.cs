using System;

namespace Waher.Networking.CoAP
{
	/// <summary>
	/// Interface for all CoAP content formats.
	/// </summary>
	public interface ICoapContentFormat
	{
		/// <summary>
		/// Content format number
		/// </summary>
		int ContentFormat
		{
			get;
		}

		/// <summary>
		/// Internet content type.
		/// </summary>
		string ContentType
		{
			get;
		}
	}
}
