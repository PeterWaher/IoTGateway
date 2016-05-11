using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Waher.Events
{
	public static class ExtensionMethods
	{
#if WINDOWS_UWP
		/// <summary>
		/// Extension method calculating <see cref="DateTime.ToOADate()"/> available the .NET framework.
		/// </summary>
		/// <param name="TP"></param>
		/// <returns>COM-DateTime value.</returns>
		public static double ToOADate(this DateTime TP)
		{
			TimeSpan TS = TP - oaDateRef;
			return TS.TotalDays;
		}

		private static readonly DateTime oaDateRef = new DateTime(1899, 12, 30, 0, 0, 0);

		/// <summary>
		/// Converts the DateTime value to a short string.
		/// </summary>
		/// <param name="TP">Timepoint.</param>
		/// <returns>Short string version of DateTime.</returns>
		public static String ToShortDateString(this DateTime TP)
		{
			return TP.ToString(System.Globalization.DateTimeFormatInfo.CurrentInfo.ShortDatePattern);
		}

		/// <summary>
		/// Converts the DateTime value to a long time string.
		/// </summary>
		/// <param name="TP">Timepoint.</param>
		/// <returns>Long time string version of DateTime.</returns>
		public static String ToLongTimeString(this DateTime TP)
		{
			return TP.ToString(System.Globalization.DateTimeFormatInfo.CurrentInfo.LongTimePattern);
		}
#endif
	}
}
