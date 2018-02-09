using System;
using System.Collections.Generic;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading.Tasks;
using Waher.Persistence.Attributes;
using Waher.Runtime.Language;
using Waher.Things.Attributes;
using Waher.Things.SensorData;
using Waher.Things.Metering;
using Waher.Things.Metering.NodeTypes;

namespace Waher.Things.IP
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
			return Task.FromResult<bool>(Parent is Root);
		}

		/// <summary>
		/// If the node accepts a presumptive child, i.e. can receive as a child (if that child accepts the node as a parent).
		/// </summary>
		/// <param name="Child">Presumptive child node.</param>
		/// <returns>If the child is acceptable.</returns>
		public override Task<bool> AcceptsChildAsync(INode Child)
		{
			return Task.FromResult<bool>(false);
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
		public string Host
		{
			get { return this.host; }
			set { this.host = value; }
		}

		/// <summary>
		/// If the node can be read.
		/// </summary>
		public override bool IsReadable => true;

		public async void StartReadout(ISensorReadout Request)
		{
			try
			{
				Ping Icmp = new Ping();

				PingReply Response = await Icmp.SendPingAsync(this.host, 10000, data, options);
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
					switch (Response.Status)
					{
						case IPStatus.BadDestination:
							Request.ReportErrors(true, new ThingError(this, Now, "Bad destination"));
							break;

						case IPStatus.BadHeader:
							Request.ReportErrors(true, new ThingError(this, Now, "Bad header"));
							break;

						case IPStatus.BadOption:
							Request.ReportErrors(true, new ThingError(this, Now, "Bad option"));
							break;

						case IPStatus.BadRoute:
							Request.ReportErrors(true, new ThingError(this, Now, "Bad route"));
							break;

						case IPStatus.DestinationHostUnreachable:
							Request.ReportErrors(true, new ThingError(this, Now, "Destination host unreachable"));
							break;

						case IPStatus.DestinationNetworkUnreachable:
							Request.ReportErrors(true, new ThingError(this, Now, "Destination network unreachable"));
							break;

						case IPStatus.DestinationPortUnreachable:
							Request.ReportErrors(true, new ThingError(this, Now, "Destination port unreachable"));
							break;

						case IPStatus.DestinationProhibited:
							Request.ReportErrors(true, new ThingError(this, Now, "Destination prohibited"));
							break;
						/*
					case IPStatus.DestinationProtocolUnreachable:
						Request.ReportErrors(true, new ThingError(this, Now, "Destination protocol unreachable"));
						break;
						*/
						case IPStatus.DestinationScopeMismatch:
							Request.ReportErrors(true, new ThingError(this, Now, "Destination scope mismatch"));
							break;

						case IPStatus.DestinationUnreachable:
							Request.ReportErrors(true, new ThingError(this, Now, "Destination unreachable"));
							break;

						case IPStatus.HardwareError:
							Request.ReportErrors(true, new ThingError(this, Now, "Hardware error"));
							break;

						case IPStatus.IcmpError:
							Request.ReportErrors(true, new ThingError(this, Now, "ICMP error"));
							break;

						case IPStatus.NoResources:
							Request.ReportErrors(true, new ThingError(this, Now, "No resources"));
							break;

						case IPStatus.PacketTooBig:
							Request.ReportErrors(true, new ThingError(this, Now, "Packet too big"));
							break;

						case IPStatus.ParameterProblem:
							Request.ReportErrors(true, new ThingError(this, Now, "Parameter problem"));
							break;

						case IPStatus.SourceQuench:
							Request.ReportErrors(true, new ThingError(this, Now, "Source quench"));
							break;

						case IPStatus.TimedOut:
							Request.ReportErrors(true, new ThingError(this, Now, "Timed out"));
							break;

						case IPStatus.TimeExceeded:
							Request.ReportErrors(true, new ThingError(this, Now, "Time exceeded"));
							break;

						case IPStatus.TtlExpired:
							Request.ReportErrors(true, new ThingError(this, Now, "TTL expired"));
							break;

						case IPStatus.TtlReassemblyTimeExceeded:
							Request.ReportErrors(true, new ThingError(this, Now, "TTL reassembly time exceeded"));
							break;

						case IPStatus.UnrecognizedNextHeader:
							Request.ReportErrors(true, new ThingError(this, Now, "Unrecognized next header"));
							break;

						case IPStatus.Unknown:
						default:
							Request.ReportErrors(true, new ThingError(this, Now, "Unknown error"));
							break;
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

		private static readonly byte[] data = Encoding.ASCII.GetBytes("aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa");
		private static PingOptions options = new PingOptions(64, true);

	}
}
