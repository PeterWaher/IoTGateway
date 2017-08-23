using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using Waher.Events;
using Waher.Networking.CoAP.Options;
using Waher.Networking.CoAP.Transport;
using Waher.Runtime.Timing;

namespace Waher.Networking.CoAP
{
	/// <summary>
	/// Determines if the resource is observable, and how notifications are sent.
	/// </summary>
	public enum Notifications
	{
		/// <summary>
		/// Resource is not observable.
		/// </summary>
		None,

		/// <summary>
		/// Resource is observable, and notifications are sent using CONfirmable messages.
		/// </summary>
		Acknowledged,

		/// <summary>
		/// Resource is observable, and notifications are sent using NON-confirmable messages.
		/// </summary>
		Unacknowledged
	}

	/// <summary>
	/// Base class for CoAP resources.
	/// </summary>
	public abstract class CoapResource
	{
		private CoapEndpoint endpoint = null;
		private ICoapGetMethod get;
		private string path;

		/// <summary>
		/// Base class for CoAP resources.
		/// </summary>
		/// <param name="Path">Path of resource.</param>
		public CoapResource(string Path)
		{
			this.path = Path;
			this.get = this as ICoapGetMethod;
		}

		/// <summary>
		/// Path of resource.
		/// </summary>
		public string Path
		{
			get { return this.path; }
		}

		/// <summary>
		/// Endpoint on which the resource is registered.
		/// </summary>
		public CoapEndpoint Endpoint
		{
			get { return this.endpoint; }
			internal set
			{
				if (value != null)
				{
					if (this.endpoint != null)
						throw new ArgumentException("Resource already registered.", "Endpoint");

					this.endpoint = value;
				}
				else
					this.endpoint = null;
			}
		}

		/// <summary>
		/// How notifications are sent, if at all.
		/// </summary>
		public virtual Notifications Notifications
		{
			get { return Notifications.None; }
		}

		/// <summary>
		/// If the resource is observable.
		/// </summary>
		public bool Observable
		{
			get { return this.Notifications != Notifications.None; }
		}

		/// <summary>
		/// Optional title of resource.
		/// </summary>
		public virtual string Title
		{
			get { return null; }
		}

		/// <summary>
		/// Optional resource type.
		/// </summary>
		public virtual string[] ResourceTypes
		{
			get { return null; }
		}

		/// <summary>
		/// Optional interface descriptions.
		/// </summary>
		public virtual string[] InterfaceDescriptions
		{
			get { return null; }
		}

		/// <summary>
		/// Optional array of supported content formats.
		/// </summary>
		public virtual int[] ContentFormats
		{
			get { return null; }
		}

		/// <summary>
		/// Optional maximum size estimate.
		/// </summary>
		public virtual int? MaximumSizeEstimate
		{
			get { return null; }
		}

		private Dictionary<string, ObservationRegistration> registrations = new Dictionary<string, ObservationRegistration>();
		private ObservationRegistration[] registeredMessages = null;

		internal ObservationRegistration RegisterSubscription(ClientBase Client, 
			CoapEndpoint Endpoint, CoapMessage Message)
		{
			ObservationRegistration Result;
			string Key = Message.From.ToString() + " " + Message.Token.ToString();

			Result = new ObservationRegistration(Client, Endpoint, Message);

			lock (this.registrations)
			{
				this.registrations[Key] = Result;
				this.registeredMessages = null;
			}

			return Result;
		}

		internal bool UnregisterSubscription(IPEndPoint RemoteEndpoint, ulong Token)
		{
			string Key = RemoteEndpoint.ToString() + " " + Token.ToString();

			lock (this.registrations)
			{
				this.registeredMessages = null;
				return this.registrations.Remove(Key);
			}
		}

		/// <summary>
		/// Gets all active registrations.
		/// </summary>
		/// <returns></returns>
		public ObservationRegistration[] GetRegistrations()
		{
			lock (this.registrations)
			{
				if (this.registeredMessages == null)
				{
					this.registeredMessages = new ObservationRegistration[this.registrations.Count];
					this.registrations.Values.CopyTo(this.registeredMessages, 0);
				}

				return this.registeredMessages;
			}
		}

		/// <summary>
		/// Sends data to all registered observers.
		/// </summary>
		public void TriggerAll()
		{
			this.Trigger(this.GetRegistrations());
		}

		/// <summary>
		/// Recurrently triggers resource using a given time interval.
		/// </summary>
		public void TriggerAll(TimeSpan Interval)
		{
			if (Interval <= TimeSpan.Zero)
				throw new ArgumentException("Interval must be positive.", "Interval");

			if (this.endpoint == null)
				throw new Exception("Resource not registered.");

			DateTime TP = DateTime.Now + Interval;

			this.TriggerAll();
			this.endpoint.ScheduleEvent(this.Retrigger, TP, new object[] { TP, Interval });
		}

		private void Retrigger(object State)
		{
			try
			{
				object[] P = (object[])State;
				DateTime TP = (DateTime)P[0];
				TimeSpan Interval = (TimeSpan)P[1];

				TP += Interval;

				this.TriggerAll();
				this.endpoint.ScheduleEvent(this.Retrigger, TP, new object[] { TP, Interval });
			}
			catch (Exception ex)
			{
				Log.Critical(ex);
			}
		}

		/// <summary>
		/// Sends data to a selected set of registered observers.
		/// </summary>
		/// <param name="Registrations">Registrations to trigger.</param>
		public void Trigger(params ObservationRegistration[] Registrations)
		{
			if (Registrations == null || Registrations.Length == 0)
				return;

			foreach (ObservationRegistration Registration in Registrations)
			{
				string Key = Registration.Request.From.Address.ToString() + " " +
					Registration.Request.Token.ToString();

				Registration.Endpoint.RemoveBlockedResponse(Key);
				Registration.Request.Block2 = null;
				Registration.Endpoint.ProcessRequest(this, Registration.Client, 
					Registration.Request, true, new CoapOptionObserve(Registration.SequenceNumber));

				Registration.IncSeqNr();
			}
		}

	}
}
