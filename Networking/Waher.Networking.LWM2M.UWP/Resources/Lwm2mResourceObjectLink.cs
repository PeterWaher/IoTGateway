using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Waher.Events;
using Waher.Networking.LWM2M.ContentFormats;
using Waher.Persistence;
using Waher.Runtime.Settings;

namespace Waher.Networking.LWM2M
{
	/// <summary>
	/// Class managing an LWM2M resource object link.
	/// </summary>
	public class Lwm2mResourceObjectLink : Lwm2mResource
	{
		private ushort? defaultRefId;
		private ushort? defaultRefInstanceId;
		private ushort? refId;
		private ushort? refInstanceId;

		/// <summary>
		/// Class managing an LWM2M resource object link.
		/// </summary>
		/// <param name="Name">Name of parameter. If null, parameter values will not be logged</param>
		/// <param name="Id">ID of object.</param>
		/// <param name="InstanceId">ID of object instance.</param>
		/// <param name="ResourceId">ID of resource.</param>
		/// <param name="CanWrite">If the resource allows servers to update the value using write commands.</param>
		/// <param name="Persist">If written values should be persisted by the resource.</param>
		/// <param name="ReferenceId">Referenced object id.</param>
		/// <param name="ReferenceInstanceId">Referenced object instance id.</param>
		public Lwm2mResourceObjectLink(string Name, ushort Id, ushort InstanceId, ushort ResourceId,
			bool CanWrite, bool Persist, ushort? ReferenceId, ushort? ReferenceInstanceId)
			: base(Name, Id, InstanceId, ResourceId, CanWrite, Persist)
		{
			this.defaultRefId = this.refId = ReferenceId;
			this.defaultRefInstanceId = this.refInstanceId = ReferenceInstanceId;
		}

		/// <summary>
		/// Loads the value of the resource, from persisted storage.
		/// </summary>
		public override async Task ReadPersistedValue()
		{
			string s = await RuntimeSettings.GetAsync(this.Path, this.StringValue);
			string[] Parts = s.Split(':');

			if (Parts.Length == 2 && ushort.TryParse(Parts[0], out ushort s1) &&
				ushort.TryParse(Parts[1], out ushort s2))
			{
				this.refId = s1;
				this.refInstanceId = s2;
			}
			else
			{
				this.refId = null;
				this.refInstanceId = null;
			}
		}

		private string StringValue
		{
			get
			{
				if (this.refId.HasValue && this.refInstanceId.HasValue)
					return this.refId.Value.ToString() + ":" + this.refInstanceId.Value.ToString();
				else
					return string.Empty;
			}
		}

		/// <summary>
		/// Saves the value of the resource, to persisted storage.
		/// </summary>
		/// <returns></returns>
		public override async Task WritePersistedValue()
		{
			await RuntimeSettings.SetAsync(this.Path, this.StringValue);
		}

		/// <summary>
		/// Value of resource.
		/// </summary>
		public override object Value
		{
			get
			{
				if (this.refId.HasValue && this.refInstanceId.HasValue)
					return new KeyValuePair<ushort, ushort>(this.refId.Value, this.refInstanceId.Value);
				else
					return null;
			}
		}

		/// <summary>
		/// Referenced object id.
		/// 
		/// Use <see cref="Set(ushort?, ushort?)"/> to change the value.
		/// </summary>
		public ushort? ReferenceId
		{
			get { return this.refId; }
		}

		/// <summary>
		/// Referenced object instance id.
		/// 
		/// Use <see cref="Set(ushort?, ushort?)"/> to change the value.
		/// </summary>
		public ushort? ReferenceInstanceId
		{
			get { return this.refInstanceId; }
		}

		/// <summary>
		/// Sets the value of the resource.
		/// </summary>
		/// <param name="ReferenceId">Referenced object id.</param>
		/// <param name="ReferenceInstanceId">Referenced object instance id.</param>
		public async Task Set(ushort? ReferenceId, ushort? ReferenceInstanceId)
		{
			if (this.refId != ReferenceId || this.refInstanceId != ReferenceInstanceId)
			{
				this.refId = ReferenceId;
				this.refInstanceId = ReferenceInstanceId;

				if (this.Persist)
					await this.WritePersistedValue();

				await this.ValueUpdated();
			}

			base.Set();
		}

		/// <summary>
		/// Reads the value from a TLV record.
		/// </summary>
		/// <param name="Record">TLV record.</param>
		public override Task Read(TlvRecord Record)
		{
			KeyValuePair<ushort, ushort> P = Record.AsObjectLink();
			return this.Set(P.Key, P.Value);
		}

		/// <summary>
		/// Reads the value from a TLV record.
		/// </summary>
		/// <param name="Output">Output.</param>
		public override void Write(ILwm2mWriter Output)
		{
			if (this.refId.HasValue && this.refInstanceId.HasValue)
				Output.Write(IdentifierType.Resource, this.ResourceId, this.refId.Value, this.refInstanceId.Value);
			else
				Output.Write(IdentifierType.Resource, this.ResourceId);
		}

		/// <summary>
		/// Resets the parameter to its default value.
		/// </summary>
		public override void Reset()
		{
			this.refId = this.defaultRefId;
			this.refInstanceId = this.defaultRefInstanceId;
		}
	}
}
