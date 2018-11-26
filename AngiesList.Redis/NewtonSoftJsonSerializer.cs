// Decompiled with JetBrains decompiler
// Type: AngiesList.Redis.SSJsonSerializer
// Assembly: AngiesList.Redis, Version=1.1.5023.23192, Culture=neutral, PublicKeyToken=null
// MVID: A019BE0F-3773-46AA-8598-F03B954DBF6D
// Assembly location: D:\projects\dotnet\vinsight\solutions\dependencies\Redis\AngiesList.Redis.dll

using System;
using System.IO;
using System.Text;
using Newtonsoft.Json;
using JsonSerializer = ServiceStack.Text.JsonSerializer;

namespace AngiesList.Redis
{
  public class NewtonsoftJsonSerializer : IValueSerializer
  {
    public byte[] Serialize(object value)
    {
      var jsonString = JsonConvert.SerializeObject(value);
      return Encoding.UTF8.GetBytes(jsonString);
    }

    public object Deserialize(byte[] bytes)
    {
        var jsonString = Encoding.UTF8.GetString(bytes);
        return JsonConvert.DeserializeObject(jsonString);
    }

    public object Deserialize(byte[] bytes, Type type)
    {
        var jsonString = Encoding.UTF8.GetString(bytes);


        var settings = new JsonSerializerSettings
        {
            NullValueHandling = NullValueHandling.Ignore,
            MissingMemberHandling = MissingMemberHandling.Ignore
        };


        return JsonConvert.DeserializeObject(jsonString, type, settings);
    }
  }
}
