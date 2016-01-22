using System;
using System.Collections.Generic;
using System.Text;

namespace Waher.Networking.XMPP.DataForms.DataTypes
{
	/// <summary>
	/// DateTime Data Type (xs:dateTime)
	/// </summary>
	public class DateTimeDataType : DataType
	{
		/// <summary>
		/// DateTime Data Type (xs:dateTime)
		/// </summary>
		/// <param name="TypeName">Type Name</param>
		public DateTimeDataType(string DataType)
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
			int i = Value.IndexOf('T');

			if (i == 10)
			{
				DateTime? DT = DateDataType.ParseDate(Value.Substring(0, 10));
				TimeSpan TS;

				if (DT.HasValue && TimeSpan.TryParse(Value.Substring(11), out TS))
					return DT.Value + TS;
			}

			DateTime DT2;

			if (DateTime.TryParse(Value, out DT2))
				return DT2;
			else
				return null;
		}
	}
}
