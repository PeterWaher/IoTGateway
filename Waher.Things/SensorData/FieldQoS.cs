using System;
using System.Collections.Generic;
using System.Text;

namespace Waher.Things.SensorData
{
	/// <summary>
	/// Field Quality of Service flags
	/// </summary>
	[Flags]
	public enum FieldQoS
	{
		/// <summary>
		/// Value is missing
		/// </summary>
		Missing,

		/// <summary>
		/// Value is in progress to be measured or calculated. The value is to be considered as unsure and not final. 
		/// Read again later to retrieve the correct value. It is more reliable than a missing value, but less reliable than an estimate.
		/// </summary>
		InProgress,

		/// <summary>
		/// An estimate of the value has been done automatically. Considered more reliable than a value in progress.
		/// </summary>
        AutomaticEstimate,

		/// <summary>
		/// The value has manually been estimated. Considered more reliable than an automatic estimate.
		/// </summary>
        ManualEstimate,

		/// <summary>
		/// Value has been manually read. Considered more reliable than a manual estimate.
		/// </summary>
        ManualReadout,

		/// <summary>
		/// Value has been automatically read. Considered more reliable than a manually read value.
		/// </summary>
        AutomaticReadout,

		/// <summary>
		/// The time was offset more than allowed and corrected during the measurement period.
		/// </summary>
        TimeOffset,

		/// <summary>
		/// A warning was logged during the measurement period.
		/// </summary>
        Warning,

		/// <summary>
		/// An error was logged during the measurement period.
		/// </summary>
        Error,

		/// <summary>
		/// The value has been signed by an operator. Considered more reliable than an automatically read value. Note that the signed quality 
		/// of service flag can be used to overwrite existing values of higher importance. Example signed + invoiced can be considered more 
		/// reliable than only invoiced, etc. 
		/// </summary>
        Signed,

		/// <summary>
		/// The value has been invoiced by an operator. Considered more reliable than a signed value.
		/// </summary>
        Invoiced,

		/// <summary>
		/// The value has been marked as an end point in a series. This can be used for instance to mark the change of tenant in an apartment.
		/// </summary>
        EndOfSeries,

		/// <summary>
		/// The device recorded a power failure during the measurement period.
		/// </summary>
        PowerFailure,

		/// <summary>
		/// The value has been invoiced by an operator and confirmed by the recipient. Considered more reliable than an invoiced value.
		/// </summary>
        InvoiceConfirmed
	}
}
