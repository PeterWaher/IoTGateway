using System.Threading.Tasks;
using System.Xml;
using Waher.Content.Xml.Attributes;
using Waher.Networking.HTTP.Interfaces;
using Waher.Persistence;
using Waher.Runtime.Counters;
using Waher.Script;

namespace Waher.Security.WAF.Model.Actions
{
	/// <summary>
	/// Tests a named counter.
	/// </summary>
	public class TestCounter : WafActions
	{
		private readonly StringAttribute name;
		private readonly Int64Attribute delta;
		private readonly Int64Attribute above;
		private readonly Int64Attribute below;

		/// <summary>
		/// Tests a named counter.
		/// </summary>
		public TestCounter()
			: base()
		{
		}

		/// <summary>
		/// Tests a named counter.
		/// </summary>
		/// <param name="Xml">XML definition.</param>
		/// <param name="Parent">Parent node.</param>
		/// <param name="Document">Document hosting the Web Application Firewall action.</param>
		public TestCounter(XmlElement Xml, WafAction Parent, WebApplicationFirewall Document)
			: base(Xml, Parent, Document)
		{
			this.name = new StringAttribute(Xml, "name");
			this.delta = new Int64Attribute(Xml, "delta");
			this.above = new Int64Attribute(Xml, "above");
			this.below = new Int64Attribute(Xml, "below");
		}

		/// <summary>
		/// XML Local Name for the XML element defining the action.
		/// </summary>
		public override string LocalName => nameof(TestCounter);

		/// <summary>
		/// Creates a WAF action from its XML definition.
		/// </summary>
		/// <param name="Xml">XML definition.</param>
		/// <param name="Parent">Parent node.</param>
		/// <param name="Document">Document hosting the Web Application Firewall action.</param>
		/// <returns>Created action object.</returns>
		public override WafAction Create(XmlElement Xml, WafAction Parent, WebApplicationFirewall Document) => new TestCounter(Xml, Parent, Document);

		/// <summary>
		/// Reviews the processing state, and returns a WAF result, if any.
		/// </summary>
		/// <param name="State">Current state.</param>
		/// <returns>Result to return, if any.</returns>
		public override async Task<WafResult?> Review(ProcessingState State)
		{
			Variables Variables = State.Variables;
			CaseInsensitiveString Name = await this.name.EvaluateAsync(Variables, string.Empty);
			long Delta = await this.delta.EvaluateAsync(Variables, 0L);
			long Count, Above, Below;

			if (Delta == 0)
				Count = await RuntimeCounters.GetCount(Name);
			else
				Count = await RuntimeCounters.IncrementCounter(Name, Delta);

			if (this.above.IsEmpty)
			{
				if (this.below.IsEmpty)
					return null;

				Below = await this.below.EvaluateAsync(Variables, 0L);

				if (Count < Below)
					return await this.ReviewChildren(State);
			}
			else
			{
				Above = await this.above.EvaluateAsync(Variables, 0L);

				if (this.below.IsEmpty)
				{
					if (Count > Above)
						return await this.ReviewChildren(State);
				}
				else
				{
					Below = await this.below.EvaluateAsync(Variables, 0L);

					if (Above < Below)
					{
						if (Count > Above && Count < Below)
							return await this.ReviewChildren(State);
					}
					else if (Above > Below)
					{
						if (Count > Above || Count < Below)
							return await this.ReviewChildren(State);
					}
				}
			}

			return null;
		}
	}
}
