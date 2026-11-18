using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Waher.Things.Matter.Messaging;
using Waher.Things.Matter.Tlv;

namespace Waher.Things.Matter.InteractionModel
{
	/// <summary>
	/// Interaction Model client, performing read, write, invoke and subscribe interactions over a secure session
	/// (Matter Core Specification, chapter 8 and 10).
	/// </summary>
	public class InteractionClient
	{
		/// <summary>
		/// Tag of the InteractionModelRevision field.
		/// </summary>
		public const int InteractionModelRevisionTag = 0xff;

		/// <summary>
		/// Default timeout of timed interactions, in milliseconds.
		/// </summary>
		public const ushort DefaultTimedRequestTimeoutMs = 10000;

		private readonly ExchangeManager manager;
		private readonly SecureSession session;

		/// <summary>
		/// Interaction Model client.
		/// </summary>
		/// <param name="Manager">Exchange manager.</param>
		/// <param name="Session">Secure session.</param>
		public InteractionClient(ExchangeManager Manager, SecureSession Session)
		{
			this.manager = Manager;
			this.session = Session;
		}

		/// <summary>
		/// Secure session.
		/// </summary>
		public SecureSession Session => this.session;

		/// <summary>
		/// Exchange manager.
		/// </summary>
		public ExchangeManager Manager => this.manager;

		/// <summary>
		/// Expected processing time for regular interactions.
		/// </summary>
		public TimeSpan ProcessingTime { get; set; } = TimeSpan.FromSeconds(5);

		#region Read

		/// <summary>
		/// Reads attributes.
		/// </summary>
		/// <param name="Paths">Attribute paths (wildcards allowed).</param>
		/// <returns>Read result.</returns>
		public Task<ReadResult> ReadAttributesAsync(params AttributePath[] Paths)
		{
			return this.ReadAsync(Paths, null, null, true, null);
		}

		/// <summary>
		/// Reads a single attribute.
		/// </summary>
		/// <param name="EndpointId">Endpoint ID.</param>
		/// <param name="ClusterId">Cluster ID.</param>
		/// <param name="AttributeId">Attribute ID.</param>
		/// <returns>Attribute value.</returns>
		/// <exception cref="ImStatusException">If the attribute could not be read.</exception>
		public async Task<TlvElement> ReadAttributeAsync(ushort EndpointId, uint ClusterId, uint AttributeId)
		{
			ReadResult Result = await this.ReadAsync(new AttributePath[] { new AttributePath(EndpointId, ClusterId, AttributeId) },
				null, null, true, null);

			AttributeReport Report = Result.GetReport(EndpointId, ClusterId, AttributeId);

			if (Report is null)
				throw new MatterException("Attribute " + EndpointId.ToString() + "/0x" + ClusterId.ToString("X4") + "/0x" + AttributeId.ToString("X4") + " not reported.");

			if (!Report.HasValue)
				throw new ImStatusException(Report.Status ?? new ImStatus(ImStatusCode.Failure, null), "Unable to read attribute " + Report.Path.ToString());

			return Report.Data;
		}

