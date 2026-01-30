using System;
using System.Xml;
using Waher.Runtime.Collections;

namespace Waher.Security.WAF.Model
{
	/// <summary>
	/// Abstract base class for Web Application Firewall actions containing other
	/// child actions.
	/// </summary>
	public abstract class WafActions : WafAction
	{
		private readonly WafAction[] actions;

		/// <summary>
		/// Abstract base class for Web Application Firewall actions containing other
		/// child actions.
		/// </summary>
		public WafActions()
		{
			this.actions = Array.Empty<WafAction>();
		}

		/// <summary>
		/// Abstract base class for Web Application Firewall actions containing other
		/// child actions.
		/// </summary>
		/// <param name="Xml">XML definition.</param>
		public WafActions(XmlElement Xml)
			: base(Xml)
		{
			ChunkedList<WafAction> Actions = new ChunkedList<WafAction>();

			foreach (XmlNode N in Xml.ChildNodes)
			{
				if (N is XmlElement E)
					Actions.Add(Parse(E));
			}

			this.actions = Actions.ToArray();
		}

		/// <summary>
		/// Child actions.
		/// </summary>
		public WafAction[] Actions => this.actions;
	}
}
