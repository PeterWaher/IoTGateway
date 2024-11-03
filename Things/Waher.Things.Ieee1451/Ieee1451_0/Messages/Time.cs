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
		/// Time representation in IEEE 1451.0
		/// </summary>
		/// <param name="Seconds">Seconds</param>
		/// <param name="Nanoseconds">Nano-seconds</param>
		public Time(ulong Seconds, uint Nanoseconds)
		{
			this.Seconds = Seconds;
			this.NanoSeconds = Nanoseconds;
		}

		/// <summary>
		/// Time representation in IEEE 1451.0
		/// </summary>
		/// <param name="Timestamp">Timestamp</param>
		public Time(DateTime Timestamp)
		{
			TimeSpan TS = Timestamp.ToUniversalTime().Subtract(JSON.UnixEpoch);
			double Seconds = TS.TotalSeconds;
			double FloorSeconds = Math.Floor(Seconds);
			this.Seconds = (ulong)FloorSeconds;
			this.NanoSeconds = (uint)((Seconds - FloorSeconds) * 1e9);
		}

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
