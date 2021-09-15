using System;
using System.Threading.Tasks;
using Waher.Networking.XMPP.DataForms;
using Waher.Runtime.Language;
using static Waher.Networking.XMPP.Concentrator.Parameters;

namespace Waher.Networking.XMPP.Concentrator
{
	/// <summary>
	/// Interface for editable objects.
	/// </summary>
	public interface IEditableObject
	{
		/// <summary>
		/// Populates a data form with parameters for the object.
		/// </summary>
		/// <param name="Parameters">Data form to host all editable parameters.</param>
		/// <param name="Language">Current language.</param>
		Task PopulateForm(DataForm Parameters, Language Language);

		/// <summary>
		/// Sets the parameters of the object, based on contents in the data form.
		/// </summary>
		/// <param name="Parameters">Data form with parameter values.</param>
		/// <param name="Language">Current language.</param>
		/// <param name="OnlySetChanged">If only changed parameters are to be set.</param>
		/// <returns>Any errors encountered, or null if parameters was set properly.</returns>
		Task<SetEditableFormResult> SetParameters(DataForm Parameters, Language Language, bool OnlySetChanged);
	}
}
