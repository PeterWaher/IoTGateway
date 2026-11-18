using System;
using System.IO;
using System.Text;

namespace Waher.Things.Matter.Test.Devices
{
	/// <summary>
	/// Patches the compiled matter.js packages, so that test devices can run on hosts without IPv6 support (for instance
	/// in some containers). Matter requires IPv6, but the matter.js devices can be used over IPv4 for testing purposes.
	/// The patch is only applied if the host does not support IPv6.
	/// </summary>
	public static class MatterJsIpv4Patch
	{
		/// <summary>
		/// Applies the patch.
		/// </summary>
		/// <param name="NodeModulesFolder">node_modules folder containing matter.js.</param>
		public static void Apply(string NodeModulesFolder)
		{
			string FileName = Path.Combine(NodeModulesFolder, "@matter", "general", "dist", "esm", "net", "udp", "UdpMulticastServer.js");

			Patch(FileName,
				"        NoAddressAvailableError.accept(error);\n" +
				"        logger.info(`IPv6 UDP interface not created because IPv6 is not available, but required my Matter`);\n" +
				"        throw error;",
				"        if (ipv4UdpSocket === void 0) throw error;\n" +
				"        logger.info(`IPv6 UDP interface not created (IPv4-only host patch): ${asError(error).message}`);\n" +
				"        ipv6UdpSocket = void 0;");

			Patch(FileName, "this.#serverIpv6.onData(", "this.#serverIpv6?.onData(");
			Patch(FileName, "await this.#serverIpv6.close();", "await this.#serverIpv6?.close();");
			Patch(FileName, "if (ipV6.length > 0) {", "if (ipV6.length > 0 && this.#serverIpv6 !== void 0) {");

			FileName = Path.Combine(NodeModulesFolder, "@matter", "node", "dist", "esm", "behavior", "system", "network", "ServerNetworkRuntime.js");

			Patch(FileName,
				"      } catch (error) {\n" +
				"        NoAddressAvailableError.accept(error);\n" +
				"        logger.warn(`IPv6 UDP interface not created because IPv6 is not available, but required by Matter.`);\n" +
				"        throw error;\n" +
				"      }\n" +
				"      let ipv4Interface;\n" +
				"      if (netconf.ipv4) {",
				"      } catch (error) {\n" +
				"        logger.warn(`IPv6 UDP interface not created (IPv4-only host patch), using IPv4 only.`);\n" +
				"        ipv6Interface = await UdpTransport.create(\n" +
				"          network,\n" +
				"          \"udp4\",\n" +
				"          port ? port : void 0,\n" +
				"          netconf.listeningAddressIpv4\n" +
				"        );\n" +
				"        ipv6Interface.__ipv4Only = true;\n" +
				"      }\n" +
				"      let ipv4Interface;\n" +
				"      if (netconf.ipv4 && !ipv6Interface.__ipv4Only) {");
		}

		private static void Patch(string FileName, string Old, string New)
		{
			string Text = File.ReadAllText(FileName, Encoding.UTF8);
			string Normalized = Text.Replace("\r\n", "\n");

			if (Normalized.Contains(New))
				return;

			int i = Normalized.IndexOf(Old);
			if (i < 0)
				throw new Exception("Unable to patch " + FileName + ". Unexpected version of matter.js.");

			Normalized = Normalized.Substring(0, i) + New + Normalized.Substring(i + Old.Length);

			File.WriteAllText(FileName, Normalized, new UTF8Encoding(false));
		}
	}
}
