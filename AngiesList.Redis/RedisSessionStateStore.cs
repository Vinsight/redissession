// Decompiled with JetBrains decompiler
// Type: AngiesList.Redis.RedisSessionStateStore
// Assembly: AngiesList.Redis, Version=1.1.5023.23192, Culture=neutral, PublicKeyToken=null
// MVID: A019BE0F-3773-46AA-8598-F03B954DBF6D
// Assembly location: D:\projects\dotnet\vinsight\solutions\dependencies\Redis\AngiesList.Redis.dll

using BookSleeve;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.SessionState;

namespace AngiesList.Redis
{
  public sealed class RedisSessionStateStore : SessionStateStoreProviderBase
  {
    private readonly object locker = (object) new{  };
    private RedisConnection redisConnection;
    private string lockHashKey;
    private RedisSessionStateConfiguration redisConfig;

    public override void Initialize(string name, NameValueCollection config)
    {
      if (string.IsNullOrWhiteSpace(name))
        name = "AspNetSession";
      base.Initialize(name, config);
      this.redisConfig = RedisSessionStateConfiguration.GetConfiguration();
      this.lockHashKey = name + ":LockedSessions";
    }

    private RedisConnection GetRedisConnection()
    {
      if (this.redisConnection == null || this.redisConnection.State != RedisConnectionBase.ConnectionState.Open && this.redisConnection.State != RedisConnectionBase.ConnectionState.Opening)
      {
        lock (this.locker)
        {
          if (this.redisConnection != null)
          {
            if (this.redisConnection.State != RedisConnectionBase.ConnectionState.Open)
            {
              if (this.redisConnection.State == RedisConnectionBase.ConnectionState.Opening)
                goto label_9;
            }
            else
              goto label_9;
          }
          this.redisConnection = new RedisConnection(this.redisConfig.Host, this.redisConfig.Port, -1, (string) null, int.MaxValue, false, 10000);
          this.redisConnection.Closed += (EventHandler) ((sender, e) => {});
          this.redisConnection.Open();
        }
      }
label_9:
      return this.redisConnection;
    }

    private string GetKeyForSessionId(string id)
    {
      return this.Name + ":" + id;
    }

    public override void SetAndReleaseItemExclusive(HttpContext context, string id, SessionStateStoreData item, object lockId, bool newItem)
    {
      RedisConnection redisConnection = this.GetRedisConnection();
      Task<byte[]> task = redisConnection.Hashes.Get(0, this.lockHashKey, id, false);
      byte[] numArray = (byte[]) lockId;
      MemoryStream memoryStream = new MemoryStream();
      BinaryWriter writer = new BinaryWriter((Stream) memoryStream);
      if (item.Items is SessionStateItemCollection)
        ((SessionStateItemCollection) item.Items).Serialize(writer);
      writer.Close();
      byte[] array = memoryStream.ToArray();
      Dictionary<string, byte[]> values = new Dictionary<string, byte[]>();
      values.Add("initialize", new byte[1]);
      values.Add("data", array);
      values.Add("timeoutMinutes", BitConverter.GetBytes(item.Timeout));
      task.Wait();
      if (task.Result == null)
      {
        redisConnection.Hashes.Set(0, this.GetKeyForSessionId(id), values, false);
      }
      else
      {
        RedisSessionStateStore.LockData data;
        if (!RedisSessionStateStore.LockData.TryParse(task.Result, out data) || !((IEnumerable<byte>) data.LockId).SequenceEqual<byte>((IEnumerable<byte>) numArray))
          return;
        redisConnection.Hashes.Set(0, this.GetKeyForSessionId(id), values, false);
        redisConnection.Hashes.Remove(0, this.lockHashKey, id, false);
      }
    }

