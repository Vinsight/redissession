// Decompiled with JetBrains decompiler
// Type: AngiesList.Redis.IValueSerializer
// Assembly: AngiesList.Redis, Version=1.1.5023.23192, Culture=neutral, PublicKeyToken=null
// MVID: A019BE0F-3773-46AA-8598-F03B954DBF6D
// Assembly location: D:\projects\dotnet\vinsight\solutions\dependencies\Redis\AngiesList.Redis.dll

namespace AngiesList.Redis
{
  public interface IValueSerializer
  {
    byte[] Serialize(object value);

    object Deserialize(byte[] bytes);
  }
}
