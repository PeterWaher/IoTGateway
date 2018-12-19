using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;
using Waher.Content;
using Waher.Events;
using Waher.Things;
using Waher.Things.SensorData;

namespace Waher.Networking.XMPP.Sensor
{
	/// <summary>
	/// Manages a sensor data client request.
	/// </summary>
	public class SensorDataSubscriptionRequest : SensorDataClientRequest 
	{
		private FieldSubscriptionRule[] fieldRules;
		private Duration minInterval;
		private Duration maxInterval;
		private Duration maxAge;
		private bool unsubscribed = false;

		/// <summary>
		/// Manages a sensor data client request.
		/// </summary>
		/// <param name="Id">Request identity.</param>
		/// <param name="SensorClient">Sensor client object.</param>
		/// <param name="RemoteJID">JID of the other side of the conversation in the sensor data readout.</param>
		/// <param name="Actor">Actor causing the request to be made.</param>
		/// <param name="Nodes">Array of nodes to read. Can be null or empty, if reading a sensor that is not a concentrator.</param>
		/// <param name="Types">Field Types to read.</param>
		/// <param name="FieldRules">Fields to read.</param>
		/// <param name="MinInterval">Smallest interval for reporting events. Events are not reported more often than this limit.</param>
		/// <param name="MaxInterval">Largest interval for reporting events. Events are not reported less often than this limit.</param>
		/// <param name="MaxAge">Maximum age of historical data.</param>
		/// <param name="ServiceToken">Optional service token.</param>
		/// <param name="DeviceToken">Optional device token.</param>
		/// <param name="UserToken">Optional user token.</param>
		internal SensorDataSubscriptionRequest(string Id, SensorClient SensorClient, string RemoteJID, string Actor, IThingReference[] Nodes, 
			FieldType Types, FieldSubscriptionRule[] FieldRules, Duration MinInterval, Duration MaxInterval, Duration MaxAge, string ServiceToken, 
			string DeviceToken, string UserToken)
			: base(Id, SensorClient, RemoteJID, Actor, Nodes, Types, ExtractFieldNames(FieldRules), 
				  DateTime.MinValue, DateTime.MaxValue, DateTime.MinValue, ServiceToken, DeviceToken, UserToken)
		{
			this.minInterval = MinInterval;
			this.maxInterval = MaxInterval;
			this.maxAge = MaxAge;
			this.fieldRules = FieldRules;
		}

		private static string[] ExtractFieldNames(FieldSubscriptionRule[] FieldRules)
		{
			if (FieldRules is null)
				return null;

			int i, c = FieldRules.Length;
			string[] Result = new string[c];

			for (i = 0; i < c; i++)
				Result[i] = FieldRules[i].FieldName;

			return Result;
		}

		/// <summary>
		/// Smallest interval for reporting events. Events are not reported more often than this limit.
		/// </summary>
		public Duration MinInterval
		{
			get { return this.minInterval; }
		}

		/// <summary>
		/// Largest interval for reporting events. Events are not reported less often than this limit.
		/// </summary>
		public Duration MaxInterval
		{
			get { return this.maxInterval; }
		}

		/// <summary>
		/// Maximum age of historical data.
		/// </summary>
		public Duration MaxAge
		{
			get { return this.maxAge; }
		}

		/// <summary>
		/// Field subscription rules.
		/// </summary>
		public FieldSubscriptionRule[] FieldRules
		{
			get { return this.fieldRules; }
		}

		/// <summary>
		/// Removes the subscription.
		/// </summary>
		public void Unsubscribe()
		{
			this.unsubscribed = true;

			if (this.sensorClient.Client.State == XmppState.Connected)
			{
				StringBuilder Xml = new StringBuilder();

				Xml.Append("<unsubscribe xmlns='");
				Xml.Append(SensorClient.NamespaceSensorEvents);
				Xml.Append("' id='");
				Xml.Append(this.Id);
				Xml.Append("'/>");

				this.sensorClient.Client.SendIqSet(this.RemoteJID, Xml.ToString(), this.UnsubscribeResponse, null);
			}
		}

		private void UnsubscribeResponse(object Sender, IqResultEventArgs e)
		{
			if (!e.Ok)
				this.Fail(e.ErrorText);
		}

		internal override void LogFields(IEnumerable<Field> Fields)
		{
			if (this.unsubscribed)
				this.Unsubscribe();
			else
				base.LogFields(Fields);
		}

		internal override void LogErrors(IEnumerable<ThingError> Errors)
		{
			if (this.unsubscribed)
				this.Unsubscribe();
			else
				base.LogErrors(Errors);
		}

	}
}
