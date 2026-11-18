using System;
using Waher.Things.Matter.Attestation;
using Waher.Things.Matter.Commissioning;

namespace Waher.Things.Matter.Controller
{
	/// <summary>
	/// Options for a Matter controller.
	/// </summary>
	public class MatterControllerOptions
	{
		/// <summary>
		/// Options for a Matter controller.
		/// </summary>
		public MatterControllerOptions()
		{
		}

		/// <summary>
		/// Local UDP port to use for Matter messages (0 = ephemeral port).
		/// </summary>
		public int Port { get; set; } = 0;

		/// <summary>
		/// If DNS-SD (mDNS) discovery should be used.
		/// </summary>
		public bool EnableDiscovery { get; set; } = true;

		/// <summary>
		/// How device attestation failures are handled during commissioning.
		/// </summary>
		public AttestationPolicy AttestationPolicy { get; set; } = AttestationPolicy.Strict;

		/// <summary>
		/// If the embedded production PAA certificates (mirrored from the CSA Distributed Compliance Ledger) are trusted.
		/// </summary>
		public bool TrustProductionPaas { get; set; } = true;

		/// <summary>
		/// If the publicly known development/test PAA certificates are trusted. Only for development and testing.
		/// </summary>
		public bool TrustDevelopmentPaas { get; set; } = false;

		/// <summary>
		/// If certification declarations signed with the publicly known test key are accepted. Only for development and testing.
		/// </summary>
		public bool AllowTestCertificationDeclarations { get; set; } = false;

		/// <summary>
		/// Optional folder containing additional trusted PAA certificates (DER or PEM).
		/// </summary>
		public string PaaFolder { get; set; } = null;

		/// <summary>
		/// Regulatory location to configure during commissioning.
		/// </summary>
		public RegulatoryLocation RegulatoryLocation { get; set; } = RegulatoryLocation.IndoorOutdoor;

		/// <summary>
		/// Country code to configure during commissioning.
		/// </summary>
		public string CountryCode { get; set; } = "XX";

		/// <summary>
		/// Fabric label to set on commissioned devices.
		/// </summary>
		public string FabricLabel { get; set; } = null;

		/// <summary>
		/// Time to wait for DNS-SD discovery responses.
		/// </summary>
		public TimeSpan DiscoveryTimeout { get; set; } = TimeSpan.FromSeconds(5);
	}
}
