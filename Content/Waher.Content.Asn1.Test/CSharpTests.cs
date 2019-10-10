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
		public static string GenerateCSharp(string FileName, string Namespace)
		{
			Asn1Document Doc = ParsingTests.ParseAsn1Document(FileName);
			string CSharp = Doc.ExportCSharp(new CSharpExportSettings(Namespace));

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

			CSharpCompilation Compilation = CSharpCompilation.Create(Namespace,
				new SyntaxTree[] { CSharpSyntaxTree.ParseText(CSharp) },
				References, new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));

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
			GenerateCSharp("World-Schema.asn1", "Test.WorldSchema");
		}

		[TestMethod]
		public void Test_02_MyShopPurchaseOrders()
		{
			GenerateCSharp("MyShopPurchaseOrders.asn1", "Test.MyShopPurchaseOrders");
		}

		[TestMethod]
		public void Test_03_RFC1155()
		{
			GenerateCSharp("SNMPv1\\RFC1155-SMI.asn1", "Test.SNMP");
		}

		[TestMethod]
		public void Test_04_RFC1157()
		{
			GenerateCSharp("SNMPv1\\RFC1157-SNMP.asn1", "Test.SNMP");
		}

	}
}
