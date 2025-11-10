using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Waher.Events;
using Waher.Output.NodeTypes;
using Waher.Persistence;
using Waher.Persistence.Serialization;
using Waher.Runtime.Language;
using Waher.Runtime.Threading;
using Waher.Script.Objects;
using Waher.Things;
using Waher.Things.Attributes;
using Waher.Things.SensorData;

namespace Waher.Output.Metering.NodeTypes
{
	/// <summary>
	/// Stores sensor data in the internal database.
	/// </summary>
	public class InternalDatabaseOutput : OutputNode, ISensorDataOutput
	{
		private string collection = string.Empty;
		private string[] fieldIndices = null;
		private bool checkCollection = false;
		private bool checkIndices = false;

		/// <summary>
		/// Stores sensor data in the internal database.
		/// </summary>
		public InternalDatabaseOutput()
			: base()
		{
		}

		/// <summary>
		/// Collection to store data in.
		/// </summary>
		[Header(2, "Collection:", 0)]
		[Page(3, "Output", 0)]
		[ToolTip(4, "Name of collection in internal database that will house the sensor data.")]
		[Required]
		public string Collection
		{
			get => this.collection;
			set
			{
				if (this.collection != value)
				{
					this.collection = value;
					this.checkCollection = true;
					this.checkIndices = true;
				}
			}
		}

		/// <summary>
		/// Fields to index in collection.
		/// </summary>
		[Header(5, "Index field names:", 10)]
		[Page(3, "Output", 0)]
		[ToolTip(6, "Indices will be added on these field names for quicker access to records.")]
		public string[] FieldIndices
		{
			get => this.fieldIndices;
			set
			{
				if (!this.fieldIndices.ElementEquals(value))
				{
					this.fieldIndices = value;
					this.checkIndices = true;
				}
			}
		}

		/// <summary>
		/// If physical quantity units should be stored.
		/// </summary>
		[Header(7, "Store physical units.", 20)]
		[Page(3, "Output", 0)]
		[ToolTip(8, "If physical units should be stored in a separate property.")]
		public bool StoreUnits { get; set; }

		/// <summary>
		/// If field type should be stored.
		/// </summary>
		[Header(9, "Store field type.", 30)]
		[Page(3, "Output", 0)]
		[ToolTip(10, "If field type should be stored in a separate property.")]
		public bool StoreFieldType { get; set; }

		/// <summary>
		/// If field type should be stored.
		/// </summary>
		[Header(11, "Store quality of service.", 40)]
		[Page(3, "Output", 0)]
		[ToolTip(12, "If quality of service should be stored in a separate property.")]
		public bool StoreQoS { get; set; }

		/// <summary>
		/// Gets the type name of the node.
		/// </summary>
		/// <param name="Language">Language to use.</param>
		/// <returns>Localized type node.</returns>
		public override Task<string> GetTypeNameAsync(Language Language)
		{
			return Language.GetStringAsync(typeof(InternalDatabaseOutput), 1, "Internal Database Output");
		}

		/// <summary>
		/// If the node accepts a presumptive child, i.e. can receive as a child (if that child accepts the node as a parent).
		/// </summary>
		/// <param name="Child">Presumptive child node.</param>
		/// <returns>If the child is acceptable.</returns>
		public override Task<bool> AcceptsChildAsync(INode Child)
		{
			return Task.FromResult(false);
		}

		/// <summary>
		/// If the node accepts a presumptive parent, i.e. can be added to that parent (if that parent accepts the node as a child).
		/// </summary>
		/// <param name="Parent">Presumptive parent node.</param>
		/// <returns>If the parent is acceptable.</returns>
		public override Task<bool> AcceptsParentAsync(INode Parent)
		{
			return Task.FromResult(Parent is Root);
		}

		/// <summary>
		/// Outputs a collection of sensor data fields.
		/// </summary>
		/// <param name="Sensor">Sensor reporting the fields.</param>
		/// <param name="Fields">Fields to output.</param>
		public async Task OutputFields(ISensor Sensor, Field[] Fields)
		{
			using Semaphore Semaphore = await Semaphores.BeginWrite("collection:" + this.collection);

			if (this.checkCollection)
			{
				if (string.IsNullOrEmpty(this.collection))
				{
					await this.LogErrorAsync("CollectionError", "Collection name cannot be empty.");
					return;
				}
				else
					await this.RemoveErrorAsync("CollectionError");

				string[] Collections = await Database.GetCollections();

				if (Array.IndexOf(Collections, this.collection) < 0)
					this.checkIndices = true;

				this.checkCollection = false;
			}

			if (this.checkIndices)
			{
				Dictionary<string, string[]> Indices = new Dictionary<string, string[]>();

				foreach (string[] Index in await Database.GetIndices(this.collection))
					Indices[IndexKey(Index)] = Index;

				await this.AddIndexIfNotDefined(Indices, new string[]
				{
					"NodeId",
					"Timestamp"
				});

				await this.AddIndexIfNotDefined(Indices, new string[]
				{
					"Timestamp",
					"NodeId"
				});

				foreach (string FieldName in this.fieldIndices ?? Array.Empty<string>())
				{
					await this.AddIndexIfNotDefined(Indices, new string[]
					{
						FieldName,
						"NodeId",
						"Timestamp"
					});

					await this.AddIndexIfNotDefined(Indices, new string[]
					{
						FieldName,
						"Timestamp",
						"NodeId"
					});
				}

				foreach (KeyValuePair<string, string[]> IndexLeft in Indices)
					await Database.RemoveIndex(this.collection, IndexLeft.Value);

				this.checkIndices = false;
			}

			Dictionary<DateTime, GenericObject> PerTime = new Dictionary<DateTime, GenericObject>();
			GenericObject Obj = null;
			DateTime LastTime = DateTime.MinValue;

			foreach (Field Field in Fields)
			{
				if (Obj is null || LastTime != Field.Timestamp)
				{
					LastTime = Field.Timestamp;

					if (!PerTime.TryGetValue(Field.Timestamp, out Obj))
					{
						Obj = new GenericObject(this.collection, string.Empty, Guid.Empty,
							new KeyValuePair<string, object>("NodeId", Sensor.NodeId),
							new KeyValuePair<string, object>("Timestamp", LastTime));
						PerTime[LastTime] = Obj;
					}
				}

				object Value = Field.ObjectValue;

				if (Value is PhysicalQuantity Q)
				{
					Obj[Field.Name] = Q.Magnitude;

					if (this.StoreUnits)
						Obj[Field.Name + ", Unit"] = Q.Unit;
				}
				else
					Obj[Field.Name] = Value;

				if (this.StoreFieldType)
					Obj[Field.Name + ", Type"] = Field.Type;

				if (this.StoreQoS)
					Obj[Field.Name + ", QoS"] = Field.QoS;
			}

			await Database.Insert(PerTime.Values);
		}

		private async Task AddIndexIfNotDefined(Dictionary<string, string[]> Indices, string[] Index)
		{
			string Key = IndexKey(Index);
			if (!Indices.Remove(Key))
				await Database.AddIndex(this.collection, Index);
		}

		private static string IndexKey(string[] Fields)
		{
			StringBuilder sb = new StringBuilder();
			bool First = true;

			foreach (string Field in Fields)
			{
				if (First)
					First = false;
				else
					sb.AppendLine();

				sb.Append(Field);
			}

			return sb.ToString();
		}
	}
}
