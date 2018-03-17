using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Waher.Things;
using Waher.Runtime.Language;
using Waher.Things.SensorData;

namespace Waher.Things
{
	/// <summary>
	/// Interface for classes managing sensor data readouts.
	/// </summary>
	public interface ISensorReadout
	{
		/// <summary>
		/// Array of nodes to read. Can be null or empty, if reading a sensor that is not a concentrator.
		/// </summary>
		IThingReference[] Nodes
		{
			get;
		}

		/// <summary>
		/// Field Types to read.
		/// </summary>
		FieldType Types
		{
			get;
		}

		/// <summary>
		/// Names of fields to read.
		/// </summary>
		string[] FieldNames
		{
			get;
		}

		/// <summary>
		/// From what time readout is to be made. Use <see cref="DateTime.MinValue"/> to specify no lower limit.
		/// </summary>
		DateTime From
		{
			get;
		}

		/// <summary>
		/// To what time readout is to be made. Use <see cref="DateTime.MaxValue"/> to specify no upper limit.
		/// </summary>
		DateTime To
		{
			get;
		}

		/// <summary>
		/// When the readout is to be made. Use <see cref="DateTime.MinValue"/> to start the readout immediately.
		/// </summary>
		DateTime When
		{
			get;
		}

		/// <summary>
		/// Actor making the request.
		/// </summary>
		string Actor
		{
			get;
		}

		/// <summary>
		/// Optional service token.
		/// </summary>
		string ServiceToken
		{
			get;
		}

		/// <summary>
		/// Optional device token.
		/// </summary>
		string DeviceToken
		{
			get;
		}

		/// <summary>
		/// Optional user token.
		/// </summary>
		string UserToken
		{
			get;
		}

		/// <summary>
		/// Checks if a field with the given parameters is included in the readout.
		/// </summary>
		/// <param name="FieldName">Unlocalized name of field.</param>
		/// <returns>If the corresponding field is included.</returns>
		bool IsIncluded(string FieldName);

		/// <summary>
		/// Checks if a field with the given parameters is included in the readout.
		/// </summary>
		/// <param name="Timestamp">Timestamp of field.</param>
		/// <returns>If the corresponding field is included.</returns>
		bool IsIncluded(DateTime Timestamp);

		/// <summary>
		/// Checks if a field with the given parameters is included in the readout.
		/// </summary>
		/// <param name="Type">Field Types</param>
		/// <returns>If the corresponding field is included.</returns>
		bool IsIncluded(FieldType Type);

		/// <summary>
		/// Checks if a field with the given parameters is included in the readout.
		/// </summary>
		/// <param name="FieldName">Unlocalized name of field.</param>
		/// <param name="Type">Field Types</param>
		/// <returns>If the corresponding field is included.</returns>
		bool IsIncluded(string FieldName, FieldType Type);

		/// <summary>
		/// Checks if a field with the given parameters is included in the readout.
		/// </summary>
		/// <param name="FieldName">Unlocalized name of field.</param>
		/// <param name="Timestamp">Timestamp of field.</param>
		/// <param name="Type">Field Types</param>
		/// <returns>If the corresponding field is included.</returns>
		bool IsIncluded(string FieldName, DateTime Timestamp, FieldType Type);

		/// <summary>
		/// Report that readout has started. Can optionally be used to report feedback to end-user when readout is slow.
		/// </summary>
		void Start();

		/// <summary>
		/// Report read fields to the client.
		/// </summary>
		/// <param name="Done">If the readout is complete (true) or if more data will be reported (false).</param>
		/// <param name="Fields">Fields that have been read.</param>
		void ReportFields(bool Done, params Field[] Fields);

		/// <summary>
		/// Report read fields to the client.
		/// </summary>
		/// <param name="Done">If the readout is complete (true) or if more data will be reported (false).</param>
		/// <param name="Fields">Fields that have been read.</param>
		void ReportFields(bool Done, IEnumerable<Field> Fields);

		/// <summary>
		/// Report error states to the client.
		/// </summary>
		/// <param name="Done">If the readout is complete (true) or if more data will be reported (false).</param>
		/// <param name="Errors">Errors that have been detected.</param>
		void ReportErrors(bool Done, params ThingError[] Errors);
	}
}
