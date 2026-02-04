using System.Threading.Tasks;
using System.Xml;
using Waher.Content.Xml.Attributes;
using Waher.Networking.HTTP.Interfaces;
using Waher.Security.WAF.Model.Comparisons;

namespace Waher.Security.WAF.Model.Conditions
{
	/// <summary>
	/// Evaluates a script condition.
	/// </summary>
	public class If : WafComparison
	{
		private readonly ExpressionAttribute condition;

		/// <summary>
		/// Evaluates a script condition.
		/// </summary>
		public If()
			: base()
		{
		}

		/// <summary>
		/// Evaluates a script condition.
		/// </summary>
		/// <param name="Xml">XML definition.</param>
		/// <param name="Parent">Parent node.</param>
		/// <param name="Document">Document hosting the Web Application Firewall action.</param>
		public If(XmlElement Xml, WafAction Parent, WebApplicationFirewall Document)
			: base(Xml, Parent, Document)
		{
			this.condition = new ExpressionAttribute(Xml, "condition");
		}

		/// <summary>
		/// XML Local Name for the XML element defining the action.
		/// </summary>
		public override string LocalName => nameof(If);

		/// <summary>
		/// Creates a WAF action from its XML definition.
		/// </summary>
		/// <param name="Xml">XML definition.</param>
		/// <param name="Parent">Parent node.</param>
		/// <param name="Document">Document hosting the Web Application Firewall action.</param>
		/// <returns>Created action object.</returns>
		public override WafAction Create(XmlElement Xml, WafAction Parent, WebApplicationFirewall Document) => new If(Xml, Parent, Document);

		/// <summary>
		/// Reviews the processing state, and returns a WAF result, if any.
		/// </summary>
		/// <param name="State">Current state.</param>
		/// <returns>Result to return, if any.</returns>
		public override async Task<WafResult?> Review(ProcessingState State)
		{
			object Obj = await this.condition.EvaluateAsync(State.Variables);

			if (Obj is bool b && b)
				return await this.ReviewChildren(State);
			else
				return null;
		}
	}
}
