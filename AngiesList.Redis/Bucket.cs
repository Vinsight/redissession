// Decompiled with JetBrains decompiler
// Type: AngiesList.Redis.Bucket
// Assembly: AngiesList.Redis, Version=1.1.5023.23192, Culture=neutral, PublicKeyToken=null
// MVID: A019BE0F-3773-46AA-8598-F03B954DBF6D
// Assembly location: D:\projects\dotnet\vinsight\solutions\dependencies\Redis\AngiesList.Redis.dll

using System;

namespace AngiesList.Redis
{
  public abstract class Bucket
  {
    /// <summary>The bucket name</summary>
    public string Name { get; private set; }

    public Bucket(string name)
    {
      this.Name = name;
    }

    public abstract void Set(string key, object value, int? expireSeconds = null);

    public abstract void Del(string key);

    public abstract void Del(string[] keys);

    public abstract void Expire(string key, int expireSeconds);

    public abstract void GetString(string key, Action<string, Exception> cb);

    public abstract string GetStringSync(string key);

    public abstract void Get<T>(string key, Action<T, Exception> cb);

    public abstract T GetSync<T>(string key);

    public abstract void GetRaw(string key, Action<byte[], Exception> cb);

    public abstract byte[] GetRawSync(string key);
  }
}
