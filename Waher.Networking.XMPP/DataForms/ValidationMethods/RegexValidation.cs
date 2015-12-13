using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace Waher.Networking.XMPP.DataForms.ValidationMethods
{
	/// <summary>
	/// Performs regex validation.
	/// 
	/// Defined in:
	/// http://xmpp.org/extensions/xep-0122.html#usercases-validatoin.regex
	/// </summary>
	public class RegexValidation : ValidationMethod
	{
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
				this.regex = new Regex(Expression);
			}
			catch (Exception)
			{
				this.regex = null;
			}
		}

	}
}
