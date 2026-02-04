using System.Threading.Tasks;
using System.Xml;
using Waher.Content.Xml.Attributes;
using Waher.Networking.HTTP.Interfaces;

namespace Waher.Security.WAF.Model.Actions
{
	/// <summary>
	/// Delays processing for a number of seconds.
	/// </summary>
	public class Delay : WafAction
	{
		private readonly PositiveIntegerAttribute seconds;

		/// <summary>
		/// Delays processing for a number of seconds.
		/// </summary>
		public Delay()
			: base()
		{
		}

		/// <summary>
		/// Delays processing for a number of seconds.
		/// </summary>
		/// <param name="Xml">XML definition.</param>
		/// <param name="Parent">Parent node.</param>
		/// <param name="Document">Document hosting the Web Application Firewall action.</param>
		public Delay(XmlElement Xml, WafAction Parent, WebApplicationFirewall Document)
			: base(Xml, Parent, Document)
		{
			this.seconds = new PositiveIntegerAttribute(Xml, "seconds");
		}

		/// <summary>
		/// XML Local Name for the XML element defining the action.
		/// </summary>
		public override string LocalName => nameof(Delay);

		/// <summary>
		/// Creates a WAF action from its XML definition.
		/// </summary>
		/// <param name="Xml">XML definition.</param>
		/// <param name="Parent">Parent node.</param>
		/// <param name="Document">Document hosting the Web Application Firewall action.</param>
		/// <returns>Created action object.</returns>
		public override WafAction Create(XmlElement Xml, WafAction Parent, WebApplicationFirewall Document) => new Delay(Xml, Parent, Document);

		/// <summary>
		/// Reviews the processing state, and returns a WAF result, if any.
		/// </summary>
		/// <param name="State">Current state.</param>
		/// <returns>Result to return, if any.</returns>
		public override async Task<WafResult?> Review(ProcessingState State)
		{
			int Seconds = await this.seconds.EvaluateAsync(State.Variables, 0);

			await Task.Delay(Seconds);

			return null;
		}
	}
}
