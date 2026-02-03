using System.Collections.Generic;
using System.Threading.Tasks;
using System.Xml;
using Waher.Content.Xml.Attributes;
using Waher.Networking.HTTP.Interfaces;
using Waher.Runtime.Collections;
using Waher.Script;

namespace Waher.Security.WAF.Model.Actions
{
	/// <summary>
	/// Defines a name-value tag.
	/// </summary>
	public class Tag : WafAction
	{
		private readonly StringAttribute name;
		private readonly ObjectAttribute value;

		/// <summary>
		/// Defines a name-value tag.
		/// </summary>
		public Tag()
			: base()
		{
		}

		/// <summary>
		/// Defines a name-value tag.
		/// </summary>
		/// <param name="Xml">XML definition.</param>
		/// <param name="Parent">Parent node.</param>
		/// <param name="Document">Document hosting the Web Application Firewall action.</param>
		public Tag(XmlElement Xml, WafAction Parent, WebApplicationFirewall Document)
			: base(Xml, Parent, Document)
		{
			this.name = new StringAttribute(Xml, "name");
			this.value = new ObjectAttribute(Xml, "value");
		}

		/// <summary>
		/// XML Local Name for the XML element defining the action.
		/// </summary>
		public override string LocalName => nameof(Tag);

		/// <summary>
		/// Creates a WAF action from its XML definition.
		/// </summary>
		/// <param name="Xml">XML definition.</param>
		/// <param name="Parent">Parent node.</param>
		/// <param name="Document">Document hosting the Web Application Firewall action.</param>
		/// <returns>Created action object.</returns>
		public override WafAction Create(XmlElement Xml, WafAction Parent, WebApplicationFirewall Document) => new Tag(Xml, Parent, Document);

		/// <summary>
		/// Reviews the processing state, and returns a WAF result, if any.
		/// </summary>
		/// <param name="State">Current state.</param>
		/// <returns>Result to return, if any.</returns>
		public override Task<WafResult?> Review(ProcessingState State)
		{
			return Task.FromResult<WafResult?>(null);
		}

		/// <summary>
		/// Evaluates the tag.
		/// </summary>
		/// <param name="State">Current state.</param>
		/// <returns>Evaluated tag.</returns>
		public async Task<KeyValuePair<string, object>> EvaluateTag(ProcessingState State)
		{
			Variables Variables = State.Variables;
			string Name = await this.name.EvaluateAsync(Variables, string.Empty);
			object Value = await this.value.EvaluateAsync(Variables, null);

			return new KeyValuePair<string, object>(Name, Value);
		}

	}
}
