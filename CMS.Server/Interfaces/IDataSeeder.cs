namespace CMS.Server.Interfaces
{
    public interface IDataSeeder
    {
        Task WipeAndSeedAsync(CancellationToken cancellationToken = default);
    }
}
