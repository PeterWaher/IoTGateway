using System;
using System.Collections.Generic;
using System.Reflection;
using System.Xml;
using Waher.Events;
using Waher.Runtime.Inventory;

namespace Waher.Security.WAF.Model
{
	/// <summary>
	/// Abstract base class for Web Application Firewall actions.
	/// </summary>
	public abstract class WafAction
	{
		private static readonly Dictionary<string, IWafAction> actionTypes = GetActionTypes();

		/// <summary>
		/// Abstract base class for Web Application Firewall actions.
		/// </summary>
		/// <param name="Xml">XML definition.</param>
		public WafAction(XmlElement Xml)
		{
		}

		/// <summary>
		/// XML Local Name for the XML element defining the action.
		/// </summary>
		public abstract string LocalName { get; }

		/// <summary>
		/// Creates a WAF action from its XML definition.
		/// </summary>
		/// <param name="Xml">XML definition.</param>
		/// <returns>Created action object.</returns>
		public abstract WafAction Create(XmlElement Xml);

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
		/// <returns>Parsed action.</returns>
		public static WafAction Parse(XmlElement Xml)
		{
			if (Xml is null)
				throw new ArgumentNullException(nameof(Xml));

			if (Xml.NamespaceURI != WebApplicationFirewall.Namespace)
				throw new Exception("Invalid WAF namespace.");

			if (!actionTypes.TryGetValue(Xml.LocalName, out IWafAction Action))
				throw new Exception("Unrecognized WAF action: " + Xml.LocalName);

			return Action.Create(Xml);
		}
	}
}
