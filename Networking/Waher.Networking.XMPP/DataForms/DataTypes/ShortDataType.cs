using System;
using System.Collections.Generic;
using System.Text;

namespace Waher.Networking.XMPP.DataForms.DataTypes
{
	/// <summary>
	/// Short Data Type (xs:short)
	/// </summary>
	public class ShortDataType : DataType
	{
		/// <summary>
		/// Short Data Type (xs:short)
		/// </summary>
		/// <param name="TypeName">Type Name</param>
		public ShortDataType(string DataType)
			: base(DataType)
		{
		}

		/// <summary>
		/// <see cref="DataType.Parse"/>
		/// </summary>
		internal override object Parse(string Value)
		{
			short Result;

			if (short.TryParse(Value, out Result))
				return Result;
			else
				return null;
		}
	}
}
