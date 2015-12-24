using System;
using System.Collections.Generic;
using System.Text;

namespace Waher.Things.SensorData
{
	/// <summary>
	/// Field Type flags
	/// </summary>
	[Flags]
	public enum FieldType
	{
		/// <summary>
		/// A momentary value represents a value measured at the time of the read-out. Examples: Energy, Volume, Power, Flow, Temperature, Pressure, etc.
		/// </summary>
		Momentary = 1,

		/// <summary>
		/// A value that can be used for identification. (Serial numbers, meter IDs, locations, names, addresses, etc.)
		/// </summary>
		Identity = 2,

		/// <summary>
		/// A value displaying status information about something. Examples: Health, Battery life time, Runtime, Expected life time, Signal strength, 
		/// Signal quality, etc. 
		/// </summary>
		Status = 4,

		/// <summary>
		/// A value that is computed instead of measured. 
		/// </summary>
		Computed = 8,

		/// <summary>
		/// A maximum or minimum value during a given period. Examples "Temperature, Max", "Temperature, Min", etc.
		/// </summary>
		Peak = 16,

		/// <summary>
		/// A value stored at a second shift (milliseconds = 0). 
		/// </summary>
		HistoricalSecond = 32,

		/// <summary>
		/// A value stored at a minute shift (seconds=milliseconds=0). Are also second values.
		/// </summary>
		HistoricalMinute = 64,

		/// <summary>
		/// A value stored at a hour shift (minutes=seconds=milliseconds=0). Are also minute and second values.
		/// </summary>
		HistoricalHour = 128,

		/// <summary>
		/// A value stored at a day shift (hours=minutes=seconds=milliseconds=0). Are also hour, minute and second values.
		/// </summary>
		HistoricalDay = 256,

		/// <summary>
		/// A value stored at a week shift (Monday, hours=minutes=seconds=milliseconds=0). Are also day, hour, minute and second values.
		/// </summary>
		HistoricalWeek = 512,

		/// <summary>
		/// A value stored at a month shift (day=1, hours=minutes=seconds=milliseconds=0). Are also day, hour, minute and second values.
		/// </summary>
		HistoricalMonth = 1024,

		/// <summary>
		/// A value stored at a quarter year shift (Month=Jan, Apr, Jul, Oct, day=1, hours=minutes=seconds=milliseconds=0). 
		/// Are also month, day, hour, minute and second values.
		/// </summary>
		HistoricalQuarter = 2048,

		/// <summary>
		/// A value stored at a year shift (Month=Jan, day=1, hours=minutes=seconds=milliseconds=0). Are also quarter, month, 
		/// day, hour, minute and second values.
		/// </summary>
		HistoricalYear = 4096,

		/// <summary>
		/// If period if historical value is not important in the request or by the device.
		/// </summary>
		HistoricalOther = 8192,

		/// <summary>
		/// Any of the historical field types.
		/// </summary>
		Historical = 32 + 64 + 128 + 256 + 512 + 1024 + 2048 + 4096 + 8192,

		/// <summary>
		/// All field types.
		/// </summary>
		All = 1 + 2 + 4 + 8 + 16 + 32 + 64 + 128 + 256 + 512 + 1024 + 2048 + 4096 + 8192
	}
}
