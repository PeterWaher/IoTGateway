using System.Threading.Tasks;

namespace Waher.Networking.XMPP.DataForms
{
	/// <summary>
	/// Interface for objects that want to handle custom properties in property forms.
	/// </summary>
	public interface ICustomFormProperties
	{
		/// <summary>
		/// Performs custom validation of a property.
		/// </summary>
		/// <param name="Field">Property field.</param>
		Task ValidateCustomProperty(Field Field);

		/// <summary>
		/// Sets the custom parameter to the value(s) provided in the field.
		/// </summary>
		/// <param name="Field">Field</param>
		Task SetCustomProperty(Field Field);
	}

}
