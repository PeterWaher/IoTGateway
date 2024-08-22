using System;
using System.Collections.Generic;
using System.Net;
using Waher.Events;
using Waher.Networking.CoAP.Options;
using Waher.Networking.CoAP.Transport;

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
		private readonly ICoapGetMethod get;
		private readonly ICoapPostMethod post;
		private readonly ICoapPutMethod put;
		private readonly ICoapDeleteMethod delete;
		private readonly ICoapFetchMethod fetch;
		private readonly ICoapIPatchMethod iPatch;
		private readonly ICoapPatchMethod patch;
		private string path;
		private DateTime nextTrigger = DateTime.MinValue;

		/// <summary>
		/// Base class for CoAP resources.
		/// </summary>
		/// <param name="Path">Path of resource.</param>
		public CoapResource(string Path)
		{
			this.path = Path;
			this.get = this as ICoapGetMethod;
			this.post = this as ICoapPostMethod;
			this.put = this as ICoapPutMethod;
			this.delete = this as ICoapDeleteMethod;
			this.fetch = this as ICoapFetchMethod;
			this.iPatch = this as ICoapIPatchMethod;
			this.patch = this as ICoapPatchMethod;
		}

		/// <summary>
		/// Path of resource.
		/// </summary>
		public string Path
		{
			get => this.path;
			protected set => this.path = value;
		}

		/// <summary>
		/// If the resource handles subpaths.
		/// </summary>
		public virtual bool HandlesSubPaths => false;

		/// <summary>
		/// GET method interface, if any.
		/// </summary>
		public ICoapGetMethod GetMethod => this.get;

		/// <summary>
		/// POST method interface, if any.
		/// </summary>
		public ICoapPostMethod PostMethod => this.post;

		/// <summary>
		/// PUT method interface, if any.
		/// </summary>
		public ICoapPutMethod PutMethod => this.put;

		/// <summary>
		/// DELETE method interface, if any.
		/// </summary>
		public ICoapDeleteMethod DeleteMethod => this.delete;

		/// <summary>
		/// FETCH method interface, if any.
		/// </summary>
		public ICoapFetchMethod FetchMethod => this.fetch;

		/// <summary>
		/// PATCH method interface, if any.
		/// </summary>
		public ICoapPatchMethod PatchMethod => this.patch;

		/// <summary>
		/// iPATCH method interface, if any.
		/// </summary>
		public ICoapIPatchMethod IPatchMethod => this.iPatch;

		/// <summary>
		/// Endpoint on which the resource is registered.
		/// </summary>
		public CoapEndpoint Endpoint
		{
			get => this.endpoint;
			internal set
			{
				if (!(value is null))
				{
					if (!(this.endpoint is null))
						throw new ArgumentException("Resource already registered.", nameof(Endpoint));

					this.endpoint = value;
				}
				else
					this.endpoint = null;
			}
		}

		/// <summary>
		/// How notifications are sent, if at all.
		/// </summary>
		public virtual Notifications Notifications => Notifications.None;

		/// <summary>
		/// If the resource is observable.
		/// </summary>
		public bool Observable => this.Notifications != Notifications.None;

		/// <summary>
		/// Optional title of resource.
		/// </summary>
		public virtual string Title => null;

		/// <summary>
		/// Optional resource type.
		/// </summary>
		public virtual string[] ResourceTypes => null;

		/// <summary>
		/// Optional interface descriptions.
		/// </summary>
		public virtual string[] InterfaceDescriptions => null;

		/// <summary>
		/// Optional array of supported content formats.
		/// </summary>
		public virtual int[] ContentFormats => null;

		/// <summary>
		/// Optional maximum size estimate.
		/// </summary>
		public virtual int? MaximumSizeEstimate => null;

		private readonly Dictionary<string, ObservationRegistration> registrations = new Dictionary<string, ObservationRegistration>();
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
			string Prefix = RemoteEndpoint.ToString() + " ";
			string Key = Prefix + Token.ToString();

			lock (this.registrations)
			{
				this.registeredMessages = null;
				if (this.registrations.Remove(Key))
					return true;

				if (Token == 0)
				{
					LinkedList<string> ToRemove = null;

					foreach (string Key2 in this.registrations.Keys)
					{
						if (Key2.StartsWith(Prefix))
						{
							if (ToRemove is null)
								ToRemove = new LinkedList<string>();

							ToRemove.AddLast(Key2);
						}
					}

					if (!(ToRemove is null))
					{
						foreach (string Key2 in ToRemove)
							this.registrations.Remove(Key2);

						return true;
					}
				}
			}

			return false;
		}

		/// <summary>
		/// Gets all active registrations.
		/// </summary>
		/// <returns></returns>
		public ObservationRegistration[] GetRegistrations()
		{
			lock (this.registrations)
			{
				if (this.registeredMessages is null)
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
				throw new ArgumentOutOfRangeException("Interval must be positive.", nameof(Interval));

			if (this.endpoint is null)
				throw new Exception("Resource not registered.");

			if (this.nextTrigger > DateTime.MinValue)
				this.endpoint.CancelScheduledEvent(this.nextTrigger);

			DateTime TP = DateTime.Now + Interval;
			object[] P = new object[] { TP, Interval, null };

			this.TriggerAll();
			this.nextTrigger = this.endpoint.ScheduleEvent(this.Retrigger, TP, P);
			P[2] = this.nextTrigger;
		}

		private void Retrigger(object State)
		{
			try
			{
				object[] P = (object[])State;
				DateTime TP = (DateTime)P[0];
				TimeSpan Interval = (TimeSpan)P[1];
				DateTime NextTrigger = (DateTime)P[2];

				if (NextTrigger != this.nextTrigger)
					return;

				TP += Interval;
				P[0] = TP;

				this.TriggerAll();
				this.nextTrigger = this.endpoint.ScheduleEvent(this.Retrigger, TP, P);
				P[2] = this.nextTrigger;
			}
			catch (Exception ex)
			{
				Log.Exception(ex);
			}
		}

		/// <summary>
		/// Sends data to a selected set of registered observers.
		/// </summary>
		/// <param name="Registrations">Registrations to trigger.</param>
		public void Trigger(params ObservationRegistration[] Registrations)
		{
			if (Registrations is null || Registrations.Length == 0)
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

		/// <summary>
		/// If resource should be published through /.well-known/core
		/// </summary>
		public virtual bool WellKnownCoRE => true;
	}
}
