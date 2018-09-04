// Decompiled with JetBrains decompiler
// Type: AngiesList.Redis.RedisSessionStateConfiguration
// Assembly: AngiesList.Redis, Version=1.1.5023.23192, Culture=neutral, PublicKeyToken=null
// MVID: A019BE0F-3773-46AA-8598-F03B954DBF6D
// Assembly location: D:\projects\dotnet\vinsight\solutions\dependencies\Redis\AngiesList.Redis.dll

using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Configuration;
using System.Web.Hosting;

namespace AngiesList.Redis
{
  /// <summary>
  /// Contains configuration for the session state provider.
  /// If untouched, this simply reads the sessionState section from web.config.
  /// Optionally, the <see cref="M:AngiesList.Redis.RedisSessionStateConfiguration.Configure(System.Action{AngiesList.Redis.RedisSessionStateConfiguration})" /> method can be called using
  /// PreApplicationStartMethodAttribute to programmatically configure the session
  /// state provider.
  /// </summary>
  public class RedisSessionStateConfiguration : RedisConfiguration
  {
    private static RedisSessionStateConfiguration Instance;

    private RedisSessionStateConfiguration()
    {
      this.CookieMode = HttpCookieMode.UseCookies;
      this.SessionTimeout = 60;
    }

    /// <summary>ASP.NET Cookie mode</summary>
    public HttpCookieMode CookieMode { get; set; }

    /// <summary>Session timeout (minutes). Defaults to 60.</summary>
    public int SessionTimeout { get; set; }

    /// <summary>
    /// Configure the redis session state provider. Note this is global,
    /// and typically would be called using PreApplicationStartMethodAttribute
    /// (see http://haacked.com/archive/2010/05/16/three-hidden-extensibility-gems-in-asp-net-4.aspx)
    /// </summary>
    /// <param name="config"></param>
    public static void Configure(Action<RedisSessionStateConfiguration> config)
    {
      if (RedisSessionStateConfiguration.Instance == null)
        RedisSessionStateConfiguration.Instance = new RedisSessionStateConfiguration();
      config(RedisSessionStateConfiguration.Instance);
    }

    /// <summary>
    /// Loads the current configuration. If no configuration has been supplied yet,
    /// the settings are initialized by <see cref="M:AngiesList.Redis.RedisSessionStateConfiguration.UseWebConfig">reading from web.config</see>.
    /// </summary>
    /// <returns></returns>
    public static RedisSessionStateConfiguration GetConfiguration()
    {
      if (RedisSessionStateConfiguration.Instance == null)
        RedisSessionStateConfiguration.UseWebConfig();
      return RedisSessionStateConfiguration.Instance;
    }

    /// <summary>
    /// Configure the redis session state provider using the regular sessionState
    /// section in web.config.
    /// </summary>
    public static void UseWebConfig()
    {
      RedisSessionStateConfiguration.Configure((Action<RedisSessionStateConfiguration>) (x => x.LoadFromWebConfig()));
    }

    /// <summary>
    /// Loads settings from the regular sessionState section in web.config.
    /// </summary>
    public void LoadFromWebConfig()
    {
      SessionStateSection section = (SessionStateSection) WebConfigurationManager.OpenWebConfiguration(HostingEnvironment.ApplicationVirtualPath).GetSection("system.web/sessionState");
      string connectionString = section.StateConnectionString;
      if (!string.IsNullOrWhiteSpace(connectionString))
      {
        string[] strArray = connectionString.Split(new char[2]
        {
          '=',
          ':'
        });
        this.Host = ((IEnumerable<string>) strArray).ElementAtOrDefault<string>(1) ?? "localhost";
        this.Port = int.Parse(((IEnumerable<string>) strArray).ElementAtOrDefault<string>(2) ?? "6379");
      }
      this.SessionTimeout = (int) section.Timeout.TotalMinutes;
    }
  }
}
