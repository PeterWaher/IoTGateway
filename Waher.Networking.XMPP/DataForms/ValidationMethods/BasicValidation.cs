using System;
using System.Collections.Generic;
using System.Text;
using Waher.Networking.XMPP.DataForms.DataTypes;

namespace Waher.Networking.XMPP.DataForms.ValidationMethods
{
	/// <summary>
	/// Performs basic validation.
	/// 
	/// Defined in:
	/// http://xmpp.org/extensions/xep-0122.html#usercases-validation.basic
	/// </summary>
	public class BasicValidation : ValidationMethod
	{
		/// <summary>
		/// Performs basic validation.
		/// 
		/// Defined in:
		/// http://xmpp.org/extensions/xep-0122.html#usercases-validation.basic
		/// </summary>
		public BasicValidation()
			: base()
		{
		}

		internal override void Serialize(StringBuilder Output)
		{
			Output.Append("<basic/>");
		}

		internal override void Validate(Field Field, DataType DataType, object[] Parsed, string[] Strings)
		{
			KeyValuePair<string, string>[] Options = Field.Options;

			if (Options != null && Options.Length > 0)
			{
				foreach (string s in Strings)
				{
					if (Array.FindIndex<KeyValuePair<string, string>>(Options, P => P.Value == s) < 0)
					{
						Field.Error = "Value not in allowed set of options.";
						return;
					}
				}
			}
		}
	}
}
