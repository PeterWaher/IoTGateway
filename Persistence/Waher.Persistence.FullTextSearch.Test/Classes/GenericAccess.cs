using Waher.Persistence.Serialization;

namespace Waher.Persistence.FullTextSearch.Test.Classes
{
	public class GenericAccess : ITestClassAccess
	{
		public void Set(object Obj, string IndexedProperty1, string IndexedProperty2,
			string NonIndexedProperty1, string NonIndexedProperty2)
		{
			GenericObject TestClass = (GenericObject)Obj;
			TestClass.CollectionName = "TestGeneric";
			TestClass["IndexedProperty1"] = IndexedProperty1;
			TestClass["IndexedProperty2"] = IndexedProperty2;
			TestClass["NonIndexedProperty1"] = NonIndexedProperty1;
			TestClass["NonIndexedProperty2"] = NonIndexedProperty2;
			TestClass["Created"] = DateTime.UtcNow;
		}

		public void Set(object Obj, string IndexedProperty2)
		{
			GenericObject TestClass = (GenericObject)Obj;
			TestClass["IndexedProperty2"] = IndexedProperty2;
		}

		public DateTime GetCreated(object Obj) => (DateTime)((GenericObject)Obj)["Created"];
	}

}
