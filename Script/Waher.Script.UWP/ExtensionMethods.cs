using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Waher.Script
{
	public static class ExtensionMethods
	{
#if WINDOWS_UWP
		/// <summary>
		/// Extension method calculating <see cref="DateTime.ToOADate()"/> available the .NET framework.
		/// </summary>
		/// <param name="TP"></param>
		/// <returns></returns>
		public static double ToOADate(this DateTime TP)
		{
			TimeSpan TS = TP - oaDateRef;
			return TS.TotalDays;
		}

		private static readonly DateTime oaDateRef = new DateTime(1899, 12, 30, 0, 0, 0);
#endif
	}
}
