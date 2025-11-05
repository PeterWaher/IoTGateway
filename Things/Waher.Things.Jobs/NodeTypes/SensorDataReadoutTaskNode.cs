using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Waher.Content;
using Waher.Runtime.Collections;
using Waher.Runtime.Language;
using Waher.Runtime.Queue;
using Waher.Script.Functions.Runtime;
using Waher.Things.Attributes;
using Waher.Things.Groups;
using Waher.Things.Metering;
using Waher.Things.SensorData;

namespace Waher.Things.Jobs.NodeTypes
{
	/// <summary>
	/// Sensor data readout task node.
	/// </summary>
	public class SensorDataReadoutTaskNode : JobTaskNode
	{
		/// <summary>
		/// Sensor data readout task node.
		/// </summary>
		public SensorDataReadoutTaskNode()
		{
		}

		/// <summary>
		/// ID of node.
		/// </summary>
		[Header(35, "Parallel readouts:", 0)]
		[Page(34, "Readout", 0)]
		[ToolTip(36, "Maximum number of parallel readouts.")]
		[Required]
		[Range(1, 100)]
		[DefaultValue(1)]
		public int ParallelReadouts { get; set; } = 1;

		/// <summary>
		/// If momentary values should be read.
		/// </summary>
		[Header(39, "Momentary values.", 0)]
		[Page(34, "Readout", 0)]
		[ToolTip(40, "Check, if momentary values should be read.")]
		[DefaultValue(true)]
		public bool Momentary { get; set; } = true;

		/// <summary>
		/// If identity values should be read.
		/// </summary>
		[Header(41, "Identity values.", 0)]
		[Page(34, "Readout", 0)]
		[ToolTip(42, "Check, if identity values should be read.")]
		[DefaultValue(false)]
		public bool Identity { get; set; } = false;

		/// <summary>
		/// If status values should be read.
		/// </summary>
		[Header(43, "Status values.", 0)]
		[Page(34, "Readout", 0)]
		[ToolTip(44, "Check, if status values should be read.")]
		[DefaultValue(false)]
		public bool Status { get; set; } = false;

		/// <summary>
		/// If computed values should be read.
		/// </summary>
		[Header(45, "Computed values.", 0)]
		[Page(34, "Readout", 0)]
		[ToolTip(46, "Check, if computed values should be read.")]
		[DefaultValue(false)]
		public bool Computed { get; set; } = false;

		/// <summary>
		/// If peak values should be read.
		/// </summary>
		[Header(47, "Peak values.", 0)]
		[Page(34, "Readout", 0)]
		[ToolTip(48, "Check, if peak values should be read.")]
		[DefaultValue(false)]
		public bool Peak { get; set; } = false;

		/// <summary>
		/// If historical values should be read.
		/// </summary>
		[Header(49, "Historical values.", 0)]
		[Page(34, "Readout", 0)]
		[ToolTip(50, "Check, if historical values should be read.")]
		[DefaultValue(false)]
		public bool Historical { get; set; } = false;

		/// <summary>
		/// Types of fields to read.
		/// </summary>
		public FieldType FieldTypes
		{
			get
			{
				FieldType Result = (FieldType)0;

				if (this.Momentary)
					Result |= FieldType.Momentary;

				if (this.Identity)
					Result |= FieldType.Identity;

				if (this.Status)
					Result |= FieldType.Status;

				if (this.Computed)
					Result |= FieldType.Computed;

				if (this.Peak)
					Result |= FieldType.Peak;

				if (this.Historical)
					Result |= FieldType.Historical;

				return Result;
			}
		}

		/// <summary>
		/// ID of node.
		/// </summary>
		[Header(37, "Field names to read:", 0)]
		[Page(34, "Readout", 0)]
		[ToolTip(38, "Leave blank to read all fields.")]
		[ContentType("text/plain")]
		public string[] FieldNames { get; set; }

		/// <summary>
		/// From when data should be read.
		/// </summary>
		[Header(51, "From:", 0)]
		[Page(34, "Readout", 0)]
		[ToolTip(52, "Read historical data from this point in time.")]
		public Duration From { get; set; } = Duration.Zero;

		/// <summary>
		/// To when data should be read.
		/// </summary>
		[Header(53, "To:", 0)]
		[Page(34, "Readout", 0)]
		[ToolTip(54, "Read historical data to this point in time.")]
		public Duration To { get; set; } = Duration.Zero;

		/// <summary>
		/// Gets the type name of the node.
		/// </summary>
		/// <param name="Language">Language to use.</param>
		/// <returns>Localized type node.</returns>
		public override Task<string> GetTypeNameAsync(Language Language)
		{
			return Language.GetStringAsync(typeof(GroupSource), 33, "Sensor Data Readout Task");
		}

		/// <summary>
		/// If the node accepts a presumptive child, i.e. can receive as a child (if that child accepts the node as a parent).
		/// </summary>
		/// <param name="Child">Presumptive child node.</param>
		/// <returns>If the child is acceptable.</returns>
		public override Task<bool> AcceptsChildAsync(INode Child)
		{
			return Task.FromResult(
				Child is MeteringNodeReference ||
				Child is GroupNodeReference);
		}

