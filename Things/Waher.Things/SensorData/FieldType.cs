using System;
using System.Collections.Generic;
using System.Text;

namespace Waher.Things.SensorData
{
    /// <summary>
    /// Field Type flags
    /// </summary>
    [Flags]
    public enum FieldType
    {
        /// <summary>
        /// A momentary value represents a value measured at the time of the read-out. Examples: Energy, Volume, Power, Flow, Temperature, Pressure, etc.
        /// </summary>
        Momentary = 1,

        /// <summary>
        /// A value that can be used for identification. (Serial numbers, meter IDs, locations, names, addresses, etc.)
        /// </summary>
        Identity = 2,

        /// <summary>
        /// A value displaying status information about something. Examples: Health, Battery life time, Runtime, Expected life time, Signal strength, 
        /// Signal quality, etc. 
        /// </summary>
        Status = 4,

        /// <summary>
        /// A value that is computed instead of measured. 
        /// </summary>
        Computed = 8,

        /// <summary>
        /// A maximum or minimum value during a given period. Examples "Temperature, Max", "Temperature, Min", etc.
        /// </summary>
        Peak = 16,

        /// <summary>
        /// A historical value.
        /// </summary>
        Historical = 32,

        /// <summary>
        /// All types, except historical values.
        /// </summary>
        AllExceptHistorical = 1 + 2 + 4 + 8 + 16,

        /// <summary>
        /// All field types.
        /// </summary>
        All = 1 + 2 + 4 + 8 + 16 + 32
    }
}
