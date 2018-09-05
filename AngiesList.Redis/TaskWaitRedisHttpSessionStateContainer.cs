// Decompiled with JetBrains decompiler
// Type: AngiesList.Redis.TaskWaitRedisHttpSessionStateContainer
// Assembly: AngiesList.Redis, Version=1.1.5023.23192, Culture=neutral, PublicKeyToken=null
// MVID: A019BE0F-3773-46AA-8598-F03B954DBF6D
// Assembly location: D:\projects\dotnet\vinsight\solutions\dependencies\Redis\AngiesList.Redis.dll

using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.SessionState;

namespace AngiesList.Redis
{
    internal class TaskWaitRedisHttpSessionStateContainer : HttpSessionStateContainer
    {
        public RedisSessionItemHash SessionItems { get; private set; }

        public TaskWaitRedisHttpSessionStateContainer(string id, RedisSessionItemHash sessionItems, HttpStaticObjectsCollection staticObjects, int timeout, bool newSession, HttpCookieMode cookieMode, SessionStateMode mode, bool isReadonly)
            : base(id, (ISessionStateItemCollection)sessionItems, staticObjects, timeout, newSession, cookieMode, mode, isReadonly)
        {
            this.SessionItems = sessionItems;
        }

        public bool WaitOnAllPersistent()
        {
            this.SessionItems.PersistChangedReferences();
            return Task.WaitAll(this.SessionItems.SetTasks.ToArray<Task>(), 1500);
        }
    }
}
