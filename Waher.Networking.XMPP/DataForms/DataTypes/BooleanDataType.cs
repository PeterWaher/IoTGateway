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

		/// <summary>
		/// <see cref="DataType.Parse"/>
		/// </summary>
		internal override object Parse(string Value)
		{
			Value = Value.ToLower();

			if (Value == "1" || Value == "true" || Value == "yes" || Value == "on")
				return true;
			else if (Value == "0" || Value == "false" || Value == "no" || Value == "off")
				return false;
			else
				return null;
		}
	}
}
