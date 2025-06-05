using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FoodDeliverySystem.Common.Caching.Dependencies;

public class RedisConnectionFactory : IDisposable
{
    private readonly string _connectionString;
    private readonly Lazy<ConnectionMultiplexer> _connection;

    public RedisConnectionFactory(string connectionString)
    {
        _connectionString = connectionString ?? throw new ArgumentNullException(nameof(connectionString));
        _connection = new Lazy<ConnectionMultiplexer>(() => ConnectionMultiplexer.Connect(_connectionString));
    }

    public IDatabase GetDatabase()
    {
        return _connection.Value.GetDatabase();
    }

    public void Dispose()
    {
        if (_connection.IsValueCreated)
        {
            _connection.Value.Dispose();
        }
    }
}