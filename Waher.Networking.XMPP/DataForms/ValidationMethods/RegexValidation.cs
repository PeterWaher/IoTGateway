using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using Waher.Networking.XMPP.DataForms.DataTypes;

namespace Waher.Networking.XMPP.DataForms.ValidationMethods
{
	/// <summary>
	/// Performs regex validation.
	/// 
	/// Defined in:
	/// http://xmpp.org/extensions/xep-0122.html#usercases-validatoin.regex
	/// </summary>
	public class RegexValidation : BasicValidation
	{
		private string expression;
		private Regex regex;

		/// <summary>
		/// Performs regex validation.
		/// 
		/// Defined in:
		/// http://xmpp.org/extensions/xep-0122.html#usercases-validatoin.regex
		/// </summary>
		public RegexValidation(string Expression)
			: base()
		{
			try
			{
				this.expression = Expression;
				this.regex = new Regex(Expression);
			}
			catch (Exception)
			{
				this.regex = null;
			}
		}

		internal override void Serialize(StringBuilder Output)
		{
			Output.Append("<regex>");
			Output.Append(XML.Encode(this.expression));
			Output.Append("</regex>");
		}

		internal override void Validate(Field Field, DataType DataType, object[] Parsed, string[] Strings)
		{
			base.Validate(Field, DataType, Parsed, Strings);

			Match M;

			foreach (string s in Strings)
			{
				M = this.regex.Match(s);
				if (!M.Success || M.Index > 0 || M.Length < s.Length)
					Field.Error = "Invalid input.";
			}
		}

	}
}
