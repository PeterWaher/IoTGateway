using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Waher.Networking.XMPP;
using Waher.Networking.XMPP.Sensor;
using Waher.Runtime.Language;
using Waher.Things.SensorData;

namespace Waher.Things.Xmpp
{
	/// <summary>
	/// A connected standalone sensor.
	/// </summary>
	public class SensorDevice : ConnectedDevice, ISensor
	{
		/// <summary>
		/// A connected standalone sensor.
		/// </summary>
		public SensorDevice()
			: base()
		{
		}

		/// <summary>
		/// Gets the type name of the node.
		/// </summary>
		/// <param name="Language">Language to use.</param>
		/// <returns>Localized type node.</returns>
		public override Task<string> GetTypeNameAsync(Language Language)
		{
			return Language.GetStringAsync(typeof(ConcentratorDevice), 58, "XMPP Sensor Device");
		}

		/// <summary>
		/// Starts the readout of the sensor.
		/// </summary>
		/// <param name="Request">Request object. All fields and errors should be reported to this interface.</param>
		public async Task StartReadout(ISensorReadout Request)
		{
			XmppClient Client;

			try
			{
				Client = await this.GetClient();
			}
			catch (Exception ex)
			{
				Request.ReportErrors(true, new ThingError(this, ex.Message));
				return;
			}

			if (Client.TryGetExtension(out SensorClient SensorClient))
			{
				RosterItem Item = Client.GetRosterItem(this.JID);
				if (Item is null)
				{
					Request.ReportErrors(true, new ThingError(this, "JID not available in roster."));
					return;
				}

				if (!Item.HasLastPresence || !Item.LastPresence.IsOnline)
				{
					Request.ReportErrors(true, new ThingError(this, "Device not online."));
					return;
				}

				TaskCompletionSource<bool> Done = new TaskCompletionSource<bool>();
				SensorDataClientRequest Request2 = SensorClient.RequestReadout(Item.LastPresenceFullJid,
					new IThingReference[] { ThingReference.Empty }, Request.Types, Request.FieldNames,
					Request.From, Request.To, Request.When, Request.ServiceToken, Request.DeviceToken, Request.UserToken);

				Request2.OnFieldsReceived += (sender, Fields) =>
				{
					foreach (Field F in Fields)
						F.Thing = this;

					Request.ReportFields(false, Fields);
					return Task.CompletedTask;
				};

				Request2.OnErrorsReceived += (sender, Errors) =>
				{
					List<ThingError> Errors2 = new List<ThingError>();

					foreach (ThingError E in Errors)
						Errors2.Add(new ThingError(this, E.ErrorMessage));

					Request.ReportErrors(false, Errors2.ToArray());
					return Task.CompletedTask;
				};

				Request2.OnStateChanged += (sender, State) =>
				{
					switch (State)
					{
						case SensorDataReadoutState.Cancelled:
							Request.ReportErrors(true, new ThingError(this, "Readout was cancelled."));
							Done.TrySetResult(false);
							break;

						case SensorDataReadoutState.Done:
							Request.ReportFields(true);
							Done.TrySetResult(true);
							break;

						case SensorDataReadoutState.Failure:
							Request.ReportErrors(true, new ThingError(this, "Readout failed."));
							Done.TrySetResult(false);
							break;
					}

					return Task.CompletedTask;
				};
			}
			else
				Request.ReportErrors(true, new ThingError(this, "No XMPP Sensor Client available."));
		}
	}
}
