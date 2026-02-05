using System.Threading.Tasks;
using System.Xml;
using Waher.Networking.HTTP.Interfaces;
using Waher.Persistence;
using Waher.Runtime.Counters;

namespace Waher.Security.WAF.Model.Actions
{
	/// <summary>
	/// Clears a named counter.
	/// </summary>
	public class ClearCounter : WafActionWithName
	{
		/// <summary>
		/// Clears a named counter.
		/// </summary>
		public ClearCounter()
			: base()
		{
		}

		/// <summary>
		/// Clears a named counter.
		/// </summary>
		/// <param name="Xml">XML definition.</param>
		/// <param name="Parent">Parent node.</param>
		/// <param name="Document">Document hosting the Web Application Firewall action.</param>
		public ClearCounter(XmlElement Xml, WafAction Parent, WebApplicationFirewall Document)
			: base(Xml, Parent, Document)
		{
		}

		/// <summary>
		/// XML Local Name for the XML element defining the action.
		/// </summary>
		public override string LocalName => nameof(ClearCounter);

		/// <summary>
		/// Creates a WAF action from its XML definition.
		/// </summary>
		/// <param name="Xml">XML definition.</param>
		/// <param name="Parent">Parent node.</param>
		/// <param name="Document">Document hosting the Web Application Firewall action.</param>
		/// <returns>Created action object.</returns>
		public override WafAction Create(XmlElement Xml, WafAction Parent, WebApplicationFirewall Document) => new ClearCounter(Xml, Parent, Document);

		/// <summary>
		/// Reviews the processing state, and returns a WAF result, if any.
		/// </summary>
		/// <param name="State">Current state.</param>
		/// <returns>Result to return, if any.</returns>
		public override async Task<WafResult?> Review(ProcessingState State)
		{
			CaseInsensitiveString Name = await this.name.EvaluateAsync(State.Variables, string.Empty);

			long Count = await RuntimeCounters.GetCount(Name);
			await RuntimeCounters.DecrementCounter(Name, Count);

			return null;
		}
	}
}
