using System;
using System.Collections.Generic;
using System.Text;

namespace Waher.Networking.XMPP.DataForms.DataTypes
{
	/// <summary>
	/// Decimal Data Type (xs:decimal)
	/// </summary>
	public class DecimalDataType : DataType
	{
		/// <summary>
		/// Decimal Data Type (xs:decimal)
		/// </summary>
		/// <param name="TypeName">Type Name</param>
		public DecimalDataType(string DataType)
			: base(DataType)
		{
		}
	}
}
