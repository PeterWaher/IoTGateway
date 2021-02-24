using System.Reflection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Waher.Runtime.Inventory.Test.Definitions;

namespace Waher.Runtime.Inventory.Test
{
	[TestClass]
	public class InstantiationTests
	{
		[AssemblyInitialize]
		public static void AssemblyInitialize(TestContext _)
		{
			Types.Initialize(typeof(InstantiationTests).Assembly);
		}

		[TestMethod]
		public void Test_01_DefaultConstructor()
		{
			DefaultConstructor Obj = Types.Instantiate<DefaultConstructor>(false);
			Assert.IsNotNull(Obj);
		}

		[TestMethod]
		public void Test_02_OneArgument()
		{
			OneArgument Obj = Types.Instantiate<OneArgument>(false, 1);
			Assert.IsNotNull(Obj);
			Assert.AreEqual(1, Obj.N);
		}

		[TestMethod]
		public void Test_03_OneArgument_Default()
		{
			OneArgument Obj = Types.Instantiate<OneArgument>(false);
			Assert.IsNotNull(Obj);
			Assert.AreEqual(0, Obj.N);
		}

		[TestMethod]
		public void Test_03_TwoArguments()
		{
			TwoArguments Obj = Types.Instantiate<TwoArguments>(false, 1, "Hello");
			Assert.IsNotNull(Obj);
			Assert.AreEqual(1, Obj.N);
			Assert.AreEqual("Hello", Obj.S);
		}

		[TestMethod]
		public void Test_04_TwoArguments_OneDefault()
		{
			TwoArguments Obj = Types.Instantiate<TwoArguments>(false, 1);
			Assert.IsNotNull(Obj);
			Assert.AreEqual(1, Obj.N);
			Assert.AreEqual(string.Empty, Obj.S);
		}

		[TestMethod]
		public void Test_05_TwoArguments_TwoDefault()
		{
			TwoArguments Obj = Types.Instantiate<TwoArguments>(false);
			Assert.IsNotNull(Obj);
			Assert.AreEqual(0, Obj.N);
			Assert.AreEqual(string.Empty, Obj.S);
		}

		[TestMethod]
		public void Test_06_Nested()
		{
			Nested Obj = Types.Instantiate<Nested>(false);
			Assert.IsNotNull(Obj);
			Assert.IsNotNull(Obj.DefaultConstructor);
			Assert.IsNotNull(Obj.OneArgument);
			Assert.AreEqual(0, Obj.OneArgument.N);
			Assert.IsNotNull(Obj.TwoArguments);
			Assert.AreEqual(0, Obj.TwoArguments.N);
			Assert.AreEqual(string.Empty, Obj.TwoArguments.S);
		}

		[TestMethod]
		public void Test_07_DefaultImplementation_Interface()
		{
			IExample Obj = Types.Instantiate<IExample>(false);
			Assert.IsNotNull(Obj);
			Assert.AreEqual(9.0, Obj.Eval(3));
		}

		[TestMethod]
		public void Test_08_DefaultImplementation_AbstractClass()
		{
			IExample Obj = Types.Instantiate<Example>(false);
			Assert.IsNotNull(Obj);
			Assert.AreEqual(9.0, Obj.Eval(3));
		}

		[TestMethod]
		public void Test_09_Singleton()
		{
			SingletonClass Obj = Types.Instantiate<SingletonClass>(false);
			SingletonClass Obj2 = Types.Instantiate<SingletonClass>(false);
			Assert.IsNotNull(Obj);
			Assert.IsNotNull(Obj2);
			Assert.AreEqual(Obj.Rnd, Obj2.Rnd);
		}

		[TestMethod]
		public void Test_10_InternalClass()
		{
			InternalClass Obj = Types.Instantiate<InternalClass>(false);
			Assert.IsNotNull(Obj);
		}

		[TestMethod]
		public void Test_11_InternalConstructor()
		{
			InternalConstructor Obj = Types.Instantiate<InternalConstructor>(false);
			Assert.IsNotNull(Obj);
		}

		[TestMethod]
		public void Test_12_PrivateConstructor()
		{
			PrivateConstructor Obj = Types.Instantiate<PrivateConstructor>(false);
			Assert.IsNotNull(Obj);
		}

		[TestMethod]
		public void Test_13_RegisterSingleton()
		{
			SingletonClass Obj = new SingletonClass();
			SingletonClass Obj2;

			if (Types.IsSingletonRegistered(typeof(SingletonClass)))
			{
				Obj2 = Types.Instantiate<SingletonClass>(false);
				Types.UnregisterSingleton(Obj2);

				Assert.AreNotEqual(Obj.Rnd, Obj2.Rnd);
			}

			Types.RegisterSingleton(Obj);

			Obj2 = Types.Instantiate<SingletonClass>(false);
			Assert.IsNotNull(Obj2);
			Assert.AreEqual(Obj.Rnd, Obj2.Rnd);
		}

		[TestMethod]
		public void Test_14_RegisterDefaultImplementation()
		{
			IExample Example1 = Types.Instantiate<IExample>(false);
			Assert.IsNotNull(Example1);
			Assert.AreEqual(9.0, Example1.Eval(3));

			Types.RegisterDefaultImplementation(typeof(IExample), typeof(Example2));

			IExample Example2 = Types.Instantiate<IExample>(false);
			Assert.IsNotNull(Example2);
			Assert.AreEqual(6.0, Example2.Eval(3));

			Types.UnregisterDefaultImplementation(typeof(IExample), typeof(Example2));

			IExample Example3 = Types.Instantiate<IExample>(false);
			Assert.IsNotNull(Example3);
			Assert.AreEqual(9.0, Example3.Eval(3));
		}

		[TestMethod]
		public void Test_15_ParamsSealed()
		{
			ParamsArguments ParamsArguments = Types.Instantiate<ParamsArguments>(false,
				typeof(InstantiationTests).Assembly,
				new Assembly[] { typeof(Types).Assembly },
				new SealedClass[] { new SealedClass("A", "B", "C") });
			Assert.IsNotNull(ParamsArguments);
		}

		[TestMethod]
		public void Test_16_NullArguments()
		{
			TwoArguments Obj = Types.Instantiate<TwoArguments>(false, 1, null);
			Assert.IsNotNull(Obj);
			Assert.AreEqual(1, Obj.N);
			Assert.IsNull(Obj.S);
		}

		[TestMethod]
		public void Test_17_ParamsSingleton()
		{
			ParamsExample ParamsExample = Types.Instantiate<ParamsExample>(false,
				(object)(new SealedClass[] { new SealedClass("A", "B", "C") }));
			Assert.IsNotNull(ParamsExample);
		}

		[TestMethod]
		public void Test_18_SingletonInterfaceDefaultImplementation()
		{
			ISingleton Obj = Types.InstantiateDefault<ISingleton>(false, "Hello");
			ISingleton Obj2 = Types.InstantiateDefault<ISingleton>(false);
			
			Assert.IsNotNull(Obj);
			Assert.AreEqual("Hello", Obj.String);

			Assert.IsNotNull(Obj2);
			Assert.AreEqual("Hello", Obj2.String);

			Assert.AreEqual(Obj.Value, Obj2.Value);
		}
	}
}