		/// <summary>
		/// Executes the task.
		/// </summary>
		/// <param name="Status">Execution status.</param>
		public override async Task ExecuteTask(JobExecutionStatus Status)
		{
			IMeteringNode[] Nodes = await this.FindNodes<IMeteringNode>();
			if (Nodes is null || Nodes.Length == 0)
				return;

			using AsyncProcessor<ReadoutWorkItem> Processor = new AsyncProcessor<ReadoutWorkItem>(this.ParallelReadouts);

			foreach (IMeteringNode Node in Nodes)
			{
				if (Node.IsReadable && Node is ISensor Sensor)
					Processor.Queue(new ReadoutWorkItem(Sensor, this, Status));
			}

			await Processor.WaitUntilIdle();
		}
		
		private Task FieldsReported(ISensor Sensor, Field[] Fields, JobExecutionStatus Status)
		{
			// TODO: Preprocess fields
			// TODO: Output fields to some storage

			return Task.CompletedTask;
		}

		private class ReadoutWorkItem : WorkItem
		{
			private readonly ISensor sensor;
			private readonly SensorDataReadoutTaskNode task;
			private readonly JobExecutionStatus status;
			private ChunkedList<Field> fields = null;
			private ChunkedList<ThingError> errors = null;

			public ReadoutWorkItem(ISensor Sensor, SensorDataReadoutTaskNode TaskNode, JobExecutionStatus Status)
			{
				this.sensor = Sensor;
				this.task = TaskNode;
				this.status = Status;
			}

			public override async Task Execute(CancellationToken Cancel)
			{
				JobReadout Readout = new JobReadout(this);
				try
				{
					await this.sensor.StartReadout(Readout);
				}
				catch (Exception ex)
				{
					await Readout.ReportErrors(true, new ThingError(this.sensor, ex.Message));
				}

				if (this.sensor is MeteringNode MeteringNode)
				{
					if (this.errors is null)
						await MeteringNode.RemoveErrorAsync("ReadoutErrors");
					else
					{
						StringBuilder sb = new StringBuilder();
						bool First = true;

						foreach (ThingError Error in this.errors)
						{
							if (First)
								First = false;
							else
								sb.AppendLine();

							sb.Append(Error.ErrorMessage);
						}

						await MeteringNode.LogErrorAsync("ReadoutErrors", sb.ToString());
					}
				}

				if (!(this.fields is null))
					await this.task.FieldsReported(this.sensor, this.fields.ToArray(), this.status);
			}

			private class JobReadout : ISensorReadout
			{
				private readonly ReadoutWorkItem item;
				private readonly DateTime from;
				private readonly DateTime to;

				public JobReadout(ReadoutWorkItem Item)
				{
					this.item = Item;
					this.from = Item.status.StartTime - this.item.task.From;
					this.to = Item.status.StartTime - this.item.task.To;
				}

				public IThingReference[] Nodes => new IThingReference[] { this.item.sensor };
				public FieldType Types => this.item.task.FieldTypes;
				public string[] FieldNames => this.item.task.FieldNames;
				public DateTime From => this.from;
				public DateTime To => this.to;
				public DateTime When => this.item.status.StartTime;
				public string Actor => this.item.task.NodeId;
				public string ServiceToken => string.Empty;
				public string DeviceToken => string.Empty;
				public string UserToken => string.Empty;

				public bool IsIncluded(string FieldName)
				{
					if ((this.item.task.FieldNames?.Length ?? 0) == 0)
						return true;
					else
						return Array.IndexOf(this.item.task.FieldNames, FieldName) >= 0;
				}

				public bool IsIncluded(DateTime Timestamp)
				{
					return Timestamp.ToUniversalTime() >= this.from && Timestamp <= this.to;
				}

				public bool IsIncluded(FieldType Type)
				{
					return (this.item.task.FieldTypes & Type) != 0;
				}

				public bool IsIncluded(string FieldName, FieldType Type)
				{
					return this.IsIncluded(FieldName) && this.IsIncluded(Type);
				}

				public bool IsIncluded(string FieldName, DateTime Timestamp, FieldType Type)
				{
					return this.IsIncluded(FieldName) && this.IsIncluded(Type) && this.IsIncluded(Timestamp);
				}

				public Task ReportErrors(bool Done, params ThingError[] Errors)
				{
					this.item.errors ??= new ChunkedList<ThingError>();
					this.item.errors.AddRange(Errors);
					return Task.CompletedTask;
				}

				public Task ReportErrors(bool Done, IEnumerable<ThingError> Errors)
				{
					this.item.errors ??= new ChunkedList<ThingError>();
					this.item.errors.AddRange(Errors);
					return Task.CompletedTask;
				}

				public Task ReportFields(bool Done, params Field[] Fields)
				{
					this.item.fields ??= new ChunkedList<Field>();
					this.item.fields.AddRange(Fields);
					return Task.CompletedTask;
				}

				public Task ReportFields(bool Done, IEnumerable<Field> Fields)
				{
					this.item.fields ??= new ChunkedList<Field>();
					this.item.fields.AddRange(Fields);
					return Task.CompletedTask;
				}

				public Task Start()
				{
					return Task.CompletedTask;
				}
			}
		}

	}
}
