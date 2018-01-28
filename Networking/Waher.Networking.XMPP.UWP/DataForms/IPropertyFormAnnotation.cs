using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Waher.Networking.XMPP.DataForms;
using Waher.Networking.XMPP.DataForms.Layout;

namespace Waher.Networking.XMPP.DataForms
{
	/// <summary>
	/// Current state of a property form being built.
	/// </summary>
	public class FormState
	{
		/// <summary>
		/// Form being built.
		/// </summary>
		public DataForm Form;

		/// <summary>
		/// Pages by page label.
		/// </summary>
		public Dictionary<string, Page> PageByLabel;

		/// <summary>
		/// Sections by page and section labels.
		/// </summary>
		public Dictionary<string, Section> SectionByPageAndSectionLabel;

		/// <summary>
		/// Default page.
		/// </summary>
		public Page DefaultPage;

		/// <summary>
		/// Language code
		/// </summary>
		public string LanguageCode;

		/// <summary>
		/// Fields
		/// </summary>
		public List<Field> Fields;

		/// <summary>
		/// Pages
		/// </summary>
		public List<Page> Pages;

		/// <summary>
		/// Field ordinal
		/// </summary>
		public int FieldOrdinal;

		/// <summary>
		/// Page ordinal
		/// </summary>
		public int PageOrdinal;
	}

	/// <summary>
	/// Interface for objects that want to annotate their property forms.
	/// </summary>
    public interface IPropertyFormAnnotation
    {
		/// <summary>
		/// Annotates the property form.
		/// </summary>
		/// <param name="Form">Form being built.</param>
		Task AnnotatePropertyForm(FormState Form);
    }
}
