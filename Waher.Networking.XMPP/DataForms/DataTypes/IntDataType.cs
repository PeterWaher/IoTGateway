using System;
using System.Collections.Generic;
using System.Text;

namespace Waher.Networking.XMPP.DataForms.DataTypes
{
	/// <summary>
	/// Int Data Type (xs:int)
	/// </summary>
	public class IntDataType : DataType
	{
		/// <summary>
		/// Int Data Type (xs:int)
		/// </summary>
		/// <param name="TypeName">Type Name</param>
		public IntDataType(string DataType)
			: base(DataType)
		{
		}

		/// <summary>
		/// <see cref="DataType.Parse"/>
		/// </summary>
		internal override object Parse(string Value)
		{
			int Result;

			if (int.TryParse(Value, out Result))
				return Result;
			else
				return null;
		}
	}
}
