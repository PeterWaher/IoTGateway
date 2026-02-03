using System.Threading.Tasks;
using System.Xml;
using Waher.Networking.HTTP.Interfaces;

namespace Waher.Security.WAF.Model
{
	/// <summary>
	/// interface for Web Application Firewall actions.
	/// </summary>
	public interface IWafAction
	{
		/// <summary>
		/// XML Local Name for the XML element defining the action.
		/// </summary>
		string LocalName { get; }

		/// <summary>
		/// Creates a WAF action from its XML definition.
		/// </summary>
		/// <param name="Xml">XML definition.</param>
		/// <param name="Parent">Parent node.</param>
		/// <param name="Document">Document hosting the Web Application Firewall action.</param>
		/// <returns>Created action object.</returns>
		WafAction Create(XmlElement Xml, WafAction Parent, WebApplicationFirewall Document);

		/// <summary>
		/// Prepares the node for processing.
		/// </summary>
		void Prepare();

		/// <summary>
		/// Reviews the processing state, and returns a WAF result, if any.
		/// </summary>
		/// <param name="State">Current state.</param>
		/// <returns>Result to return, if any.</returns>
		Task<WafResult?> Review(ProcessingState State);
	}
}
