using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Text;
using Waher.Persistence.Attributes;
using Waher.Networking.CoAP.Options;
using Waher.Networking.CoAP.LWM2M.ContentFormats;

namespace Waher.Networking.CoAP.LWM2M
{
	/// <summary>
	/// Base class for all LWM2M object instances.
	/// </summary>
	[CollectionName("Lwm2mObjectInstances")]
	[TypeName(TypeNameSerialization.FullName)]
	[Index("Id", "SubId")]
	public abstract class Lwm2mObjectInstance : CoapResource, ICoapGetMethod
	{
		private Lwm2mObject obj = null;
		private string objectId = null;
		private int id;
		private int subId;

		/// <summary>
		/// Base class for all LWM2M objects.
		/// </summary>
		/// <param name="Id">ID of object.</param>
		/// <param name="SubId">ID of object instance.</param>
		public Lwm2mObjectInstance(int Id, int SubId)
			: base("/" + Id.ToString() + "/" + SubId.ToString())
		{
			this.id = Id;
			this.subId = SubId;
		}

		/// <summary>
		/// Object ID in database.
		/// </summary>
		[ObjectId]
		public string ObjectId
		{
			get { return this.objectId; }
			set { this.objectId = value; }
		}

		/// <summary>
		/// LWM2M object.
		/// </summary>
		public Lwm2mObject Object
		{
			get { return this.obj; }
			internal set { this.obj = value; }
		}

		/// <summary>
		/// ID of object.
		/// </summary>
		public int Id
		{
			get { return this.id; }
			set
			{
				if (this.id != value)
				{
					this.id = value;
					this.Path = "/" + this.id.ToString() + "/" + this.subId.ToString();
				}
			}
		}

		/// <summary>
		/// Sub-ID of object instance.
		/// </summary>
		public int SubId
		{
			get { return this.subId; }
			set
			{
				if (this.subId != value)
				{
					this.subId = value;
					this.Path = "/" + this.id.ToString() + "/" + this.subId.ToString();
				}
			}
		}

		/// <summary>
		/// Loads any Bootstrap information.
		/// </summary>
		public virtual Task LoadBootstrapInfo()
		{
			return Task.CompletedTask;
		}

		/// <summary>
		/// Deletes any Bootstrap information.
		/// </summary>
		public virtual Task DeleteBootstrapInfo()
		{
			return Task.CompletedTask;
		}

		/// <summary>
		/// Applies any Bootstrap information.
		/// </summary>
		public virtual Task ApplyBootstrapInfo()
		{
			return Task.CompletedTask;
		}

		/// <summary>
		/// Encodes available object links.
		/// </summary>
		/// <param name="IncludeSecurity">If security objects are to be included.</param>
		/// <param name="Output">Link output.</param>
		public void EncodeObjectLinks(bool IncludeSecurity, StringBuilder Output)
		{
			if (this.id > 0 || IncludeSecurity)
			{
				Output.Append(',');
				Output.Append("</");
				Output.Append(this.id.ToString());
				Output.Append('/');
				Output.Append(this.subId.ToString());
				Output.Append('>');

				this.EncodeLinkParameters(Output);
			}
		}

		/// <summary>
		/// Encodes any link parameters to the object link.
		/// </summary>
		/// <param name="Output">Link output.</param>
		public virtual void EncodeLinkParameters(StringBuilder Output)
		{
		}

		/// <summary>
		/// <see cref="object.ToString()"/>
		/// </summary>
		public override string ToString()
		{
			return this.Path + ": " + this.GetType().FullName;
		}

		/// <summary>
		/// If the GET method is allowed.
		/// </summary>
		public bool AllowsGET => true;

		/// <summary>
		/// If the resource handles subpaths.
		/// </summary>
		public override bool HandlesSubPaths => true;

		/// <summary>
		/// Executes the GET method on the resource.
		/// </summary>
		/// <param name="Request">CoAP Request</param>
		/// <param name="Response">CoAP Response</param>
		/// <exception cref="CoapException">If an error occurred when processing the method.</exception>
		public virtual void GET(CoapMessage Request, CoapResponse Response)
		{
			ILwm2mWriter Writer;
			int? ResourceID;

			if (string.IsNullOrEmpty(Request.SubPath))
				ResourceID = null;
			else if (int.TryParse(Request.SubPath.Substring(1), out int i))
				ResourceID = i;
			else
			{
				Response.RST(CoapCode.NotFound);
				return;
			}

			// TODO: Plain text, opaque, JSON

			if (Request.IsAcceptable(Tlv.ContentFormatCode))
				Writer = new TlvWriter();
			else
			{
				Response.RST(CoapCode.NotAcceptable);
				return;
			}

			this.Export(ResourceID, Writer);

			byte[] Payload = Writer.ToArray();

			if (Payload.Length == 0)
				Response.RST(CoapCode.NotFound);
			else
			{
				Response.Respond(CoapCode.Content, Payload, 64,
					new CoapOptionContentFormat(Writer.ContentFormat));
			}
		}

		/// <summary>
		/// Exports resources.
		/// </summary>
		/// <param name="ResourceID">Resource ID, if a single resource is to be exported, otherwise null.</param>
		/// <param name="Writer">Output</param>
		public abstract void Export(int? ResourceID, ILwm2mWriter Writer);

	}
}
