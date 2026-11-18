using System;
using System.Collections.Generic;
using System.Globalization;
using System.Net;
using System.Threading.Tasks;
using Waher.Things.Matter.Messaging;
using Waher.Things.Matter.Onboarding;

namespace Waher.Things.Matter.Discovery
{
	/// <summary>
	/// Commissioning mode advertised by a commissionable node.
	/// </summary>
	public enum CommissioningMode
	{
		/// <summary>
		/// Not in commissioning mode.
		/// </summary>
		NotCommissioning = 0,

		/// <summary>
		/// Basic commissioning mode (passcode from onboarding payload).
		/// </summary>
		Basic = 1,

		/// <summary>
		/// Enhanced commissioning mode (passcode provided by an administrator).
		/// </summary>
		Enhanced = 2
	}

	/// <summary>
	/// A commissionable node discovered using DNS-SD (Matter Core Specification, 4.3.1).
	/// </summary>
	public class CommissionableNode
	{
		/// <summary>
		/// A commissionable node.
		/// </summary>
		/// <param name="Instance">DNS-SD instance.</param>
		public CommissionableNode(DnsSdInstance Instance)
		{
			this.Instance = Instance;

			string s = Instance.GetTxt("D");
			if (ushort.TryParse(s, NumberStyles.None, CultureInfo.InvariantCulture, out ushort D))
				this.Discriminator = D;

			s = Instance.GetTxt("CM");
			if (int.TryParse(s, NumberStyles.None, CultureInfo.InvariantCulture, out int Cm))
				this.Mode = (CommissioningMode)Cm;
			else
				this.Mode = CommissioningMode.NotCommissioning;

			s = Instance.GetTxt("VP");
			if (!string.IsNullOrEmpty(s))
			{
				string[] Parts = s.Split('+');

				if (ushort.TryParse(Parts[0], NumberStyles.None, CultureInfo.InvariantCulture, out ushort Vid))
					this.VendorId = Vid;

				if (Parts.Length > 1 && ushort.TryParse(Parts[1], NumberStyles.None, CultureInfo.InvariantCulture, out ushort Pid))
					this.ProductId = Pid;
			}

			s = Instance.GetTxt("DT");
			if (uint.TryParse(s, NumberStyles.None, CultureInfo.InvariantCulture, out uint Dt))
				this.DeviceType = Dt;

			this.DeviceName = Instance.GetTxt("DN");
			this.PairingHint = Instance.GetTxt("PH");
			this.PairingInstruction = Instance.GetTxt("PI");
			this.SessionParameters = MatterDiscovery.ParseMrp(Instance);
		}

		/// <summary>
		/// DNS-SD instance.
		/// </summary>
		public DnsSdInstance Instance { get; }

		/// <summary>
		/// Long (12-bit) discriminator.
		/// </summary>
		public ushort? Discriminator { get; }

		/// <summary>
		/// Commissioning mode.
		/// </summary>
		public CommissioningMode Mode { get; }

		/// <summary>
		/// Vendor ID, if advertised.
		/// </summary>
		public ushort? VendorId { get; }

		/// <summary>
		/// Product ID, if advertised.
		/// </summary>
		public ushort? ProductId { get; }

		/// <summary>
		/// Device type, if advertised.
		/// </summary>
		public uint? DeviceType { get; }

		/// <summary>
		/// Device name, if advertised.
		/// </summary>
		public string DeviceName { get; }

		/// <summary>
		/// Pairing hint, if advertised.
		/// </summary>
		public string PairingHint { get; }

		/// <summary>
		/// Pairing instruction, if advertised.
		/// </summary>
		public string PairingInstruction { get; }

		/// <summary>
		/// Session (MRP) parameters advertised by the node.
		/// </summary>
		public SessionParameters SessionParameters { get; }

