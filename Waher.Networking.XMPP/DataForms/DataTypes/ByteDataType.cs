using System;
using System.Collections.Generic;
using System.Text;

namespace Waher.Networking.XMPP.DataForms.DataTypes
{
	/// <summary>
	/// Byte Data Type (xs:byte)
	/// </summary>
	public class ByteDataType : DataType
	{
		/// <summary>
		/// Byte Data Type (xs:byte)
		/// </summary>
		/// <param name="TypeName">Type Name</param>
		public ByteDataType(string DataType)
			: base(DataType)
		{
		}
	}
}
