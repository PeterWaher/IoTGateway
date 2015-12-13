using System;
using System.Collections.Generic;
using System.Text;

namespace Waher.Networking.XMPP.DataForms.DataTypes
{
	/// <summary>
	/// Any URI Data Type (xs:anyUri)
	/// </summary>
	public class AnyUriDataType : DataType
	{
		/// <summary>
		/// Any URI Data Type (xs:anyUri)
		/// </summary>
		/// <param name="TypeName">Type Name</param>
		public AnyUriDataType(string DataType)
			: base(DataType)
		{
		}
	}
}
