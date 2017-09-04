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
	/// Class managing an LWM2M resource integer.
	/// </summary>
	public class Lwm2mResourceInteger : Lwm2mResource
	{
		private long? defaultValue;
		private long? value;
		bool signed;

		/// <summary>
		/// Class managing an LWM2M resource integer.
		/// </summary>
		/// <param name="Name">Name of parameter. If null, parameter values will not be logged</param>
		/// <param name="Id">ID of object.</param>
		/// <param name="InstanceId">ID of object instance.</param>
		/// <param name="ResourceId">ID of resource.</param>
		/// <param name="Value">Value of resource.</param>
		/// <param name="CanWrite">If the resource allows servers to update the value using write commands.</param>
		/// <param name="Persist">If written values should be persisted by the resource.</param>
		/// <param name="Signed">If integers on this resource are signed (true), or unsigned (false).</param>
		public Lwm2mResourceInteger(string Name, ushort Id, ushort InstanceId, ushort ResourceId,
			bool CanWrite, bool Persist, long? Value, bool Signed)
			: base(Name, Id, InstanceId, ResourceId, CanWrite, Persist)
		{
			this.defaultValue = this.value = Value;
			this.signed = Signed;
		}

		/// <summary>
		/// Loads the value of the resource, from persisted storage.
		/// </summary>
		public override async Task ReadPersistedValue()
		{
			this.value = await RuntimeSettings.GetAsync(this.Path, this.value.HasValue ? this.value.Value : long.MinValue);
			if (this.value == long.MinValue)
				this.value = null;
		}

		/// <summary>
		/// Saves the value of the resource, to persisted storage.
		/// </summary>
		/// <returns></returns>
		public override async Task WritePersistedValue()
		{
			if (this.value.HasValue)
				await RuntimeSettings.SetAsync(this.Path, this.value.Value);
		}

		/// <summary>
		/// Value of resource.
		/// </summary>
		public override object Value => this.value;

		/// <summary>
		/// Resource value.
		/// 
		/// Use the <see cref="Set(long?)"/> method to set the value of a persistent resource.
		/// </summary>
		public long? IntegerValue
		{
			get { return this.value; }
			set { this.value = value; }
		}

		/// <summary>
		/// Sets the resource value.
		/// </summary>
		/// <param name="Value">Value to set.</param>
		public async Task Set(long? Value)
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
			return this.Set(Record.AsSignedInteger());
		}

		/// <summary>
		/// Reads the value from a TLV record.
		/// </summary>
		/// <param name="Output">Output.</param>
		public override void Write(ILwm2mWriter Output)
		{
			if (this.value.HasValue)
			{
				long l = this.value.Value;

				if (this.signed)
				{
					if (l >= sbyte.MinValue && l <= sbyte.MaxValue)
						Output.Write(IdentifierType.Resource, this.ResourceId, (sbyte)l);
					else if (l >= short.MinValue && l <= short.MaxValue)
						Output.Write(IdentifierType.Resource, this.ResourceId, (short)l);
					else if (l >= int.MinValue && l <= int.MaxValue)
						Output.Write(IdentifierType.Resource, this.ResourceId, (int)l);
					else
						Output.Write(IdentifierType.Resource, this.ResourceId, l);
				}
				else
				{
					if (l >= byte.MinValue && l <= byte.MaxValue)
						Output.Write(IdentifierType.Resource, this.ResourceId, (sbyte)l);
					else if (l >= ushort.MinValue && l <= ushort.MaxValue)
						Output.Write(IdentifierType.Resource, this.ResourceId, (short)l);
					else if (l >= uint.MinValue && l <= uint.MaxValue)
						Output.Write(IdentifierType.Resource, this.ResourceId, (int)l);
					else
						Output.Write(IdentifierType.Resource, this.ResourceId, l);
				}
			}
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
