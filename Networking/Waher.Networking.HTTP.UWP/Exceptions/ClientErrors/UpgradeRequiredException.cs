using System;
using System.Collections.Generic;
using System.Text;

namespace Waher.Networking.HTTP
{
	/// <summary>
	/// The client should switch to a different protocol such as TLS/1.0, given in the Upgrade header field.
	/// </summary>
	public class UpgradeRequiredException : HttpException
	{
		/// <summary>
		/// The client should switch to a different protocol such as TLS/1.0, given in the Upgrade header field.
		/// </summary>
		/// <param name="Protocol">Protocol to upgrade to.</param>
		public UpgradeRequiredException(string Protocol)
			: base(426, "Upgrade Required", new KeyValuePair<string, string>("Upgrade", Protocol))
		{
		}
	}
}
