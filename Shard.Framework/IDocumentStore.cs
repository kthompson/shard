namespace Shard
{
    public interface IDocumentStore
    {
        IDocumentSession OpenSession();
    }
}