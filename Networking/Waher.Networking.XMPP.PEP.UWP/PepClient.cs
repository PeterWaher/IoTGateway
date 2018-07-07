using System;
using System.Collections.Generic;
using System.Reflection;
using System.Xml;
using Waher.Events;
using Waher.Networking.XMPP.PubSub;
using Waher.Runtime.Inventory;

namespace Waher.Networking.XMPP.PEP
{
	/// <summary>
	/// Client managing the Personal Eventing Protocol (XEP-0163).
	/// https://xmpp.org/extensions/xep-0163.html
	/// </summary>
	public class PepClient : XmppExtension
	{
		private static Dictionary<string, IPersonalEvent> personalEventTypes = GetPersonalEventTypes();

		private PubSubClient pubSubClient;
		private readonly Dictionary<Type, PersonalEventNotificationEventHandler[]> handlers = new Dictionary<Type, PersonalEventNotificationEventHandler[]>();

		/// <summary>
		/// Client managing the Personal Eventing Protocol (XEP-0163).
		/// https://xmpp.org/extensions/xep-0163.html
		/// </summary>
		/// <param name="Client">XMPP Client to use.</param>
		public PepClient(XmppClient Client)
			: base(Client)
		{
			this.pubSubClient = new PubSubClient(Client, string.Empty);

			this.pubSubClient.ItemNotification += PubSubClient_ItemNotification;
		}

		/// <summary>
		/// <see cref="IDisposable.Dispose"/>
		/// </summary>
		public override void Dispose()
		{
			if (this.pubSubClient != null)
			{
				this.pubSubClient.ItemNotification -= PubSubClient_ItemNotification;

				this.pubSubClient.Dispose();
				this.pubSubClient = null;
			}

			base.Dispose();
		}

		/// <summary>
		/// <see cref="PubSubClient"/> used for the Personal Eventing Protocol. Use this client to perform administrative tasks
		/// of the PEP service.
		/// </summary>
		public PubSubClient PubSubClient
		{
			get { return this.pubSubClient; }
		}

		/// <summary>
		/// Implemented extensions.
		/// </summary>
		public override string[] Extensions => new string[] { "XEP-0163" };

		/// <summary>
		/// Publishes an item on a node.
		/// </summary>
		/// <param name="Node">Node name.</param>
		/// <param name="Callback">Method to call when operation completes.</param>
		/// <param name="State">State object to pass on to callback method.</param>
		public void Publish(string Node, ItemResultEventHandler Callback, object State)
		{
			this.pubSubClient?.Publish(Node, Callback, State);
		}

		/// <summary>
		/// Publishes an item on a node.
		/// </summary>
		/// <param name="Node">Node name.</param>
		/// <param name="PayloadXml">Payload XML.</param>
		/// <param name="Callback">Method to call when operation completes.</param>
		/// <param name="State">State object to pass on to callback method.</param>
		public void Publish(string Node, string PayloadXml, ItemResultEventHandler Callback, object State)
		{
			this.pubSubClient?.Publish(Node, PayloadXml, Callback, State);
		}

		/// <summary>
		/// Publishes an item on a node.
		/// </summary>
		/// <param name="PersonalEvent">Personal event.</param>
		/// <param name="Callback">Method to call when operation completes.</param>
		/// <param name="State">State object to pass on to callback method.</param>
		public void Publish(IPersonalEvent PersonalEvent, ItemResultEventHandler Callback, object State)
		{
			this.pubSubClient?.Publish(PersonalEvent.Node, PersonalEvent.PayloadXml, Callback, State);
		}

		/// <summary>
		/// Publishes an item on a node.
		/// </summary>
		/// <param name="Node">Node name.</param>
		/// <param name="ItemId">Item identity, if available. If used, and an existing item
		/// is available with that identity, it will be updated with the new content.</param>
		/// <param name="PayloadXml">Payload XML.</param>
		/// <param name="Callback">Method to call when operation completes.</param>
		/// <param name="State">State object to pass on to callback method.</param>
		public void Publish(string Node, string ItemId, string PayloadXml, ItemResultEventHandler Callback, object State)
		{
			this.pubSubClient?.Publish(Node, ItemId, PayloadXml, Callback, State);
		}

