using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Waher.Things.Attributes
{
	/// <summary>
	/// Defines a localizable header string for the property.
	/// </summary>
	[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = false, Inherited = true)]
	public class HeaderAttribute : Attribute
	{
		/// <summary>
		/// Default priority of parameters (100).
		/// </summary>
		public const int DefaultPriority = 100;

		private int stringId;
		private string header;
		private int priority;

		/// <summary>
		/// Defines a header string for the property.
		/// </summary>
		/// <param name="Header">Header string.</param>
		public HeaderAttribute(string Header)
			: this(0, Header, DefaultPriority)
		{
		}

		/// <summary>
		/// Defines a localizable header string for the property.
		/// </summary>
		/// <param name="StringId">String ID in the namespace of the current class, in the default language defined for the class.</param>
		/// <param name="Header">Default header string, in the default language defined for the class.</param>
		public HeaderAttribute(int StringId, string Header)
			: this(StringId, Header, DefaultPriority)
		{
		}

		/// <summary>
		/// Defines a header string for the property.
		/// </summary>
		/// <param name="Header">Header string.</param>
		/// <param name="Priority">Priority of parameter (default=100).</param>
		public HeaderAttribute(string Header, int Priority)
			: this(0, Header, Priority)
		{
		}

		/// <summary>
		/// Defines a localizable header string for the property.
		/// </summary>
		/// <param name="StringId">String ID in the namespace of the current class, in the default language defined for the class.</param>
		/// <param name="Header">Default header string, in the default language defined for the class.</param>
		/// <param name="Priority">Priority of parameter (default=100).</param>
		public HeaderAttribute(int StringId, string Header, int Priority)
		{
			this.stringId = StringId;
			this.header = Header;
			this.priority = Priority;
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

		/// <summary>
		/// Priority of parameter (default=100).
		/// </summary>
		public int Priority
		{
			get { return this.priority; }
		}
	}
}
