using System;
using System.Collections.Generic;
using System.Text;

namespace Waher.Networking.XMPP.DataForms.DataTypes
{
	/// <summary>
	/// Boolean Data Type (xs:boolean)
	/// </summary>
	public class BooleanDataType : DataType
	{
		/// <summary>
		/// Boolean Data Type (xs:boolean)
		/// </summary>
		/// <param name="TypeName">Type Name</param>
		public BooleanDataType(string DataType)
			: base(DataType)
		{
		}
	}
}
