// Decompiled with JetBrains decompiler
// Type: AngiesList.Redis.SSJsonSerializer
// Assembly: AngiesList.Redis, Version=1.1.5023.23192, Culture=neutral, PublicKeyToken=null
// MVID: A019BE0F-3773-46AA-8598-F03B954DBF6D
// Assembly location: D:\projects\dotnet\vinsight\solutions\dependencies\Redis\AngiesList.Redis.dll

using ServiceStack.Text;
using System;
using System.IO;

namespace AngiesList.Redis
{
  public class SSJsonSerializer : IValueSerializer
  {
    public byte[] Serialize(object value)
    {
      MemoryStream memoryStream = new MemoryStream(4);
      JsonSerializer.SerializeToStream<object>(value, (Stream) memoryStream);
      byte[] array = memoryStream.ToArray();
      memoryStream.Close();
      return array;
    }

    public object Deserialize(byte[] bytes)
    {
      throw new NotImplementedException();
    }

    public object Deserialize(Type type, byte[] bytes)
    {
      MemoryStream memoryStream = new MemoryStream(bytes);
      object obj = JsonSerializer.DeserializeFromStream(type, (Stream) memoryStream);
      memoryStream.Close();
      return obj;
    }
  }
}
