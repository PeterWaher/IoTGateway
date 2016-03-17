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
		/// Date Data Type (xs:date)
		/// </summary>
		/// <param name="TypeName">Type Name</param>
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
				int Year, Month, Day;

				if (int.TryParse(Value.Substring(0, 4), out Year) &&
					int.TryParse(Value.Substring(5, 2), out Month) &&
					int.TryParse(Value.Substring(8, 2), out Day))
				{
					return new DateTime(Year, Month, Day);
				}
			}

			DateTime DT;

			if (DateTime.TryParse(Value, out DT))
				return DT;
			else
				return null;
		}
	}
}
