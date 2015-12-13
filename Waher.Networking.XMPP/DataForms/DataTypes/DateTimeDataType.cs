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
	}
}
