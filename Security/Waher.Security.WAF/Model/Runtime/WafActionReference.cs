using System.Xml;
using Waher.Content.Xml;
using Waher.Events;

namespace Waher.Security.WAF.Model.Runtime
{
	/// <summary>
	/// Abstract base class for WAF actions that has tags.
	/// </summary>
	public abstract class WafActionReference : WafActions
	{
		private readonly string reference;
		private WafAction referencedAction = null;

		/// <summary>
		/// Abstract base class for WAF actions that has tags.
		/// </summary>
		public WafActionReference()
			: base()
		{
		}

		/// <summary>
		/// Abstract base class for WAF actions that has tags.
		/// </summary>
		/// <param name="Xml">XML definition.</param>
		/// <param name="Parent">Parent node.</param>
		/// <param name="Document">Document hosting the Web Application Firewall action.</param>
		public WafActionReference(XmlElement Xml, WafAction Parent, WebApplicationFirewall Document)
			: base(Xml, Parent, Document)
		{
			this.reference = XML.Attribute(Xml, "reference");
		}

		/// <summary>
		/// Prepares the node for processing.
		/// </summary>
		public override void Prepare()
		{
			if (!this.Document.TryGetActionById(this.reference, out this.referencedAction))
			{
				this.referencedAction = null;
				Log.Error("Referenced action not found: " + this.reference, this.Document.FileName);
			}
		}

		/// <summary>
		/// Node reference.
		/// </summary>
		public string Reference => this.reference;

		/// <summary>
		/// Referenced action, if found.
		/// </summary>
		public WafAction ReferencedAction => this.referencedAction;
	}
}
