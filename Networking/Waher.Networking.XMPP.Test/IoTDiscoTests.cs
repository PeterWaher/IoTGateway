using System;
using System.Threading;
using System.Reflection;
using System.Collections.Generic;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Waher.Networking.XMPP.Provisioning;
using Waher.Networking.XMPP.Provisioning.SearchOperators;

namespace Waher.Networking.XMPP.Test
{
	[TestClass]
	public class IoTDiscoTests
	{
		[TestMethod]
		public void Test_01_ClaimUri()
		{
			MetaDataTag[] Tags = ThingRegistryClient.DecodeIoTDiscoClaimURI("iotdisco:SN=98734238472634;MAN=www.example.org;MODEL=Device;#V=1.0;KEY=3453485763440213840928;R=discovery.example.org");

			Assert.AreEqual(6, Tags.Length);

			Assert.AreEqual("SN", Tags[0].Name);
			Assert.AreEqual("98734238472634", ((MetaDataStringTag)Tags[0]).Value);

			Assert.AreEqual("MAN", Tags[1].Name);
			Assert.AreEqual("www.example.org", ((MetaDataStringTag)Tags[1]).Value);

			Assert.AreEqual("MODEL", Tags[2].Name);
			Assert.AreEqual("Device", ((MetaDataStringTag)Tags[2]).Value);

			Assert.AreEqual("V", Tags[3].Name);
			Assert.AreEqual(1.0, ((MetaDataNumericTag)Tags[3]).Value);

			Assert.AreEqual("KEY", Tags[4].Name);
			Assert.AreEqual("3453485763440213840928", ((MetaDataStringTag)Tags[4]).Value);

			Assert.AreEqual("R", Tags[5].Name);
			Assert.AreEqual("discovery.example.org", ((MetaDataStringTag)Tags[5]).Value);
		}

		[TestMethod]
		public void Test_02_ClaimUriEscape()
		{
			MetaDataTag[] Tags = ThingRegistryClient.DecodeIoTDiscoClaimURI("iotdisco:SN=98734238472634;MAN=www.example.org;MODEL=Device;#V=1.0;KEY=3453485763440213840928;R=\\discovery\\.example\\;.org");

			Assert.AreEqual(6, Tags.Length);

			Assert.AreEqual("SN", Tags[0].Name);
			Assert.AreEqual("98734238472634", ((MetaDataStringTag)Tags[0]).Value);

			Assert.AreEqual("MAN", Tags[1].Name);
			Assert.AreEqual("www.example.org", ((MetaDataStringTag)Tags[1]).Value);

			Assert.AreEqual("MODEL", Tags[2].Name);
			Assert.AreEqual("Device", ((MetaDataStringTag)Tags[2]).Value);

			Assert.AreEqual("V", Tags[3].Name);
			Assert.AreEqual(1.0, ((MetaDataNumericTag)Tags[3]).Value);

			Assert.AreEqual("KEY", Tags[4].Name);
			Assert.AreEqual("3453485763440213840928", ((MetaDataStringTag)Tags[4]).Value);

			Assert.AreEqual("R", Tags[5].Name);
			Assert.AreEqual("discovery.example;.org", ((MetaDataStringTag)Tags[5]).Value);
		}

		[TestMethod]
		public void Test_03_ClaimUriEmptyValues()
		{
			MetaDataTag[] Tags = ThingRegistryClient.DecodeIoTDiscoClaimURI("iotdisco:SN=98734238472634;MAN=www.example.org;MODEL=Device;#V=1.0;KEY=;R=");

			Assert.AreEqual(6, Tags.Length);

			Assert.AreEqual("SN", Tags[0].Name);
			Assert.AreEqual("98734238472634", ((MetaDataStringTag)Tags[0]).Value);

			Assert.AreEqual("MAN", Tags[1].Name);
			Assert.AreEqual("www.example.org", ((MetaDataStringTag)Tags[1]).Value);

			Assert.AreEqual("MODEL", Tags[2].Name);
			Assert.AreEqual("Device", ((MetaDataStringTag)Tags[2]).Value);

			Assert.AreEqual("V", Tags[3].Name);
			Assert.AreEqual(1.0, ((MetaDataNumericTag)Tags[3]).Value);

			Assert.AreEqual("KEY", Tags[4].Name);
			Assert.AreEqual(string.Empty, ((MetaDataStringTag)Tags[4]).Value);

			Assert.AreEqual("R", Tags[5].Name);
			Assert.AreEqual(string.Empty, ((MetaDataStringTag)Tags[5]).Value);
		}

