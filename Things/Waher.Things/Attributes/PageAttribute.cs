using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Waher.Things.Attributes
{
	/// <summary>
	/// Places the parameter on a localizable page.
	/// </summary>
	[AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
	public class PageAttribute : Attribute
	{
		private int stringId;
		private string label;

		/// <summary>
		/// Places the parameter on a page.
		/// </summary>
		/// <param name="Label">Label string</param>
		public PageAttribute(string Label)
			: this(0, Label)
		{
		}

		/// <summary>
		/// Places the parameter on a localizable page.
		/// </summary>
		/// <param name="StringId">String ID in the namespace of the current class, in the default language defined for the class.</param>
		/// <param name="Label">Default label string, in the default language defined for the class.</param>
		public PageAttribute(int StringId, string Label)
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
