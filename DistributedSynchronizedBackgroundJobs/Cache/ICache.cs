namespace DistributedSynchronizedBackgroundJobs.Cache
{
    public interface ICache
    {
        Task SetAsync<T>(string key, T data,
                                      TimeSpan? absoluteExpireTime = null,
                                      TimeSpan? unUsedExpireTime = null);

        Task SetAsync(string key, string jsonData,
                                         TimeSpan? absoluteExpireTime = null,
                                         TimeSpan? unUsedExpireTime = null);

        Task<T?> GetAsync<T>(string key);

    }
}
