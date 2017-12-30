using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Waher.Things.Attributes
{
	/// <summary>
	/// Defines the default language for the class.
	/// </summary>
	[AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
	public class DefaultLanguageAttribute : Attribute
	{
		private string languageCode;

		/// <summary>
		/// Defines the default language for the class.
		/// </summary>
		/// <param name="LanguageCode">Default language code.</param>
		public DefaultLanguageAttribute(string LanguageCode)
		{
			this.languageCode = LanguageCode;
		}

		/// <summary>
		/// Default language code.
		/// </summary>
		public string LanguageCode
		{
			get { return this.languageCode; }
		}
	}
}
