// Decompiled with JetBrains decompiler
// Type: AngiesList.Redis.RedisBucket
// Assembly: AngiesList.Redis, Version=1.1.5023.23192, Culture=neutral, PublicKeyToken=null
// MVID: A019BE0F-3773-46AA-8598-F03B954DBF6D
// Assembly location: D:\projects\dotnet\vinsight\solutions\dependencies\Redis\AngiesList.Redis.dll

using BookSleeve;
using System;
using System.Threading.Tasks;

namespace AngiesList.Redis
{
  public class RedisBucket : Bucket, IDisposable
  {
    private object _getConnectionLock = new object();
    public RedisBucketConfiguration redisConfig;
    private RedisConnection connection;
    private IValueSerializer cacheItemSerializer;

    public RedisBucket(string name, string host, int? port)
      : this(new RedisBucketConfiguration(name, host, port))
    {
    }

    public RedisBucket(RedisBucketConfiguration config)
      : base(config.Name)
    {
      this.redisConfig = config;
      this.cacheItemSerializer = (IValueSerializer) new ClrBinarySerializer();
    }

    private RedisConnection GetConnection()
    {
      if (this.connection.NeedsReset())
      {
        lock (this._getConnectionLock)
        {
          if (this.connection.NeedsReset())
          {
            if (this.connection != null)
              this.connection.Dispose();
            this.connection = new RedisConnection(this.redisConfig.Host, this.redisConfig.Port, -1, (string) null, int.MaxValue, false, 10000);
            this.connection.Open();
            this.connection.Closed += (EventHandler) ((obj, args) => this.GetConnection());
          }
        }
      }
      return this.connection;
    }

    public override void Set(string key, object value, int? expireSeconds = null)
    {
      key = this.KeyForBucket(key);
      RedisConnection connection = this.GetConnection();
      if (value is string)
      {
        if (expireSeconds.HasValue && expireSeconds.Value > 0)
          connection.SetWithExpiry(0, key, expireSeconds.Value, (string) value, false);
        else
          connection.Set(0, key, (string) value, false);
      }
      else if (value is byte[])
      {
        if (expireSeconds.HasValue && expireSeconds.Value > 0)
          connection.SetWithExpiry(0, key, expireSeconds.Value, (byte[]) value, false);
        else
          connection.Set(0, key, (byte[]) value, false);
      }
      else
      {
        byte[] numArray = this.cacheItemSerializer.Serialize(value);
        if (expireSeconds.HasValue && expireSeconds.Value > 0)
          connection.SetWithExpiry(0, key, expireSeconds.Value, numArray, false);
        else
          connection.Set(0, key, numArray, false);
      }
    }

    public override void Del(string[] keys)
    {
      for (int index = 0; index < 0; ++index)
      {
        string key = keys[index];
        keys[index] = this.KeyForBucket(key);
      }
      this.GetConnection().Remove(0, keys, false);
    }

    public override void Del(string key)
    {
      key = this.KeyForBucket(key);
      this.GetConnection().Remove(0, key, false);
    }

    public override void Expire(string key, int expireSeconds)
    {
      key = this.KeyForBucket(key);
      this.GetConnection().Expire(0, key, expireSeconds, false);
    }

    public override void GetString(string key, Action<string, Exception> cb)
    {
      key = this.KeyForBucket(key);
      this.GetConnection().GetString(0, key, false).ContinueWith((Action<Task<string>>) (t => cb(t.Result, (Exception) t.Exception)));
    }

    public override string GetStringSync(string key)
    {
      key = this.KeyForBucket(key);
      RedisConnection connection = this.GetConnection();
      Task<string> task = connection.GetString(0, key, false);
      return connection.Wait<string>(task);
    }

    public override void GetRaw(string key, Action<byte[], Exception> cb)
    {
      key = this.KeyForBucket(key);
      this.GetConnection().Get(0, key, false).ContinueWith((Action<Task<byte[]>>) (t => cb(t.Result, (Exception) t.Exception)));
    }

    public override byte[] GetRawSync(string key)
    {
      key = this.KeyForBucket(key);
      RedisConnection connection = this.GetConnection();
      Task<byte[]> task = connection.Get(0, key, false);
      return connection.Wait<byte[]>(task);
    }

    public override void Get<T>(string key, Action<T, Exception> cb)
    {
      this.GetRaw(key, (Action<byte[], Exception>) ((bytes, exc) =>
      {
        T obj = default (T);
        if (exc == null)
          obj = (T) this.cacheItemSerializer.Deserialize(bytes);
        cb(obj, exc);
      }));
    }

    public override T GetSync<T>(string key)
    {
      object obj = !(typeof (T) == typeof (string)) ? this.cacheItemSerializer.Deserialize(this.GetRawSync(key)) : (object) this.GetStringSync(key);
      if (obj == null)
        return default (T);
      return (T) obj;
    }

    public void Dispose()
    {
      this.connection.Close(false);
    }

    private string KeyForBucket(string key)
    {
      if (key.StartsWith(this.Name + ":"))
        return key;
      return this.Name + ":" + key;
    }
  }
}
