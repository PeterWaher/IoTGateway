using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Waher.Networking.XMPP.Concentrator.Attributes
{
	/// <summary>
	/// Validates input against a regular expression.
	/// </summary>
	[AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
	public class RegularExpressionAttribute : Attribute
	{
		private string pattern;

		/// <summary>
		/// Validates input against a regular expression.
		/// </summary>
		/// <param name="Pattern">Regular expression to validate against.</param>
		public RegularExpressionAttribute(string Pattern)
		{
			this.pattern = Pattern;
		}

		/// <summary>
		/// Regular expression to validate against.
		/// </summary>
		public string Pattern
		{
			get { return this.pattern; }
		}
	}
}
