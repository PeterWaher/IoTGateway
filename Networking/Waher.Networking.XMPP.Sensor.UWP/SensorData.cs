using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;
using Waher.Content.Xml;
using Waher.Networking.XMPP.PEP;
using Waher.Things;
using Waher.Things.SensorData;

namespace Waher.Networking.XMPP.Sensor
{
	/// <summary>
	/// Contains personal sensor data.
	/// </summary>
	public class SensorData : PersonalEvent
	{
		private IEnumerable<Field> fields = null;
		private IEnumerable<ThingError> errors = null;
		private string payloadXml = null;
		private string id = string.Empty;
		private bool done = true;

		/// <summary>
		/// Contains personal sensor data.
		/// </summary>
		public SensorData()
		{
		}

		/// <summary>
		/// Contains personal sensor data.
		/// </summary>
		/// <param name="Fields">Sensor data fields.</param>
		public SensorData(IEnumerable<Field> Fields)
		{
			this.fields = Fields;
			this.errors = null;
		}

		/// <summary>
		/// Contains personal sensor data.
		/// </summary>
		/// <param name="Fields">Sensor data fields.</param>
		public SensorData(params Field[] Fields)
		{
			this.fields = Fields;
			this.errors = null;
		}

		/// <summary>
		/// Contains personal sensor data.
		/// </summary>
		/// <param name="Errors">Thing errors.</param>
		public SensorData(IEnumerable<ThingError> Errors)
		{
			this.fields = null;
			this.errors = Errors;
		}

		/// <summary>
		/// Contains personal sensor data.
		/// </summary>
		/// <param name="Errors">Thing errors.</param>
		public SensorData(params ThingError[] Errors)
		{
			this.fields = null;
			this.errors = Errors;
		}

		/// <summary>
		/// Sensor data fields.
		/// </summary>
		public IEnumerable<Field> Fields
		{
			get { return this.fields; }
			set
			{
				this.fields = value;
				this.payloadXml = null;
			}
		}

		/// <summary>
		/// Thing errors.
		/// </summary>
		public IEnumerable<ThingError> Errors
		{
			get { return this.errors; }
			set
			{
				this.errors = value;
				this.payloadXml = null;
			}
		}

		/// <summary>
		/// If sensor data is complete.
		/// </summary>
		public bool Done
		{
			get { return this.done; }
			set
			{
				this.done = value;
				this.payloadXml = null;
			}
		}

		/// <summary>
		/// ID of sensor data readout.
		/// </summary>
		public string Id
		{
			get { return this.id; }
			set
			{
				this.id = value;
				this.payloadXml = null;
			}
		}

		/// <summary>
		/// Local name of the event element.
		/// </summary>
		public override string LocalName => "resp";

		/// <summary>
		/// Namespace of the event element.
		/// </summary>
		public override string Namespace => SensorClient.NamespaceSensorData;

		/// <summary>
		/// XML representation of the event.
		/// </summary>
		public override string PayloadXml
		{
			get
			{
				if (this.payloadXml == null)
				{
					StringBuilder Xml = new StringBuilder();
					XmlWriter w = XmlWriter.Create(Xml, XML.WriterSettings(false, true));
					SensorDataServerRequest.OutputFields(w, this.fields, this.errors, this.id, this.done, null);
					w.Flush();

					this.payloadXml = Xml.ToString();
				}

				return this.payloadXml;
			}
		}

		/// <summary>
		/// Parses a personal event from its XML representation
		/// </summary>
		/// <param name="E">XML representation of personal event.</param>
		/// <returns>Personal event object.</returns>
		public override IPersonalEvent Parse(XmlElement E)
		{
			return SensorClient.ParseFields(E);
		}
	}
}
