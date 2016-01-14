using System;
using System.Collections.Generic;
using System.Text;
using Waher.Content;

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
			bool Result;

			if (CommonTypes.TryParse(Value, out Result))
				return Result;
			else
				return null;
		}
	}
}