    public override SessionStateStoreData GetItem(HttpContext context, string id, out bool locked, out TimeSpan lockAge, out object lockId, out SessionStateActions actions)
    {
      RedisConnection redisConnection = this.GetRedisConnection();
      Task<Dictionary<string, byte[]>> all = redisConnection.Hashes.GetAll(0, this.GetKeyForSessionId(id), false);
      locked = false;
      lockAge = new TimeSpan(0L);
      lockId = (object) null;
      actions = SessionStateActions.None;
      if (all.Result == null)
        return (SessionStateStoreData) null;
      SessionStateItemCollection stateItemCollection = new SessionStateItemCollection();
      Dictionary<string, byte[]> result = all.Result;
      if (result.Count == 3)
      {
        MemoryStream memoryStream = new MemoryStream(result["data"]);
        if (memoryStream.Length > 0L)
          stateItemCollection = SessionStateItemCollection.Deserialize(new BinaryReader((Stream) memoryStream));
        int int32 = BitConverter.ToInt32(result["timeoutMinutes"], 0);
        redisConnection.Keys.Expire(0, this.GetKeyForSessionId(id), int32 * 60, false);
      }
      return new SessionStateStoreData((ISessionStateItemCollection) stateItemCollection, SessionStateUtility.GetSessionStaticObjects(context), this.redisConfig.SessionTimeout);
    }

    public override SessionStateStoreData GetItemExclusive(HttpContext context, string id, out bool locked, out TimeSpan lockAge, out object lockId, out SessionStateActions actions)
    {
      RedisConnection redisConnection = this.GetRedisConnection();
      Task<byte[]> task1 = redisConnection.Hashes.Get(0, this.lockHashKey, id, false);
      actions = SessionStateActions.None;
      locked = false;
      lockId = (object) null;
      lockAge = TimeSpan.MinValue;
      task1.Wait();
      byte[] result1 = task1.Result;
      if (result1 == null)
      {
        RedisSessionStateStore.LockData data = RedisSessionStateStore.LockData.New();
        using (RedisTransaction transaction = redisConnection.CreateTransaction())
        {
          Task<bool> task2 = transaction.Hashes.SetIfNotExists(0, this.lockHashKey, id, data.ToByteArray(), false);
          Task<Dictionary<string, byte[]>> all = transaction.Hashes.GetAll(0, this.GetKeyForSessionId(id), false);
          transaction.Execute(false, (object) null);
          task2.Wait();
          if (task2.Result)
          {
            locked = true;
            lockAge = new TimeSpan(0L);
            lockId = (object) data.LockId;
            SessionStateItemCollection stateItemCollection = new SessionStateItemCollection();
            Dictionary<string, byte[]> dictionary = redisConnection.Wait<Dictionary<string, byte[]>>(all);
            if (dictionary.Count == 3)
            {
              actions = (int) dictionary["initialize"][0] == 1 ? SessionStateActions.InitializeItem : SessionStateActions.None;
              MemoryStream memoryStream = new MemoryStream(dictionary["data"]);
              if (memoryStream.Length > 0L)
                stateItemCollection = SessionStateItemCollection.Deserialize(new BinaryReader((Stream) memoryStream));
              int int32 = BitConverter.ToInt32(dictionary["timeoutMinutes"], 0);
              redisConnection.Keys.Expire(0, this.GetKeyForSessionId(id), int32 * 60, false);
            }
            return new SessionStateStoreData((ISessionStateItemCollection) stateItemCollection, SessionStateUtility.GetSessionStaticObjects(context), this.redisConfig.SessionTimeout);
          }
          byte[] result2 = redisConnection.Hashes.Get(0, this.lockHashKey, id, false).Result;
          if (result2 != null && RedisSessionStateStore.LockData.TryParse(result2, out data))
          {
            locked = true;
            lockId = (object) data.LockId;
            lockAge = DateTime.UtcNow - data.LockUtcTime;
          }
          return (SessionStateStoreData) null;
        }
      }
      else
      {
        RedisSessionStateStore.LockData data;
        if (RedisSessionStateStore.LockData.TryParse(result1, out data))
        {
          locked = true;
          lockId = (object) data.LockId;
          lockAge = DateTime.UtcNow - data.LockUtcTime;
        }
        return (SessionStateStoreData) null;
      }
    }

    public override SessionStateStoreData CreateNewStoreData(HttpContext context, int timeout)
    {
      return new SessionStateStoreData((ISessionStateItemCollection) new SessionStateItemCollection(), SessionStateUtility.GetSessionStaticObjects(context), timeout);
    }

