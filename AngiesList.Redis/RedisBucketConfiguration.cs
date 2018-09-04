// Decompiled with JetBrains decompiler
// Type: AngiesList.Redis.RedisBucketConfiguration
// Assembly: AngiesList.Redis, Version=1.1.5023.23192, Culture=neutral, PublicKeyToken=null
// MVID: A019BE0F-3773-46AA-8598-F03B954DBF6D
// Assembly location: D:\projects\dotnet\vinsight\solutions\dependencies\Redis\AngiesList.Redis.dll

namespace AngiesList.Redis
{
  public class RedisBucketConfiguration : RedisConfiguration
  {
    public RedisBucketConfiguration()
    {
    }

    /// <summary>
    /// Create a RedisBucketConfiguration from existing RedisConfiguration
    /// </summary>
    /// <param name="redisConfig"></param>
    /// <param name="name"></param>
    public RedisBucketConfiguration(RedisConfiguration redisConfig, string name)
    {
      this.Host = redisConfig.Host;
      this.Port = redisConfig.Port;
      this.Name = name;
    }

    public RedisBucketConfiguration(string name, string host, int? port)
    {
      this.Name = name;
      this.Host = host;
      if (!port.HasValue)
        return;
      this.Port = port.Value;
    }

    public string Name { get; set; }
  }
}
