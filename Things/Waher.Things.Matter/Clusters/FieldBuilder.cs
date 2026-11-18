using System;
using System.Collections.Generic;
using Waher.Things.SensorData;

namespace Waher.Things.Matter.Clusters
{
	/// <summary>
	/// Keeps track of field names used on an endpoint, making sure generic field names are unique and stable.
	/// </summary>
	public class FieldNameRegistry
	{
		private readonly Dictionary<ulong, string> namesByKey = new Dictionary<ulong, string>();
		private readonly Dictionary<string, ulong> keysByName = new Dictionary<string, ulong>(StringComparer.OrdinalIgnoreCase);
		private readonly HashSet<string> fixedNames = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

		/// <summary>
		/// Keeps track of field names used on an endpoint, making sure generic field names are unique and stable.
		/// </summary>
		public FieldNameRegistry()
		{
		}

		/// <summary>
		/// Registers a fixed field name (used by a specific cluster handler).
		/// </summary>
		/// <param name="Name">Field name.</param>
		public void RegisterFixed(string Name)
		{
			lock (this.namesByKey)
			{
				this.fixedNames.Add(Name);
			}
		}

		/// <summary>
		/// Gets a unique field name for an attribute or event of a cluster.
		/// </summary>
		/// <param name="ClusterId">Cluster ID.</param>
		/// <param name="Id">Attribute or event ID. Events use the high bit (0x80000000) to separate them from attributes.</param>
		/// <param name="Name">Preferred name.</param>
		/// <param name="ClusterName">Cluster name, used as prefix if name is not unique.</param>
		/// <returns>Unique field name.</returns>
		public string GetName(uint ClusterId, uint Id, string Name, string ClusterName)
		{
			ulong Key = ((ulong)ClusterId << 32) | Id;

			lock (this.namesByKey)
			{
				if (this.namesByKey.TryGetValue(Key, out string Result))
					return Result;

				Result = Name;

				if (this.fixedNames.Contains(Result) || this.keysByName.ContainsKey(Result))
				{
					Result = ClusterName + ", " + Name;

					int i = 2;
					string s = Result;

					while (this.fixedNames.Contains(s) || this.keysByName.ContainsKey(s))
						s = Result + " " + (i++).ToString();

					Result = s;
				}

				this.namesByKey[Key] = Result;
				this.keysByName[Result] = Key;

				return Result;
			}
		}
	}

	/// <summary>
	/// Builds sensor data fields for a node, respecting any field filters in the readout request.
	/// </summary>
	public class FieldBuilder
	{
		private readonly List<Field> fields = new List<Field>();
		private readonly ThingReference thing;
		private readonly DateTime timestamp;
		private readonly ISensorReadout request;
		private readonly FieldNameRegistry names;

		/// <summary>
		/// Builds sensor data fields for a node, respecting any field filters in the readout request.
		/// </summary>
		/// <param name="Thing">Thing reference to which fields belong.</param>
		/// <param name="Timestamp">Timestamp of fields.</param>
		/// <param name="Request">Readout request, if any, or null if fields are generated from momentary changes.</param>
		/// <param name="Names">Field name registry.</param>
		public FieldBuilder(ThingReference Thing, DateTime Timestamp, ISensorReadout Request, FieldNameRegistry Names)
		{
			this.thing = Thing;
			this.timestamp = Timestamp;
			this.request = Request;
			this.names = Names ?? new FieldNameRegistry();
		}

		/// <summary>
		/// Thing reference to which fields belong.
		/// </summary>
		public ThingReference Thing => this.thing;

		/// <summary>
		/// Timestamp of fields.
		/// </summary>
		public DateTime Timestamp => this.timestamp;

		/// <summary>
		/// Readout request, if any, or null if fields are generated from momentary changes.
		/// </summary>
		public ISensorReadout Request => this.request;

		/// <summary>
		/// Field name registry.
		/// </summary>
		public FieldNameRegistry Names => this.names;

		/// <summary>
		/// Fields generated.
		/// </summary>
		public List<Field> Fields => this.fields;

		/// <summary>
		/// Number of fields generated.
		/// </summary>
		public int Count => this.fields.Count;

		/// <summary>
		/// Checks if a field should be included.
		/// </summary>
		/// <param name="Name">Field name.</param>
		/// <param name="Type">Field type.</param>
		/// <returns>If field should be included.</returns>
		public bool IsIncluded(string Name, FieldType Type)
		{
			if (this.request is null)
				return true;

			return this.request.IsIncluded(Name, Type);
		}

		/// <summary>
		/// Checks if a field type should be included.
		/// </summary>
		/// <param name="Type">Field type.</param>
		/// <returns>If field type should be included.</returns>
		public bool IsIncluded(FieldType Type)
		{
			if (this.request is null)
				return true;

			return this.request.IsIncluded(Type);
		}

		/// <summary>
		/// Adds a field.
		/// </summary>
		/// <param name="Field">Field.</param>
		public void Add(Field Field)
		{
			if (this.IsIncluded(Field.Name, Field.Type))
				this.fields.Add(Field);
		}

