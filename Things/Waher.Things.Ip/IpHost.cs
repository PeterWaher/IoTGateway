using System;
using System.Collections.Generic;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading.Tasks;
using Waher.Content;
using Waher.Networking;
using Waher.Networking.HTTP.ScriptExtensions;
using Waher.Persistence.Attributes;
using Waher.Runtime.Collections;
using Waher.Runtime.Language;
using Waher.Script.Constants;
using Waher.Script.Functions.Runtime;
using Waher.Things.Attributes;
using Waher.Things.DisplayableParameters;
using Waher.Things.Metering;
using Waher.Things.Metering.NodeTypes;
using Waher.Things.SensorData;

namespace Waher.Things.Ip
{
	/// <summary>
	/// Node representing an IP Host machine.
	/// </summary>
	public class IpHost : ProvisionedMeteringNode, ISensor, IHostReference
	{
		private string host = string.Empty;
		private bool pingHost = true;

		/// <summary>
		/// If the node accepts a presumptive parent, i.e. can be added to that parent (if that parent accepts the node as a child).
		/// </summary>
		/// <param name="Parent">Presumptive parent node.</param>
		/// <returns>If the parent is acceptable.</returns>
		public override Task<bool> AcceptsParentAsync(INode Parent)
		{
			return Task.FromResult(
				Parent is Root ||
				Parent is NodeCollection ||
				Parent is IpHost);
		}

		/// <summary>
		/// If the node accepts a presumptive child, i.e. can receive as a child (if that child accepts the node as a parent).
		/// </summary>
		/// <param name="Child">Presumptive child node.</param>
		/// <returns>If the child is acceptable.</returns>
		public override Task<bool> AcceptsChildAsync(INode Child)
		{
			return Task.FromResult(
				Child is IpHost ||
				Child is PortMonitor);
		}

		/// <summary>
		/// Gets the type name of the node.
		/// </summary>
		/// <param name="Language">Language to use.</param>
		/// <returns>Localized type node.</returns>
		public override Task<string> GetTypeNameAsync(Language Language)
		{
			return Language.GetStringAsync(typeof(IpHost), 6, "IP Host");
		}

		/// <summary>
		/// Host name or IP address.
		/// </summary>
		[Page(1, "IP")]
		[Header(2, "Host Name:", 50)]
		[ToolTip(3, "Host Name or IP Address of device.")]
		[DefaultValueStringEmpty]
		[Required]
		public string Host
		{
			get => this.host;
			set => this.host = value;
		}

		/// <summary>
		/// If host should be pinged during readout.
		/// </summary>
		[Page(1, "IP")]
		[Header(38, "Ping host.", 50)]
		[ToolTip(39, "If host name should be pinged during readout.")]
		[DefaultValue(true)]
		public bool PingHost
		{
			get => this.pingHost;
			set => this.pingHost = value;
		}

		/// <summary>
		/// If the node can be read.
		/// </summary>
		public override bool IsReadable => true;

		/// <summary>
		/// Starts the readout of the sensor.
		/// </summary>
		/// <param name="Request">Request object. All fields and errors should be reported to this interface.</param>
		public async virtual Task StartReadout(ISensorReadout Request)
		{
			try
			{
				string Module = typeof(IpHost).Namespace;
				ChunkedList<Field> Fields = new ChunkedList<Field>();

				foreach (INode Node in await this.ChildNodes)
				{
					if (Node is PortMonitor Monitor)
					{
						DateTime Now = DateTime.UtcNow;
						string Protocol = Monitor.Protocol;

						try
						{
							if (string.IsNullOrEmpty(Protocol))
								Protocol = Monitor.Port.ToString();

							using BinaryTcpClient Client = await Monitor.ConnectTcp(false);

							Fields.Add(new QuantityField(this, Now, Protocol,
								(DateTime.UtcNow - Now).TotalMilliseconds, 0, "ms",
								FieldType.Momentary, FieldQoS.AutomaticReadout));
						}
						catch (Exception ex)
						{
							await Request.ReportErrors(false, new ThingError(this, Now,
								"Unable to connect using " + Protocol + ": " + ex.Message));
						}
					}
				}

				if (this.pingHost)
				{
					using Ping Icmp = new Ping();

					PingReply Response = await Icmp.SendPingAsync(this.host, 2000, data, options);
					DateTime Now = DateTime.UtcNow;

					if (Response.Status == IPStatus.Success)
					{
						if (Request.IsIncluded(FieldType.Identity))
						{
							Fields.Add(new StringField(this, Now, "IP Address", Response.Address.ToString(), FieldType.Identity, FieldQoS.AutomaticReadout, Module, 8));
							this.AddIdentityReadout(Fields, Now);
						}
					}
					else
					{
						await Request.ReportErrors(false, new ThingError(this, Now,
							GetErrorMessage(Response)));
					}
				}

				await Request.ReportFields(true, Fields.ToArray());
			}
			catch (PingException ex)
			{
				if (!(ex.InnerException is null))
					await Request.ReportErrors(true, new ThingError(this, ex.InnerException.Message));
				else
					await Request.ReportErrors(true, new ThingError(this, ex.Message));
			}
			catch (Exception ex)
			{
				await Request.ReportErrors(true, new ThingError(this, ex.Message));
			}
		}

		private static string GetErrorMessage(PingReply Response)
		{
			return Response.Status switch
			{
				IPStatus.Success => null,
				IPStatus.BadDestination => "Bad destination",
				IPStatus.BadHeader => "Bad header",
				IPStatus.BadOption => "Bad option",
				IPStatus.BadRoute => "Bad route",
				IPStatus.DestinationHostUnreachable => "Destination host unreachable",
				IPStatus.DestinationNetworkUnreachable => "Destination network unreachable",
				IPStatus.DestinationPortUnreachable => "Destination port unreachable",
				IPStatus.DestinationProhibited => "Destination prohibited",
				//case IPStatus.DestinationProtocolUnreachable: return "Destination protocol unreachable";
				IPStatus.DestinationScopeMismatch => "Destination scope mismatch",
				IPStatus.DestinationUnreachable => "Destination unreachable",
				IPStatus.HardwareError => "Hardware error",
				IPStatus.IcmpError => "ICMP error",
				IPStatus.NoResources => "No resources",
				IPStatus.PacketTooBig => "Packet too big",
				IPStatus.ParameterProblem => "Parameter problem",
				IPStatus.SourceQuench => "Source quench",
				IPStatus.TimedOut => "Timed out",
				IPStatus.TimeExceeded => "Time exceeded",
				IPStatus.TtlExpired => "TTL expired",
				IPStatus.TtlReassemblyTimeExceeded => "TTL reassembly time exceeded",
				IPStatus.UnrecognizedNextHeader => "Unrecognized next header",
				_ => "Unknown error",
			};
		}

		private static readonly byte[] data = Encoding.ASCII.GetBytes("aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa");
		private static readonly PingOptions options = new PingOptions(64, true);

		/// <summary>
		/// Gets displayable parameters.
		/// </summary>
		/// <param name="Language">Language to use.</param>
		/// <param name="Caller">Information about caller.</param>
		/// <returns>Set of displayable parameters.</returns>
		public async override Task<IEnumerable<Parameter>> GetDisplayableParametersAsync(Language Language, RequestOrigin Caller)
		{
			LinkedList<Parameter> Result = await base.GetDisplayableParametersAsync(Language, Caller) as LinkedList<Parameter>;

			Result.AddLast(new StringParameter("Host", await Language.GetStringAsync(typeof(IpHost), 9, "Host"), this.host));

			return Result;
		}
	}
}