		/// <summary>
		/// Reads attributes and/or events.
		/// </summary>
		/// <param name="AttributePaths">Attribute paths, or null.</param>
		/// <param name="EventPaths">Event paths, or null.</param>
		/// <param name="EventMin">Minimum event number (event filter), or null.</param>
		/// <param name="FabricFiltered">If fabric-scoped data should be filtered.</param>
		/// <param name="DataVersionFilters">Data version filters, or null.</param>
		/// <returns>Read result.</returns>
		public async Task<ReadResult> ReadAsync(AttributePath[] AttributePaths, EventPath[] EventPaths, ulong? EventMin,
			bool FabricFiltered, DataVersionFilter[] DataVersionFilters)
		{
			TlvWriter w = new TlvWriter();

			w.StartStructure();

			if (!(AttributePaths is null) && AttributePaths.Length > 0)
			{
				w.StartArray(0);
				foreach (AttributePath P in AttributePaths)
					P.Write(w, TlvTag.Anonymous);
				w.EndContainer();
			}

			if (!(EventPaths is null) && EventPaths.Length > 0)
			{
				w.StartArray(1);
				foreach (EventPath P in EventPaths)
					P.Write(w, TlvTag.Anonymous);
				w.EndContainer();

				if (EventMin.HasValue)
				{
					w.StartArray(2);
					w.StartStructure(TlvTag.Anonymous);
					w.WriteUInt(1, EventMin.Value);
					w.EndContainer();
					w.EndContainer();
				}
			}

			w.WriteBoolean(3, FabricFiltered);

			if (!(DataVersionFilters is null) && DataVersionFilters.Length > 0)
			{
				w.StartArray(4);
				foreach (DataVersionFilter F in DataVersionFilters)
					F.Write(w, TlvTag.Anonymous);
				w.EndContainer();
			}

			w.WriteUInt(InteractionModelRevisionTag, SessionParameters.OwnInteractionModelRevision);
			w.EndContainer();

			Exchange Exchange = this.manager.CreateExchange(this.session);

			try
			{
				await Exchange.SendAsync(ProtocolIds.InteractionModel, (byte)InteractionModelOpcode.ReadRequest, w.ToArray());

				ReadResult Result = new ReadResult();
				TimeSpan Timeout = this.manager.GetResponseTimeout(this.session, this.ProcessingTime);

				while (true)
				{
					MatterMessage Response = await Exchange.ReceiveAsync(Timeout);
					CheckStatusResponse(Response);

					if (!Response.Is(ProtocolIds.InteractionModel, (byte)InteractionModelOpcode.ReportData))
						throw new MatterException("Unexpected response to read request: " + Response.ProtocolHeader.ToString());

					ParseReportData(Response.DecodeTlv(), Result, out bool MoreChunks, out bool SuppressResponse);

					if (MoreChunks)
						await SendStatusResponse(Exchange, ImStatusCode.Success);
					else
					{
						if (!SuppressResponse)
							await SendStatusResponse(Exchange, ImStatusCode.Success);

						break;
					}
				}

				return Result;
			}
			finally
			{
				await Exchange.CloseAsync();
			}
		}

		/// <summary>
		/// Parses a ReportDataMessage.
		/// </summary>
		/// <param name="Report">Decoded message.</param>
		/// <param name="Result">Result to add reports to.</param>
		/// <param name="MoreChunks">If more chunks follow.</param>
		/// <param name="SuppressResponse">If response is suppressed.</param>
		public static void ParseReportData(TlvElement Report, ReadResult Result, out bool MoreChunks, out bool SuppressResponse)
		{
			if (Report.TryGet(0, out TlvElement E))
				Result.SubscriptionId = E.AsUInt32();

			if (Report.TryGet(1, out TlvElement Attributes))
			{
				AttributePath Previous = null;

				foreach (TlvElement AttributeReportIB in Attributes.Children)
				{
					if (AttributeReportIB.TryGet(1, out TlvElement AttributeData))
					{
						AttributePath Path = AttributePath.Parse(AttributeData.Required(1), Previous);
						uint? DataVersion = AttributeData[0]?.AsUInt32();
						TlvElement Data = AttributeData.Required(2);

						Result.Add(new AttributeReport(Path, DataVersion, Data, null));
						Previous = Path;
					}
					else if (AttributeReportIB.TryGet(0, out TlvElement AttributeStatus))
					{
						AttributePath Path = AttributePath.Parse(AttributeStatus.Required(0), Previous);
						ImStatus Status = ImStatus.Parse(AttributeStatus.Required(1));

						Result.Add(new AttributeReport(Path, null, null, Status));
						Previous = Path;
					}
				}
			}

			if (Report.TryGet(2, out TlvElement Events))
			{
				foreach (TlvElement EventReportIB in Events.Children)
				{
					if (EventReportIB.TryGet(1, out TlvElement EventData))
					{
						EventPath Path = EventPath.Parse(EventData.Required(0));
						ulong EventNumber = EventData.Required(1).AsUInt64();
						EventPriority Priority = (EventPriority)(EventData[2]?.AsByte() ?? 0);
						DateTime? Timestamp = null;
						ulong? SystemTimestamp = null;

						if (EventData.TryGet(3, out TlvElement T))
							Timestamp = DateTimeOffset.FromUnixTimeMilliseconds((long)T.AsUInt64()).UtcDateTime;

						if (EventData.TryGet(4, out T))
							SystemTimestamp = T.AsUInt64();

						Result.Add(new EventReport(Path, EventNumber, Priority, Timestamp, SystemTimestamp, EventData[7], null));
					}
					else if (EventReportIB.TryGet(0, out TlvElement EventStatus))
					{
						EventPath Path = EventPath.Parse(EventStatus.Required(0));
						ImStatus Status = ImStatus.Parse(EventStatus.Required(1));

						Result.Add(new EventReport(Path, 0, EventPriority.Debug, null, null, null, Status));
					}
				}
			}

			MoreChunks = Report[3]?.AsBoolean() ?? false;
			SuppressResponse = Report[4]?.AsBoolean() ?? false;
		}

