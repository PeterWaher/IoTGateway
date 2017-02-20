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
		public AnyUriDataType()
			: this("xs:anyUri")
		{
		}

		/// <summary>
		/// Any URI Data Type (xs:anyUri)
		/// </summary>
		/// <param name="TypeName">Type Name</param>
		public AnyUriDataType(string DataType)
			: base(DataType)
		{
		}

		/// <summary>
		/// Parses a string.
		/// </summary>
		/// <param name="Value">String value.</param>
		/// <returns>Parsed value, if possible, null otherwise.</returns>
		public override object Parse(string Value)
		{
			try
			{
				return new Uri(Value);
			}
			catch (Exception)
			{
				return null;
			}
		}
	}
}
