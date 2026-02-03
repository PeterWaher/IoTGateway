using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;
using System.Xml;
using Waher.Content.Xml;
using Waher.Events;
using Waher.Networking.HTTP.Interfaces;
using Waher.Runtime.Inventory;

namespace Waher.Security.WAF.Model
{
	/// <summary>
	/// Abstract base class for Web Application Firewall actions.
	/// </summary>
	public abstract class WafAction : IWafAction
	{
		private static readonly Dictionary<string, IWafAction> actionTypes = GetActionTypes();

		private readonly string id;
		private readonly WafAction parent;
		private readonly WebApplicationFirewall document;

		/// <summary>
		/// Abstract base class for Web Application Firewall actions.
		/// </summary>
		public WafAction()
		{
		}

		/// <summary>
		/// Abstract base class for Web Application Firewall actions.
		/// </summary>
		/// <param name="Xml">XML definition.</param>
		/// <param name="Parent">Parent node.</param>
		/// <param name="Document">Document hosting the Web Application Firewall action.</param>
		public WafAction(XmlElement Xml, WafAction Parent, WebApplicationFirewall Document)
		{
			this.id = XML.Attribute(Xml, "id");
			this.parent = Parent;
			this.document = Document;

			if (!string.IsNullOrEmpty(this.id))
				Document.RegisterAction(this);
		}

		/// <summary>
		/// XML Local Name for the XML element defining the action.
		/// </summary>
		public abstract string LocalName { get; }

		/// <summary>
		/// ID of action in document, if any.
		/// </summary>
		public string Id => this.id;

		/// <summary>
		/// Web Application Firewall document.
		/// </summary>
		public WebApplicationFirewall Document => this.document;

		/// <summary>
		/// Creates a WAF action from its XML definition.
		/// </summary>
		/// <param name="Xml">XML definition.</param>
		/// <param name="Parent">Parent node.</param>
		/// <param name="Document">Document hosting the Web Application Firewall action.</param>
		/// <returns>Created action object.</returns>
		public abstract WafAction Create(XmlElement Xml, WafAction Parent, WebApplicationFirewall Document);

		private static Dictionary<string, IWafAction> GetActionTypes()
		{
			Dictionary<string, IWafAction> Result = new Dictionary<string, IWafAction>();

			foreach (Type ActionType in Types.GetTypesImplementingInterface(typeof(IWafAction)))
			{
				ConstructorInfo CI = Types.GetDefaultConstructor(ActionType);
				if (CI is null)
					continue;

				IWafAction Action = (IWafAction)CI.Invoke(Array.Empty<object>());

				if (Result.ContainsKey(Action.LocalName))
					Log.Error("Duplicate WAF action definition: " + Action.LocalName);
				else
					Result[Action.LocalName] = Action;
			}

			return Result;
		}

		/// <summary>
		/// Parses an XML Element defining a WAF action.
		/// </summary>
		/// <param name="Xml">XML Definition</param>
		/// <param name="Parent">Parent node.</param>
		/// <param name="Document">Document hosting the Web Application Firewall action.</param>
		/// <returns>Parsed action.</returns>
		public static WafAction Parse(XmlElement Xml, WafAction Parent, WebApplicationFirewall Document)
		{
			if (Xml is null)
				throw new ArgumentNullException(nameof(Xml));

			if (Xml.NamespaceURI != WebApplicationFirewall.Namespace)
				throw new Exception("Invalid WAF namespace.");

			if (!actionTypes.TryGetValue(Xml.LocalName, out IWafAction Action))
				throw new Exception("Unrecognized WAF action: " + Xml.LocalName);

			return Action.Create(Xml, Parent, Document);
		}

		/// <summary>
		/// Prepares the node for processing.
		/// </summary>
		public virtual void Prepare()
		{
			// Do nothing by default.
		}

		/// <summary>
		/// Reviews the processing state, and returns a WAF result, if any.
		/// </summary>
		/// <param name="State">Current state.</param>
		/// <returns>Result to return, if any.</returns>
		public abstract Task<WafResult?> Review(ProcessingState State);

		/// <summary>
		/// Five minutes time span.
		/// </summary>
		protected static readonly TimeSpan fiveMinutes = TimeSpan.FromMinutes(5);
	}
}
