using System;
using System.Collections.Generic;
using System.Text;

namespace Waher.Networking.XMPP.DataForms.DataTypes
{
	/// <summary>
	/// Long Data Type (xs:long)
	/// </summary>
	public class LongDataType : DataType
	{
		/// <summary>
		/// Long Data Type (xs:long)
		/// </summary>
		/// <param name="TypeName">Type Name</param>
		public LongDataType(string DataType)
			: base(DataType)
		{
		}

		/// <summary>
		/// <see cref="DataType.Parse"/>
		/// </summary>
		internal override object Parse(string Value)
		{
			long Result;

			if (long.TryParse(Value, out Result))
				return Result;
			else
				return null;
		}
	}
}
