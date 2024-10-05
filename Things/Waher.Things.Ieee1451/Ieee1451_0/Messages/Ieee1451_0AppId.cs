using System;

namespace Waher.Things.Ieee1451.Ieee1451_0.Messages
{
	/// <summary>
	/// Application identifying information
	/// </summary>
	public class Ieee1451_0AppId
	{
		/// <summary>
		/// Error Code
		/// </summary>
		public ushort ErrorCode;

		/// <summary>
		/// Application ID
		/// </summary>
		public Guid ApplicationId;
	}
}
