using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Waher.Events;
using Waher.Networking.LWM2M.ContentFormats;
using Waher.Security;
using Waher.Runtime.Settings;

namespace Waher.Networking.LWM2M
{
	/// <summary>
	/// Class managing an LWM2M resource opaque value.
	/// </summary>
	public class Lwm2mResourceOpaque : Lwm2mResource
	{
		private byte[] defaultValue;
		private byte[] value;

		/// <summary>
		/// Class managing an LWM2M resource opaque value.
		/// </summary>
		/// <param name="Name">Name of parameter. If null, parameter values will not be logged</param>
		/// <param name="Id">ID of object.</param>
		/// <param name="InstanceId">ID of object instance.</param>
		/// <param name="ResourceId">ID of resource.</param>
		/// <param name="CanWrite">If the resource allows servers to update the value using write commands.</param>
		/// <param name="Persist">If written values should be persisted by the resource.</param>
		/// <param name="Value">Value of resource.</param>
		public Lwm2mResourceOpaque(string Name, ushort Id, ushort InstanceId, ushort ResourceId,
			bool CanWrite, bool Persist, byte[] Value)
			: base(Name, Id, InstanceId, ResourceId, CanWrite, Persist)
		{
			this.defaultValue = this.value = Value;
		}

		/// <summary>
		/// Loads the value of the resource, from persisted storage.
		/// </summary>
		public override async Task ReadPersistedValue()
		{
			string s = this.value == null ? string.Empty :
				await RuntimeSettings.GetAsync(this.Path, Hashes.BinaryToString(this.value));

			if (s == null)
				this.value = null;
			else
				this.value = Hashes.StringToBinary(s);
		}

		/// <summary>
		/// Saves the value of the resource, to persisted storage.
		/// </summary>
		/// <returns></returns>
		public override async Task WritePersistedValue()
		{
			if (this.value != null)
				await RuntimeSettings.SetAsync(this.Path, Hashes.BinaryToString(this.value));
		}

		/// <summary>
		/// Value of resource.
		/// </summary>
		public override object Value => this.value;

		/// <summary>
		/// Resource value.
		/// 
		/// Use the <see cref="Set(Byte[])"/> method to set the value of a persistent resource.
		/// </summary>
		public byte[] OpaqueValue
		{
			get { return this.value; }
			set { this.value = value; }
		}

		/// <summary>
		/// Sets the resource value.
		/// </summary>
		/// <param name="Value">Value to set.</param>
		public async Task Set(byte[] Value)
		{
			if (this.value != Value)
			{
				this.value = Value;

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
			return this.Set(Record.RawValue);
		}

		/// <summary>
		/// Reads the value from a TLV record.
		/// </summary>
		/// <param name="Output">Output.</param>
		public override void Write(ILwm2mWriter Output)
		{
			if (this.value != null)
				Output.Write(IdentifierType.Resource, this.ResourceId, this.value);
			else
				Output.Write(IdentifierType.Resource, this.ResourceId);
		}

		/// <summary>
		/// Resets the parameter to its default value.
		/// </summary>
		public override void Reset()
		{
			this.value = this.defaultValue;
		}
	}
}
