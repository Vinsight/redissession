// Decompiled with JetBrains decompiler
// Type: AngiesList.Redis.KeyValueStore
// Assembly: AngiesList.Redis, Version=1.1.5023.23192, Culture=neutral, PublicKeyToken=null
// MVID: A019BE0F-3773-46AA-8598-F03B954DBF6D
// Assembly location: D:\projects\dotnet\vinsight\solutions\dependencies\Redis\AngiesList.Redis.dll

using System.Collections.Concurrent;

namespace AngiesList.Redis
{
  public sealed class KeyValueStore
  {
    private static ConcurrentDictionary<string, Bucket> bucketsPool = new ConcurrentDictionary<string, Bucket>();
    private static readonly object locker = new object();

    private KeyValueStore()
    {
    }

    /// <summary>
    /// Create a bucket using the specified redis connection.
    /// Note: Breaking API change: passing null,null for host,port
    /// no longer reads the config file. Please use Bucket(name)
    /// instead.
    /// </summary>
    /// <param name="name"></param>
    /// <param name="host"></param>
    /// <param name="port"></param>
    /// <returns></returns>
    public static Bucket Bucket(string name, string host, int? port)
    {
      return KeyValueStore.Bucket(new RedisBucketConfiguration(name, host, port));
    }

    /// <summary>Create Bucket from RedisBucketConfiguration</summary>
    /// <param name="config"></param>
    /// <returns></returns>
    public static Bucket Bucket(RedisBucketConfiguration config)
    {
      string key = config.Name + config.Host + (object) config.Port;
      if (!KeyValueStore.bucketsPool.ContainsKey(key))
      {
        lock (KeyValueStore.locker)
          KeyValueStore.bucketsPool.TryAdd(key, (Bucket) new RedisBucket(config.Name, config.Host, new int?(config.Port)));
      }
      return KeyValueStore.bucketsPool[key];
    }

    /// <summary>Read the default configuration file for host/port</summary>
    /// <param name="name"></param>
    /// <returns></returns>
    public static Bucket Bucket(string name)
    {
      return KeyValueStore.Bucket(new RedisBucketConfiguration(RedisConfiguration.ReadConfigFile("KeyValueStore.config", true), name));
    }
  }
}
