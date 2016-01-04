using System;
using System.Collections.Generic;
using System.Text;
using Waher.Networking.XMPP.DataForms.DataTypes;

namespace Waher.Networking.XMPP.DataForms.ValidationMethods
{
	/// <summary>
	/// Performs open validation.
	/// 
	/// Defined in:
	/// http://xmpp.org/extensions/xep-0122.html#usercases-validation.open
	/// </summary>
	public class OpenValidation : ValidationMethod
	{
		/// <summary>
		/// Performs open validation.
		/// 
		/// Defined in:
		/// http://xmpp.org/extensions/xep-0122.html#usercases-validation.open
		/// </summary>
		public OpenValidation()
			: base()
		{
		}

		internal override void Serialize(StringBuilder Output)
		{
			Output.Append("<open/>");
		}

		internal override void Validate(Field Field, DataType DataType, object[] Parsed, string[] Strings)
		{
			// Valid, as data has already been parsed and seen to be OK. Values can be outside options.
		}
	}
}
