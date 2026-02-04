using System.Threading.Tasks;
using System.Xml;
using Waher.Content.Xml.Attributes;
using Waher.Networking.HTTP.Interfaces;

namespace Waher.Security.WAF.Model.Comparisons
{
	/// <summary>
	/// Abstract base class for limit comparisons.
	/// </summary>
	public abstract class LimitComparison : WafComparison
	{
		private readonly Int64Attribute limit;

		/// <summary>
		/// Abstract base class for limit comparisons.
		/// </summary>
		public LimitComparison()
			: base()
		{
		}

		/// <summary>
		/// Abstract base class for limit comparisons.
		/// </summary>
		/// <param name="Xml">XML definition.</param>
		/// <param name="Parent">Parent node.</param>
		/// <param name="Document">Document hosting the Web Application Firewall action.</param>
		public LimitComparison(XmlElement Xml, WafAction Parent, WebApplicationFirewall Document)
			: base(Xml, Parent, Document)
		{
			this.limit = new Int64Attribute(Xml, "limit");
		}

		/// <summary>
		/// Reviews the processing state, and returns a WAF result, if any.
		/// </summary>
		/// <param name="State">Current state.</param>
		/// <param name="Value">Value being reviewed.</param>
		/// <returns>Result to return, if any.</returns>
		public async Task<WafResult?> Review(ProcessingState State, long Value)
		{
			long Limit = await this.limit.EvaluateAsync(State.Variables, 0);

			if (Value > Limit)
				return await this.ReviewChildren(State);
			else
				return null;
		}
	}
}