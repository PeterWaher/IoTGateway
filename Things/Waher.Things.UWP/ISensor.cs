using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Waher.Things;
using Waher.Runtime.Language;
using Waher.Things.DisplayableParameters;

namespace Waher.Things
{
	/// <summary>
	/// Interface for sensor nodes.
	/// </summary>
	public interface ISensor : INode 
	{
		/// <summary>
		/// Starts the readout of the sensor.
		/// </summary>
		/// <param name="Request">Request object. All fields and errors should be reported to this interface.</param>
		void StartReadout(ISensorReadout Request);
	}
}
