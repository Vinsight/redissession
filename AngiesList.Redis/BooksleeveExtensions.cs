// Decompiled with JetBrains decompiler
// Type: AngiesList.Redis.BooksleeveExtensions
// Assembly: AngiesList.Redis, Version=1.1.5023.23192, Culture=neutral, PublicKeyToken=null
// MVID: A019BE0F-3773-46AA-8598-F03B954DBF6D
// Assembly location: D:\projects\dotnet\vinsight\solutions\dependencies\Redis\AngiesList.Redis.dll

using BookSleeve;

namespace AngiesList.Redis
{
    internal static class BooksleeveExtensions
    {
        public static bool NeedsReset(this RedisConnectionBase connection)
        {
            if (connection == null)
                return true;
            if (connection.State != RedisConnectionBase.ConnectionState.Open)
                return connection.State != RedisConnectionBase.ConnectionState.Opening;
            return false;
        }
    }
}
