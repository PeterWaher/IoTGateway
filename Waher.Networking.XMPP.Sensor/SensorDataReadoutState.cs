using System;
using System.Collections.Generic;
using System.Text;
using Waher.Things;
using Waher.Things.SensorData;

namespace Waher.Networking.XMPP.Sensor
{
	/// <summary>
	/// Sensor Data Readout States.
	/// </summary>
	public enum SensorDataReadoutState
	{
		/// <summary>
		/// Sensor data has been requested.
		/// </summary>
		Requested,

		/// <summary>
		/// Sensor data request has been accepted. The <see cref="SensorDataClientRequest.Queued"/> tells if the request has been queued.
		/// </summary>
		Accepted,

		/// <summary>
		/// Sensor data readout has been started.
		/// </summary>
		Started,

		/// <summary>
		/// Sensor data is being received.
		/// </summary>
		Receiving,

		/// <summary>
		/// Sensor data readout is complete.
		/// </summary>
		Done,

		/// <summary>
		/// Sensor data readout has been cancelled.
		/// </summary>
		Cancelled,

		/// <summary>
		/// Sensor data readout failed. If the failure is partial, the request object will contain the partially read data.
		/// The request object will also contain detailed information about what has failed.
		/// </summary>
		Failure
	}
}