		#endregion

		#region Write

		/// <summary>
		/// Writes a single attribute.
		/// </summary>
		/// <param name="EndpointId">Endpoint ID.</param>
		/// <param name="ClusterId">Cluster ID.</param>
		/// <param name="AttributeId">Attribute ID.</param>
		/// <param name="Value">Value (TLV element or .NET value).</param>
		/// <param name="Timed">If a timed interaction is to be used.</param>
		/// <exception cref="ImStatusException">If the write fails.</exception>
		public async Task WriteAttributeAsync(ushort EndpointId, uint ClusterId, uint AttributeId, object Value, bool Timed = false)
		{
			WriteResult[] Results = await this.WriteAsync(Timed, new AttributeWrite(EndpointId, ClusterId, AttributeId, Value));

			foreach (WriteResult Result in Results)
			{
				if (!Result.Status.IsSuccess)
					throw new ImStatusException(Result.Status, "Unable to write attribute " + Result.Path.ToString());
			}
		}

		/// <summary>
		/// Writes attributes.
		/// </summary>
		/// <param name="Timed">If a timed interaction is to be used.</param>
		/// <param name="Writes">Attribute writes.</param>
		/// <returns>Write results.</returns>
		public async Task<WriteResult[]> WriteAsync(bool Timed, params AttributeWrite[] Writes)
		{
			TlvWriter w = new TlvWriter();

			w.StartStructure();
			w.WriteBoolean(0, false);
			w.WriteBoolean(1, Timed);
			w.StartArray(2);

			foreach (AttributeWrite Write in Writes)
			{
				w.StartStructure(TlvTag.Anonymous);

				if (Write.DataVersion.HasValue)
					w.WriteUInt(0, Write.DataVersion.Value);

				Write.Path.Write(w, 1);
				w.WriteObject(2, Write.Value);
				w.EndContainer();
			}

			w.EndContainer();
			w.WriteUInt(InteractionModelRevisionTag, SessionParameters.OwnInteractionModelRevision);
			w.EndContainer();

			Exchange Exchange = this.manager.CreateExchange(this.session);

			try
			{
				if (Timed)
					await this.SendTimedRequest(Exchange, DefaultTimedRequestTimeoutMs);

				MatterMessage Response = await Exchange.RequestAsync(ProtocolIds.InteractionModel, (byte)InteractionModelOpcode.WriteRequest,
					w.ToArray(), this.ProcessingTime);

				CheckStatusResponse(Response);

				if (!Response.Is(ProtocolIds.InteractionModel, (byte)InteractionModelOpcode.WriteResponse))
					throw new MatterException("Unexpected response to write request: " + Response.ProtocolHeader.ToString());

				List<WriteResult> Results = new List<WriteResult>();
				TlvElement Resp = Response.DecodeTlv();
				AttributePath Previous = null;

				if (Resp.TryGet(0, out TlvElement Statuses))
				{
					foreach (TlvElement AttributeStatusIB in Statuses.Children)
					{
						AttributePath Path = AttributePath.Parse(AttributeStatusIB.Required(0), Previous);
						ImStatus Status = ImStatus.Parse(AttributeStatusIB.Required(1));
						Results.Add(new WriteResult(Path, Status));
						Previous = Path;
					}
				}

				return Results.ToArray();
			}
			finally
			{
				await Exchange.CloseAsync();
			}
		}

		#endregion

		#region Invoke

