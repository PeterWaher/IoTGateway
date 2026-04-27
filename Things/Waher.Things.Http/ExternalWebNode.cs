using System.Collections.Generic;
using System.Threading.Tasks;
using Waher.Runtime.Language;
using Waher.Things.Metering;
using Waher.Things.SensorData;

namespace Waher.Things.Http
{
	/// <summary>
	/// Node representing an external web node.
	/// </summary>
	public class ExternalWebNode : MeteringNode, ISensor
	{
		private static readonly Dictionary<string, KeyValuePair<Field[], ThingError[]>> sensorData = new Dictionary<string, KeyValuePair<Field[], ThingError[]>>();

		/// <summary>
		/// Node representing an external web node.
		/// </summary>
		public ExternalWebNode()
			: base()
		{
		}

		/// <summary>
		/// Type name representing data.
		/// </summary>
		public override Task<string> GetTypeNameAsync(Language Language)
		{
			return Language.GetStringAsync(typeof(LocalWebServerNode), 2, "External Web Node");
		}

		/// <summary>
		/// TODO
		/// </summary>
		public override Task<bool> AcceptsChildAsync(INode Child)
		{
			return Task.FromResult(Child is ExternalWebNode);
		}

		/// <summary>
		/// If the node accepts a presumptive parent, i.e. can be added to that parent (if that parent accepts the node as a child).
		/// </summary>
		/// <param name="Parent">Presumptive parent node.</param>
		/// <returns>If the parent is acceptable.</returns>
		public override Task<bool> AcceptsParentAsync(INode Parent)
		{
			return Task.FromResult(Parent is ExternalWebNode || Parent is LocalWebServerNode);
		}

		/// <summary>
		/// Is called when new sensor data have been posted to the node.
		/// </summary>
		/// <param name="Fields">New fields.</param>
		/// <param name="Errors">New errors.</param>
		internal async Task NewSensorData(Field[] Fields, ThingError[] Errors)
		{
			bool PrevError;

			lock (sensorData)
			{
				PrevError = sensorData.TryGetValue(this.NodeId, out KeyValuePair<Field[], ThingError[]> P)
					&& (P.Value?.Length ?? 0) > 0;

				sensorData[this.NodeId] = new KeyValuePair<Field[], ThingError[]>(Fields, Errors);
			}

			if (!(Fields is null))
			{
				List<Field> MomentaryValues = null;

				foreach (Field Field in Fields)
				{
					if (Field.Type.HasFlag(FieldType.Momentary))
					{
						MomentaryValues ??= new List<Field>();
						MomentaryValues.Add(Field);
					}
				}

				if (!(MomentaryValues is null))
					this.NewMomentaryValues(MomentaryValues.ToArray());
			}

			if (Errors is null)
			{
				if (PrevError)
					await this.RemoveErrorAsync();
			}
			else
			{
				foreach (ThingError Error in Errors)
					await this.LogErrorAsync(Error.ErrorMessage);
			}
		}

		/// <summary>
		/// Starts the readout of the sensor.
		/// </summary>
		/// <param name="Request">Request object. All fields and errors should be reported to this interface.</param>
		public async Task StartReadout(ISensorReadout Request)
		{
			KeyValuePair<Field[], ThingError[]> Data;
			bool Found;

			lock (sensorData)
			{
				Found = sensorData.TryGetValue(this.NodeId, out Data);
			}

			if (Found)
			{
				if (!(Data.Key is null))
					await Request.ReportFields(Data.Value is null, Data.Key);

				if (!(Data.Value is null))
					await Request.ReportErrors(true, Data.Value);

				if (Data.Key is null && Data.Value is null)
					await Request.ReportErrors(true, new ThingError(this, "No Sensor Data reported."));
			}
			else
				await Request.ReportErrors(true, new ThingError(this, "No Sensor Data reported."));
		}
	}
}
