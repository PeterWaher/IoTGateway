using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Waher.Networking.XMPP.Sensor;
using Waher.Runtime.Language;
using Waher.Things.ControlParameters;
using Waher.Things.SensorData;

namespace Waher.Things.Modbus
{
	/// <summary>
	/// Represents a composition of nodes, under a unit.
	/// </summary>
	public class ModbusCompositeChildNode : ModbusUnitChildNode, ISensor, IActuator
	{
		/// <summary>
		/// Represents a composition of nodes, under a unit.
		/// </summary>
		public ModbusCompositeChildNode()
			: base()
		{
		}

		/// <summary>
		/// Gets the type name of the node.
		/// </summary>
		/// <param name="Language">Language to use.</param>
		/// <returns>Localized type node.</returns>
		public override Task<string> GetTypeNameAsync(Language Language)
		{
			return Language.GetStringAsync(typeof(ModbusGatewayNode), 28, "Composite Node");
		}

		/// <summary>
		/// If the node accepts a presumptive parent, i.e. can be added to that parent (if that parent accepts the node as a child).
		/// </summary>
		/// <param name="Parent">Presumptive parent node.</param>
		/// <returns>If the parent is acceptable.</returns>
		public override Task<bool> AcceptsParentAsync(INode Parent)
		{
			return Task.FromResult(Parent is ModbusUnitNode);
		}

		/// <summary>
		/// If the node accepts a presumptive child, i.e. can receive as a child (if that child accepts the node as a parent).
		/// </summary>
		/// <param name="Child">Presumptive child node.</param>
		/// <returns>If the child is acceptable.</returns>
		public override Task<bool> AcceptsChildAsync(INode Child)
		{
			return Task.FromResult(Child is ModbusUnitChildNode);
		}

		/// <summary>
		/// Starts the readout of the sensor.
		/// </summary>
		/// <param name="Request">Request object. All fields and errors should be reported to this interface.</param>
		public async Task StartReadout(ISensorReadout Request)
		{
			try
			{
				foreach (INode Child in await this.ChildNodes)
				{
					if (Child is ISensor Sensor)
					{
						TaskCompletionSource<bool> ReadoutCompleted = new TaskCompletionSource<bool>();

						InternalReadoutRequest InternalReadout = new InternalReadoutRequest(this.LogId, null, Request.Types, Request.FieldNames,
							Request.From, Request.To,
							(Sender, e) =>
							{
								foreach (Field F in e.Fields)
									F.Thing = this;

								Request.ReportFields(false, e.Fields);

								if (e.Done)
									ReadoutCompleted.TrySetResult(true);

								return Task.CompletedTask;
							},
							(Sender, e) =>
							{
								List<ThingError> Errors2 = new List<ThingError>();

								foreach (ThingError Error in e.Errors)
									Errors2.Add(new ThingError(this, Error.ErrorMessage));

								Request.ReportErrors(false, Errors2.ToArray());

								if (e.Done)
									ReadoutCompleted.TrySetResult(true);

								return Task.CompletedTask;
							}, null);

						await Sensor.StartReadout(InternalReadout);

						Task Timeout = Task.Delay(60000);

						Task T = await Task.WhenAny(ReadoutCompleted.Task, Timeout);

						if (!ReadoutCompleted.Task.IsCompleted)
							await Request.ReportErrors(false, new ThingError(this, "Timeout."));
					}
				}
			}
			catch (Exception ex)
			{
				await Request.ReportErrors(false, new ThingError(this, ex.Message));
			}
			finally
			{
				await Request.ReportFields(true);
			}
		}

		/// <summary>
		/// Get control parameters for the actuator.
		/// </summary>
		/// <returns>Collection of control parameters for actuator.</returns>
		public async Task<ControlParameter[]> GetControlParameters()
		{
			List<ControlParameter> Result = new List<ControlParameter>();

			foreach (INode Child in await this.ChildNodes)
			{
				if (Child is IActuator Actuator)
					Result.AddRange(await Actuator.GetControlParameters());
			}

			return Result.ToArray();
		}

	}
}