		private void PubSubClient_ItemNotification(object Sender, ItemNotificationEventArgs e)
		{
			IPersonalEvent PersonalEvent = null;

			foreach (XmlNode N in e.Item.ChildNodes)
			{
				if (N is XmlElement E && personalEventTypes.TryGetValue(E.LocalName + " " + E.NamespaceURI, out IPersonalEvent PersonalEvent2))
				{
					PersonalEvent = PersonalEvent2.Parse(E);
					break;
				}
			}

			if (PersonalEvent != null)
			{
				PersonalEventNotificationEventHandler[] Handlers;

				lock (this.handlers)
				{
					if (!this.handlers.TryGetValue(PersonalEvent.GetType(), out Handlers))
						return;
				}

				PersonalEventNotificationEventArgs e2 = new PersonalEventNotificationEventArgs(PersonalEvent, e);

				foreach (PersonalEventNotificationEventHandler Handler in Handlers)
				{
					try
					{
						Handler.Invoke(this, e2);
					}
					catch (Exception ex)
					{
						Log.Critical(ex);
					}
				}
			}
		}

		private static Dictionary<string, IPersonalEvent> GetPersonalEventTypes()
		{
			Types.OnInvalidated += Types_OnInvalidated;
			return GetPersonalEventTypes2();
		}

		private static Dictionary<string, IPersonalEvent> GetPersonalEventTypes2()
		{
			Dictionary<string, IPersonalEvent> Result = new Dictionary<string, IPersonalEvent>();

			foreach (Type T in Types.GetTypesImplementingInterface(typeof(IPersonalEvent)))
			{
				if (T.GetTypeInfo().IsAbstract)
					continue;

				try
				{
					IPersonalEvent PersonalEvent = (IPersonalEvent)Activator.CreateInstance(T);
					Result[PersonalEvent.LocalName + " " + PersonalEvent.Namespace] = PersonalEvent;
				}
				catch (Exception ex)
				{
					Log.Critical(ex);
				}
			}

			return Result;
		}

		private static void Types_OnInvalidated(object sender, EventArgs e)
		{
			personalEventTypes = GetPersonalEventTypes2();
		}

		/// <summary>
		/// Registers an event handler of a specific type of personal events.
		/// </summary>
		/// <param name="PersonalEventType">Type of personal event.</param>
		/// <param name="Handler">Event handler.</param>
		public void RegisterHandler(Type PersonalEventType, PersonalEventNotificationEventHandler Handler)
		{
			if (!typeof(IPersonalEvent).GetTypeInfo().IsAssignableFrom(PersonalEventType.GetTypeInfo()))
				throw new ArgumentException("Not a personal event type.", nameof(PersonalEventType));

			IPersonalEvent PersonalEvent = (IPersonalEvent)Activator.CreateInstance(PersonalEventType);

			lock (this.handlers)
			{
				if (!this.handlers.TryGetValue(PersonalEventType, out PersonalEventNotificationEventHandler[] Handlers))
					Handlers = null;

				if (Handlers == null)
					Handlers = new PersonalEventNotificationEventHandler[] { Handler };
				else
				{
					int c = Handlers.Length;
					PersonalEventNotificationEventHandler[] Handlers2 = new PersonalEventNotificationEventHandler[c + 1];
					Array.Copy(Handlers, 0, Handlers2, 0, c);
					Handlers2[c] = Handler;
					Handlers = Handlers2;
				}

				this.handlers[PersonalEventType] = Handlers;
			}

			this.client.RegisterFeature(PersonalEvent.Namespace + "+notify");
		}

		/// <summary>
		/// Unregisters an event handler of a specific type of personal events.
		/// </summary>
		/// <param name="PersonalEventType">Type of personal event.</param>
		/// <param name="Handler">Event handler.</param>
		/// <returns>If the event handler was found and removed.</returns>
		public bool UnregisterHandler(Type PersonalEventType, PersonalEventNotificationEventHandler Handler)
		{
			lock (this.handlers)
			{
				if (!this.handlers.TryGetValue(PersonalEventType, out PersonalEventNotificationEventHandler[] Handlers))
					return false;

				List<PersonalEventNotificationEventHandler> List = new List<PersonalEventNotificationEventHandler>();
				List.AddRange(Handlers);

				if (!List.Remove(Handler))
					return false;

				if (List.Count == 0)
					this.handlers.Remove(PersonalEventType);
				else
				{
					Handlers = List.ToArray();
					this.handlers[PersonalEventType] = Handlers;
				}

				this.client.UnregisterFeature(PersonalEventType.Namespace + "+notify");

				return true;
			}
		}

	}
}