		[TestMethod]
		public void Test_04_SearchUri_1()
		{
			IEnumerable<SearchOperator> Operators = ThingRegistryClient.DecodeIoTDiscoURI("iotdisco:MAN=www.example.org;MODEL=Device;SN~*9873*;#V>=1.0;#V<2;#LON>=-72;#LON<=-70;#LAT>=-34;#LAT<=-33");
			this.Test_SearchUri_1(Operators);
		}

		[TestMethod]
		public void Test_05_SearchUri_1_ReverseRangeOrder()
		{
			IEnumerable<SearchOperator> Operators = ThingRegistryClient.DecodeIoTDiscoURI("iotdisco:MAN=www.example.org;MODEL=Device;SN~*9873*;#V<2;#V>=1.0;#LON<=-70;#LON>=-72;#LAT<=-33;#LAT>=-34");
			this.Test_SearchUri_1(Operators);
		}

		private void Test_SearchUri_1(IEnumerable<SearchOperator> Operators)
		{ 
			Dictionary<string, bool> Tags = new Dictionary<string, bool>();

			foreach (SearchOperator Op in Operators)
			{
				Tags[Op.Name] = true;

				switch (Op.Name)
				{
					case "MAN":
						Assert.IsTrue(Op is StringTagEqualTo);
						Assert.AreEqual("www.example.org", ((StringTagEqualTo)Op).Value);
						break;

					case "MODEL":
						Assert.IsTrue(Op is StringTagEqualTo);
						Assert.AreEqual("Device", ((StringTagEqualTo)Op).Value);
						break;

					case "SN":
						Assert.IsTrue(Op is StringTagMask);
						Assert.AreEqual("9873*", ((StringTagMask)Op).Value);
						Assert.AreEqual("*", ((StringTagMask)Op).Wildcard);
						break;

					case "V":
						Assert.IsTrue(Op is NumericTagInRange);
						Assert.AreEqual(1.0, ((NumericTagInRange)Op).Min);
						Assert.AreEqual(true, ((NumericTagInRange)Op).MinIncluded);
						Assert.AreEqual(2.0, ((NumericTagInRange)Op).Max);
						Assert.AreEqual(false, ((NumericTagInRange)Op).MaxIncluded);
						break;

					case "LON":
						Assert.IsTrue(Op is NumericTagInRange);
						Assert.AreEqual(-72, ((NumericTagInRange)Op).Min);
						Assert.AreEqual(true, ((NumericTagInRange)Op).MinIncluded);
						Assert.AreEqual(-70, ((NumericTagInRange)Op).Max);
						Assert.AreEqual(true, ((NumericTagInRange)Op).MaxIncluded);
						break;

					case "LAT":
						Assert.IsTrue(Op is NumericTagInRange);
						Assert.AreEqual(-34, ((NumericTagInRange)Op).Min);
						Assert.AreEqual(true, ((NumericTagInRange)Op).MinIncluded);
						Assert.AreEqual(-33, ((NumericTagInRange)Op).Max);
						Assert.AreEqual(true, ((NumericTagInRange)Op).MaxIncluded);
						break;

					default:
						Assert.Fail("Too many operators");
						break;
				}
			}

			Assert.AreEqual(6, Tags.Count);
		}

		[TestMethod]
		public void Test_06_SearchUri_2()
		{
			IEnumerable<SearchOperator> Operators = ThingRegistryClient.DecodeIoTDiscoURI("iotdisco:#A=1;#B>1;#C<1;#D>=1;#E<=1;#F<>1;#G>1;#G<2;#H>=1;#H<2;#I>1;#I<=2;#J>=1;#J<=2;#K>3;#K<2;#L>=3;#L<2;#M>3;#M<=2;#N>=3;#N<=2");
			this.Test_SearchUri_2(Operators);
		}

