// Decompiled with JetBrains decompiler
// Type: AngiesList.Redis.RedisSessionStateModule
// Assembly: AngiesList.Redis, Version=1.1.5023.23192, Culture=neutral, PublicKeyToken=null
// MVID: A019BE0F-3773-46AA-8598-F03B954DBF6D
// Assembly location: D:\projects\dotnet\vinsight\solutions\dependencies\Redis\AngiesList.Redis.dll

using BookSleeve;
using System;
using System.Web;
using System.Web.SessionState;

namespace AngiesList.Redis
{
  public sealed class RedisSessionStateModule : IHttpModule, IDisposable
  {
    private bool initialized;
    private bool releaseCalled;
    private ISessionIDManager sessionIDManager;
    private RedisConnection redisConnection;
    private RedisSessionStateConfiguration redisConfig;

    public void Init(HttpApplication app)
    {
      if (this.initialized)
        return;
      lock (typeof (RedisSessionStateModule))
      {
        if (this.initialized)
          return;
        this.redisConfig = RedisSessionStateConfiguration.GetConfiguration();
        app.AcquireRequestState += new EventHandler(this.OnAcquireRequestState);
        app.ReleaseRequestState += new EventHandler(this.OnReleaseRequestState);
        app.EndRequest += new EventHandler(this.OnEndRequest);
        this.sessionIDManager = (ISessionIDManager) new SessionIDManager();
        this.sessionIDManager.Initialize();
        this.redisConnection = new RedisConnection(this.redisConfig.Host, this.redisConfig.Port, -1, (string) null, int.MaxValue, false, 10000);
        this.initialized = true;
      }
    }

    private RedisConnection GetRedisConnection()
    {
      if (this.redisConnection.NeedsReset())
      {
        lock (typeof (RedisSessionStateModule))
        {
          if (this.redisConnection.NeedsReset())
          {
            this.redisConnection = new RedisConnection(this.redisConfig.Host, this.redisConfig.Port, -1, (string) null, int.MaxValue, false, 10000);
            this.redisConnection.Closed += (EventHandler) ((sender, e) => {});
            this.redisConnection.Open();
          }
        }
      }
      return this.redisConnection;
    }

    public void Dispose()
    {
      this.redisConnection.Dispose();
    }

    private bool RequiresSessionState(HttpContextBase context)
    {
      if (context.Session != null)
      {
        int mode = (int) context.Session.Mode;
        if (context.Session.Mode == SessionStateMode.Off)
          return false;
      }
      if (!(context.Handler is IRequiresSessionState))
        return context.Handler is IReadOnlySessionState;
      return true;
    }

    private void OnAcquireRequestState(object source, EventArgs args)
    {
      HttpContext context = ((HttpApplication) source).Context;
      bool newSession = false;
      bool supportSessionIDReissue = true;
      this.sessionIDManager.InitializeRequest(context, false, out supportSessionIDReissue);
      string sessionId = this.sessionIDManager.GetSessionID(context);
      if (sessionId == null)
      {
        sessionId = this.sessionIDManager.CreateSessionID(context);
        bool redirected;
        bool cookieAdded;
        this.sessionIDManager.SaveSessionID(context, sessionId, out redirected, out cookieAdded);
        newSession = true;
        if (redirected)
          return;
      }
      if (!this.RequiresSessionState((HttpContextBase) new HttpContextWrapper(context)))
        return;
      this.releaseCalled = false;
      RedisSessionItemHash sessionItems = new RedisSessionItemHash(sessionId, this.redisConfig.SessionTimeout, this.GetRedisConnection());
      if (sessionItems.Count == 0)
        newSession = true;
      SessionStateUtility.AddHttpSessionStateToContext(context, (IHttpSessionState) new TaskWaitRedisHttpSessionStateContainer(sessionId, sessionItems, SessionStateUtility.GetSessionStaticObjects(context), this.redisConfig.SessionTimeout, newSession, this.redisConfig.CookieMode, SessionStateMode.Custom, false));
      if (!newSession || this.Start == null)
        return;
      this.Start((object) this, EventArgs.Empty);
    }

    public event EventHandler Start;

    private void OnReleaseRequestState(object source, EventArgs args)
    {
      HttpContext context = ((HttpApplication) source).Context;
      if (context == null || context.Session == null)
        return;
      this.releaseCalled = true;
      TaskWaitRedisHttpSessionStateContainer stateFromContext = (TaskWaitRedisHttpSessionStateContainer) SessionStateUtility.GetHttpSessionStateFromContext(context);
      if (stateFromContext.IsAbandoned)
      {
        stateFromContext.Clear();
        SessionStateUtility.RaiseSessionEnd((IHttpSessionState) stateFromContext, (object) this, EventArgs.Empty);
      }
      else
        stateFromContext.WaitOnAllPersistent();
      SessionStateUtility.RemoveHttpSessionStateFromContext(context);
    }

    private void OnEndRequest(object source, EventArgs eventArgs)
    {
      if (this.releaseCalled)
        return;
      this.OnReleaseRequestState(source, eventArgs);
    }
  }
}
