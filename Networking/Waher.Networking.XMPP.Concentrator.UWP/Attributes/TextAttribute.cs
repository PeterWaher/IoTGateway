using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Waher.Networking.XMPP.Concentrator.Attributes
{
	/// <summary>
	/// Shows a text segment before the parameter.
	/// </summary>
	[AttributeUsage(AttributeTargets.Property, AllowMultiple = true, Inherited = true)]
	public class TextAttribute : Attribute
	{
		private int stringId;
		private string label;

		/// <summary>
		/// Shows a text segment before the parameter.
		/// </summary>
		/// <param name="Label">Label string</param>
		public TextAttribute(string Label)
			: this(0, Label)
		{
		}

		/// <summary>
		/// Shows a text segment before the parameter.
		/// </summary>
		/// <param name="StringId">String ID in the namespace of the current class, in the default language defined for the class.</param>
		/// <param name="Label">Default label string, in the default language defined for the class.</param>
		public TextAttribute(int StringId, string Label)
		{
			this.stringId = StringId;
			this.label = Label;
		}

		/// <summary>
		/// String ID in the namespace of the current class, in the default language defined for the class.
		/// </summary>
		public int StringId
		{
			get { return this.stringId; }
		}

		/// <summary>
		/// Default label string, in the default language defined for the class.
		/// </summary>
		public string Label
		{
			get { return this.label; }
		}
	}
}
