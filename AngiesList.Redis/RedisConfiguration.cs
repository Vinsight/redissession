// Decompiled with JetBrains decompiler
// Type: AngiesList.Redis.RedisConfiguration
// Assembly: AngiesList.Redis, Version=1.1.5023.23192, Culture=neutral, PublicKeyToken=null
// MVID: A019BE0F-3773-46AA-8598-F03B954DBF6D
// Assembly location: D:\projects\dotnet\vinsight\solutions\dependencies\Redis\AngiesList.Redis.dll

using System.IO;
using System.Web;
using System.Xml;

namespace AngiesList.Redis
{
  public class RedisConfiguration
  {
    protected const string CACHE_KEY = "KeyValueStoreConfiguration";
    protected const string SETTINGS_SECTION = "KeyValueStore/Master";
    protected const string CONFIG_FILE = "KeyValueStore.config";

    public RedisConfiguration()
    {
      this.Host = "localhost";
      this.Port = 6379;
    }

    /// <summary>Hostname of redis server. Defaults to localhost.</summary>
    public string Host { get; set; }

    /// <summary>Port number of redis server. Defaults to 6379.</summary>
    public int Port { get; set; }

    /// <summary>Reads the configuration file.</summary>
    /// <param name="path">The path to the config file. Defaults to KeyValueStore.config</param>
    /// <param name="watchFile">If the config file should be watched and automatically reloaded.</param>
    /// <returns></returns>
    public static RedisConfiguration ReadConfigFile(string path = "KeyValueStore.config", bool watchFile = true)
    {
      string str = HttpContext.Current != null ? Path.Combine(HttpContext.Current.Server.MapPath("~/"), path) : Path.Combine(Directory.GetCurrentDirectory(), path);
      XmlNode xmlNode = RedisConfiguration.LoadXmlDocFromPath(str).SelectSingleNode("KeyValueStore/Master");
      RedisConfiguration config = new RedisConfiguration();
      if (xmlNode != null && xmlNode.Attributes != null)
      {
        if (xmlNode.Attributes["host"] != null)
          config.Host = xmlNode.Attributes["host"].Value;
        if (xmlNode.Attributes["port"] != null)
          config.Port = int.Parse(xmlNode.Attributes["port"].Value);
      }
      RedisConfiguration.SetUpFileWatcher(str, config);
      return config;
    }

    private static XmlDocument LoadXmlDocFromPath(string path)
    {
      XmlDocument xmlDocument = new XmlDocument();
      xmlDocument.Load(path);
      return xmlDocument;
    }

    private static void SetUpFileWatcher(string fullPath, RedisConfiguration config)
    {
      new FileSystemWatcher(Path.GetDirectoryName(fullPath), Path.GetFileName(fullPath))
      {
        EnableRaisingEvents = true
      }.Changed += (FileSystemEventHandler) ((obj, args) =>
      {
        RedisConfiguration redisConfiguration = RedisConfiguration.ReadConfigFile(fullPath, false);
        config.Host = redisConfiguration.Host;
        config.Port = redisConfiguration.Port;
      });
    }
  }
}