		/// <summary>
		/// Adds a quantity field.
		/// </summary>
		/// <param name="Name">Field name.</param>
		/// <param name="Value">Value.</param>
		/// <param name="NrDecimals">Number of decimals.</param>
		/// <param name="Unit">Unit.</param>
		/// <param name="Type">Field type.</param>
		/// <param name="Writable">If the field corresponds to a control parameter.</param>
		public void Quantity(string Name, double Value, byte NrDecimals, string Unit, FieldType Type = FieldType.Momentary, bool Writable = false)
		{
			this.names.RegisterFixed(Name);

			if (double.IsNaN(Value) || double.IsInfinity(Value))
				return;

			if (this.IsIncluded(Name, Type))
			{
				this.fields.Add(new QuantityField(this.thing, this.timestamp, Name, Math.Round(Value, NrDecimals), NrDecimals,
					Unit ?? string.Empty, Type, FieldQoS.AutomaticReadout, Writable));
			}
		}

		/// <summary>
		/// Adds a boolean field.
		/// </summary>
		/// <param name="Name">Field name.</param>
		/// <param name="Value">Value.</param>
		/// <param name="Type">Field type.</param>
		/// <param name="Writable">If the field corresponds to a control parameter.</param>
		public void Boolean(string Name, bool Value, FieldType Type = FieldType.Momentary, bool Writable = false)
		{
			this.names.RegisterFixed(Name);

			if (this.IsIncluded(Name, Type))
				this.fields.Add(new BooleanField(this.thing, this.timestamp, Name, Value, Type, FieldQoS.AutomaticReadout, Writable));
		}

		/// <summary>
		/// Adds a string field.
		/// </summary>
		/// <param name="Name">Field name.</param>
		/// <param name="Value">Value.</param>
		/// <param name="Type">Field type.</param>
		/// <param name="Writable">If the field corresponds to a control parameter.</param>
		public void String(string Name, string Value, FieldType Type = FieldType.Momentary, bool Writable = false)
		{
			this.names.RegisterFixed(Name);

			if (Value is null)
				return;

			if (this.IsIncluded(Name, Type))
				this.fields.Add(new StringField(this.thing, this.timestamp, Name, Value, Type, FieldQoS.AutomaticReadout, Writable));
		}

		/// <summary>
		/// Adds an integer field.
		/// </summary>
		/// <param name="Name">Field name.</param>
		/// <param name="Value">Value.</param>
		/// <param name="Type">Field type.</param>
		/// <param name="Writable">If the field corresponds to a control parameter.</param>
		public void Integer(string Name, long Value, FieldType Type = FieldType.Momentary, bool Writable = false)
		{
			this.names.RegisterFixed(Name);

			if (this.IsIncluded(Name, Type))
			{
				if (Value >= int.MinValue && Value <= int.MaxValue)
					this.fields.Add(new Int32Field(this.thing, this.timestamp, Name, (int)Value, Type, FieldQoS.AutomaticReadout, Writable));
				else
					this.fields.Add(new Int64Field(this.thing, this.timestamp, Name, Value, Type, FieldQoS.AutomaticReadout, Writable));
			}
		}

		/// <summary>
		/// Adds a date and time field.
		/// </summary>
		/// <param name="Name">Field name.</param>
		/// <param name="Value">Value.</param>
		/// <param name="Type">Field type.</param>
		/// <param name="Writable">If the field corresponds to a control parameter.</param>
		public void DateTime(string Name, DateTime Value, FieldType Type = FieldType.Momentary, bool Writable = false)
		{
			this.names.RegisterFixed(Name);

			if (this.IsIncluded(Name, Type))
				this.fields.Add(new DateTimeField(this.thing, this.timestamp, Name, Value, Type, FieldQoS.AutomaticReadout, Writable));
		}

		/// <summary>
		/// Gets a unique field name for a generic attribute.
		/// </summary>
		/// <param name="ClusterId">Cluster ID.</param>
		/// <param name="AttributeId">Attribute ID.</param>
		/// <param name="Name">Preferred name.</param>
		/// <param name="ClusterName">Cluster name.</param>
		/// <returns>Unique name.</returns>
		public string GetAttributeFieldName(uint ClusterId, uint AttributeId, string Name, string ClusterName)
		{
			return this.names.GetName(ClusterId, AttributeId, Name, ClusterName);
		}

		/// <summary>
		/// Gets a unique field name for a generic event.
		/// </summary>
		/// <param name="ClusterId">Cluster ID.</param>
		/// <param name="EventId">Event ID.</param>
		/// <param name="Name">Preferred name.</param>
		/// <param name="ClusterName">Cluster name.</param>
		/// <returns>Unique name.</returns>
		public string GetEventFieldName(uint ClusterId, uint EventId, string Name, string ClusterName)
		{
			return this.names.GetName(ClusterId, 0x80000000 | EventId, Name, ClusterName);
		}
	}
}
