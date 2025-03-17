using System;

namespace ClientApp.Services
{
    public interface ICacheService
    {
        T GetOrSet<T>(string key, Func<T> getFunction, TimeSpan? expirationTime = null);
        void Remove(string key);

    }
}
