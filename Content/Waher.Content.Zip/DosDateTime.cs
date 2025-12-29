using System;

namespace Waher.Content.Zip
{
	/// <summary>
	/// Static class for converting DateTime values to DOS date and time formats.
	/// </summary>
	public static class DosDateTime
	{
		/// <summary>
		/// Convert DateTime to DOS time format (time stored as hh:mm:ss, 2-second increments)
		/// </summary>
		/// <param name="Timestamp">DateTime value.</param>
		/// <returns>DOS time value.</returns>
		public static ushort ToDosTime(this DateTime Timestamp)
		{
			int Hour = Timestamp.Hour;
			int Minute = Timestamp.Minute;
			int Second = Timestamp.Second / 2;

			return (ushort)((Hour << 11) | (Minute << 5) | Second);
		}

		/// <summary>
		/// Convert DateTime to DOS date format (date stored as year-from-1980, month, day)
		/// </summary>
		/// <param name="Timestamp">DateTime value.</param>
		/// <returns>DOS date value.</returns>
		public static ushort ToDosDate(this DateTime Timestamp)
		{
			int Year = Timestamp.Year < 1980 ? 0 : Timestamp.Year - 1980;
			int Month = Timestamp.Month;
			int Day = Timestamp.Day;

			return (ushort)((Year << 9) | (Month << 5) | Day);
		}
	}
}
