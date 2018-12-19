using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Xml;

namespace Waher.Networking.UPnP
{
	/// <summary>
	/// Contains the information provided in a Service Description Document, downloaded from a service in the network.
	/// </summary>
	public class ServiceDescriptionDocument
	{
		private Dictionary<string, UPnPAction> actionsByName = new Dictionary<string, UPnPAction>();
		private Dictionary<string, UPnPStateVariable> variablesByName = new Dictionary<string, UPnPStateVariable>();
		private UPnPAction[] actions;
		private UPnPStateVariable[] variables;
		private UPnPService service;
		private XmlDocument xml;
		private int majorVersion;
		private int minorVersion;

		internal ServiceDescriptionDocument(XmlDocument Xml, UPnPClient Client, UPnPService Service)
		{
			List<UPnPStateVariable> Variables = new List<UPnPStateVariable>();
			List<UPnPAction> Actions = new List<UPnPAction>();
			this.xml = Xml;
			this.service = Service;

			if (Xml.DocumentElement != null && Xml.DocumentElement.LocalName == "scpd" &&
				Xml.DocumentElement.NamespaceURI == "urn:schemas-upnp-org:service-1-0")
			{
				foreach (XmlNode N in Xml.DocumentElement.ChildNodes)
				{
					switch (N.LocalName)
					{
						case "specVersion":
							foreach (XmlNode N2 in N.ChildNodes)
							{
								switch (N2.LocalName)
								{
									case "major":
										this.majorVersion = int.Parse(N2.InnerText);
										break;

									case "minor":
										this.minorVersion = int.Parse(N2.InnerText);
										break;
								}
							}
							break;

						case "actionList":
							foreach (XmlNode N2 in N.ChildNodes)
							{
								if (N2.LocalName == "action")
								{
									UPnPAction Action = new UPnPAction((XmlElement)N2, this);
									Actions.Add(Action);
									this.actionsByName[Action.Name] = Action;
								}
							}
							break;

						case "serviceStateTable":
							foreach (XmlNode N2 in N.ChildNodes)
							{
								if (N2.LocalName == "stateVariable")
								{
									UPnPStateVariable Variable = new UPnPStateVariable((XmlElement)N2);
									Variables.Add(Variable);
									this.variablesByName[Variable.Name] = Variable;
								}
							}
							break;
					}
				}
			}
			else
				throw new Exception("Unrecognized file format.");

			this.actions = Actions.ToArray();
			this.variables = Variables.ToArray();
		}

		/// <summary>
		/// Underlying XML Document.
		/// </summary>
		public XmlDocument Xml
		{
			get { return this.xml; }
		}

		/// <summary>
		/// Major version
		/// </summary>
		public int MajorVersion
		{
			get { return this.majorVersion; }
		}

		/// <summary>
		/// Minor version
		/// </summary>
		public int MinorVersion
		{
			get { return this.minorVersion; }
		}

		/// <summary>
		/// Service Actions.
		/// </summary>
		public UPnPAction[] Actions
		{
			get { return this.actions; }
		}

		/// <summary>
		/// State variables.
		/// </summary>
		public UPnPStateVariable[] Variables
		{
			get { return this.variables; }
		}

		/// <summary>
		/// Parent service object.
		/// </summary>
		public UPnPService Service
		{
			get { return this.service; }
		}

		/// <summary>
		/// Gets an action, given its name. If not found, null is returned.
		/// </summary>
		/// <param name="Name">Action name.</param>
		/// <returns>Action, if found, or null if not found.</returns>
		public UPnPAction GetAction(string Name)
		{
			if (this.actionsByName.TryGetValue(Name, out UPnPAction Action))
				return Action;
			else
				return null;
		}

		/// <summary>
		/// Gets an action, given its name. If not found, null is returned.
		/// </summary>
		/// <param name="Name">Action name.</param>
		/// <returns>Action, if found, or null if not found.</returns>
		public UPnPStateVariable GetVariable(string Name)
		{
			if (this.variablesByName.TryGetValue(Name, out UPnPStateVariable Variable))
				return Variable;
			else
				return null;
		}

		/// <summary>
		/// Invokes an action.
		/// </summary>
		/// <param name="ActionName">Action Name</param>
		/// <param name="Timeout">Timeout, in milliseconds.</param>
		/// <param name="InputValues">Input values.</param>
		/// <returns>Return value, if any, null otherwise, together with any output values found in response.</returns>
		/// <exception cref="ArgumentException">If action is not found.</exception>
		public Task<KeyValuePair<object, Dictionary<string, object>>> InvokeAsync(string ActionName, int Timeout, params KeyValuePair<string, object>[] InputValues)
		{
			UPnPAction Action = this.GetAction(ActionName);
			if (Action is null)
				throw new ArgumentException("Action not found: " + ActionName, nameof(ActionName));

			return Action.InvokeAsync(Timeout, InputValues);
		}

		/// <summary>
		/// Invokes an action.
		/// </summary>
		/// <param name="ActionName">Action Name</param>
		/// <param name="InputValues">Input values.</param>
		/// <param name="Timeout">Timeout, in milliseconds.</param>
		/// <returns>Return value, if any, null otherwise, together with any output values found in response.</returns>
		/// <exception cref="ArgumentException">If action is not found.</exception>
		public Task<KeyValuePair<object, Dictionary<string, object>>> InvokeAsync(string ActionName, Dictionary<string, object> InputValues, int Timeout)
		{
			UPnPAction Action = this.GetAction(ActionName);
			if (Action is null)
				throw new ArgumentException("Action not found: " + ActionName, nameof(ActionName));

			return Action.InvokeAsync(InputValues, Timeout);
		}

	}
}
