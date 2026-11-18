using System;
using System.Collections.Generic;
using Waher.Runtime.Inventory;
using Waher.Things.ControlParameters;
using Waher.Things.Matter.DataModel;
using Waher.Things.Matter.Tlv;
using Waher.Things.SensorData;

namespace Waher.Things.Matter.Clusters
{
	/// <summary>
	/// Generic cluster handler, used for clusters without specific handlers. It uses the metadata available
	/// in the data model to present scalar attributes as fields, writable scalar attributes as control parameters,
	/// commands without mandatory arguments as commands, and events as fields.
	/// </summary>
	public class GenericClusterHandler : ClusterHandler
	{
		/// <summary>
		/// Generic cluster handler, used for clusters without specific handlers.
		/// </summary>
		public GenericClusterHandler()
		{
		}

		/// <inheritdoc/>
		public override Grade Supports(uint ClusterId)
		{
			return Grade.Barely;
		}

		/// <inheritdoc/>
		public override bool HasValues(MatterClusterInfo Cluster)
		{
			ClusterDefinition Definition = Cluster.GetDefinition();
			if (Definition is null)
				return true;     // Unknown and manufacturer-specific clusters are reported in raw form.

			foreach (uint AttributeId in Cluster.GetAttributeIds())
			{
				AttributeDefinition Attribute = Definition.GetAttribute(AttributeId);

				if (!(Attribute is null) && !Attribute.IsList && !GlobalAttributeIds.IsGlobal(AttributeId) &&
					ValueConverter.IsScalar(ValueConverter.Classify(Cluster.ClusterId, Attribute.Type)))
				{
					return true;
				}
			}

			return false;
		}

		/// <inheritdoc/>
		public override bool HasEvents(MatterClusterInfo Cluster)
		{
			ClusterDefinition Definition = Cluster.GetDefinition();

			if (!(Definition is null))
			{
				foreach (EventDefinition _ in Definition.Events)
					return true;
			}

			return false;
		}

		/// <inheritdoc/>
		public override void AddFields(ClusterContext Context, ClusterValues Values, FieldBuilder Fields)
		{
			List<uint> AttributeIds = new List<uint>(Values.Reported);
			AttributeIds.Sort();

			foreach (uint AttributeId in AttributeIds)
			{
				if (!Values.TryGetReported(AttributeId, out TlvElement Value))
					continue;

				AttributeDefinition Attribute = Context.Definition?.GetAttribute(AttributeId);
				bool Writable = IsGenericControllable(Context.ClusterId, Attribute);

				AddGenericField(Context, Fields, AttributeId, Value, FieldType.Momentary, Writable);
			}
		}

		/// <inheritdoc/>
		public override bool IsControllable(MatterClusterInfo Cluster)
		{
			ClusterDefinition Definition = Cluster.GetDefinition();
			if (Definition is null)
				return false;

			foreach (uint AttributeId in Cluster.GetAttributeIds())
			{
				if (IsGenericControllable(Cluster.ClusterId, Definition.GetAttribute(AttributeId)))
					return true;
			}

			return false;
		}

		/// <inheritdoc/>
		public override void AddControlParameters(ClusterContext Context, List<ControlParameter> Parameters)
		{
			if (Context.Definition is null)
				return;

			uint[] AttributeIds = Context.Cluster.GetAttributeIds();
			Array.Sort(AttributeIds);

			foreach (uint AttributeId in AttributeIds)
			{
				AttributeDefinition Attribute = Context.Definition.GetAttribute(AttributeId);
				ControlParameter Parameter = CreateGenericControlParameter(Context, Attribute);

				if (!(Parameter is null))
					Parameters.Add(Parameter);
			}
		}

		/// <inheritdoc/>
		public override void AddCommands(ClusterContext Context, List<ICommand> Commands)
		{
			AddGenericCommands(Context, Commands);
		}
	}
}