    public override void CreateUninitializedItem(HttpContext context, string id, int timeout)
    {
      RedisConnection redisConnection = this.GetRedisConnection();
      MemoryStream memoryStream = new MemoryStream();
      BinaryWriter writer = new BinaryWriter((Stream) memoryStream);
      new SessionStateItemCollection().Serialize(writer);
      writer.Close();
      byte[] array = memoryStream.ToArray();
      redisConnection.Hashes.Set(0, this.GetKeyForSessionId(id), new Dictionary<string, byte[]>()
      {
        {
          "data",
          array
        },
        {
          "initialize",
          new byte[1]
        },
        {
          "timeoutMinutes",
          BitConverter.GetBytes(timeout)
        }
      }, false);
      redisConnection.Keys.Expire(0, this.GetKeyForSessionId(id), timeout * 60, false);
    }

    public override void Dispose()
    {
      IDisposable redisConnection;
      if ((RedisConnection) (redisConnection = (IDisposable) this.GetRedisConnection()) == null)
        return;
      redisConnection.Dispose();
    }

    public override void ReleaseItemExclusive(HttpContext context, string id, object lockId)
    {
      this.GetRedisConnection().Hashes.Remove(0, this.lockHashKey, id, false);
    }

    public override void RemoveItem(HttpContext context, string id, object lockId, SessionStateStoreData item)
    {
      RedisConnection redisConnection = this.GetRedisConnection();
      Task<byte[]> task = redisConnection.Hashes.Get(0, this.lockHashKey, id, false);
      byte[] numArray = (byte[]) lockId;
      RedisSessionStateStore.LockData data;
      if (task.Result == null || !RedisSessionStateStore.LockData.TryParse(task.Result, out data) || !((IEnumerable<byte>) data.LockId).SequenceEqual<byte>((IEnumerable<byte>) numArray))
        return;
      redisConnection.Keys.Remove(0, this.GetKeyForSessionId(id), false);
      redisConnection.Hashes.Remove(0, this.lockHashKey, id, false);
    }

    public override bool SetItemExpireCallback(SessionStateItemExpireCallback expireCallback)
    {
      return false;
    }

    public override void ResetItemTimeout(HttpContext context, string id)
    {
      RedisConnection redisConnection = this.GetRedisConnection();
      Task<byte[]> task = redisConnection.Hashes.Get(0, this.GetKeyForSessionId(id), "timeoutMinutes", false);
      if (task.Result == null)
        return;
      int int32 = BitConverter.ToInt32(task.Result, 0);
      redisConnection.Keys.Expire(0, this.GetKeyForSessionId(id), int32 * 60, false);
    }

    public override void InitializeRequest(HttpContext context)
    {
    }

    public override void EndRequest(HttpContext context)
    {
    }

    internal struct LockData
    {
      private static readonly byte SEPERATOR = Encoding.ASCII.GetBytes(";")[0];
      public byte[] LockId;
      public DateTime LockUtcTime;

      public static RedisSessionStateStore.LockData New()
      {
        return new RedisSessionStateStore.LockData()
        {
          LockId = Guid.NewGuid().ToByteArray(),
          LockUtcTime = DateTime.UtcNow
        };
      }

      public static bool TryParse(byte[] raw, out RedisSessionStateStore.LockData data)
      {
        if (raw != null && raw.Length > 1)
        {
          byte[] array = ((IEnumerable<byte>) raw).Take<byte>(16).ToArray<byte>();
          long int64 = BitConverter.ToInt64(raw, 17);
          data = new RedisSessionStateStore.LockData()
          {
            LockId = array,
            LockUtcTime = new DateTime(int64)
          };
          return true;
        }
        data = new RedisSessionStateStore.LockData();
        return false;
      }

      public override string ToString()
      {
        return BitConverter.ToString(this.LockId) + ";" + (object) this.LockUtcTime.Ticks;
      }

      public byte[] ToByteArray()
      {
        return ((IEnumerable<byte>) this.LockId).Concat<byte>(((IEnumerable<byte>) new byte[1]
        {
          RedisSessionStateStore.LockData.SEPERATOR
        }).Concat<byte>((IEnumerable<byte>) BitConverter.GetBytes(this.LockUtcTime.Ticks))).ToArray<byte>();
      }
    }
  }
}
