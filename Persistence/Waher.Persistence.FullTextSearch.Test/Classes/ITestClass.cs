namespace Waher.Persistence.FullTextSearch.Test.Classes
{
    public interface ITestClass
    {
        string? ObjectID { get; set; }
        string IndexedProperty1 { get; set; }
        string IndexedProperty2 { get; set; }
        string NonIndexedProperty1 { get; set; }
        string NonIndexedProperty2 { get; set; }
    }
}
