using System.Xml;
using Waher.Content.Xml;

namespace Waher.Security.WAF.Model.Actions
{
	/// <summary>
	/// Delays processing for a number of seconds.
	/// </summary>
	public class Delay : WafAction
	{
		private readonly int seconds;

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
		public Delay(XmlElement Xml)
			: base(Xml)
		{
			this.seconds = XML.Attribute(Xml, "seconds", 0);
		}

		/// <summary>
		/// XML Local Name for the XML element defining the action.
		/// </summary>
		public override string LocalName => nameof(LogEvent);

		/// <summary>
		/// Creates a WAF action from its XML definition.
		/// </summary>
		/// <param name="Xml">XML definition.</param>
		/// <returns>Created action object.</returns>
		public override WafAction Create(XmlElement Xml) => new LogEvent(Xml);
	}
}
