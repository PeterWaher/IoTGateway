using System;
using System.Collections.Generic;
using System.Text;
using Waher.Content.Xml;

namespace Waher.Networking.XMPP.Synchronization
{
	/// <summary>
	/// Date and Time value based on the intenal high-frequency timer.
	/// </summary>
	public class DateTimeHF
	{
		private readonly int year;
		private readonly int month;
		private readonly int day;
		private readonly int hour;
		private readonly int minute;
		private readonly int second;
		private readonly int millisecond;
		private readonly int microsecond;
		private readonly int nanosecond100;
		private readonly DateTimeKind kind;

		/// <summary>
		/// Date and Time value based on the intenal high-frequency timer.
		/// </summary>
		/// <param name="TP">DateTime-value</param>
		/// <param name="Microsecond">Microsecond</param>
		/// <param name="Nanosecond100">100-nanosecond</param>
		public DateTimeHF(DateTime TP, int Microsecond, int Nanosecond100)
			: this(TP.Year, TP.Month, TP.Day, TP.Hour, TP.Minute, TP.Second, TP.Millisecond,
				  Microsecond, Nanosecond100, TP.Kind)
		{
		}

		/// <summary>
		/// Date and Time value based on the intenal high-frequency timer.
		/// </summary>
		/// <param name="Year">Year</param>
		/// <param name="Month">Month</param>
		/// <param name="Day">Day</param>
		/// <param name="Hour">Hour</param>
		/// <param name="Minute">Minute</param>
		/// <param name="Second">Second</param>
		/// <param name="Millisecond">Millisecond</param>
		/// <param name="Microsecond">Microsecond</param>
		/// <param name="Nanosecond100">100-nanosecond</param>
		/// <param name="Kind">Kind</param>
		public DateTimeHF(int Year, int Month, int Day, int Hour, int Minute, int Second,
			int Millisecond, int Microsecond, int Nanosecond100, DateTimeKind Kind)
		{
			this.year = Year;
			this.month = Month;
			this.day = Day;
			this.hour = Hour;
			this.minute = Minute;
			this.second = Second;
			this.millisecond = Millisecond;
			this.microsecond = Microsecond;
			this.nanosecond100 = Nanosecond100;
			this.kind = Kind;
		}

		/// <summary>
		/// Year
		/// </summary>
		public int Year => this.year;

		/// <summary>
		/// Month
		/// </summary>
		public int Month => this.month;

		/// <summary>
		/// Day
		/// </summary>
		public int Day => this.day;

		/// <summary>
		/// Hour
		/// </summary>
		public int Hour => this.hour;

		/// <summary>
		/// Minute
		/// </summary>
		public int Minute => this.minute;

		/// <summary>
		/// Second
		/// </summary>
		public int Second => this.second;

		/// <summary>
		/// Millisecond
		/// </summary>
		public int Millisecond => this.millisecond;

		/// <summary>
		/// Microsecond
		/// </summary>
		public int Microsecond => this.microsecond;

		/// <summary>
		/// 100-nanosecond
		/// </summary>
		public int Nanosecond100 => this.nanosecond100;

		/// <summary>
		/// Tries to parse a high-frequency date and time value.
		/// </summary>
		/// <param name="s">String-representation</param>
		/// <param name="Value">Parsed value, if successful.</param>
		/// <returns>If value could be parsed.</returns>
		public static bool TryParse(string s, out DateTimeHF Value)
		{
			int i = s.IndexOf('.');

			if (i < 0)
			{
				if (XML.TryParse(s, out DateTime TP))
				{
					Value = new DateTimeHF(TP, 0, 0);
					return true;
				}
				else
				{
					Value = null;
					return false;
				}
			}
			else
			{
				int j = i + 1;
				int c = s.Length;
				char ch;

				while (j < c && (ch = s[j]) >= '0' && ch <= '9')
					j++;

				j -= i;

				if (j <= 3)
				{
					if (XML.TryParse(s, out DateTime TP))
					{
						Value = new DateTimeHF(TP, 0, 0);
						return true;
					}
					else
					{
						Value = null;
						return false;
					}
				}
				else
				{
					string s2;

					j -= 3;
					if (j < 4)
						s2 = s.Substring(i + 4, j) + new string('0', 4 - j);
					else if (j > 4)
						s2 = s.Substring(i + 4, 4);
					else
						s2 = s.Substring(i + 4, j);

					s = s.Remove(i + 4, j);

					if (XML.TryParse(s, out DateTime TP) && int.TryParse(s2, out j))
					{
						Value = new DateTimeHF(TP, j / 10, j % 10);
						return true;
					}
					else
					{
						Value = null;
						return false;
					}
				}
			}
		}

		/// <summary>
		/// Normal Date and Time representation.
		/// </summary>
		public DateTime DateTime
		{
			get
			{
				return new DateTime(this.year, this.month, this.day,
					this.hour, this.minute, this.second, this.millisecond, this.kind);
			}
		}

		/// <summary>
		/// Calculates the differente, in units of 100 ns, between two high-frequency
		/// based date and time values.
		/// </summary>
		/// <param name="Timestamp1">Timestamp 1</param>
		/// <param name="Timestamp2">Timestamp 2</param>
		/// <returns>Difference, in units of 100 ns.</returns>
		public static long operator -(DateTimeHF Timestamp1, DateTimeHF Timestamp2)
		{
			long Diff = (long)((Timestamp1.DateTime - Timestamp2.DateTime).TotalMilliseconds + 0.5);

			Diff *= 1000;
			Diff += (Timestamp1.microsecond - Timestamp2.microsecond);

			Diff *= 10;
			Diff += (Timestamp1.nanosecond100 - Timestamp2.nanosecond100);

			return Diff;
		}

	}
}
