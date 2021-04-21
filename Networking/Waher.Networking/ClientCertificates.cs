using System;

namespace Waher.Networking
{
	/// <summary>
	/// Client Certificate Options
	/// </summary>
	public enum ClientCertificates
	{
		/// <summary>
		/// Client certificates are not used, and will not be requested.
		/// </summary>
		NotUsed,

		/// <summary>
		/// Client certificates are requested, but not required.
		/// </summary>
		Optional,

		/// <summary>
		/// Client certificates are requested, and required.
		/// </summary>
		Required
	}
}