		[TestMethod]
		public void Test_07_SearchUri_2_ReverseRangeOrder()
		{
			IEnumerable<SearchOperator> Operators = ThingRegistryClient.DecodeIoTDiscoURI("iotdisco:#A=1;#B>1;#C<1;#D>=1;#E<=1;#F<>1;#G<2;#G>1;#H<2;#H>=1;#I<=2;#I>1;#J<=2;#J>=1;#K<2;#K>3;#L<2;#L>=3;#M<=2;#M>3;#N<=2;#N>=3");
			this.Test_SearchUri_2(Operators);
		}

		private void Test_SearchUri_2(IEnumerable<SearchOperator> Operators)
		{ 
			Dictionary<string, bool> Tags = new Dictionary<string, bool>();

			foreach (SearchOperator Op in Operators)
			{
				Tags[Op.Name] = true;

				switch (Op.Name)
				{
					case "A":
						Assert.IsTrue(Op is NumericTagEqualTo);
						Assert.AreEqual(1.0, ((NumericTagEqualTo)Op).Value);
						break;

					case "B":
						Assert.IsTrue(Op is NumericTagGreaterThan);
						Assert.AreEqual(1.0, ((NumericTagGreaterThan)Op).Value);
						break;

					case "C":
						Assert.IsTrue(Op is NumericTagLesserThan);
						Assert.AreEqual(1.0, ((NumericTagLesserThan)Op).Value);
						break;

					case "D":
						Assert.IsTrue(Op is NumericTagGreaterThanOrEqualTo);
						Assert.AreEqual(1.0, ((NumericTagGreaterThanOrEqualTo)Op).Value);
						break;

					case "E":
						Assert.IsTrue(Op is NumericTagLesserThanOrEqualTo);
						Assert.AreEqual(1.0, ((NumericTagLesserThanOrEqualTo)Op).Value);
						break;

					case "F":
						Assert.IsTrue(Op is NumericTagNotEqualTo);
						Assert.AreEqual(1.0, ((NumericTagNotEqualTo)Op).Value);
						break;

					case "G":
						Assert.IsTrue(Op is NumericTagInRange);
						Assert.AreEqual(1.0, ((NumericTagInRange)Op).Min);
						Assert.AreEqual(false, ((NumericTagInRange)Op).MinIncluded);
						Assert.AreEqual(2.0, ((NumericTagInRange)Op).Max);
						Assert.AreEqual(false, ((NumericTagInRange)Op).MaxIncluded);
						break;

					case "H":
						Assert.IsTrue(Op is NumericTagInRange);
						Assert.AreEqual(1.0, ((NumericTagInRange)Op).Min);
						Assert.AreEqual(true, ((NumericTagInRange)Op).MinIncluded);
						Assert.AreEqual(2.0, ((NumericTagInRange)Op).Max);
						Assert.AreEqual(false, ((NumericTagInRange)Op).MaxIncluded);
						break;
						
					case "I":
						Assert.IsTrue(Op is NumericTagInRange);
						Assert.AreEqual(1.0, ((NumericTagInRange)Op).Min);
						Assert.AreEqual(false, ((NumericTagInRange)Op).MinIncluded);
						Assert.AreEqual(2.0, ((NumericTagInRange)Op).Max);
						Assert.AreEqual(true, ((NumericTagInRange)Op).MaxIncluded);
						break;

					case "J":
						Assert.IsTrue(Op is NumericTagInRange);
						Assert.AreEqual(1.0, ((NumericTagInRange)Op).Min);
						Assert.AreEqual(true, ((NumericTagInRange)Op).MinIncluded);
						Assert.AreEqual(2.0, ((NumericTagInRange)Op).Max);
						Assert.AreEqual(true, ((NumericTagInRange)Op).MaxIncluded);
						break;

					case "K":
						Assert.IsTrue(Op is NumericTagNotInRange);
						Assert.AreEqual(2.0, ((NumericTagNotInRange)Op).Min);
						Assert.AreEqual(true, ((NumericTagNotInRange)Op).MinIncluded);
						Assert.AreEqual(3.0, ((NumericTagNotInRange)Op).Max);
						Assert.AreEqual(true, ((NumericTagNotInRange)Op).MaxIncluded);
						break;

					case "L":
						Assert.IsTrue(Op is NumericTagNotInRange);
						Assert.AreEqual(2.0, ((NumericTagNotInRange)Op).Min);
						Assert.AreEqual(true, ((NumericTagNotInRange)Op).MinIncluded);
						Assert.AreEqual(3.0, ((NumericTagNotInRange)Op).Max);
						Assert.AreEqual(false, ((NumericTagNotInRange)Op).MaxIncluded);
						break;

					case "M":
						Assert.IsTrue(Op is NumericTagNotInRange);
						Assert.AreEqual(2.0, ((NumericTagNotInRange)Op).Min);
						Assert.AreEqual(false, ((NumericTagNotInRange)Op).MinIncluded);
						Assert.AreEqual(3.0, ((NumericTagNotInRange)Op).Max);
						Assert.AreEqual(true, ((NumericTagNotInRange)Op).MaxIncluded);
						break;

					case "N":
						Assert.IsTrue(Op is NumericTagNotInRange);
						Assert.AreEqual(2.0, ((NumericTagNotInRange)Op).Min);
						Assert.AreEqual(false, ((NumericTagNotInRange)Op).MinIncluded);
						Assert.AreEqual(3.0, ((NumericTagNotInRange)Op).Max);
						Assert.AreEqual(false, ((NumericTagNotInRange)Op).MaxIncluded);
						break;

					default:
						Assert.Fail("Too many operators");
						break;
				}
			}

			Assert.AreEqual(14, Tags.Count);
		}

