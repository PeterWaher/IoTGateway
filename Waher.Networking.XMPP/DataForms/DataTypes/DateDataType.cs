using System;
using System.Collections.Generic;
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
	}
}
