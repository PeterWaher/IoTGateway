using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Waher.Networking.XMPP;
using Waher.Networking.XMPP.Sensor;
using Waher.Runtime.Language;
using Waher.Things.Metering;
using Waher.Things.SensorData;

namespace Waher.Things.Xmpp
{
	/// <summary>
	/// A node in a concentrator.
	/// </summary>
	public class SensorNode : ConcentratorNode, ISensor
	{
		/// <summary>
		/// A node in a concentrator.
		/// </summary>
		public SensorNode()
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
			return Language.GetStringAsync(typeof(ConcentratorDevice), 12, "Sensor Node");
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
				await Request.ReportErrors(true, new ThingError(this, ex.Message));
				return;
			}

			if (Client.TryGetExtension(out SensorClient SensorClient))
			{
				string JID = string.Empty;
				string NID = this.RemoteNodeID;
				string SID = string.Empty;
				string PID = string.Empty;

				INode Loop = await this.GetParent();
				while (!(Loop is null))
				{
					if (Loop is ConcentratorSourceNode SourceNode)
						SID = SourceNode.RemoteSourceID;
					else if (Loop is ConcentratorPartitionNode PartitionNode)
						PID = PartitionNode.RemotePartitionID;
					else if (Loop is ConcentratorDevice ConcentratorDevice)
					{
						JID = ConcentratorDevice.JID;
						break;
					}

					if (Loop is MeteringNode MeteringNode)
						Loop = await MeteringNode.GetParent();
					else
						Loop = Loop.Parent;
				}

				RosterItem Item = Client.GetRosterItem(JID);
				if (Item is null)
				{
					await Request.ReportErrors(true, new ThingError(this, "JID not available in roster."));
					return;
				}

				if (!Item.HasLastPresence || !Item.LastPresence.IsOnline)
				{
					await Request.ReportErrors(true, new ThingError(this, "Concentrator not online."));
					return;
				}

				TaskCompletionSource<bool> Done = new TaskCompletionSource<bool>();
				SensorDataClientRequest Request2 = await SensorClient.RequestReadout(Item.LastPresenceFullJid,
					new IThingReference[] { new ThingReference(NID, SID, PID) }, Request.Types, Request.FieldNames,
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
				await Request.ReportErrors(true, new ThingError(this, "No XMPP Sensor Client available."));
		}
	}
}