		/// <summary>
		/// Invokes a command.
		/// </summary>
		/// <param name="EndpointId">Endpoint ID.</param>
		/// <param name="ClusterId">Cluster ID.</param>
		/// <param name="CommandId">Command ID.</param>
		/// <param name="Fields">Command fields (TLV structure), or null if command has no fields.</param>
		/// <param name="Timed">If a timed interaction is to be used.</param>
		/// <param name="ProcessingTime">Expected processing time, or null for default.</param>
		/// <returns>Invoke result.</returns>
		public async Task<InvokeResult> InvokeAsync(ushort EndpointId, uint ClusterId, uint CommandId, TlvElement Fields,
			bool Timed = false, TimeSpan? ProcessingTime = null)
		{
			CommandPath Path = new CommandPath(EndpointId, ClusterId, CommandId);
			TlvWriter w = new TlvWriter();

			w.StartStructure();
			w.WriteBoolean(0, false);
			w.WriteBoolean(1, Timed);
			w.StartArray(2);
			w.StartStructure(TlvTag.Anonymous);
			Path.Write(w, 0);

			if (Fields is null)
			{
				w.StartStructure(1);
				w.EndContainer();
			}
			else
			{
				TlvTag Prev = Fields.Tag;
				Fields.Tag = 1;
				try
				{
					Fields.Encode(w);
				}
				finally
				{
					Fields.Tag = Prev;
				}
			}

			w.EndContainer();
			w.EndContainer();
			w.WriteUInt(InteractionModelRevisionTag, SessionParameters.OwnInteractionModelRevision);
			w.EndContainer();

			Exchange Exchange = this.manager.CreateExchange(this.session);

			try
			{
				if (Timed)
					await this.SendTimedRequest(Exchange, DefaultTimedRequestTimeoutMs);

				MatterMessage Response = await Exchange.RequestAsync(ProtocolIds.InteractionModel, (byte)InteractionModelOpcode.InvokeRequest,
					w.ToArray(), ProcessingTime ?? this.ProcessingTime);

				InvokeResult Result = null;

				while (true)
				{
					CheckStatusResponse(Response);

					if (!Response.Is(ProtocolIds.InteractionModel, (byte)InteractionModelOpcode.InvokeResponse))
						throw new MatterException("Unexpected response to invoke request: " + Response.ProtocolHeader.ToString());

					TlvElement Resp = Response.DecodeTlv();

					if (Resp.TryGet(1, out TlvElement Responses))
					{
						foreach (TlvElement InvokeResponseIB in Responses.Children)
						{
							if (InvokeResponseIB.TryGet(0, out TlvElement CommandData))
							{
								Result = new InvokeResult(CommandPath.Parse(CommandData.Required(0)), CommandData[1],
									null, CommandData[2]?.AsUInt16());
							}
							else if (InvokeResponseIB.TryGet(1, out TlvElement CommandStatus))
							{
								Result = new InvokeResult(CommandPath.Parse(CommandStatus.Required(0)), null,
									ImStatus.Parse(CommandStatus.Required(1)), CommandStatus[2]?.AsUInt16());
							}
						}
					}

					bool MoreChunks = Resp[2]?.AsBoolean() ?? false;
					if (!MoreChunks)
						break;

					await SendStatusResponse(Exchange, ImStatusCode.Success);
					Response = await Exchange.ReceiveAsync(this.manager.GetResponseTimeout(this.session, this.ProcessingTime));
				}

				if (Result is null)
					throw new MatterException("No response to invoked command.");

				return Result;
			}
			finally
			{
				await Exchange.CloseAsync();
			}
		}

		#endregion

		#region Subscribe

