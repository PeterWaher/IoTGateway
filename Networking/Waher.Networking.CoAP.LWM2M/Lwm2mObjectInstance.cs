using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Text;
using Waher.Persistence.Attributes;

namespace Waher.Networking.CoAP.LWM2M
{
	/// <summary>
	/// Base class for all LWM2M object instances.
	/// </summary>
	[CollectionName("Lwm2mObjectInstances")]
	[TypeName(TypeNameSerialization.FullName)]
	[Index("Id", "SubId")]
	public abstract class Lwm2mObjectInstance : CoapResource
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

	}
}
