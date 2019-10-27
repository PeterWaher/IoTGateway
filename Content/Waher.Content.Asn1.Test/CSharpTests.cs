using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Emit;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Waher.Content.Asn1.Test
{
	[TestClass]
	public class CSharpTests
	{
		public static string GenerateCSharp(string FileName)
		{
			string BaseNamespace = "Test";
			Asn1Document Doc = ParsingTests.ParseAsn1Document(FileName);
			CSharpExportSettings Settings = new CSharpExportSettings(BaseNamespace,
				"Examples\\SNMPv1", "Examples\\IEEE1451");
			string CSharp = Doc.ExportCSharp(Settings);
			List<SyntaxTree> Modules = new List<SyntaxTree>() { CSharpSyntaxTree.ParseText(CSharp) };

			foreach (string ImportedModule in Settings.Modules)
			{
				string CSharp2 = Settings.GetCode(ImportedModule);
				Modules.Add(CSharpSyntaxTree.ParseText(CSharp2));
			}

			Dictionary<string, bool> Dependencies = new Dictionary<string, bool>()
			{
				{ GetLocation(typeof(object)), true },
				{ Path.Combine(Path.GetDirectoryName(GetLocation(typeof(object))), "System.Runtime.dll"), true },
				{ Path.Combine(Path.GetDirectoryName(GetLocation(typeof(Encoding))), "System.Text.Encoding.dll"), true },
				{ Path.Combine(Path.GetDirectoryName(GetLocation(typeof(MemoryStream))), "System.IO.dll"), true },
				{ Path.Combine(Path.GetDirectoryName(GetLocation(typeof(MemoryStream))), "System.Runtime.Extensions.dll"), true },
				{ Path.Combine(Path.GetDirectoryName(GetLocation(typeof(Task))), "System.Threading.Tasks.dll"), true },
				{ GetLocation(typeof(Asn1Document)), true }
			};

			List<MetadataReference> References = new List<MetadataReference>();

			foreach (string Location in Dependencies.Keys)
			{
				if (!string.IsNullOrEmpty(Location))
					References.Add(MetadataReference.CreateFromFile(Location));
			}

			CSharpCompilation Compilation = CSharpCompilation.Create(
				BaseNamespace, Modules.ToArray(), References, 
				new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));

			MemoryStream Output = new MemoryStream();
			MemoryStream PdbOutput = new MemoryStream();

			EmitResult CompilerResults = Compilation.Emit(Output, pdbStream: PdbOutput);

			if (!CompilerResults.Success)
			{
				StringBuilder sb = new StringBuilder();

				foreach (Diagnostic Error in CompilerResults.Diagnostics)
				{
					sb.AppendLine();
					sb.Append(Error.Location.ToString());
					sb.Append(": ");
					sb.Append(Error.GetMessage());
				}

				sb.AppendLine();
				sb.AppendLine();
				sb.AppendLine("Code generated:");
				sb.AppendLine();
				sb.AppendLine(CSharp);

				foreach (string ImportedModule in Settings.Modules)
				{
					string CSharp2 = Settings.GetCode(ImportedModule);

					sb.AppendLine();
					sb.AppendLine(new string('-', 80));
					sb.AppendLine();
					sb.AppendLine(CSharp2);
				}

				throw new Exception(sb.ToString());
			}

			Console.Out.WriteLine(CSharp);

			return CSharp;
		}

		private static string GetLocation(Type T)
		{
			System.Reflection.TypeInfo TI = T.GetTypeInfo();
			string s = TI.Assembly.Location;

			if (!string.IsNullOrEmpty(s))
				return s;

			return Path.Combine(Path.GetDirectoryName(GetLocation(typeof(CSharpTests))), TI.Module.ScopeName);
		}

		[TestMethod]
		public void Test_01_WorldSchema()
		{
			GenerateCSharp("World-Schema.asn1");
		}

		[TestMethod]
		public void Test_02_MyShopPurchaseOrders()
		{
			GenerateCSharp("MyShopPurchaseOrders.asn1");
		}

		[TestMethod]
		public void Test_03_RFC1155()
		{
			GenerateCSharp("SNMPv1\\RFC1155-SMI.asn1");
		}

		[TestMethod]
		public void Test_04_RFC1157()
		{
			GenerateCSharp("SNMPv1\\RFC1157-SNMP.asn1");
		}

		[TestMethod]
		public void Test_05_1451_1()
		{
			GenerateCSharp("IEEE1451\\P21451-N1-T1-MIB.txt.asn1");
		}

		[TestMethod]
		public void Test_06_1451_2()
		{
			GenerateCSharp("IEEE1451\\P21451-N1-T2-MIB.txt.asn1");
		}

		[TestMethod]
		public void Test_07_1451_3()
		{
			GenerateCSharp("IEEE1451\\P21451-N1-T3-MIB.txt.asn1");
		}

	}
}