		[TestMethod]
		public void Test_08_SearchUri_3()
		{
			IEnumerable<SearchOperator> Operators = ThingRegistryClient.DecodeIoTDiscoURI("iotdisco:A=1;B>1;C<1;D>=1;E<=1;F<>1;G>1;G<2;H>=1;H<2;I>1;I<=2;J>=1;J<=2;K>3;K<2;L>=3;L<2;M>3;M<=2;N>=3;N<=2;O~+1+2");
			this.Test_SearchUri_3(Operators);
		}

		[TestMethod]
		public void Test_09_SearchUri_3_ReverseRangeOrder()
		{
			IEnumerable<SearchOperator> Operators = ThingRegistryClient.DecodeIoTDiscoURI("iotdisco:A=1;B>1;C<1;D>=1;E<=1;F<>1;G<2;G>1;H<2;H>=1;I<=2;I>1;J<=2;J>=1;K<2;K>3;L<2;L>=3;M<=2;M>3;N<=2;N>=3;O~+1+2");
			this.Test_SearchUri_3(Operators);
		}

		private void Test_SearchUri_3(IEnumerable<SearchOperator> Operators)
		{
			Dictionary<string, bool> Tags = new Dictionary<string, bool>();

			foreach (SearchOperator Op in Operators)
			{
				Tags[Op.Name] = true;

				switch (Op.Name)
				{
					case "A":
						Assert.IsTrue(Op is StringTagEqualTo);
						Assert.AreEqual("1", ((StringTagEqualTo)Op).Value);
						break;

					case "B":
						Assert.IsTrue(Op is StringTagGreaterThan);
						Assert.AreEqual("1", ((StringTagGreaterThan)Op).Value);
						break;

					case "C":
						Assert.IsTrue(Op is StringTagLesserThan);
						Assert.AreEqual("1", ((StringTagLesserThan)Op).Value);
						break;

					case "D":
						Assert.IsTrue(Op is StringTagGreaterThanOrEqualTo);
						Assert.AreEqual("1", ((StringTagGreaterThanOrEqualTo)Op).Value);
						break;

					case "E":
						Assert.IsTrue(Op is StringTagLesserThanOrEqualTo);
						Assert.AreEqual("1", ((StringTagLesserThanOrEqualTo)Op).Value);
						break;

					case "F":
						Assert.IsTrue(Op is StringTagNotEqualTo);
						Assert.AreEqual("1", ((StringTagNotEqualTo)Op).Value);
						break;

					case "G":
						Assert.IsTrue(Op is StringTagInRange);
						Assert.AreEqual("1", ((StringTagInRange)Op).Min);
						Assert.AreEqual(false, ((StringTagInRange)Op).MinIncluded);
						Assert.AreEqual("2", ((StringTagInRange)Op).Max);
						Assert.AreEqual(false, ((StringTagInRange)Op).MaxIncluded);
						break;

					case "H":
						Assert.IsTrue(Op is StringTagInRange);
						Assert.AreEqual("1", ((StringTagInRange)Op).Min);
						Assert.AreEqual(true, ((StringTagInRange)Op).MinIncluded);
						Assert.AreEqual("2", ((StringTagInRange)Op).Max);
						Assert.AreEqual(false, ((StringTagInRange)Op).MaxIncluded);
						break;

					case "I":
						Assert.IsTrue(Op is StringTagInRange);
						Assert.AreEqual("1", ((StringTagInRange)Op).Min);
						Assert.AreEqual(false, ((StringTagInRange)Op).MinIncluded);
						Assert.AreEqual("2", ((StringTagInRange)Op).Max);
						Assert.AreEqual(true, ((StringTagInRange)Op).MaxIncluded);
						break;

					case "J":
						Assert.IsTrue(Op is StringTagInRange);
						Assert.AreEqual("1", ((StringTagInRange)Op).Min);
						Assert.AreEqual(true, ((StringTagInRange)Op).MinIncluded);
						Assert.AreEqual("2", ((StringTagInRange)Op).Max);
						Assert.AreEqual(true, ((StringTagInRange)Op).MaxIncluded);
						break;

					case "K":
						Assert.IsTrue(Op is StringTagNotInRange);
						Assert.AreEqual("2", ((StringTagNotInRange)Op).Min);
						Assert.AreEqual(true, ((StringTagNotInRange)Op).MinIncluded);
						Assert.AreEqual("3", ((StringTagNotInRange)Op).Max);
						Assert.AreEqual(true, ((StringTagNotInRange)Op).MaxIncluded);
						break;

					case "L":
						Assert.IsTrue(Op is StringTagNotInRange);
						Assert.AreEqual("2", ((StringTagNotInRange)Op).Min);
						Assert.AreEqual(true, ((StringTagNotInRange)Op).MinIncluded);
						Assert.AreEqual("3", ((StringTagNotInRange)Op).Max);
						Assert.AreEqual(false, ((StringTagNotInRange)Op).MaxIncluded);
						break;

					case "M":
						Assert.IsTrue(Op is StringTagNotInRange);
						Assert.AreEqual("2", ((StringTagNotInRange)Op).Min);
						Assert.AreEqual(false, ((StringTagNotInRange)Op).MinIncluded);
						Assert.AreEqual("3", ((StringTagNotInRange)Op).Max);
						Assert.AreEqual(true, ((StringTagNotInRange)Op).MaxIncluded);
						break;

					case "N":
						Assert.IsTrue(Op is StringTagNotInRange);
						Assert.AreEqual("2", ((StringTagNotInRange)Op).Min);
						Assert.AreEqual(false, ((StringTagNotInRange)Op).MinIncluded);
						Assert.AreEqual("3", ((StringTagNotInRange)Op).Max);
						Assert.AreEqual(false, ((StringTagNotInRange)Op).MaxIncluded);
						break;

					case "O":
						Assert.IsTrue(Op is StringTagMask);
						Assert.AreEqual("1+2", ((StringTagMask)Op).Value);
						Assert.AreEqual("+", ((StringTagMask)Op).Wildcard);
						break;

					default:
						Assert.Fail("Too many operators");
						break;
				}
			}

			Assert.AreEqual(15, Tags.Count);
		}


	}
}
