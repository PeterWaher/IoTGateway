using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Waher.Content;
using Waher.Groups.Metering.NodeTypes;
using Waher.Jobs.NodeTypes;
using Waher.Output.Metering;
using Waher.Processors.Metering;
using Waher.Runtime.Collections;
using Waher.Runtime.Language;
using Waher.Runtime.Queue;
using Waher.Script;
using Waher.Script.Functions.Runtime;
using Waher.Things;
using Waher.Things.Attributes;
using Waher.Things.Metering;
using Waher.Things.SensorData;

namespace Waher.Jobs.Metering.NodeTypes
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
		/// Maximum number of parallel readouts.
		/// </summary>
		[Header(2, "Parallel readouts:", 0)]
		[Page(1, "Readout", 100)]
		[ToolTip(3, "Maximum number of parallel readouts.")]
		[Required]
		[Range(1, 100)]
		[DefaultValue(1)]
		public int ParallelReadouts { get; set; } = 1;

		/// <summary>
		/// If momentary values should be read.
		/// </summary>
		[Header(4, "Momentary values.", 0)]
		[Page(1, "Readout", 100)]
		[ToolTip(5, "Check, if momentary values should be read.")]
		[DefaultValue(true)]
		public bool Momentary { get; set; } = true;

		/// <summary>
		/// If identity values should be read.
		/// </summary>
		[Header(6, "Identity values.", 0)]
		[Page(1, "Readout", 100)]
		[ToolTip(7, "Check, if identity values should be read.")]
		[DefaultValue(false)]
		public bool Identity { get; set; } = false;

		/// <summary>
		/// If status values should be read.
		/// </summary>
		[Header(8, "Status values.", 0)]
		[Page(1, "Readout", 100)]
		[ToolTip(9, "Check, if status values should be read.")]
		[DefaultValue(false)]
		public bool Status { get; set; } = false;

		/// <summary>
		/// If computed values should be read.
		/// </summary>
		[Header(10, "Computed values.", 0)]
		[Page(1, "Readout", 100)]
		[ToolTip(11, "Check, if computed values should be read.")]
		[DefaultValue(false)]
		public bool Computed { get; set; } = false;

		/// <summary>
		/// If peak values should be read.
		/// </summary>
		[Header(12, "Peak values.", 0)]
		[Page(1, "Readout", 100)]
		[ToolTip(13, "Check, if peak values should be read.")]
		[DefaultValue(false)]
		public bool Peak { get; set; } = false;

		/// <summary>
		/// If historical values should be read.
		/// </summary>
		[Header(14, "Historical values.", 0)]
		[Page(1, "Readout", 100)]
		[ToolTip(15, "Check, if historical values should be read.")]
		[DefaultValue(false)]
		public bool Historical { get; set; } = false;

		/// <summary>
		/// Types of fields to read.
		/// </summary>
		public FieldType FieldTypes
		{
			get
			{
				FieldType Result = 0;

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
		/// Field names to read.
		/// </summary>
		[Header(16, "Field names to read:", 0)]
		[Page(1, "Readout", 100)]
		[ToolTip(17, "Leave blank to read all fields.")]
		[ContentType("text/plain")]
		public string[] FieldNames { get; set; }

		/// <summary>
		/// From when data should be read.
		/// </summary>
		[Header(18, "From:", 0)]
		[Page(1, "Readout", 100)]
		[ToolTip(19, "Read historical data from this point in time.")]
		public Duration From { get; set; } = Duration.Zero;

		/// <summary>
		/// To when data should be read.
		/// </summary>
		[Header(20, "To:", 0)]
		[Page(1, "Readout", 100)]
		[ToolTip(21, "Read historical data to this point in time.")]
		public Duration To { get; set; } = Duration.Zero;

		/// <summary>
		/// Gets the type name of the node.
		/// </summary>
		/// <param name="Language">Language to use.</param>
		/// <returns>Localized type node.</returns>
		public override Task<string> GetTypeNameAsync(Language Language)
		{
			return Language.GetStringAsync(typeof(SensorDataReadoutTaskNode), 22, "Sensor Data Readout Task");
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
				Child is MeteringGroupReference);
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

			ISensorDataProcessor[] Processors = await this.FindNodes<ISensorDataProcessor>();
			ISensorDataOutput[] Outputs = await this.FindNodes<ISensorDataOutput>();

			using AsyncProcessor<ReadoutWorkItem> Processor = new AsyncProcessor<ReadoutWorkItem>(this.ParallelReadouts);

			foreach (IMeteringNode Node in Nodes)
			{
				if (Node.IsReadable && Node is ISensor Sensor)
					Processor.Queue(new ReadoutWorkItem(Sensor, this, Status, Processors, Outputs));
			}

			await Processor.WaitUntilIdle();

			if (Status.Variables.TryGetVariable("Errors", out Variable v) &&
				v.ValueObject is ChunkedList<ThingError> JobErrors &&
				JobErrors.Count > 0)
			{
				await Status.Job.LogErrorAsync("ReadoutErrors", JobErrors.Count.ToString() + " errors reported during readout.");
			}
			else
				await Status.Job.RemoveErrorAsync("ReadoutErrors");
		}

		private async Task FieldsReported(ISensor Sensor, Field[] Fields,
			ThingError[] Errors, JobExecutionStatus Status,
			ISensorDataProcessor[] Processors, ISensorDataOutput[] Outputs)
		{
			if (!(Fields is null))
			{
				if ((Processors?.Length ?? 0) > 0)
				{
					foreach (ISensorDataProcessor Processor in Processors)
					{
						try
						{
							Fields = await Processor.ProcessFields(Sensor, Fields);
							if ((Fields?.Length ?? 0) == 0)
								return;
						}
						catch (Exception ex)
						{
							await Processor.LogErrorAsync("ProcessingError", ex.Message);
						}
					}
				}

				if ((Outputs?.Length ?? 0) > 0)
				{
					foreach (ISensorDataOutput Output in Outputs)
					{
						try
						{
							await Output.OutputFields(Sensor, Fields);
						}
						catch (Exception ex)
						{
							await Output.LogErrorAsync("OutputError", ex.Message);
						}
					}
				}
			}

			if (!(Errors is null))
			{
				if (!Status.Variables.TryGetVariable("Errors", out Variable v))
					Status.Variables["Errors"] = new ChunkedList<ThingError>(Errors);
				else if (v.ValueObject is ChunkedList<ThingError> JobErrors)
					JobErrors.AddRange(Errors);
			}
		}

		private class ReadoutWorkItem : WorkItem
		{
			private readonly ISensor sensor;
			private readonly SensorDataReadoutTaskNode task;
			private readonly JobExecutionStatus status;
			private readonly ISensorDataProcessor[] processors;
			private readonly ISensorDataOutput[] outputs;
			private ChunkedList<Field> fields = null;
			private ChunkedList<ThingError> errors = null;

			public ReadoutWorkItem(ISensor Sensor, SensorDataReadoutTaskNode TaskNode,
				JobExecutionStatus Status, ISensorDataProcessor[] Processors,
				ISensorDataOutput[] Outputs)
			{
				this.sensor = Sensor;
				this.task = TaskNode;
				this.status = Status;
				this.processors = Processors;
				this.outputs = Outputs;
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

				await this.task.FieldsReported(this.sensor, this.fields?.ToArray(),
					this.errors?.ToArray(), this.status, this.processors, this.outputs);
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
