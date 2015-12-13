using System;
using System.Collections.Generic;
using System.Text;

namespace Waher.Networking.XMPP.DataForms.DataTypes
{
	/// <summary>
	/// Integer Data Type (xs:integer)
	/// </summary>
	public class IntegerDataType : DataType
	{
		/// <summary>
		/// Integer Data Type (xs:integer)
		/// </summary>
		/// <param name="TypeName">Type Name</param>
		public IntegerDataType(string DataType)
			: base(DataType)
		{
		}
	}
}
