namespace IdentityBase.Services
{
    public interface IStoreInitializer
    {
        void InitializeStores();
        void CleanupStores();
    }
}
