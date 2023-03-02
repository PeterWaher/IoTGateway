using System;
using System.Collections.Generic;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading.Tasks;
using Waher.Persistence.Attributes;
using Waher.Runtime.Language;
using Waher.Things.Attributes;
using Waher.Things.DisplayableParameters;
using Waher.Things.SensorData;
using Waher.Things.Metering;
using Waher.Things.Metering.NodeTypes;
using Waher.Script.Constants;

namespace Waher.Things.Ip
{
	public class IpHost : ProvisionedMeteringNode, ISensor
	{
		private string host = string.Empty;

		/// <summary>
		/// If the node accepts a presumptive parent, i.e. can be added to that parent (if that parent accepts the node as a child).
		/// </summary>
		/// <param name="Parent">Presumptive parent node.</param>
		/// <returns>If the parent is acceptable.</returns>
		public override Task<bool> AcceptsParentAsync(INode Parent)
		{
			return Task.FromResult(Parent is Root || Parent is IpHost);
		}

		/// <summary>
		/// If the node accepts a presumptive child, i.e. can receive as a child (if that child accepts the node as a parent).
		/// </summary>
		/// <param name="Child">Presumptive child node.</param>
		/// <returns>If the child is acceptable.</returns>
		public override Task<bool> AcceptsChildAsync(INode Child)
		{
			return Task.FromResult(Child is IpHost);
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
				using (Ping Icmp = new Ping())
				{
					PingReply Response = await Icmp.SendPingAsync(this.host, 2000, data, options);
					DateTime Now = DateTime.Now;
					string Module = typeof(IpHost).Namespace;

					if (Response.Status == IPStatus.Success)
					{
						List<Field> Fields = new List<Field>()
						{
							new QuantityField(this, Now, "Ping", Response.RoundtripTime, 0, "ms", FieldType.Momentary, FieldQoS.AutomaticReadout, Module, 7)
						};

						if (Request.IsIncluded(FieldType.Identity))
						{
							Fields.Add(new StringField(this, Now, "IP Address", Response.Address.ToString(), FieldType.Identity, FieldQoS.AutomaticReadout, Module, 8));
							this.AddIdentityReadout(Fields, Now);
						}

						Request.ReportFields(true, Fields);
					}
					else
					{
						Request.ReportErrors(true, new ThingError(this, Now,
							GetErrorMessage(Response)));
					}
				}
			}
			catch (PingException ex)
			{
				if (ex.InnerException != null)
					Request.ReportErrors(true, new ThingError(this, ex.InnerException.Message));
				else
					Request.ReportErrors(true, new ThingError(this, ex.Message));
			}
			catch (Exception ex)
			{
				Request.ReportErrors(true, new ThingError(this, ex.Message));
			}
		}

		private static string GetErrorMessage(PingReply Response)
		{
			switch (Response.Status)
			{
				case IPStatus.Success: return null;
				case IPStatus.BadDestination: return "Bad destination";
				case IPStatus.BadHeader: return "Bad header";
				case IPStatus.BadOption: return "Bad option";
				case IPStatus.BadRoute: return "Bad route";
				case IPStatus.DestinationHostUnreachable: return "Destination host unreachable";
				case IPStatus.DestinationNetworkUnreachable: return "Destination network unreachable";
				case IPStatus.DestinationPortUnreachable: return "Destination port unreachable";
				case IPStatus.DestinationProhibited: return "Destination prohibited";
				//case IPStatus.DestinationProtocolUnreachable: return "Destination protocol unreachable";
				case IPStatus.DestinationScopeMismatch: return "Destination scope mismatch";
				case IPStatus.DestinationUnreachable: return "Destination unreachable";
				case IPStatus.HardwareError: return "Hardware error";
				case IPStatus.IcmpError: return "ICMP error";
				case IPStatus.NoResources: return "No resources";
				case IPStatus.PacketTooBig: return "Packet too big";
				case IPStatus.ParameterProblem: return "Parameter problem";
				case IPStatus.SourceQuench: return "Source quench";
				case IPStatus.TimedOut: return "Timed out";
				case IPStatus.TimeExceeded: return "Time exceeded";
				case IPStatus.TtlExpired: return "TTL expired";
				case IPStatus.TtlReassemblyTimeExceeded: return "TTL reassembly time exceeded";
				case IPStatus.UnrecognizedNextHeader: return "Unrecognized next header";
				case IPStatus.Unknown:
				default:
					return "Unknown error";
			}
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
