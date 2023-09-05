using System.Collections.Generic;
using System.Threading.Tasks;
using Waher.Runtime.Language;
using Waher.Things.ControlParameters;
using Waher.Things.Metering;
using Waher.Things.Metering.NodeTypes;
using Waher.Things.Virtual;

namespace Waher.Things.Script
{
	/// <summary>
	/// Node defined by script.
	/// </summary>
	public class ScriptNode : ProvisionedMeteringNode, ISensor, IActuator
	{
		/// <summary>
		/// Node defined by script.
		/// </summary>
		public ScriptNode()
			: base()
		{
		}

		/// <summary>
		/// Script for generating sensor-data.
		/// </summary>
		public string SensorScript { get; set; }

		/// <summary>
		/// Gets the type name of the node.
		/// </summary>
		/// <param name="Language">Language to use.</param>
		/// <returns>Localized type node.</returns>
		public override Task<string> GetTypeNameAsync(Language Language)
		{
			return Language.GetStringAsync(typeof(ScriptNode), 1, "Script Node");
		}

		/// <summary>
		/// If the node accepts a presumptive child, i.e. can receive as a child (if that child accepts the node as a parent).
		/// </summary>
		/// <param name="Child">Presumptive child node.</param>
		/// <returns>If the child is acceptable.</returns>
		public override Task<bool> AcceptsChildAsync(INode Child)
		{
			return Task.FromResult(Child is ScriptNode);
		}

		/// <summary>
		/// If the node accepts a presumptive parent, i.e. can be added to that parent (if that parent accepts the node as a child).
		/// </summary>
		/// <param name="Parent">Presumptive parent node.</param>
		/// <returns>If the parent is acceptable.</returns>
		public override Task<bool> AcceptsParentAsync(INode Parent)
		{
			return Task.FromResult(Parent is Root || Parent is VirtualNode || Parent is ScriptNode);
		}

		/// <summary>
		/// Starts the readout of the sensor.
		/// </summary>
		/// <param name="Request">Request object. All fields and errors should be reported to this interface.</param>
		public Task StartReadout(ISensorReadout Request)
		{
			Request.ReportFields(true);
			return Task.CompletedTask;
		}

		/// <summary>
		/// Get control parameters for the actuator.
		/// </summary>
		/// <returns>Collection of control parameters for actuator.</returns>
		public Task<ControlParameter[]> GetControlParameters()
		{
			return Task.FromResult<ControlParameter[]>(new ControlParameter[0]);
		}
	}
}
