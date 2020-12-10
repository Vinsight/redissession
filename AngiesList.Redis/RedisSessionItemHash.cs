// Decompiled with JetBrains decompiler
// Type: AngiesList.Redis.RedisSessionItemHash
// Assembly: AngiesList.Redis, Version=1.1.5023.23192, Culture=neutral, PublicKeyToken=null
// MVID: A019BE0F-3773-46AA-8598-F03B954DBF6D
// Assembly location: D:\projects\dotnet\vinsight\solutions\dependencies\Redis\AngiesList.Redis.dll

using BookSleeve;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Threading.Tasks;
using System.Web.SessionState;

namespace AngiesList.Redis
{
  public sealed class RedisSessionItemHash : NameObjectCollectionBase, ISessionStateItemCollection, ICollection, IEnumerable
  {
    public IValueSerializer Serializer = (IValueSerializer) new ClrBinarySerializer();
    private IDictionary<string, object> persistentValues = (IDictionary<string, object>) new Dictionary<string, object>();
    private object deserializeLock = new object();
    private HashSet<string> namesAdded = new HashSet<string>();
    private const string TYPE_PREFIX = "__CLR_TYPE__";
    private const string VALUE_PREFIX = "val:";
    private RedisConnection redis;
    private string sessionId;
    private int timeoutMinutes;
    private Dictionary<string, byte[]> rawItems;
    private bool keysAdded;
    private bool timeoutReset;

    public RedisSessionItemHash(string sessionId, int timeoutMinutes, RedisConnection redisConnection)
    {
      this.sessionId = sessionId;
      this.timeoutMinutes = timeoutMinutes;
      this.redis = redisConnection;
      this.SetTasks = (IList<Task>) new List<Task>();
    }

    private string GetKeyForSession()
    {
      return "sess:" + this.sessionId;
    }

    private Dictionary<string, byte[]> GetRawItems()
    {
      if (this.rawItems == null)
      {
        this.rawItems = this.redis.Hashes.GetAll(0, this.GetKeyForSession(), false).Result;
        this.OneTimeResetTimeout();
      }
      return this.rawItems;
    }

    private void AddKeysToBase()
    {
      if (this.keysAdded)
        return;
      foreach (string key in this.GetRawItems().Keys)
      {
        if (!key.StartsWith("__CLR_TYPE__"))
          this.BaseAdd(key.Substring("val:".Length), (object) null);
      }
      this.keysAdded = true;
    }

    private void AddFieldToBaseFromRaw(string name)
    {
      this.AddKeysToBase();
      lock (this.deserializeLock)
      {
        if (!this.GetRawItems().ContainsKey("val:" + name))
          return;
        byte[] rawItem = this.GetRawItems()["val:" + name];
        object obj1 = this.Serializer.Deserialize(rawItem);
        object obj2 = this.Serializer.Deserialize(rawItem);
        this.BaseSet(name, obj1);
        this.persistentValues.Add(name, obj2);
      }
    }

    private void AddAllFieldsToBaseFromRaw()
    {
      this.AddKeysToBase();
      lock (this.deserializeLock)
      {
        foreach (string allKey in this.BaseGetAllKeys())
          this.Get(allKey);
      }
    }

    private object Get(string name)
    {
      if (!this.namesAdded.Contains(name))
      {
        this.AddFieldToBaseFromRaw(name);
        this.namesAdded.Add(name);
      }
      return this.BaseGet(name);
    }

    private void Set(string name, object value)
    {
      if (value != null && this.namesAdded.Contains(name) && value.Equals(this.persistentValues.ContainsKey(name) ? this.persistentValues[name] : (object) null))
        return;
      byte[] bytes = this.Serializer.Serialize(value);
      byte[] numArray;
      if (this.GetRawItems().TryGetValue("val:" + name, out numArray) && ((IEnumerable<byte>) bytes).SequenceEqual<byte>((IEnumerable<byte>) numArray))
        return;
      this.SetTasks.Add(this.redis.Hashes.Set(0, this.GetKeyForSession(), new Dictionary<string, byte[]>(1)
      {
        {
          "val:" + name,
          bytes
        }
      }, false));
      this.OneTimeResetTimeout();
      if (this.rawItems.ContainsKey("val:" + name))
        this.rawItems["val:" + name] = bytes;
      else
        this.rawItems.Add("val:" + name, bytes);
      if (this.persistentValues.ContainsKey(name))
        this.persistentValues.Remove(name);
      object obj = this.Serializer.Deserialize(bytes);
      this.persistentValues.Add(name, obj);
      if (!this.namesAdded.Contains(name))
      {
        this.namesAdded.Add(name);
        if (this.keysAdded && ((IEnumerable<string>) this.BaseGetAllKeys()).Contains<string>(name))
          this.BaseSet(name, value);
        else
          this.BaseAdd(name, value);
      }
      else
        this.BaseSet(name, value);
    }

    internal void PersistChangedReferences()
    {
      Dictionary<string, object> dictionary = new Dictionary<string, object>();
      foreach (string allKey in this.BaseGetAllKeys())
      {
        if (this.namesAdded.Contains(allKey))
        {
          object obj = this.BaseGet(allKey);
          if (!(obj is ValueType))
            dictionary.Add(allKey, obj);
        }
      }
      foreach (KeyValuePair<string, object> keyValuePair in dictionary)
        this.Set(keyValuePair.Key, keyValuePair.Value);
    }

    private void OneTimeResetTimeout()
    {
      if (this.timeoutReset)
        return;

    var expireTask = redis.Keys.Expire(0, this.GetKeyForSession(), timeoutMinutes * 60, false);
    expireTask.Wait();
    this.timeoutReset = expireTask.Result;

    }

    public object this[string name]
    {
      get
      {
        return this.Get(name);
      }
      set
      {
        this.Set(name, value);
      }
    }

    public void Clear()
    {
      this.redis.Keys.Remove(0, this.GetKeyForSession(), false);
      this.BaseClear();
    }

    public void Remove(string name)
    {
      this.redis.Hashes.Remove(0, this.GetKeyForSession(), "val:" + name, false);
      this.BaseRemove(name);
    }

    public override int Count
    {
      get
      {
        this.AddKeysToBase();
        return base.Count;
      }
    }

    public override NameObjectCollectionBase.KeysCollection Keys
    {
      get
      {
        this.AddKeysToBase();
        NameValueCollection nameValueCollection = new NameValueCollection();
        foreach (string allKey in this.BaseGetAllKeys())
          nameValueCollection.Add(allKey, (string) null);
        return nameValueCollection.Keys;
      }
    }

    public override IEnumerator GetEnumerator()
    {
      this.AddAllFieldsToBaseFromRaw();
      return base.GetEnumerator();
    }

    public bool Dirty
    {
      get
      {
        return false;
      }
      set
      {
      }
    }

    public IList<Task> SetTasks { get; private set; }

    public void RemoveAt(int index)
    {
      throw new NotImplementedException();
    }

    public object this[int index]
    {
      get
      {
        throw new NotImplementedException();
      }
      set
      {
        throw new NotImplementedException();
      }
    }
  }
}
