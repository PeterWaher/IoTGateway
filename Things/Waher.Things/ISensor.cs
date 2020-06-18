using System;
using System.Threading.Tasks;

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
		Task StartReadout(ISensorReadout Request);
	}
}
