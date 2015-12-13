using System;
using System.Collections.Generic;
using System.Text;

namespace Waher.Networking.XMPP.DataForms.DataTypes
{
	/// <summary>
	/// Double Data Type (xs:double)
	/// </summary>
	public class DoubleDataType : DataType
	{
		/// <summary>
		/// Double Data Type (xs:double)
		/// </summary>
		/// <param name="TypeName">Type Name</param>
		public DoubleDataType(string DataType)
			: base(DataType)
		{
		}
	}
}
