using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Waher.Networking.XMPP.Concentrator.Attributes
{
	/// <summary>
	/// Defines a localizable header string for the property.
	/// </summary>
	[AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
	public class HeaderAttribute : Attribute
	{
		private int stringId;
		private string header;

		/// <summary>
		/// Defines a header string for the property.
		/// </summary>
		/// <param name="Header">Header string.</param>
		public HeaderAttribute(string Header)
			: this(0, Header)
		{
		}

		/// <summary>
		/// Defines a localizable header string for the property.
		/// </summary>
		/// <param name="StringId">String ID in the namespace of the current class, in the default language defined for the class.</param>
		/// <param name="Header">Default header string, in the default language defined for the class.</param>
		public HeaderAttribute(int StringId, string Header)
		{
			this.stringId = StringId;
			this.header = Header;
		}

		/// <summary>
		/// String ID in the namespace of the current class, in the default language defined for the class.
		/// </summary>
		public int StringId
		{
			get { return this.stringId; }
		}

		/// <summary>
		/// Default header string, in the default language defined for the class.
		/// </summary>
		public string Header
		{
			get { return this.header; }
		}
	}
}
