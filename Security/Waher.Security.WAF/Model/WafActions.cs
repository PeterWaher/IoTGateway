using System;
using System.Threading.Tasks;
using System.Xml;
using Waher.Networking.HTTP.Interfaces;
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
			: base()
		{
			this.actions = Array.Empty<WafAction>();
		}

		/// <summary>
		/// Abstract base class for Web Application Firewall actions containing other
		/// child actions.
		/// </summary>
		/// <param name="Xml">XML definition.</param>
		/// <param name="Parent">Parent node.</param>
		/// <param name="Document">Document hosting the Web Application Firewall action.</param>
		public WafActions(XmlElement Xml, WafAction Parent, WebApplicationFirewall Document)
			: base(Xml, Parent, Document)
		{
			ChunkedList<WafAction> Actions = new ChunkedList<WafAction>();

			foreach (XmlNode N in Xml.ChildNodes)
			{
				if (N is XmlElement E)
					Actions.Add(Parse(E, this, Document));
			}

			this.actions = Actions.ToArray();
		}

		/// <summary>
		/// Child actions.
		/// </summary>
		public WafAction[] Actions => this.actions;

		/// <summary>
		/// If the action contains no child actions.
		/// </summary>
		public bool IsEmpty => this.actions.Length == 0;

		/// <summary>
		/// Prepares the node for processing.
		/// </summary>
		public override void Prepare()
		{
			foreach (WafAction Action in this.actions)
				Action.Prepare();
		}

		/// <summary>
		/// Reviews the processing state, and returns a WAF result, if any.
		/// </summary>
		/// <param name="State">Current state.</param>
		/// <returns>Result to return, if any.</returns>
		public async Task<WafResult?> ReviewChildren(ProcessingState State)
		{
			foreach (WafAction Action in this.actions)
			{
				WafResult? Result = await Action.Review(State);
				if (Result.HasValue)
					return Result;
			}

			return null;
		}
	}
}
