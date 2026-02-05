using System.Threading.Tasks;
using System.Xml;
using Waher.Networking.HTTP.Interfaces;
using Waher.Persistence;
using Waher.Runtime.Counters;
using Waher.Script;

namespace Waher.Security.WAF.Model.Actions
{
	/// <summary>
	/// Decrements a named counter.
	/// </summary>
	public class DecrementCounter : WafActionWithNameAndDelta
	{
		/// <summary>
		/// Decrements a named counter.
		/// </summary>
		public DecrementCounter()
			: base()
		{
		}

		/// <summary>
		/// Decrements a named counter.
		/// </summary>
		/// <param name="Xml">XML definition.</param>
		/// <param name="Parent">Parent node.</param>
		/// <param name="Document">Document hosting the Web Application Firewall action.</param>
		public DecrementCounter(XmlElement Xml, WafAction Parent, WebApplicationFirewall Document)
			: base(Xml, Parent, Document)
		{
		}

		/// <summary>
		/// XML Local Name for the XML element defining the action.
		/// </summary>
		public override string LocalName => nameof(DecrementCounter);

		/// <summary>
		/// Creates a WAF action from its XML definition.
		/// </summary>
		/// <param name="Xml">XML definition.</param>
		/// <param name="Parent">Parent node.</param>
		/// <param name="Document">Document hosting the Web Application Firewall action.</param>
		/// <returns>Created action object.</returns>
		public override WafAction Create(XmlElement Xml, WafAction Parent, WebApplicationFirewall Document) => new DecrementCounter(Xml, Parent, Document);

		/// <summary>
		/// Reviews the processing state, and returns a WAF result, if any.
		/// </summary>
		/// <param name="State">Current state.</param>
		/// <returns>Result to return, if any.</returns>
		public override async Task<WafResult?> Review(ProcessingState State)
		{
			Variables Variables = State.Variables;
			CaseInsensitiveString Name = await this.name.EvaluateAsync(Variables, string.Empty);
			long Delta = await this.delta.EvaluateAsync(Variables, 1L);

			await RuntimeCounters.DecrementCounter(Name, Delta);

			return null;
		}
	}
}
