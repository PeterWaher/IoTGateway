namespace Waher.Persistence.FullTextSearch.Test.Classes
{
	public class TestClassAccess : ITestClassAccess
	{
		public void Set(object Obj, string IndexedProperty1, string IndexedProperty2,
			string NonIndexedProperty1, string NonIndexedProperty2)
		{
			ITestClass TestClass = (ITestClass)Obj;
			TestClass.IndexedProperty1 = IndexedProperty1;
			TestClass.IndexedProperty2 = IndexedProperty2;
			TestClass.NonIndexedProperty1 = NonIndexedProperty1;
			TestClass.NonIndexedProperty2 = NonIndexedProperty2;
			TestClass.Created = DateTime.UtcNow;
		}

		public void Set(object Obj, string IndexedProperty2)
		{
			ITestClass TestClass = (ITestClass)Obj;
			TestClass.IndexedProperty2 = IndexedProperty2;
		}

		public DateTime GetCreated(object Obj) => ((ITestClass)Obj).Created;

	}
}
