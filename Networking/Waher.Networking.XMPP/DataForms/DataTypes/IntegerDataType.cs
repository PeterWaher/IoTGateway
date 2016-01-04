using System;
using System.Collections.Generic;
using System.Text;
using System.Numerics;

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

		/// <summary>
		/// <see cref="DataType.Parse"/>
		/// </summary>
		internal override object Parse(string Value)
		{
			BigInteger Result;

			if (BigInteger.TryParse(Value, out Result))
				return Result;
			else
				return null;
		}
	}
}
