using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using System.Threading.Tasks;
using Waher.Content.Xml;
using Waher.Runtime.Console;
using Waher.Runtime.Inventory;
using Waher.Script;
using Waher.Script.Abstraction.Elements;

namespace Waher.Runtime.Text.Test
{
	[TestClass]
	public class MappingTests
	{
		private static HarmonizedTextMap wordStyleMaps;
		private static HarmonizedTextMap networkRecordMaps;

		[AssemblyInitialize]
		public static void AssemblyInitialize(TestContext _)
		{
			Types.Initialize(typeof(Expression).Assembly);
		}

		[ClassInitialize]
		public static void ClassInitialize(TestContext _)
		{
			wordStyleMaps = CreateWordStyleMaps();
			networkRecordMaps = CreateNetworkRecordMaps();
		}

		private static HarmonizedTextMap CreateWordStyleMaps()
		{
			HarmonizedTextMap Result = new();

			Result.RegisterMapping(@"H(EAD(ING)?)?\s*(?'N'\d+)", "H{N}");
			Result.RegisterMapping(@"RUBRIK\s*(?'N'\d+)", "H{N}");
			Result.RegisterMapping(@"RUBRIEK\s*(?'N'\d+)", "H{N}");
			Result.RegisterMapping(@"ÜBERSCHRIFT\s*(?'N'\d+)", "H{N}");
			Result.RegisterMapping(@"OVERSKRIFT\s*(?'N'\d+)", "H{N}");
			Result.RegisterMapping(@"TITRE\s*(?'N'\d+)", "H{N}");
			Result.RegisterMapping(@"TITOLO\s*(?'N'\d+)", "H{N}");
			Result.RegisterMapping(@"TÍTULO\s*(?'N'\d+)", "H{N}");
			Result.RegisterMapping(@"ЗАГОЛОВОК\s*(?'N'\d+)", "H{N}");
			Result.RegisterMapping(@"OTSIKKO\s*(?'N'\d+)", "H{N}");
			Result.RegisterMapping(@"NAGŁÓWEK\s*(?'N'\d+)", "H{N}");
			Result.RegisterMapping(@"NADPIS\s*(?'N'\d+)", "H{N}");

			Result.RegisterMapping(@"NORMAL", "NORMAL");
			Result.RegisterMapping(@"SUBTITLE", "NORMAL");
			Result.RegisterMapping(@"BODY TEXT(\s+\d+)", "NORMAL");

			Result.RegisterMapping(@"TITLE", "TITLE");
			Result.RegisterMapping(@"BOOK\s+TITLE", "TITLE");

			Result.RegisterMapping(@"LIST PARAGRAPH", "UL");
			Result.RegisterMapping(@"LISTSTYCKE", "UL");

			Result.RegisterMapping(@"QUOTE", "QUOTE");
			Result.RegisterMapping(@"INTENSE\s+QUOTE", "QUOTE");

			return Result;
		}

		[DataTestMethod]
		[DataRow("HEADING 1", "H1")]
		[DataRow("HEAD2", "H2")]
		[DataRow("H3", "H3")]
		[DataRow("RUBRIK1", "H1")]
		[DataRow("RUBRIK2", "H2")]
		[DataRow("LISTSTYCKE", "UL")]
		public void Test_01_WordStyles(string Input, string Expected)
		{
			Assert.IsTrue(wordStyleMaps.TryMap(Input, out string Harmonized));
			Assert.AreEqual(Expected, Harmonized);
		}

		private static HarmonizedTextMap CreateNetworkRecordMaps()
		{
			HarmonizedTextMap Result = new();

			Result.RegisterMapping(@"Port:\s*(?<Port>0x[0-9a-fA-F]{4})\s+Tipo:\s*(?<Type>[^\s]+)\s+Fecha:\s+(?<Day>\d{2})/(?<Month>\d{2})/(?<Year>\d{4})\s+Hora:\s*(?<Hour>\d{1,2}):(?<Minute>\d{1,2}):(?<Second>\d{1,2})\s*IP:\s*(?<IP>(?:\d{1,3}(?:\.\d{1,3}){3}|(?:[0-9a-fA-F]*:)*(?:[0-9a-fA-F]*::?[0-9a-fA-F]*)\*?))",
				"{'Port':System.Int32.Parse('{Port}'.Substring(2),System.Globalization.NumberStyles.HexNumber),'Type':'{Type}','Timestamp':DateTime('{Year}-{Month}-{Day}T{Hour}:{Minute}:{Second}'),'Endpoint':'{IP}'}");

			Result.RegisterMapping(@"Port:\s*(?<Port>[0-9]{1,5})\s+Tipo:\s*(?<Type>[^\s]+)\s+Fecha:\s+(?<Day>\d{2})/(?<Month>\d{2})/(?<Year>\d{4})\s+Hora:\s*(?<Hour>\d{1,2}):(?<Minute>\d{1,2}):(?<Second>\d{1,2})\s*IP:\s*(?<IP>(?:\d{1,3}(?:\.\d{1,3}){3}|(?:[0-9a-fA-F]*:)*(?:[0-9a-fA-F]*::?[0-9a-fA-F]*)\*?))",
				"{'Port':{Port},'Type':'{Type}','Timestamp':DateTime('{Year}-{Month}-{Day}T{Hour}:{Minute}:{Second}'),'Endpoint':'{IP}'}");

			Result.RegisterMapping(@"Port:\s*(?<Port>http)\s+Tipo:\s*(?<Type>[^\s]+)\s+Fecha:\s+(?<Day>\d{2})/(?<Month>\d{2})/(?<Year>\d{4})\s+Hora:\s*(?<Hour>\d{1,2}):(?<Minute>\d{1,2}):(?<Second>\d{1,2})\s*IP:\s*(?<IP>(?:\d{1,3}(?:\.\d{1,3}){3}|(?:[0-9a-fA-F]*:)*(?:[0-9a-fA-F]*::?[0-9a-fA-F]*)\*?))",
				"{'Port':80,'Type':'{Type}','Timestamp':DateTime('{Year}-{Month}-{Day}T{Hour}:{Minute}:{Second}'),'Endpoint':'{IP}'}");

			Result.RegisterMapping(@"Port:\s*(?<Port>https)\s+Tipo:\s*(?<Type>[^\s]+)\s+Fecha:\s+(?<Day>\d{2})/(?<Month>\d{2})/(?<Year>\d{4})\s+Hora:\s*(?<Hour>\d{1,2}):(?<Minute>\d{1,2}):(?<Second>\d{1,2})\s*IP:\s*(?<IP>(?:\d{1,3}(?:\.\d{1,3}){3}|(?:[0-9a-fA-F]*:)*(?:[0-9a-fA-F]*::?[0-9a-fA-F]*)\*?))",
				"{'Port':443,'Type':'{Type}','Timestamp':DateTime('{Year}-{Month}-{Day}T{Hour}:{Minute}:{Second}'),'Endpoint':'{IP}'}");

			Result.RegisterMapping(@"Port:\s*(?<Port>domain)\s+Tipo:\s*(?<Type>[^\s]+)\s+Fecha:\s+(?<Day>\d{2})/(?<Month>\d{2})/(?<Year>\d{4})\s+Hora:\s*(?<Hour>\d{1,2}):(?<Minute>\d{1,2}):(?<Second>\d{1,2})\s*IP:\s*(?<IP>(?:\d{1,3}(?:\.\d{1,3}){3}|(?:[0-9a-fA-F]*:)*(?:[0-9a-fA-F]*::?[0-9a-fA-F]*)\*?))",
				"{'Port':53,'Type':'{Type}','Timestamp':DateTime('{Year}-{Month}-{Day}T{Hour}:{Minute}:{Second}'),'Endpoint':'{IP}'}");

			return Result;
		}

