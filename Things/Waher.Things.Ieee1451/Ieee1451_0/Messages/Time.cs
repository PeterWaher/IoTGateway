using System;
using Waher.Content;

namespace Waher.Things.Ieee1451.Ieee1451_0.Messages
{
	/// <summary>
	/// Time representation in IEEE 1451.0
	/// </summary>
	public struct Time
	{
		/// <summary>
		/// Seconds
		/// </summary>
		public ulong Seconds;

		/// <summary>
		/// Nano-seconds
		/// </summary>
		public uint NanoSeconds;

		/// <summary>
		/// Converts the Time structure to a <see cref="DateTime"/>.
		/// </summary>
		/// <returns>DateTime Value.</returns>
		public DateTime ToDateTime()
		{
			DateTime TP = JSON.UnixEpoch.AddSeconds(this.Seconds);

			if (this.NanoSeconds != 0)
			{
				double Milliseconds = this.NanoSeconds * 1e-6;
				TP = TP.AddMilliseconds(Milliseconds);
			}

			return TP;
		}
	}
}
