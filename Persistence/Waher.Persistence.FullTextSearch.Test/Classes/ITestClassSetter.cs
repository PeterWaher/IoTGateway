namespace Waher.Persistence.FullTextSearch.Test.Classes
{
    public interface ITestClassSetter
	{
        void Set(object Obj, string IndexedProperty1, string IndexedProperty2,
            string NonIndexedProperty1, string NonIndexedProperty2);

		void Set(object Obj, string IndexedProperty2);
	}
}
