namespace Waher.Persistence.FullTextSearch.Test.Classes
{
	public class TestClassSetter : ITestClassSetter
	{
		public void Set(object Obj, string IndexedProperty1, string IndexedProperty2,
			string NonIndexedProperty1, string NonIndexedProperty2)
		{
			ITestClass TestClass = (ITestClass)Obj;
			TestClass.IndexedProperty1 = IndexedProperty1;
			TestClass.IndexedProperty2 = IndexedProperty2;
			TestClass.NonIndexedProperty1 = NonIndexedProperty1;
			TestClass.NonIndexedProperty2 = NonIndexedProperty2;
		}

		public void Set(object Obj, string IndexedProperty2)
		{
			ITestClass TestClass = (ITestClass)Obj;
			TestClass.IndexedProperty2 = IndexedProperty2;
		}
	}
}
