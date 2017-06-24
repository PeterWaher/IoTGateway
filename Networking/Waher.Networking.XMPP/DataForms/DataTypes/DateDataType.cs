using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace Waher.Networking.XMPP.DataForms.DataTypes
{
	/// <summary>
	/// Date Data Type (xs:date)
	/// </summary>
	public class DateDataType : DataType
	{
		/// <summary>
		/// Public instance of data type.
		/// </summary>
		public static readonly DateDataType Instance = new DateDataType();

		/// <summary>
		/// Date Data Type (xs:date)
		/// </summary>
		public DateDataType()
			: this("xs:date")
		{
		}

		/// <summary>
		/// Date Data Type (xs:date)
		/// </summary>
		/// <param name="DataType">Data Type</param>
		public DateDataType(string DataType)
			: base(DataType)
		{
		}

		/// <summary>
		/// Parses a string.
		/// </summary>
		/// <param name="Value">String value.</param>
		/// <returns>Parsed value, if possible, null otherwise.</returns>
		public override object Parse(string Value)
		{
			DateTime? DT = ParseDate(Value);

			if (DT.HasValue)
				return DT.Value;
			else
				return null;
		}

		internal static DateTime? ParseDate(string Value)
		{
			if (Value.Length == 10 && Value[4] == '-' && Value[7] == '-')
			{
				if (int.TryParse(Value.Substring(0, 4), out int Year) &&
					int.TryParse(Value.Substring(5, 2), out int Month) &&
					int.TryParse(Value.Substring(8, 2), out int Day))
				{
					return new DateTime(Year, Month, Day);
				}
			}

			if (DateTime.TryParse(Value, out DateTime DT))
				return DT;
			else
				return null;
		}
	}
}
