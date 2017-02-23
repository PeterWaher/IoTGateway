using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Waher.Things;
using Waher.Runtime.Language;
using Waher.Things.ControlParameters;

namespace Waher.Things
{
	/// <summary>
	/// Interface for actuator nodes.
	/// </summary>
	public interface IActuator : INode 
	{
		/// <summary>
		/// Get control parameters for the actuator.
		/// </summary>
		/// <returns>Collection of control parameters for actuator.</returns>
		ControlParameter[] GetControlParameters();
	}
}
