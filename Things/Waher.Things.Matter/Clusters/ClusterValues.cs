using System.Collections.Generic;
using Waher.Things.Matter.Tlv;

namespace Waher.Things.Matter.Clusters
{
	/// <summary>
	/// Attribute values of a cluster, as reported by a read or subscription report.
	/// </summary>
	public class ClusterValues
	{
		private readonly ClusterContext context;
		private readonly Dictionary<uint, TlvElement> reported;

		/// <summary>
		/// Attribute values of a cluster, as reported by a read or subscription report.
		/// </summary>
		/// <param name="Context">Cluster context.</param>
		/// <param name="Reported">Reported attribute values.</param>
		public ClusterValues(ClusterContext Context, Dictionary<uint, TlvElement> Reported)
		{
			this.context = Context;
			this.reported = Reported;
		}

		/// <summary>
		/// If the report is a complete readout (true), or a partial report of changed values (false).
		/// </summary>
		public bool IsReadout { get; set; }

		/// <summary>
		/// IDs of attributes that have been reported.
		/// </summary>
		public IEnumerable<uint> Reported => this.reported.Keys;

		/// <summary>
		/// Checks if an attribute has been reported.
		/// </summary>
		/// <param name="AttributeId">Attribute ID.</param>
		/// <returns>If attribute has been reported.</returns>
		public bool IsReported(uint AttributeId)
		{
			return this.reported.ContainsKey(AttributeId);
		}

		/// <summary>
		/// Checks if any of a set of attributes have been reported.
		/// </summary>
		/// <param name="AttributeIds">Attribute IDs.</param>
		/// <returns>If any of the attributes has been reported.</returns>
		public bool IsAnyReported(params uint[] AttributeIds)
		{
			foreach (uint AttributeId in AttributeIds)
			{
				if (this.reported.ContainsKey(AttributeId))
					return true;
			}

			return false;
		}

		/// <summary>
		/// Gets the reported value of an attribute, or the last known value, if not reported.
		/// </summary>
		/// <param name="AttributeId">Attribute ID.</param>
		/// <returns>Attribute value, or null if not known.</returns>
		public TlvElement Get(uint AttributeId)
		{
			if (this.reported.TryGetValue(AttributeId, out TlvElement Value))
				return Value;
			else
				return this.context.GetCachedValue(AttributeId);
		}

		/// <summary>
		/// Tries to get a reported attribute value. Only values reported will be returned. The value can be a null value.
		/// </summary>
		/// <param name="AttributeId">Attribute ID.</param>
		/// <param name="Value">Value, if reported.</param>
		/// <returns>If the attribute has been reported.</returns>
		public bool TryGetReported(uint AttributeId, out TlvElement Value)
		{
			return this.reported.TryGetValue(AttributeId, out Value);
		}

		/// <summary>
		/// Tries to get a reported, non-null, numeric attribute value.
		/// </summary>
		/// <param name="AttributeId">Attribute ID.</param>
		/// <param name="Value">Value, if reported.</param>
		/// <returns>If the attribute has been reported, and is a non-null number.</returns>
		public bool TryGetReported(uint AttributeId, out double Value)
		{
			if (this.reported.TryGetValue(AttributeId, out TlvElement E) && !(E is null) && !E.IsNull)
			{
				switch (E.Type)
				{
					case TlvValueType.SignedInteger:
					case TlvValueType.UnsignedInteger:
					case TlvValueType.Float:
					case TlvValueType.Double:
						Value = E.AsDouble();
						return true;
				}
			}

			Value = 0;
			return false;
		}

		/// <summary>
		/// Tries to get a reported, non-null, boolean attribute value.
		/// </summary>
		/// <param name="AttributeId">Attribute ID.</param>
		/// <param name="Value">Value, if reported.</param>
		/// <returns>If the attribute has been reported, and is a non-null boolean.</returns>
		public bool TryGetReported(uint AttributeId, out bool Value)
		{
			if (this.reported.TryGetValue(AttributeId, out TlvElement E) && !(E is null) && E.Type == TlvValueType.Boolean)
			{
				Value = E.AsBoolean();
				return true;
			}

			Value = false;
			return false;
		}

		/// <summary>
		/// Tries to get a reported, non-null, string attribute value.
		/// </summary>
		/// <param name="AttributeId">Attribute ID.</param>
		/// <param name="Value">Value, if reported.</param>
		/// <returns>If the attribute has been reported, and is a non-null string.</returns>
		public bool TryGetReported(uint AttributeId, out string Value)
		{
			if (this.reported.TryGetValue(AttributeId, out TlvElement E) && !(E is null) && E.Type == TlvValueType.String)
			{
				Value = E.AsString();
				return true;
			}

			Value = null;
			return false;
		}

		/// <summary>
		/// Tries to get a reported, non-null, integer attribute value.
		/// </summary>
		/// <param name="AttributeId">Attribute ID.</param>
		/// <param name="Value">Value, if reported.</param>
		/// <returns>If the attribute has been reported, and is a non-null integer.</returns>
		public bool TryGetReported(uint AttributeId, out long Value)
		{
			if (this.reported.TryGetValue(AttributeId, out TlvElement E) && !(E is null) && E.IsInteger)
			{
				Value = E.AsInt64();
				return true;
			}

			Value = 0;
			return false;
		}

		/// <summary>
		/// Tries to get a known (reported or cached), non-null, numeric attribute value.
		/// </summary>
		/// <param name="AttributeId">Attribute ID.</param>
		/// <param name="Value">Value, if known.</param>
		/// <returns>If the attribute value is known, and is a non-null number.</returns>
		public bool TryGetKnown(uint AttributeId, out double Value)
		{
			TlvElement E = this.Get(AttributeId);

			if (!(E is null) && !E.IsNull)
			{
				switch (E.Type)
				{
					case TlvValueType.SignedInteger:
					case TlvValueType.UnsignedInteger:
					case TlvValueType.Float:
					case TlvValueType.Double:
						Value = E.AsDouble();
						return true;
				}
			}

			Value = 0;
			return false;
		}

		/// <summary>
		/// Tries to get a known (reported or cached), non-null, integer attribute value.
		/// </summary>
		/// <param name="AttributeId">Attribute ID.</param>
		/// <param name="Value">Value, if known.</param>
		/// <returns>If the attribute value is known, and is a non-null integer.</returns>
		public bool TryGetKnown(uint AttributeId, out long Value)
		{
			TlvElement E = this.Get(AttributeId);

			if (!(E is null) && E.IsInteger)
			{
				Value = E.AsInt64();
				return true;
			}

			Value = 0;
			return false;
		}
	}
}