		private const int https = 443;
		private const int domain = 53;

		[DataTestMethod]
		[DataRow("Port: 0x0000 \t Tipo: Exploits \t Fecha: 29/11/2023 \t Hora: 17:20:18 \t IP: fe80::e657:40ff:f*", 0x0000, "Exploits", "2023-11-29T17:20:18", "fe80::e657:40ff:f*")]
		[DataRow("Port: https \t Tipo: DoS \t Fecha: 29/11/2023 \t Hora: 17:20:18 \t IP: 2800:150:14c:697:*", https, "DoS", "2023-11-29T17:20:18", "2800:150:14c:697:*")]
		[DataRow("Port: 54915 \t Tipo: Exploits \t Fecha: 29/11/2023 \t Hora: 17:20:18 \t IP: 192.168.0.4", 54915, "Exploits", "2023-11-29T17:20:18", "192.168.0.4")]
		[DataRow("Port: 61229 \t Tipo: Exploits \t Fecha: 29/11/2023 \t Hora: 17:20:18 \t IP: 162.159.135.234", 61229, "Exploits", "2023-11-29T17:20:18", "162.159.135.234")]
		[DataRow("Port: https \t Tipo: DoS \t Fecha: 29/11/2023 \t Hora: 17:20:18 \t IP: 2800:150:14c:697:*", https, "DoS", "2023-11-29T17:20:18", "2800:150:14c:697:*")]
		[DataRow("Port: 50001 \t Tipo: Exploits \t Fecha: 29/11/2023 \t Hora: 17:20:18 \t IP: 192.168.0.4", 50001, "Exploits", "2023-11-29T17:20:18", "192.168.0.4")]
		[DataRow("Port: https \t Tipo: DoS \t Fecha: 29/11/2023 \t Hora: 17:20:18 \t IP: 2800:150:14c:697:*", https, "DoS", "2023-11-29T17:20:18", "2800:150:14c:697:*")]
		[DataRow("Port: 54915 \t Tipo: Exploits \t Fecha: 29/11/2023 \t Hora: 17:20:18 \t IP: 192.168.0.4", 54915, "Exploits", "2023-11-29T17:20:18", "192.168.0.4")]
		[DataRow("Port: https \t Tipo: DoS \t Fecha: 29/11/2023 \t Hora: 17:20:18 \t IP: 2800:150:14c:697:*", https, "DoS", "2023-11-29T17:20:18", "2800:150:14c:697:*")]
		[DataRow("Port: https \t Tipo: Exploits \t Fecha: 29/11/2023 \t Hora: 17:20:18 \t IP: 192.168.0.4", https, "Exploits", "2023-11-29T17:20:18", "192.168.0.4")]
		[DataRow("Port: https \t Tipo: Generic \t Fecha: 29/11/2023 \t Hora: 17:20:18 \t IP: 192.168.0.4", https, "Generic", "2023-11-29T17:20:18", "192.168.0.4")]
		[DataRow("Port: 54915 \t Tipo: Exploits \t Fecha: 29/11/2023 \t Hora: 17:20:18 \t IP: 192.168.0.4", 54915, "Exploits", "2023-11-29T17:20:18", "192.168.0.4")]
		[DataRow("Port: domain \t Tipo: DoS \t Fecha: 29/11/2023 \t Hora: 17:20:18 \t IP: 2800:150:14c:697:*", domain, "DoS", "2023-11-29T17:20:18", "2800:150:14c:697:*")]
		[DataRow("Port: https \t Tipo: DoS \t Fecha: 29/11/2023 \t Hora: 17:20:18 \t IP: 2800:150:14c:697:*", https, "DoS", "2023-11-29T17:20:18", "2800:150:14c:697:*")]
		[DataRow("Port: 54915 \t Tipo: Exploits \t Fecha: 29/11/2023 \t Hora: 17:20:18 \t IP: 192.168.0.4", 54915, "Exploits", "2023-11-29T17:20:18", "192.168.0.4")]
		[DataRow("Port: 50001 \t Tipo: Exploits \t Fecha: 29/11/2023 \t Hora: 17:20:18 \t IP: 192.168.0.4", 50001, "Exploits", "2023-11-29T17:20:18", "192.168.0.4")]
		[DataRow("Port: domain \t Tipo: DoS \t Fecha: 29/11/2023 \t Hora: 17:20:18 \t IP: 2800:150:14c:697:*", domain, "DoS", "2023-11-29T17:20:18", "2800:150:14c:697:*")]
		[DataRow("Port: domain \t Tipo: DoS \t Fecha: 29/11/2023 \t Hora: 17:20:18 \t IP: 2800:150:14c:697:*", domain, "DoS", "2023-11-29T17:20:18", "2800:150:14c:697:*")]
		[DataRow("Port: https \t Tipo: DoS \t Fecha: 29/11/2023 \t Hora: 17:20:19 \t IP: 2800:150:14c:697:*", https, "DoS", "2023-11-29T17:20:19", "2800:150:14c:697:*")]
		[DataRow("Port: 54915 \t Tipo: Exploits \t Fecha: 29/11/2023 \t Hora: 17:20:19 \t IP: 192.168.0.4", 54915, "Exploits", "2023-11-29T17:20:19", "192.168.0.4")]
		[DataRow("Port: 54915 \t Tipo: Exploits \t Fecha: 29/11/2023 \t Hora: 17:20:19 \t IP: 192.168.0.4", 54915, "Exploits", "2023-11-29T17:20:19", "192.168.0.4")]
		[DataRow("Port: 54915 \t Tipo: Exploits \t Fecha: 29/11/2023 \t Hora: 17:20:19 \t IP: 192.168.0.4", 54915, "Exploits", "2023-11-29T17:20:19", "192.168.0.4")]
		[DataRow("Port: 54915 \t Tipo: Exploits \t Fecha: 29/11/2023 \t Hora: 17:20:19 \t IP: 192.168.0.4", 54915, "Exploits", "2023-11-29T17:20:19", "192.168.0.4")]
		[DataRow("Port: https \t Tipo: DoS \t Fecha: 29/11/2023 \t Hora: 17:20:19 \t IP: 2800:150:14c:697:*", https, "DoS", "2023-11-29T17:20:19", "2800:150:14c:697:*")]
		[DataRow("Port: https \t Tipo: DoS \t Fecha: 29/11/2023 \t Hora: 17:20:19 \t IP: 2800:150:14c:697:*", https, "DoS", "2023-11-29T17:20:19", "2800:150:14c:697:*")]
		[DataRow("Port: domain \t Tipo: DoS \t Fecha: 29/11/2023 \t Hora: 17:20:19 \t IP: 2800:150:14c:697:*", domain, "DoS", "2023-11-29T17:20:19", "2800:150:14c:697:*")]
		[DataRow("Port: 54915 \t Tipo: Exploits \t Fecha: 29/11/2023 \t Hora: 17:20:19 \t IP: 192.168.0.4", 54915, "Exploits", "2023-11-29T17:20:19", "192.168.0.4")]
		[DataRow("Port: 54915 \t Tipo: Exploits \t Fecha: 29/11/2023 \t Hora: 17:20:19 \t IP: 192.168.0.4", 54915, "Exploits", "2023-11-29T17:20:19", "192.168.0.4")]
		[DataRow("Port: https \t Tipo: DoS \t Fecha: 29/11/2023 \t Hora: 17:20:19 \t IP: 2800:150:14c:697:*", https, "DoS", "2023-11-29T17:20:19", "2800:150:14c:697:*")]
		[DataRow("Port: 51612 \t Tipo: Exploits \t Fecha: 29/11/2023 \t Hora: 17:20:19 \t IP: 162.159.130.234", 51612, "Exploits", "2023-11-29T17:20:19", "162.159.130.234")]
		[DataRow("Port: 54915 \t Tipo: Exploits \t Fecha: 29/11/2023 \t Hora: 17:20:19 \t IP: 192.168.0.4", 54915, "Exploits", "2023-11-29T17:20:19", "192.168.0.4")]
		[DataRow("Port: https \t Tipo: DoS \t Fecha: 29/11/2023 \t Hora: 17:20:19 \t IP: 2800:150:14c:697:*", https, "DoS", "2023-11-29T17:20:19", "2800:150:14c:697:*")]
		[DataRow("Port: 51612 \t Tipo: Exploits \t Fecha: 29/11/2023 \t Hora: 17:20:19 \t IP: 162.159.130.234", 51612, "Exploits", "2023-11-29T17:20:19", "162.159.130.234")]
		[DataRow("Port: 54915 \t Tipo: Exploits \t Fecha: 29/11/2023 \t Hora: 17:20:19 \t IP: 192.168.0.4", 54915, "Exploits", "2023-11-29T17:20:19", "192.168.0.4")]
		[DataRow("Port: https \t Tipo: DoS \t Fecha: 29/11/2023 \t Hora: 17:20:19 \t IP: 2800:150:14c:697:*", https, "DoS", "2023-11-29T17:20:19", "2800:150:14c:697:*")]
		[DataRow("Port: 51612 \t Tipo: Exploits \t Fecha: 29/11/2023 \t Hora: 17:20:19 \t IP: 162.159.130.234", 51612, "Exploits", "2023-11-29T17:20:19", "162.159.130.234")]
		[DataRow("Port: 51612 \t Tipo: Generic \t Fecha: 29/11/2023 \t Hora: 17:20:19 \t IP: 162.159.130.234", 51612, "Generic", "2023-11-29T17:20:19", "162.159.130.234")]
		[DataRow("Port: 51612 \t Tipo: Exploits \t Fecha: 29/11/2023 \t Hora: 17:20:19 \t IP: 162.159.130.234", 51612, "Exploits", "2023-11-29T17:20:19", "162.159.130.234")]
		[DataRow("Port: 54915 \t Tipo: Exploits \t Fecha: 29/11/2023 \t Hora: 17:20:19 \t IP: 192.168.0.4", 54915, "Exploits", "2023-11-29T17:20:19", "192.168.0.4")]
		[DataRow("Port: domain \t Tipo: Generic \t Fecha: 29/11/2023 \t Hora: 17:20:19 \t IP: 2800:150:14c:697:*", domain, "Generic", "2023-11-29T17:20:19", "2800:150:14c:697:*")]
		[DataRow("Port: https \t Tipo: DoS \t Fecha: 29/11/2023 \t Hora: 17:20:19 \t IP: 2800:150:14c:697:*", https, "DoS", "2023-11-29T17:20:19", "2800:150:14c:697:*")]
		[DataRow("Port: 51612 \t Tipo: Generic \t Fecha: 29/11/2023 \t Hora: 17:20:20 \t IP: 162.159.130.234", 51612, "Generic", "2023-11-29T17:20:20", "162.159.130.234")]
		[DataRow("Port: 54915 \t Tipo: Exploits \t Fecha: 29/11/2023 \t Hora: 17:20:20 \t IP: 192.168.0.4", 54915, "Exploits", "2023-11-29T17:20:20", "192.168.0.4")]
		[DataRow("Port: domain \t Tipo: DoS \t Fecha: 29/11/2023 \t Hora: 17:20:20 \t IP: 2800:150:14c:697:*", domain, "DoS", "2023-11-29T17:20:20", "2800:150:14c:697:*")]
		[DataRow("Port: domain \t Tipo: DoS \t Fecha: 29/11/2023 \t Hora: 17:20:20 \t IP: 2800:150:14c:697:*", domain, "DoS", "2023-11-29T17:20:20", "2800:150:14c:697:*")]
		[DataRow("Port: domain \t Tipo: DoS \t Fecha: 29/11/2023 \t Hora: 17:20:20 \t IP: 2800:150:14c:697:*", domain, "DoS", "2023-11-29T17:20:20", "2800:150:14c:697:*")]
		[DataRow("Port: domain \t Tipo: DoS \t Fecha: 29/11/2023 \t Hora: 17:20:20 \t IP: 2800:150:14c:697:*", domain, "DoS", "2023-11-29T17:20:20", "2800:150:14c:697:*")]
		[DataRow("Port: 54915 \t Tipo: Exploits \t Fecha: 29/11/2023 \t Hora: 17:20:20 \t IP: 192.168.0.4", 54915, "Exploits", "2023-11-29T17:20:20", "192.168.0.4")]
		[DataRow("Port: domain \t Tipo: DoS \t Fecha: 29/11/2023 \t Hora: 17:20:20 \t IP: 192.168.0.15", domain, "DoS", "2023-11-29T17:20:20", "192.168.0.15")]
		[DataRow("Port: domain \t Tipo: DoS \t Fecha: 29/11/2023 \t Hora: 17:20:20 \t IP: 2800:150:14c:697:*", domain, "DoS", "2023-11-29T17:20:20", "2800:150:14c:697:*")]
		[DataRow("Port: domain \t Tipo: DoS \t Fecha: 29/11/2023 \t Hora: 17:20:20 \t IP: 2800:150:14c:697:*", domain, "DoS", "2023-11-29T17:20:20", "2800:150:14c:697:*")]
		[DataRow("Port: 51612 \t Tipo: Exploits \t Fecha: 29/11/2023 \t Hora: 17:20:20 \t IP: 162.159.130.234", 51612, "Exploits", "2023-11-29T17:20:20", "162.159.130.234")]
		[DataRow("Port: https \t Tipo: DoS \t Fecha: 29/11/2023 \t Hora: 17:20:20 \t IP: 2800:150:14c:697:*", https, "DoS", "2023-11-29T17:20:20", "2800:150:14c:697:*")]
		[DataRow("Port: 51612 \t Tipo: Generic \t Fecha: 29/11/2023 \t Hora: 17:20:20 \t IP: 162.159.130.234", 51612, "Generic", "2023-11-29T17:20:20", "162.159.130.234")]
		[DataRow("Port: https \t Tipo: DoS \t Fecha: 29/11/2023 \t Hora: 17:20:20 \t IP: 2800:150:14c:697:*", https, "DoS", "2023-11-29T17:20:20", "2800:150:14c:697:*")]
		[DataRow("Port: 54915 \t Tipo: Exploits \t Fecha: 29/11/2023 \t Hora: 17:20:20 \t IP: 192.168.0.4", 54915, "Exploits", "2023-11-29T17:20:20", "192.168.0.4")]
		[DataRow("Port: 54915 \t Tipo: Exploits \t Fecha: 29/11/2023 \t Hora: 17:20:20 \t IP: 192.168.0.4", 54915, "Exploits", "2023-11-29T17:20:20", "192.168.0.4")]
		[DataRow("Port: 54915 \t Tipo: Exploits \t Fecha: 29/11/2023 \t Hora: 17:20:20 \t IP: 192.168.0.4", 54915, "Exploits", "2023-11-29T17:20:20", "192.168.0.4")]
		[DataRow("Port: domain \t Tipo: DoS \t Fecha: 29/11/2023 \t Hora: 17:20:20 \t IP: 2800:150:14c:697:*", domain, "DoS", "2023-11-29T17:20:20", "2800:150:14c:697:*")]
		[DataRow("Port: domain \t Tipo: DoS \t Fecha: 29/11/2023 \t Hora: 17:20:20 \t IP: 2800:150:14c:697:*", domain, "DoS", "2023-11-29T17:20:20", "2800:150:14c:697:*")]
		[DataRow("Port: domain \t Tipo: DoS \t Fecha: 29/11/2023 \t Hora: 17:20:20 \t IP: 2800:150:14c:697:*", domain, "DoS", "2023-11-29T17:20:20", "2800:150:14c:697:*")]
		[DataRow("Port: https \t Tipo: DoS \t Fecha: 29/11/2023 \t Hora: 17:20:20 \t IP: 2800:150:14c:697:*", https, "DoS", "2023-11-29T17:20:20", "2800:150:14c:697:*")]
		[DataRow("Port: 54915 \t Tipo: Exploits \t Fecha: 29/11/2023 \t Hora: 17:20:20 \t IP: 192.168.0.4", 54915, "Exploits", "2023-11-29T17:20:20", "192.168.0.4")]
		[DataRow("Port: https \t Tipo: DoS \t Fecha: 29/11/2023 \t Hora: 17:20:20 \t IP: 2800:150:14c:697:*", https, "DoS", "2023-11-29T17:20:20", "2800:150:14c:697:*")]
		[DataRow("Port: 54915 \t Tipo: Exploits \t Fecha: 29/11/2023 \t Hora: 17:20:20 \t IP: 192.168.0.4", 54915, "Exploits", "2023-11-29T17:20:20", "192.168.0.4")]
		[DataRow("Port: domain \t Tipo: DoS \t Fecha: 29/11/2023 \t Hora: 17:20:20 \t IP: 2800:150:14c:697:*", domain, "DoS", "2023-11-29T17:20:20", "2800:150:14c:697:*")]
		[DataRow("Port: domain \t Tipo: DoS \t Fecha: 29/11/2023 \t Hora: 17:20:20 \t IP: 2800:150:14c:697:*", domain, "DoS", "2023-11-29T17:20:20", "2800:150:14c:697:*")]
		[DataRow("Port: https \t Tipo: Exploits \t Fecha: 29/11/2023 \t Hora: 17:20:20 \t IP: 2800:150:14c:697:*", https, "Exploits", "2023-11-29T17:20:20", "2800:150:14c:697:*")]
		[DataRow("Port: 51612 \t Tipo: Exploits \t Fecha: 29/11/2023 \t Hora: 17:20:20 \t IP: 162.159.130.234", 51612, "Exploits", "2023-11-29T17:20:20", "162.159.130.234")]
		[DataRow("Port: https \t Tipo: DoS \t Fecha: 29/11/2023 \t Hora: 17:20:20 \t IP: 2800:150:14c:697:*", https, "DoS", "2023-11-29T17:20:20", "2800:150:14c:697:*")]
		[DataRow("Port: https \t Tipo: DoS \t Fecha: 29/11/2023 \t Hora: 17:20:20 \t IP: 2800:150:14c:697:*", https, "DoS", "2023-11-29T17:20:20", "2800:150:14c:697:*")]
		[DataRow("Port: 51612 \t Tipo: Generic \t Fecha: 29/11/2023 \t Hora: 17:20:20 \t IP: 162.159.130.234", 51612, "Generic", "2023-11-29T17:20:20", "162.159.130.234")]
		[DataRow("Port: 54915 \t Tipo: Exploits \t Fecha: 29/11/2023 \t Hora: 17:20:21 \t IP: 192.168.0.4", 54915, "Exploits", "2023-11-29T17:20:21", "192.168.0.4")]
		[DataRow("Port: 61088 \t Tipo: Exploits \t Fecha: 29/11/2023 \t Hora: 17:20:21 \t IP: 185.197.24.245", 61088, "Exploits", "2023-11-29T17:20:21", "185.197.24.245")]
		[DataRow("Port: 54915 \t Tipo: Exploits \t Fecha: 29/11/2023 \t Hora: 17:20:21 \t IP: 192.168.0.4", 54915, "Exploits", "2023-11-29T17:20:21", "192.168.0.4")]
		[DataRow("Port: 51612 \t Tipo: Generic \t Fecha: 29/11/2023 \t Hora: 17:20:21 \t IP: 162.159.130.234", 51612, "Generic", "2023-11-29T17:20:21", "162.159.130.234")]
		[DataRow("Port: https \t Tipo: DoS \t Fecha: 29/11/2023 \t Hora: 17:20:21 \t IP: 2800:150:14c:697:*", https, "DoS", "2023-11-29T17:20:21", "2800:150:14c:697:*")]
		[DataRow("Port: https \t Tipo: DoS \t Fecha: 29/11/2023 \t Hora: 17:20:21 \t IP: 2800:150:14c:697:*", https, "DoS", "2023-11-29T17:20:21", "2800:150:14c:697:*")]
		[DataRow("Port: domain \t Tipo: DoS \t Fecha: 29/11/2023 \t Hora: 17:20:21 \t IP: 2800:150:14c:697:*", domain, "DoS", "2023-11-29T17:20:21", "2800:150:14c:697:*")]
		[DataRow("Port: domain \t Tipo: DoS \t Fecha: 29/11/2023 \t Hora: 17:20:21 \t IP: 2800:150:14c:697:*", domain, "DoS", "2023-11-29T17:20:21", "2800:150:14c:697:*")]
		[DataRow("Port: 54915 \t Tipo: Exploits \t Fecha: 29/11/2023 \t Hora: 17:20:21 \t IP: 192.168.0.4", 54915, "Exploits", "2023-11-29T17:20:21", "192.168.0.4")]
		[DataRow("Port: domain \t Tipo: DoS \t Fecha: 29/11/2023 \t Hora: 17:20:21 \t IP: 192.168.0.15", domain, "DoS", "2023-11-29T17:20:21", "192.168.0.15")]
		[DataRow("Port: 51612 \t Tipo: Exploits \t Fecha: 29/11/2023 \t Hora: 17:20:21 \t IP: 162.159.130.234", 51612, "Exploits", "2023-11-29T17:20:21", "162.159.130.234")]
		[DataRow("Port: domain \t Tipo: DoS \t Fecha: 29/11/2023 \t Hora: 17:20:21 \t IP: 2800:150:14c:697:*", domain, "DoS", "2023-11-29T17:20:21", "2800:150:14c:697:*")]
		[DataRow("Port: https \t Tipo: DoS \t Fecha: 29/11/2023 \t Hora: 17:20:21 \t IP: 2800:150:14c:697:*", https, "DoS", "2023-11-29T17:20:21", "2800:150:14c:697:*")]
		[DataRow("Port: https \t Tipo: DoS \t Fecha: 29/11/2023 \t Hora: 17:20:21 \t IP: 2800:150:14c:697:*", https, "DoS", "2023-11-29T17:20:21", "2800:150:14c:697:*")]
		[DataRow("Port: domain \t Tipo: DoS \t Fecha: 29/11/2023 \t Hora: 17:20:21 \t IP: 2800:150:14c:697:*", domain, "DoS", "2023-11-29T17:20:21", "2800:150:14c:697:*")]
		[DataRow("Port: domain \t Tipo: DoS \t Fecha: 29/11/2023 \t Hora: 17:20:21 \t IP: 2800:150:14c:697:*", domain, "DoS", "2023-11-29T17:20:21", "2800:150:14c:697:*")]
		[DataRow("Port: domain \t Tipo: DoS \t Fecha: 29/11/2023 \t Hora: 17:20:21 \t IP: 2800:150:14c:697:*", domain, "DoS", "2023-11-29T17:20:21", "2800:150:14c:697:*")]
		[DataRow("Port: domain \t Tipo: DoS \t Fecha: 29/11/2023 \t Hora: 17:20:21 \t IP: 2800:150:14c:697:*", domain, "DoS", "2023-11-29T17:20:21", "2800:150:14c:697:*")]
		[DataRow("Port: domain \t Tipo: DoS \t Fecha: 29/11/2023 \t Hora: 17:20:21 \t IP: 2800:150:14c:697:*", domain, "DoS", "2023-11-29T17:20:21", "2800:150:14c:697:*")]
		[DataRow("Port: domain \t Tipo: DoS \t Fecha: 29/11/2023 \t Hora: 17:20:21 \t IP: 2800:150:14c:697:*", domain, "DoS", "2023-11-29T17:20:21", "2800:150:14c:697:*")]
		[DataRow("Port: domain \t Tipo: DoS \t Fecha: 29/11/2023 \t Hora: 17:20:21 \t IP: 2800:150:14c:697:*", domain, "DoS", "2023-11-29T17:20:21", "2800:150:14c:697:*")]
		[DataRow("Port: domain \t Tipo: DoS \t Fecha: 29/11/2023 \t Hora: 17:20:21 \t IP: 2800:150:14c:697:*", domain, "DoS", "2023-11-29T17:20:21", "2800:150:14c:697:*")]
		[DataRow("Port: domain \t Tipo: DoS \t Fecha: 29/11/2023 \t Hora: 17:20:21 \t IP: 2800:150:14c:697:*", domain, "DoS", "2023-11-29T17:20:21", "2800:150:14c:697:*")]
		[DataRow("Port: https \t Tipo: DoS \t Fecha: 29/11/2023 \t Hora: 17:20:21 \t IP: 2800:150:14c:697:*", https, "DoS", "2023-11-29T17:20:21", "2800:150:14c:697:*")]
		[DataRow("Port: 51612 \t Tipo: Exploits \t Fecha: 29/11/2023 \t Hora: 17:20:22 \t IP: 162.159.130.234", 51612, "Exploits", "2023-11-29T17:20:22", "162.159.130.234")]
		[DataRow("Port: 54915 \t Tipo: Exploits \t Fecha: 29/11/2023 \t Hora: 17:20:22 \t IP: 192.168.0.4", 54915, "Exploits", "2023-11-29T17:20:22", "192.168.0.4")]
		[DataRow("Port: https \t Tipo: DoS \t Fecha: 29/11/2023 \t Hora: 17:20:22 \t IP: 2800:150:14c:697:*", https, "DoS", "2023-11-29T17:20:22", "2800:150:14c:697:*")]
		[DataRow("Port: 54915 \t Tipo: Exploits \t Fecha: 29/11/2023 \t Hora: 17:20:22 \t IP: 192.168.0.4", 54915, "Exploits", "2023-11-29T17:20:22", "192.168.0.4")]
		[DataRow("Port: https \t Tipo: DoS \t Fecha: 29/11/2023 \t Hora: 17:20:22 \t IP: 2800:150:14c:697:*", https, "DoS", "2023-11-29T17:20:22", "2800:150:14c:697:*")]
		[DataRow("Port: https \t Tipo: DoS \t Fecha: 29/11/2023 \t Hora: 17:20:22 \t IP: 2800:150:14c:697:*", https, "DoS", "2023-11-29T17:20:22", "2800:150:14c:697:*")]
		[DataRow("Port: 54915 \t Tipo: Exploits \t Fecha: 29/11/2023 \t Hora: 17:20:22 \t IP: 192.168.0.4", 54915, "Exploits", "2023-11-29T17:20:22", "192.168.0.4")]
		[DataRow("Port: 50001 \t Tipo: Exploits \t Fecha: 29/11/2023 \t Hora: 17:20:22 \t IP: 192.168.0.4", 50001, "Exploits", "2023-11-29T17:20:22", "192.168.0.4")]
		[DataRow("Port: https \t Tipo: DoS \t Fecha: 29/11/2023 \t Hora: 17:20:22 \t IP: 2800:150:14c:697:*", https, "DoS", "2023-11-29T17:20:22", "2800:150:14c:697:*")]
		[DataRow("Port: https \t Tipo: DoS \t Fecha: 29/11/2023 \t Hora: 17:20:22 \t IP: 2800:150:14c:697:*", https, "DoS", "2023-11-29T17:20:22", "2800:150:14c:697:*")]
		[DataRow("Port: 51612 \t Tipo: Exploits \t Fecha: 29/11/2023 \t Hora: 17:20:22 \t IP: 162.159.130.234", 51612, "Exploits", "2023-11-29T17:20:22", "162.159.130.234")]
		[DataRow("Port: 54915 \t Tipo: Exploits \t Fecha: 29/11/2023 \t Hora: 17:20:22 \t IP: 192.168.0.4", 54915, "Exploits", "2023-11-29T17:20:22", "192.168.0.4")]
		[DataRow("Port: https \t Tipo: DoS \t Fecha: 29/11/2023 \t Hora: 17:20:22 \t IP: 2800:150:14c:697:*", https, "DoS", "2023-11-29T17:20:22", "2800:150:14c:697:*")]
		[DataRow("Port: 27018 \t Tipo: Exploits \t Fecha: 29/11/2023 \t Hora: 17:20:22 \t IP: 192.168.0.4", 27018, "Exploits", "2023-11-29T17:20:22", "192.168.0.4")]
		[DataRow("Port: domain \t Tipo: DoS \t Fecha: 29/11/2023 \t Hora: 17:20:22 \t IP: 2800:150:14c:697:*", domain, "DoS", "2023-11-29T17:20:22", "2800:150:14c:697:*")]
		[DataRow("Port: domain \t Tipo: Generic \t Fecha: 29/11/2023 \t Hora: 17:20:23 \t IP: 2800:150:14c:697:*", domain, "Generic", "2023-11-29T17:20:23", "2800:150:14c:697:*")]
		[DataRow("Port: 51612 \t Tipo: Exploits \t Fecha: 29/11/2023 \t Hora: 17:20:23 \t IP: 162.159.130.234", 51612, "Exploits", "2023-11-29T17:20:23", "162.159.130.234")]
		[DataRow("Port: 54915 \t Tipo: Exploits \t Fecha: 29/11/2023 \t Hora: 17:20:23 \t IP: 192.168.0.4", 54915, "Exploits", "2023-11-29T17:20:23", "192.168.0.4")]
		[DataRow("Port: https \t Tipo: DoS \t Fecha: 29/11/2023 \t Hora: 17:20:23 \t IP: 2800:150:14c:697:*", https, "DoS", "2023-11-29T17:20:23", "2800:150:14c:697:*")]
		[DataRow("Port: domain \t Tipo: DoS \t Fecha: 29/11/2023 \t Hora: 17:20:23 \t IP: 2800:150:14c:697:*", domain, "DoS", "2023-11-29T17:20:23", "2800:150:14c:697:*")]
		[DataRow("Port: domain \t Tipo: DoS \t Fecha: 29/11/2023 \t Hora: 17:20:23 \t IP: 2800:150:14c:697:*", domain, "DoS", "2023-11-29T17:20:23", "2800:150:14c:697:*")]
		[DataRow("Port: 51612 \t Tipo: Exploits \t Fecha: 29/11/2023 \t Hora: 17:20:23 \t IP: 162.159.130.234", 51612, "Exploits", "2023-11-29T17:20:23", "162.159.130.234")]
		[DataRow("Port: 51612 \t Tipo: Generic \t Fecha: 29/11/2023 \t Hora: 17:20:23 \t IP: 162.159.130.234", 51612, "Generic", "2023-11-29T17:20:23", "162.159.130.234")]
		[DataRow("Port: 50004 \t Tipo: Exploits \t Fecha: 29/11/2023 \t Hora: 17:20:23 \t IP: 192.168.0.4", 50004, "Exploits", "2023-11-29T17:20:23", "192.168.0.4")]
		[DataRow("Port: domain \t Tipo: DoS \t Fecha: 29/11/2023 \t Hora: 17:20:23 \t IP: 2800:150:14c:697:*", domain, "DoS", "2023-11-29T17:20:23", "2800:150:14c:697:*")]
		[DataRow("Port: domain \t Tipo: DoS \t Fecha: 29/11/2023 \t Hora: 17:20:23 \t IP: 2800:150:14c:697:*", domain, "DoS", "2023-11-29T17:20:23", "2800:150:14c:697:*")]
		[DataRow("Port: domain \t Tipo: DoS \t Fecha: 29/11/2023 \t Hora: 17:20:23 \t IP: 2800:150:14c:697:*", domain, "DoS", "2023-11-29T17:20:23", "2800:150:14c:697:*")]
		[DataRow("Port: domain \t Tipo: DoS \t Fecha: 29/11/2023 \t Hora: 17:20:23 \t IP: 2800:150:14c:697:*", domain, "DoS", "2023-11-29T17:20:23", "2800:150:14c:697:*")]
		[DataRow("Port: 54915 \t Tipo: Exploits \t Fecha: 29/11/2023 \t Hora: 17:20:23 \t IP: 192.168.0.4", 54915, "Exploits", "2023-11-29T17:20:23", "192.168.0.4")]
		[DataRow("Port: https \t Tipo: DoS \t Fecha: 29/11/2023 \t Hora: 17:20:23 \t IP: 2800:150:14c:697:*", https, "DoS", "2023-11-29T17:20:23", "2800:150:14c:697:*")]
		[DataRow("Port: 54915 \t Tipo: Exploits \t Fecha: 29/11/2023 \t Hora: 17:20:23 \t IP: 192.168.0.4", 54915, "Exploits", "2023-11-29T17:20:23", "192.168.0.4")]
		[DataRow("Port: domain \t Tipo: DoS \t Fecha: 29/11/2023 \t Hora: 17:20:23 \t IP: 2800:150:14c:697:*", domain, "DoS", "2023-11-29T17:20:23", "2800:150:14c:697:*")]
		[DataRow("Port: domain \t Tipo: DoS \t Fecha: 29/11/2023 \t Hora: 17:20:23 \t IP: 2800:150:14c:697:*", domain, "DoS", "2023-11-29T17:20:23", "2800:150:14c:697:*")]
		[DataRow("Port: domain \t Tipo: DoS \t Fecha: 29/11/2023 \t Hora: 17:20:23 \t IP: 2800:150:14c:697:*", domain, "DoS", "2023-11-29T17:20:23", "2800:150:14c:697:*")]
		[DataRow("Port: domain \t Tipo: DoS \t Fecha: 29/11/2023 \t Hora: 17:20:23 \t IP: 2800:150:14c:697:*", domain, "DoS", "2023-11-29T17:20:23", "2800:150:14c:697:*")]
		[DataRow("Port: https \t Tipo: Exploits \t Fecha: 29/11/2023 \t Hora: 17:20:23 \t IP: 192.168.0.4", https, "Exploits", "2023-11-29T17:20:23", "192.168.0.4")]
		[DataRow("Port: https \t Tipo: DoS \t Fecha: 29/11/2023 \t Hora: 17:20:24 \t IP: 2800:150:14c:697:*", https, "DoS", "2023-11-29T17:20:24", "2800:150:14c:697:*")]
		[DataRow("Port: 27018 \t Tipo: Exploits \t Fecha: 29/11/2023 \t Hora: 17:20:24 \t IP: 192.168.0.4", 27018, "Exploits", "2023-11-29T17:20:24", "192.168.0.4")]
		[DataRow("Port: 51612 \t Tipo: Exploits \t Fecha: 29/11/2023 \t Hora: 17:20:24 \t IP: 162.159.130.234", 51612, "Exploits", "2023-11-29T17:20:24", "162.159.130.234")]
		[DataRow("Port: 54915 \t Tipo: Exploits \t Fecha: 29/11/2023 \t Hora: 17:20:24 \t IP: 192.168.0.4", 54915, "Exploits", "2023-11-29T17:20:24", "192.168.0.4")]
		[DataRow("Port: https \t Tipo: DoS \t Fecha: 29/11/2023 \t Hora: 17:20:24 \t IP: 2800:150:14c:697:*", https, "DoS", "2023-11-29T17:20:24", "2800:150:14c:697:*")]
		[DataRow("Port: 51612 \t Tipo: Exploits \t Fecha: 29/11/2023 \t Hora: 17:20:24 \t IP: 162.159.130.234", 51612, "Exploits", "2023-11-29T17:20:24", "162.159.130.234")]
		[DataRow("Port: https \t Tipo: DoS \t Fecha: 29/11/2023 \t Hora: 17:20:24 \t IP: 2800:150:14c:697:*", https, "DoS", "2023-11-29T17:20:24", "2800:150:14c:697:*")]
		[DataRow("Port: 51612 \t Tipo: Exploits \t Fecha: 29/11/2023 \t Hora: 17:20:24 \t IP: 162.159.130.234", 51612, "Exploits", "2023-11-29T17:20:24", "162.159.130.234")]
		[DataRow("Port: https \t Tipo: DoS \t Fecha: 29/11/2023 \t Hora: 17:20:24 \t IP: 2800:150:14c:697:*", https, "DoS", "2023-11-29T17:20:24", "2800:150:14c:697:*")]
		[DataRow("Port: 54915 \t Tipo: Exploits \t Fecha: 29/11/2023 \t Hora: 17:20:24 \t IP: 192.168.0.4", 54915, "Exploits", "2023-11-29T17:20:24", "192.168.0.4")]
		[DataRow("Port: 27018 \t Tipo: Exploits \t Fecha: 29/11/2023 \t Hora: 17:20:24 \t IP: 192.168.0.4", 27018, "Exploits", "2023-11-29T17:20:24", "192.168.0.4")]
		[DataRow("Port: 54915 \t Tipo: Exploits \t Fecha: 29/11/2023 \t Hora: 17:20:24 \t IP: 192.168.0.4", 54915, "Exploits", "2023-11-29T17:20:24", "192.168.0.4")]
		[DataRow("Port: 27018 \t Tipo: Exploits \t Fecha: 29/11/2023 \t Hora: 17:20:24 \t IP: 192.168.0.4", 27018, "Exploits", "2023-11-29T17:20:24", "192.168.0.4")]
		[DataRow("Port: 54915 \t Tipo: Exploits \t Fecha: 29/11/2023 \t Hora: 17:20:24 \t IP: 192.168.0.4", 54915, "Exploits", "2023-11-29T17:20:24", "192.168.0.4")]
		[DataRow("Port: https \t Tipo: DoS \t Fecha: 29/11/2023 \t Hora: 17:20:24 \t IP: 2800:150:14c:697:*", https, "DoS", "2023-11-29T17:20:24", "2800:150:14c:697:*")]
		[DataRow("Port: 54915 \t Tipo: Exploits \t Fecha: 29/11/2023 \t Hora: 17:20:24 \t IP: 192.168.0.4", 54915, "Exploits", "2023-11-29T17:20:24", "192.168.0.4")]
		[DataRow("Port: 51612 \t Tipo: Exploits \t Fecha: 29/11/2023 \t Hora: 17:20:24 \t IP: 162.159.130.234", 51612, "Exploits", "2023-11-29T17:20:24", "162.159.130.234")]
		[DataRow("Port: https \t Tipo: DoS \t Fecha: 29/11/2023 \t Hora: 17:20:24 \t IP: 2800:150:14c:697:*", https, "DoS", "2023-11-29T17:20:24", "2800:150:14c:697:*")]
		[DataRow("Port: 54915 \t Tipo: Exploits \t Fecha: 29/11/2023 \t Hora: 17:20:24 \t IP: 192.168.0.4", 54915, "Exploits", "2023-11-29T17:20:24", "192.168.0.4")]
		[DataRow("Port: https \t Tipo: DoS \t Fecha: 29/11/2023 \t Hora: 17:20:24 \t IP: 2800:150:14c:697:*", https, "DoS", "2023-11-29T17:20:24", "2800:150:14c:697:*")]
		[DataRow("Port: domain \t Tipo: DoS \t Fecha: 29/11/2023 \t Hora: 17:20:24 \t IP: 2800:150:14c:697:*", domain, "DoS", "2023-11-29T17:20:24", "2800:150:14c:697:*")]
		[DataRow("Port: domain \t Tipo: DoS \t Fecha: 29/11/2023 \t Hora: 17:20:24 \t IP: 2800:150:14c:697:*", domain, "DoS", "2023-11-29T17:20:24", "2800:150:14c:697:*")]
		[DataRow("Port: 54915 \t Tipo: Exploits \t Fecha: 29/11/2023 \t Hora: 17:20:25 \t IP: 192.168.0.4", 54915, "Exploits", "2023-11-29T17:20:25", "192.168.0.4")]
		[DataRow("Port: https \t Tipo: DoS \t Fecha: 29/11/2023 \t Hora: 17:20:25 \t IP: 2800:150:14c:697:*", https, "DoS", "2023-11-29T17:20:25", "2800:150:14c:697:*")]
		[DataRow("Port: 52609 \t Tipo: Exploits \t Fecha: 29/11/2023 \t Hora: 17:20:25 \t IP: 162.159.130.234", 52609, "Exploits", "2023-11-29T17:20:25", "162.159.130.234")]
		[DataRow("Port: domain \t Tipo: DoS \t Fecha: 29/11/2023 \t Hora: 17:20:25 \t IP: 2800:150:14c:697:*", domain, "DoS", "2023-11-29T17:20:25", "2800:150:14c:697:*")]
		[DataRow("Port: domain \t Tipo: DoS \t Fecha: 29/11/2023 \t Hora: 17:20:25 \t IP: 2800:150:14c:697:*", domain, "DoS", "2023-11-29T17:20:25", "2800:150:14c:697:*")]
		[DataRow("Port: 27018 \t Tipo: Exploits \t Fecha: 29/11/2023 \t Hora: 17:20:25 \t IP: 192.168.0.4", 27018, "Exploits", "2023-11-29T17:20:25", "192.168.0.4")]
		[DataRow("Port: 54915 \t Tipo: Exploits \t Fecha: 29/11/2023 \t Hora: 17:20:25 \t IP: 192.168.0.4", 54915, "Exploits", "2023-11-29T17:20:25", "192.168.0.4")]
		[DataRow("Port: 54760 \t Tipo: Exploits \t Fecha: 29/11/2023 \t Hora: 17:20:25 \t IP: 162.159.134.234", 54760, "Exploits", "2023-11-29T17:20:25", "162.159.134.234")]
		[DataRow("Port: 54915 \t Tipo: Exploits \t Fecha: 29/11/2023 \t Hora: 17:20:25 \t IP: 192.168.0.4", 54915, "Exploits", "2023-11-29T17:20:25", "192.168.0.4")]
		[DataRow("Port: https \t Tipo: DoS \t Fecha: 29/11/2023 \t Hora: 17:20:25 \t IP: 2800:150:14c:697:*", https, "DoS", "2023-11-29T17:20:25", "2800:150:14c:697:*")]
		[DataRow("Port: 54760 \t Tipo: Exploits \t Fecha: 29/11/2023 \t Hora: 17:20:25 \t IP: 162.159.134.234", 54760, "Exploits", "2023-11-29T17:20:25", "162.159.134.234")]
		[DataRow("Port: https \t Tipo: DoS \t Fecha: 29/11/2023 \t Hora: 17:20:25 \t IP: 2800:150:14c:697:*", https, "DoS", "2023-11-29T17:20:25", "2800:150:14c:697:*")]
		[DataRow("Port: 54760 \t Tipo: Exploits \t Fecha: 29/11/2023 \t Hora: 17:20:25 \t IP: 162.159.134.234", 54760, "Exploits", "2023-11-29T17:20:25", "162.159.134.234")]
		[DataRow("Port: 54915 \t Tipo: Exploits \t Fecha: 29/11/2023 \t Hora: 17:20:25 \t IP: 192.168.0.4", 54915, "Exploits", "2023-11-29T17:20:25", "192.168.0.4")]
		[DataRow("Port: https \t Tipo: DoS \t Fecha: 29/11/2023 \t Hora: 17:20:25 \t IP: 2800:150:14c:697:*", https, "DoS", "2023-11-29T17:20:25", "2800:150:14c:697:*")]
		[DataRow("Port: https \t Tipo: DoS \t Fecha: 29/11/2023 \t Hora: 17:20:25 \t IP: 2800:150:14c:697:*", https, "DoS", "2023-11-29T17:20:25", "2800:150:14c:697:*")]
		[DataRow("Port: 54915 \t Tipo: Exploits \t Fecha: 29/11/2023 \t Hora: 17:20:25 \t IP: 192.168.0.4", 54915, "Exploits", "2023-11-29T17:20:25", "192.168.0.4")]
		[DataRow("Port: 54760 \t Tipo: Generic \t Fecha: 29/11/2023 \t Hora: 17:20:25 \t IP: 162.159.134.234", 54760, "Generic", "2023-11-29T17:20:25", "162.159.134.234")]
		[DataRow("Port: 54915 \t Tipo: Exploits \t Fecha: 29/11/2023 \t Hora: 17:20:26 \t IP: 192.168.0.4", 54915, "Exploits", "2023-11-29T17:20:26", "192.168.0.4")]
		[DataRow("Port: 54915 \t Tipo: Exploits \t Fecha: 29/11/2023 \t Hora: 17:20:26 \t IP: 192.168.0.4", 54915, "Exploits", "2023-11-29T17:20:26", "192.168.0.4")]
		[DataRow("Port: domain \t Tipo: DoS \t Fecha: 29/11/2023 \t Hora: 17:20:26 \t IP: 2800:150:14c:697:*", domain, "DoS", "2023-11-29T17:20:26", "2800:150:14c:697:*")]
		[DataRow("Port: domain \t Tipo: DoS \t Fecha: 29/11/2023 \t Hora: 17:20:26 \t IP: 2800:150:14c:697:*", domain, "DoS", "2023-11-29T17:20:26", "2800:150:14c:697:*")]
		[DataRow("Port: https \t Tipo: DoS \t Fecha: 29/11/2023 \t Hora: 17:20:26 \t IP: 2800:150:14c:697:*", https, "DoS", "2023-11-29T17:20:26", "2800:150:14c:697:*")]
		[DataRow("Port: 54760 \t Tipo: Generic \t Fecha: 29/11/2023 \t Hora: 17:20:26 \t IP: 162.159.134.234", 54760, "Generic", "2023-11-29T17:20:26", "162.159.134.234")]
		[DataRow("Port: https \t Tipo: DoS \t Fecha: 29/11/2023 \t Hora: 17:20:26 \t IP: 2800:150:14c:697:*", https, "DoS", "2023-11-29T17:20:26", "2800:150:14c:697:*")]
		[DataRow("Port: 54760 \t Tipo: Exploits \t Fecha: 29/11/2023 \t Hora: 17:20:26 \t IP: 162.159.134.234", 54760, "Exploits", "2023-11-29T17:20:26", "162.159.134.234")]
		[DataRow("Port: 59443 \t Tipo: DoS \t Fecha: 29/11/2023 \t Hora: 17:20:26 \t IP: 190.217.33.69", 59443, "DoS", "2023-11-29T17:20:26", "190.217.33.69")]
		[DataRow("Port: 54760 \t Tipo: Exploits \t Fecha: 29/11/2023 \t Hora: 17:20:26 \t IP: 162.159.134.234", 54760, "Exploits", "2023-11-29T17:20:26", "162.159.134.234")]
		[DataRow("Port: https \t Tipo: DoS \t Fecha: 29/11/2023 \t Hora: 17:20:26 \t IP: 2800:150:14c:697:*", https, "DoS", "2023-11-29T17:20:26", "2800:150:14c:697:*")]
		[DataRow("Port: 54915 \t Tipo: Exploits \t Fecha: 29/11/2023 \t Hora: 17:20:26 \t IP: 192.168.0.4", 54915, "Exploits", "2023-11-29T17:20:26", "192.168.0.4")]
		[DataRow("Port: https \t Tipo: DoS \t Fecha: 29/11/2023 \t Hora: 17:20:26 \t IP: 2800:150:14c:697:*", https, "DoS", "2023-11-29T17:20:26", "2800:150:14c:697:*")]
		[DataRow("Port: https \t Tipo: Exploits \t Fecha: 29/11/2023 \t Hora: 17:20:26 \t IP: 10.0.3.15", https, "Exploits", "2023-11-29T17:20:26", "10.0.3.15")]
		public async Task Test_02_NetworkRecords(string Input, double Port, string Type, string XmlTimestamp, string Endpoint)
		{
			ConsoleOut.WriteLine("Input:");
			ConsoleOut.WriteLine(Input);
			ConsoleOut.WriteLine();

			Assert.IsTrue(networkRecordMaps.TryMap(Input, out string Harmonized));

			ConsoleOut.WriteLine("Harmonized:");
			ConsoleOut.WriteLine(Harmonized);
			ConsoleOut.WriteLine();

			object Obj = await Expression.EvalAsync(Harmonized);
			Dictionary<string, IElement> Record = Obj as Dictionary<string, IElement>;
			Assert.IsNotNull(Record);

			Assert.AreEqual(Port, Record["Port"]?.AssociatedObjectValue);
			Assert.AreEqual(Type, Record["Type"]?.AssociatedObjectValue);
			Assert.AreEqual(XML.ParseDateTime(XmlTimestamp), Record["Timestamp"]?.AssociatedObjectValue);
			Assert.AreEqual(Endpoint, Record["Endpoint"]?.AssociatedObjectValue);
		}
	}
}