		/// <summary>
		/// Checks if the node matches an onboarding payload (discriminator, and vendor/product if known).
		/// </summary>
		/// <param name="Payload">Onboarding payload.</param>
		/// <returns>If the node matches.</returns>
		public bool Matches(SetupPayload Payload)
		{
			if (!this.Discriminator.HasValue)
				return false;

			if (Payload.IsShortDiscriminator)
			{
				if ((this.Discriminator.Value >> 8) != Payload.ShortDiscriminator)
					return false;
			}
			else if (this.Discriminator.Value != Payload.Discriminator)
				return false;

			if (Payload.VendorId != 0 && this.VendorId.HasValue && this.VendorId.Value != Payload.VendorId)
				return false;

			if (Payload.ProductId != 0 && this.ProductId.HasValue && this.ProductId.Value != Payload.ProductId)
				return false;

			return true;
		}

		/// <inheritdoc/>
		public override string ToString()
		{
			return (this.DeviceName ?? this.Instance.InstanceName) + " (discriminator " + this.Discriminator?.ToString() + ", " +
				this.Mode.ToString() + ") " + this.Instance.ToString();
		}
	}

	/// <summary>
	/// An operational node discovered using DNS-SD (Matter Core Specification, 4.3.2).
	/// </summary>
	public class OperationalNode
	{
		/// <summary>
		/// An operational node.
		/// </summary>
		/// <param name="Instance">DNS-SD instance.</param>
		public OperationalNode(DnsSdInstance Instance)
		{
			this.Instance = Instance;

			string[] Parts = Instance.InstanceName.Split('-');

			if (Parts.Length == 2 &&
				ulong.TryParse(Parts[0], NumberStyles.HexNumber, CultureInfo.InvariantCulture, out ulong CompressedFabricId) &&
				ulong.TryParse(Parts[1], NumberStyles.HexNumber, CultureInfo.InvariantCulture, out ulong NodeId))
			{
				this.CompressedFabricId = CompressedFabricId;
				this.NodeId = NodeId;
			}

			this.SessionParameters = MatterDiscovery.ParseMrp(Instance);
		}

		/// <summary>
		/// DNS-SD instance.
		/// </summary>
		public DnsSdInstance Instance { get; }

		/// <summary>
		/// Compressed fabric ID.
		/// </summary>
		public ulong CompressedFabricId { get; }

		/// <summary>
		/// Operational node ID.
		/// </summary>
		public ulong NodeId { get; }

		/// <summary>
		/// Session (MRP) parameters advertised by the node.
		/// </summary>
		public SessionParameters SessionParameters { get; }

		/// <inheritdoc/>
		public override string ToString()
		{
			return this.Instance.ToString();
		}
	}

	/// <summary>
	/// Matter-specific DNS-SD discovery (Matter Core Specification, 4.3).
	/// </summary>
	public static class MatterDiscovery
	{
		/// <summary>
		/// Service type of commissionable nodes.
		/// </summary>
		public const string CommissionableServiceType = "_matterc._udp.local";

		/// <summary>
		/// Service type of operational nodes.
		/// </summary>
		public const string OperationalServiceType = "_matter._tcp.local";

		/// <summary>
		/// Service type of commissioners.
		/// </summary>
		public const string CommissionerServiceType = "_matterd._udp.local";

		/// <summary>
		/// Parses MRP parameters from TXT records (SII, SAI, SAT).
		/// </summary>
		/// <param name="Instance">DNS-SD instance.</param>
		/// <returns>Session parameters.</returns>
		public static SessionParameters ParseMrp(DnsSdInstance Instance)
		{
			SessionParameters Result = new SessionParameters();

			if (uint.TryParse(Instance.GetTxt("SII"), NumberStyles.None, CultureInfo.InvariantCulture, out uint Sii))
				Result.IdleIntervalMs = Sii;

			if (uint.TryParse(Instance.GetTxt("SAI"), NumberStyles.None, CultureInfo.InvariantCulture, out uint Sai))
				Result.ActiveIntervalMs = Sai;

			if (ushort.TryParse(Instance.GetTxt("SAT"), NumberStyles.None, CultureInfo.InvariantCulture, out ushort Sat))
				Result.ActiveThresholdMs = Sat;

			return Result;
		}