		/// <summary>
		/// Subscribes to attributes and/or events. The priming report is returned in the subscription object.
		/// </summary>
		/// <param name="Subscriptions">Subscription manager, used to route subsequent reports.</param>
		/// <param name="AttributePaths">Attribute paths, or null.</param>
		/// <param name="EventPaths">Event paths, or null.</param>
		/// <param name="MinIntervalFloor">Minimum interval between reports, in seconds.</param>
		/// <param name="MaxIntervalCeiling">Maximum interval ceiling, in seconds.</param>
		/// <param name="KeepSubscriptions">If existing subscriptions should be kept.</param>
		/// <param name="FabricFiltered">If fabric-scoped data should be filtered.</param>
		/// <param name="EventMin">Minimum event number, or null.</param>
		/// <param name="DataVersionFilters">Data version filters, or null.</param>
		/// <returns>Established subscription.</returns>
		public async Task<Subscription> SubscribeAsync(SubscriptionManager Subscriptions, AttributePath[] AttributePaths, EventPath[] EventPaths,
			ushort MinIntervalFloor, ushort MaxIntervalCeiling, bool KeepSubscriptions = true, bool FabricFiltered = true,
			ulong? EventMin = null, DataVersionFilter[] DataVersionFilters = null)
		{
			TlvWriter w = new TlvWriter();

			w.StartStructure();
			w.WriteBoolean(0, KeepSubscriptions);
			w.WriteUInt(1, MinIntervalFloor);
			w.WriteUInt(2, MaxIntervalCeiling);

			if (!(AttributePaths is null) && AttributePaths.Length > 0)
			{
				w.StartArray(3);
				foreach (AttributePath P in AttributePaths)
					P.Write(w, TlvTag.Anonymous);
				w.EndContainer();
			}

			if (!(EventPaths is null) && EventPaths.Length > 0)
			{
				w.StartArray(4);
				foreach (EventPath P in EventPaths)
					P.Write(w, TlvTag.Anonymous);
				w.EndContainer();

				if (EventMin.HasValue)
				{
					w.StartArray(5);
					w.StartStructure(TlvTag.Anonymous);
					w.WriteUInt(1, EventMin.Value);
					w.EndContainer();
					w.EndContainer();
				}
			}

			w.WriteBoolean(7, FabricFiltered);

			if (!(DataVersionFilters is null) && DataVersionFilters.Length > 0)
			{
				w.StartArray(8);
				foreach (DataVersionFilter F in DataVersionFilters)
					F.Write(w, TlvTag.Anonymous);
				w.EndContainer();
			}

			w.WriteUInt(InteractionModelRevisionTag, SessionParameters.OwnInteractionModelRevision);
			w.EndContainer();

			Exchange Exchange = this.manager.CreateExchange(this.session);

			try
			{
				await Exchange.SendAsync(ProtocolIds.InteractionModel, (byte)InteractionModelOpcode.SubscribeRequest, w.ToArray());

				ReadResult Priming = new ReadResult();
				TimeSpan Timeout = this.manager.GetResponseTimeout(this.session, this.ProcessingTime);

				while (true)
				{
					MatterMessage Response = await Exchange.ReceiveAsync(Timeout);
					CheckStatusResponse(Response);

					if (Response.Is(ProtocolIds.InteractionModel, (byte)InteractionModelOpcode.ReportData))
					{
						ParseReportData(Response.DecodeTlv(), Priming, out _, out _);
						await SendStatusResponse(Exchange, ImStatusCode.Success);
					}
					else if (Response.Is(ProtocolIds.InteractionModel, (byte)InteractionModelOpcode.SubscribeResponse))
					{
						TlvElement Resp = Response.DecodeTlv();
						uint SubscriptionId = Resp.Required(0).AsUInt32();
						ushort MaxInterval = Resp.Required(2).AsUInt16();

						Subscription Result = new Subscription(this, SubscriptionId, MinIntervalFloor, MaxInterval,
							AttributePaths, EventPaths, Priming);

						Subscriptions.Register(Result);

						return Result;
					}
					else
						throw new MatterException("Unexpected response to subscribe request: " + Response.ProtocolHeader.ToString());
				}
			}
			finally
			{
				await Exchange.CloseAsync();
			}
		}

		#endregion

		#region Helpers

		private async Task SendTimedRequest(Exchange Exchange, ushort TimeoutMs)
		{
			TlvWriter w = new TlvWriter();

			w.StartStructure();
			w.WriteUInt(0, TimeoutMs);
			w.WriteUInt(InteractionModelRevisionTag, SessionParameters.OwnInteractionModelRevision);
			w.EndContainer();

			MatterMessage Response = await Exchange.RequestAsync(ProtocolIds.InteractionModel, (byte)InteractionModelOpcode.TimedRequest,
				w.ToArray(), this.ProcessingTime);

			if (!Response.Is(ProtocolIds.InteractionModel, (byte)InteractionModelOpcode.StatusResponse))
				throw new MatterException("Unexpected response to timed request: " + Response.ProtocolHeader.ToString());

			ImStatusCode Status = (ImStatusCode)Response.DecodeTlv().Required(0).AsByte();
			if (Status != ImStatusCode.Success)
				throw new ImStatusException(new ImStatus(Status, null), "Timed request rejected");
		}

		/// <summary>
		/// Sends a StatusResponse message on an exchange.
		/// </summary>
		/// <param name="Exchange">Exchange.</param>
		/// <param name="Status">Status code.</param>
		public static Task SendStatusResponse(Exchange Exchange, ImStatusCode Status)
		{
			TlvWriter w = new TlvWriter();

			w.StartStructure();
			w.WriteUInt(0, (byte)Status);
			w.WriteUInt(InteractionModelRevisionTag, SessionParameters.OwnInteractionModelRevision);
			w.EndContainer();

			return Exchange.SendAsync(ProtocolIds.InteractionModel, (byte)InteractionModelOpcode.StatusResponse, w.ToArray());
		}

		/// <summary>
		/// Throws an exception if a message is an error StatusResponse.
		/// </summary>
		/// <param name="Message">Message.</param>
		public static void CheckStatusResponse(MatterMessage Message)
		{
			if (Message.Is(ProtocolIds.InteractionModel, (byte)InteractionModelOpcode.StatusResponse))
			{
				ImStatusCode Status = (ImStatusCode)Message.DecodeTlv().Required(0).AsByte();
				throw new ImStatusException(new ImStatus(Status, null), "Interaction failed");
			}
			else if (Message.IsStatusReport)
				throw new StatusReportException(StatusReport.Decode(Message.Payload));
		}

		#endregion
	}
}
