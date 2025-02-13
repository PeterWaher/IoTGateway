﻿using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Waher.Events;
using Waher.Networking.CoAP;
using Waher.Networking.CoAP.Options;
using Waher.Networking.LWM2M.ContentFormats;
using Waher.Networking.LWM2M.Events;

namespace Waher.Networking.LWM2M
{
	/// <summary>
	/// Base class for all LWM2M resources.
	/// </summary>
	public abstract class Lwm2mResource : CoapResource, ICoapGetMethod, ICoapPutMethod, ICoapPostMethod
	{
		private Lwm2mObjectInstance objInstance = null;
		private readonly string name;
		private ushort id;
		private ushort instanceId;
		private ushort resourceId;
		private readonly bool canWrite;
		private readonly bool persist;

		/// <summary>
		/// Base class for all LWM2M resources.
		/// </summary>
		/// <param name="Name">Name of parameter. If null, parameter values will not be logged</param>
		/// <param name="Id">ID of object.</param>
		/// <param name="InstanceId">ID of object instance.</param>
		/// <param name="ResourceId">ID of resource.</param>
		/// <param name="CanWrite">If the resource allows servers to update the value using write commands.</param>
		/// <param name="Persist">If written values should be persisted by the resource.</param>
		public Lwm2mResource(string Name, ushort Id, ushort InstanceId, ushort ResourceId, bool CanWrite, bool Persist)
			: base("/" + Id.ToString() + "/" + InstanceId.ToString() + "/" + ResourceId.ToString())
		{
			this.name = Name;
			this.id = Id;
			this.instanceId = InstanceId;
			this.resourceId = ResourceId;
			this.canWrite = CanWrite;
			this.persist = Persist;
		}

		/// <summary>
		/// LWM2M object instance.
		/// </summary>
		public Lwm2mObjectInstance ObjectInstance
		{
			get => this.objInstance;
			internal set => this.objInstance = value;
		}

		/// <summary>
		/// How notifications are sent, if at all.
		/// </summary>
		public override Notifications Notifications => Notifications.Acknowledged;

		/// <summary>
		/// Name of resource.
		/// </summary>
		public string Name => this.name;

		/// <summary>
		/// ID of object.
		/// </summary>
		public ushort Id
		{
			get => this.id;
			set
			{
				if (this.id != value)
				{
					this.id = value;
					this.SetPath();
				}
			}
		}

		private void SetPath()
		{
			this.Path = "/" + this.id.ToString() + "/" + this.instanceId.ToString() +
				"/" + this.resourceId.ToString();
		}

		/// <summary>
		/// ID of Object Instance.
		/// </summary>
		public ushort InstanceId
		{
			get => this.instanceId;
			set
			{
				if (this.instanceId != value)
				{
					this.instanceId = value;
					this.SetPath();
				}
			}
		}

		/// <summary>
		/// ID of resource.
		/// </summary>
		public ushort ResourceId
		{
			get => this.resourceId;
			set
			{
				if (this.resourceId != value)
				{
					this.resourceId = value;
					this.SetPath();
				}
			}
		}

		/// <summary>
		/// If the resource allows servers to update the value using write commands.
		/// </summary>
		public bool CanWrite => this.canWrite;

		/// <summary>
		/// If written values should be persisted by the resource.
		/// </summary>
		public bool Persist => this.persist;

		/// <summary>
		/// Loads the value of the resource, from persisted storage.
		/// </summary>
		public virtual Task ReadPersistedValue()
		{
			return Task.CompletedTask;
		}

		/// <summary>
		/// Saves the value of the resource, to persisted storage.
		/// </summary>
		public virtual Task WritePersistedValue()
		{
			return Task.CompletedTask;
		}

		/// <summary>
		/// Reads the value from a TLV record.
		/// </summary>
		/// <param name="Record">TLV record.</param>
		public abstract Task Read(TlvRecord Record);

		/// <summary>
		/// Reads the value from a TLV record.
		/// </summary>
		/// <param name="Output">Output.</param>
		public abstract void Write(ILwm2mWriter Output);

		/// <summary>
		/// Resets the parameter to its default value.
		/// </summary>
		public abstract void Reset();

		/// <summary>
		/// Value of resource.
		/// </summary>
		public abstract object Value
		{
			get;
		}

		/// <summary>
		/// Method called when the resource value has been updated.
		/// </summary>
		protected virtual Task ValueUpdated()
		{
			if (!(this.objInstance is null))
				return this.objInstance.ValueUpdated(this);
			else
				return Task.CompletedTask;
		}

		/// <summary>
		/// If the GET method is allowed.
		/// </summary>
		public bool AllowsGET => true;

