using System;
using System.Collections.Generic;
using System.Text;

namespace Waher.Networking.XMPP.DataForms.DataTypes
{
	/// <summary>
	/// Time Data Type (xs:time)
	/// </summary>
	public class TimeDataType : DataType
	{
		/// <summary>
		/// Time Data Type (xs:time)
		/// </summary>
		/// <param name="TypeName">Type Name</param>
		public TimeDataType(string DataType)
			: base(DataType)
		{
		}
	}
}