		/// <summary>
		/// Discovers commissionable nodes.
		/// </summary>
		/// <param name="Client">mDNS client.</param>
		/// <param name="Timeout">Time to wait for responses.</param>
		/// <param name="Payload">Optional onboarding payload, used to filter by discriminator. Discovery stops when a match is found.</param>
		/// <returns>Commissionable nodes.</returns>
		public static async Task<CommissionableNode[]> DiscoverCommissionableAsync(MdnsClient Client, TimeSpan Timeout, SetupPayload Payload = null)
		{
			string ServiceType = CommissionableServiceType;

			if (!(Payload is null))
			{
				if (Payload.IsShortDiscriminator)
					ServiceType = "_S" + Payload.ShortDiscriminator.ToString() + "._sub." + CommissionableServiceType;
				else
					ServiceType = "_L" + Payload.Discriminator.ToString() + "._sub." + CommissionableServiceType;
			}

			DnsSdInstance[] Instances = await Client.BrowseAsync(ServiceType, Timeout, Payload is null ? (Func<DnsSdInstance, bool>)null :
				(Instance) => new CommissionableNode(Instance).Matches(Payload));

			if (Instances.Length == 0 && !(Payload is null))
			{
				// Some devices do not advertise subtypes properly. Fallback to browsing all commissionable nodes.

				Instances = await Client.BrowseAsync(CommissionableServiceType, TimeSpan.FromSeconds(Math.Min(3, Timeout.TotalSeconds)),
					(Instance) => new CommissionableNode(Instance).Matches(Payload));
			}

			List<CommissionableNode> Result = new List<CommissionableNode>();

			foreach (DnsSdInstance Instance in Instances)
			{
				CommissionableNode Node = new CommissionableNode(Instance);

				if (Payload is null || Node.Matches(Payload))
					Result.Add(Node);
			}

			return Result.ToArray();
		}

		/// <summary>
		/// Resolves the operational address of a node.
		/// </summary>
		/// <param name="Client">mDNS client.</param>
		/// <param name="CompressedFabricId">Compressed fabric ID.</param>
		/// <param name="NodeId">Operational node ID.</param>
		/// <param name="Timeout">Time to wait for responses.</param>
		/// <returns>Operational node, or null if not found.</returns>
		public static async Task<OperationalNode> ResolveOperationalAsync(MdnsClient Client, ulong CompressedFabricId, ulong NodeId, TimeSpan Timeout)
		{
			string FullName = CompressedFabricId.ToString("X16") + "-" + NodeId.ToString("X16") + "." + OperationalServiceType;
			DnsSdInstance Instance = await Client.ResolveAsync(FullName, Timeout);

			if (Instance is null)
				return null;
			else
				return new OperationalNode(Instance);
		}

		/// <summary>
		/// Browses for operational nodes of a fabric.
		/// </summary>
		/// <param name="Client">mDNS client.</param>
		/// <param name="CompressedFabricId">Compressed fabric ID.</param>
		/// <param name="Timeout">Time to wait for responses.</param>
		/// <returns>Operational nodes.</returns>
		public static async Task<OperationalNode[]> BrowseOperationalAsync(MdnsClient Client, ulong CompressedFabricId, TimeSpan Timeout)
		{
			string ServiceType = "_I" + CompressedFabricId.ToString("X16") + "._sub." + OperationalServiceType;
			DnsSdInstance[] Instances = await Client.BrowseAsync(ServiceType, Timeout);
			List<OperationalNode> Result = new List<OperationalNode>();

			foreach (DnsSdInstance Instance in Instances)
			{
				OperationalNode Node = new OperationalNode(Instance);
				if (Node.CompressedFabricId == CompressedFabricId)
					Result.Add(Node);
			}

			return Result.ToArray();
		}

		/// <summary>
		/// Gets the first usable endpoint of a resolved instance.
		/// </summary>
		/// <param name="Instance">Instance.</param>
		/// <param name="Transport">Transport (determines supported address families).</param>
		/// <returns>Endpoint, or null if none usable.</returns>
		public static IPEndPoint GetEndpoint(DnsSdInstance Instance, UdpTransport Transport)
		{
			IPAddress[] Addresses = Instance.GetPreferredAddresses(Transport.SupportsIPv4, Transport.SupportsIPv6);

			if (Addresses.Length == 0)
				return null;
			else
				return new IPEndPoint(Addresses[0], Instance.Port);
		}
	}
}
