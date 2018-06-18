namespace IdentityBase.EntityFramework.DbInitializer
{
    public interface IExampleDataStoreInitializer
    {
        void CleanupStores();
        void InitializeStores();
    }
}