		/// <summary>
		/// Executes the GET method on the resource.
		/// </summary>
		/// <param name="Request">CoAP Request</param>
		/// <param name="Response">CoAP Response</param>
		/// <exception cref="CoapException">If an error occurred when processing the method.</exception>
		public virtual async Task GET(CoapMessage Request, CoapResponse Response)
		{
			ILwm2mWriter Writer;
			bool FromBootstrapServer = this.objInstance.Object.Client.IsFromBootstrapServer(Request);

			if (this.id == 0 && !FromBootstrapServer)
			{
				await Response.RST(CoapCode.Unauthorized);
				return;
			}

			if (Request.Accept is null)
				Writer = new TextWriter();
			else if (Request.IsAcceptable(Tlv.ContentFormatCode))
				Writer = new TlvWriter();
			else if (Request.IsAcceptable(Json.ContentFormatCode))
				Writer = new JsonWriter(this.objInstance.Path + "/");
			else if (Request.IsAcceptable(CoAP.ContentFormats.PlainText.ContentFormatCode))
				Writer = new TextWriter();
			else if (Request.IsAcceptable(CoAP.ContentFormats.Binary.ContentFormatCode))
				Writer = new OpaqueWriter();
			else
			{
				await Response.RST(CoapCode.NotAcceptable);
				return;
			}

			if (!(this.OnBeforeGet is null))
				await this.OnBeforeGet.Raise(this, new CoapRequestEventArgs(Request));

			this.Write(Writer);

			byte[] Payload = Writer.ToArray();

			await Response.Respond(CoapCode.Content, Payload, 64,
				new CoapOptionContentFormat(Writer.ContentFormat));
		}

		/// <summary>
		/// Event raised before the response to a GET request is generated.
		/// </summary>
		public event EventHandlerAsync<CoapRequestEventArgs> OnBeforeGet = null;

		/// <summary>
		/// Event raised after the resource has been registered
		/// </summary>
		public event EventHandlerAsync OnAfterRegister = null;

		/// <summary>
		/// Called after the resource has been registered on a CoAP Endpoint.
		/// </summary>
		public virtual Task AfterRegister()
		{
			return this.OnAfterRegister.Raise(this, EventArgs.Empty);
		}

		/// <summary>
		/// If the POST method is allowed.
		/// </summary>
		public bool AllowsPOST => this.AllowsPUT;

		/// <summary>
		/// Executes the POST method on the resource.
		/// </summary>
		/// <param name="Request">CoAP Request</param>
		/// <param name="Response">CoAP Response</param>
		/// <exception cref="CoapException">If an error occurred when processing the method.</exception>
		public Task POST(CoapMessage Request, CoapResponse Response)
		{
			return this.PUT(Request, Response);
		}

		/// <summary>
		/// If the PUT method is allowed.
		/// </summary>
		public virtual bool AllowsPUT => true;

		/// <summary>
		/// Executes the PUT method on the resource.
		/// </summary>
		/// <param name="Request">CoAP Request</param>
		/// <param name="Response">CoAP Response</param>
		/// <exception cref="CoapException">If an error occurred when processing the method.</exception>
		public virtual async Task PUT(CoapMessage Request, CoapResponse Response)
		{
			bool FromBootstrapServer = this.objInstance.Object.Client.IsFromBootstrapServer(Request);

			if (this.id == 0 && !FromBootstrapServer)
			{
				await Response.RST(CoapCode.Unauthorized);
				return;
			}

			if (!(Request.UriQuery is null) && Request.UriQuery.Count > 0)    // Write attributes
			{
				if (!FromBootstrapServer)
				{
					await Response.RST(CoapCode.Unauthorized);
					return;
				}

				foreach (KeyValuePair<string, string> P in Request.UriQuery)
				{
					switch (P.Key)
					{
						case "pmin":
						case "pmax":
						case "gt":
						case "lt":
						case "st":
							// TODO: Implement support
							break;

						default:
							await Response.RST(CoapCode.BadRequest);
							return;
					}
				}
			}

			if (!(Request.ContentFormat is null))      // Write operation
			{
				if (!this.canWrite && !FromBootstrapServer)
				{
					await Response.Respond(CoapCode.BadRequest);
					return;
				}

				object Decoded = await Request.DecodeAsync();

				if (Decoded is TlvRecord[] Records)
				{
					foreach (TlvRecord Rec in Records)
						await this.Read(Rec);
				}
				else
				{
					await Response.Respond(CoapCode.NotAcceptable);
					return;
				}

				await this.RemoteUpdate(Request);
			}
			else
				await this.Execute(Request);

			await Response.Respond(CoapCode.Changed);
		}

		internal Task RemoteUpdate(CoapMessage Request)
		{
			if (!string.IsNullOrEmpty(this.name))
			{
				Log.Informational("Parameter updated.", this.name, Request.From.ToString(),
					new KeyValuePair<string, object>("Value", this.Value));
			}

			return this.OnRemoteUpdate.Raise(this, new CoapRequestEventArgs(Request));
		}

		/// <summary>
		/// Even raised when a new value has been written to the resource from a remote source.
		/// </summary>
		public event EventHandlerAsync<CoapRequestEventArgs> OnRemoteUpdate = null;

		/// <summary>
		/// Executes an action on the resource.
		/// </summary>
		/// <param name="Request">Request message.</param>
		public virtual Task Execute(CoapMessage Request)
		{
			if (!string.IsNullOrEmpty(this.name))
				Log.Informational("Executing action.", this.name, Request.From.ToString());

			return this.OnExecute.Raise(this, new CoapRequestEventArgs(Request));
		}

		/// <summary>
		/// Event raised when the resource is executed.
		/// </summary>
		public event EventHandlerAsync<CoapRequestEventArgs> OnExecute = null;

		/// <summary>
		/// Event raised when the resource value has been set.
		/// </summary>
		public event EventHandlerAsync OnSet = null;

		/// <summary>
		/// Raises the <see cref="OnSet"/> event.
		/// </summary>
		protected Task Set()
		{
			return this.OnSet.Raise(this, EventArgs.Empty);
		}
	}
}
