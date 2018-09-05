// Decompiled with JetBrains decompiler
// Type: AngiesList.Redis.ClrBinarySerializer
// Assembly: AngiesList.Redis, Version=1.1.5023.23192, Culture=neutral, PublicKeyToken=null
// MVID: A019BE0F-3773-46AA-8598-F03B954DBF6D
// Assembly location: D:\projects\dotnet\vinsight\solutions\dependencies\Redis\AngiesList.Redis.dll

using System;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;

namespace AngiesList.Redis
{
    public class ClrBinarySerializer : IValueSerializer
    {
        private static readonly ArraySegment<byte> NullArray = new ArraySegment<byte>(new byte[0]);
        private const ushort RawDataFlag = 64082;

        public virtual byte[] Serialize(object value)
        {
            ClrBinarySerializer.CacheItem cacheItem = this.SerializeImpl(value);
            byte[] numArray = new byte[cacheItem.Data.Count + 2];
            Array.Copy((Array)BitConverter.GetBytes(cacheItem.Flags), 0, (Array)numArray, 0, 2);
            Array.Copy((Array)cacheItem.Data.Array, cacheItem.Data.Offset, (Array)numArray, 2, cacheItem.Data.Count);
            return numArray;
        }

        private ClrBinarySerializer.CacheItem SerializeImpl(object value)
        {
            if (value is ArraySegment<byte>)
                return new ClrBinarySerializer.CacheItem((ushort)64082, (ArraySegment<byte>)value);
            byte[] array = value as byte[];
            if (array != null)
                return new ClrBinarySerializer.CacheItem((ushort)64082, new ArraySegment<byte>(array));
            TypeCode typeCode = value == null ? TypeCode.DBNull : Type.GetTypeCode(value.GetType());
            ArraySegment<byte> data;
            switch (typeCode)
            {
                case TypeCode.DBNull:
                    data = this.SerializeNull();
                    break;
                case TypeCode.Boolean:
                    data = this.SerializeBoolean((bool)value);
                    break;
                case TypeCode.Char:
                    data = this.SerializeChar((char)value);
                    break;
                case TypeCode.Int16:
                    data = this.SerializeInt16((short)value);
                    break;
                case TypeCode.UInt16:
                    data = this.SerializeUInt16((ushort)value);
                    break;
                case TypeCode.Int32:
                    data = this.SerializeInt32((int)value);
                    break;
                case TypeCode.UInt32:
                    data = this.SerializeUInt32((uint)value);
                    break;
                case TypeCode.Int64:
                    data = this.SerializeInt64((long)value);
                    break;
                case TypeCode.UInt64:
                    data = this.SerializeUInt64((ulong)value);
                    break;
                case TypeCode.Single:
                    data = this.SerializeSingle((float)value);
                    break;
                case TypeCode.Double:
                    data = this.SerializeDouble((double)value);
                    break;
                case TypeCode.Decimal:
                    data = this.SerializeDecimal((Decimal)value);
                    break;
                case TypeCode.DateTime:
                    data = this.SerializeDateTime((DateTime)value);
                    break;
                case TypeCode.String:
                    data = this.SerializeString((string)value);
                    break;
                default:
                    data = this.SerializeObject(value);
                    break;
            }
            return new ClrBinarySerializer.CacheItem((ushort)((uint)(ushort)typeCode | 256U), data);
        }

        public virtual object Deserialize(byte[] bytes)
        {
            if (bytes == null)
                return (object)null;
            byte[] array = new byte[bytes.Length - 2];
            Array.Copy((Array)bytes, 2, (Array)array, 0, array.Length);
            return this.Deserialize(new ClrBinarySerializer.CacheItem(BitConverter.ToUInt16(bytes, 0), new ArraySegment<byte>(array)));
        }

        private object Deserialize(ClrBinarySerializer.CacheItem item)
        {
            if (item.Data.Array == null)
                return (object)null;
            if ((int)item.Flags == 64082)
            {
                ArraySegment<byte> data = item.Data;
                if (data.Count == data.Array.Length)
                    return (object)data.Array;
                byte[] numArray = new byte[data.Count];
                Array.Copy((Array)data.Array, data.Offset, (Array)numArray, 0, data.Count);
                return (object)numArray;
            }
            TypeCode typeCode = (TypeCode)((int)item.Flags & (int)byte.MaxValue);
            ArraySegment<byte> data1 = item.Data;
            switch (typeCode)
            {
                case TypeCode.Empty:
                    if (data1.Array != null && data1.Count != 0)
                        return (object)this.DeserializeString(data1);
                    return (object)null;
                case TypeCode.Object:
                    return this.DeserializeObject(data1);
                case TypeCode.DBNull:
                    return (object)null;
                case TypeCode.Boolean:
                    return (object)this.DeserializeBoolean(data1);
                case TypeCode.Char:
                    return (object)this.DeserializeChar(data1);
                case TypeCode.Int16:
                    return (object)this.DeserializeInt16(data1);
                case TypeCode.UInt16:
                    return (object)this.DeserializeUInt16(data1);
                case TypeCode.Int32:
                    return (object)this.DeserializeInt32(data1);
                case TypeCode.UInt32:
                    return (object)this.DeserializeUInt32(data1);
                case TypeCode.Int64:
                    return (object)this.DeserializeInt64(data1);
                case TypeCode.UInt64:
                    return (object)this.DeserializeUInt64(data1);
                case TypeCode.Single:
                    return (object)this.DeserializeSingle(data1);
                case TypeCode.Double:
                    return (object)this.DeserializeDouble(data1);
                case TypeCode.Decimal:
                    return (object)this.DeserializeDecimal(data1);
                case TypeCode.DateTime:
                    return (object)this.DeserializeDateTime(data1);
                case TypeCode.String:
                    return (object)this.DeserializeString(data1);
                default:
                    throw new InvalidOperationException("Unknown TypeCode was returned: " + (object)typeCode);
            }
        }

        protected virtual ArraySegment<byte> SerializeNull()
        {
            return ClrBinarySerializer.NullArray;
        }

        protected virtual ArraySegment<byte> SerializeString(string value)
        {
            return new ArraySegment<byte>(Encoding.UTF8.GetBytes(value));
        }

        protected virtual ArraySegment<byte> SerializeBoolean(bool value)
        {
            return new ArraySegment<byte>(BitConverter.GetBytes(value));
        }

        protected virtual ArraySegment<byte> SerializeInt16(short value)
        {
            return new ArraySegment<byte>(BitConverter.GetBytes(value));
        }

        protected virtual ArraySegment<byte> SerializeInt32(int value)
        {
            return new ArraySegment<byte>(BitConverter.GetBytes(value));
        }

        protected virtual ArraySegment<byte> SerializeInt64(long value)
        {
            return new ArraySegment<byte>(BitConverter.GetBytes(value));
        }

        protected virtual ArraySegment<byte> SerializeUInt16(ushort value)
        {
            return new ArraySegment<byte>(BitConverter.GetBytes(value));
        }

        protected virtual ArraySegment<byte> SerializeUInt32(uint value)
        {
            return new ArraySegment<byte>(BitConverter.GetBytes(value));
        }

        protected virtual ArraySegment<byte> SerializeUInt64(ulong value)
        {
            return new ArraySegment<byte>(BitConverter.GetBytes(value));
        }

        protected virtual ArraySegment<byte> SerializeChar(char value)
        {
            return new ArraySegment<byte>(BitConverter.GetBytes(value));
        }

        protected virtual ArraySegment<byte> SerializeDateTime(DateTime value)
        {
            return new ArraySegment<byte>(BitConverter.GetBytes(value.ToBinary()));
        }

        protected virtual ArraySegment<byte> SerializeDouble(double value)
        {
            return new ArraySegment<byte>(BitConverter.GetBytes(value));
        }

        protected virtual ArraySegment<byte> SerializeSingle(float value)
        {
            return new ArraySegment<byte>(BitConverter.GetBytes(value));
        }

        protected virtual ArraySegment<byte> SerializeDecimal(Decimal value)
        {
            using (MemoryStream memoryStream = new MemoryStream())
            {
                using (BinaryWriter binaryWriter = new BinaryWriter((Stream)memoryStream))
                    binaryWriter.Write(value);
                return new ArraySegment<byte>(memoryStream.ToArray());
            }
        }

        protected virtual ArraySegment<byte> SerializeObject(object value)
        {
            using (MemoryStream memoryStream = new MemoryStream())
            {
                new BinaryFormatter().Serialize((Stream)memoryStream, value);
                return new ArraySegment<byte>(memoryStream.GetBuffer(), 0, (int)memoryStream.Length);
            }
        }

        protected virtual string DeserializeString(ArraySegment<byte> value)
        {
            return Encoding.UTF8.GetString(value.Array, value.Offset, value.Count);
        }

        protected virtual bool DeserializeBoolean(ArraySegment<byte> value)
        {
            return BitConverter.ToBoolean(value.Array, value.Offset);
        }

        protected virtual short DeserializeInt16(ArraySegment<byte> value)
        {
            return BitConverter.ToInt16(value.Array, value.Offset);
        }

        protected virtual int DeserializeInt32(ArraySegment<byte> value)
        {
            return BitConverter.ToInt32(value.Array, value.Offset);
        }

        protected virtual long DeserializeInt64(ArraySegment<byte> value)
        {
            return BitConverter.ToInt64(value.Array, value.Offset);
        }

        protected virtual ushort DeserializeUInt16(ArraySegment<byte> value)
        {
            return BitConverter.ToUInt16(value.Array, value.Offset);
        }

        protected virtual uint DeserializeUInt32(ArraySegment<byte> value)
        {
            return BitConverter.ToUInt32(value.Array, value.Offset);
        }

        protected virtual ulong DeserializeUInt64(ArraySegment<byte> value)
        {
            return BitConverter.ToUInt64(value.Array, value.Offset);
        }

        protected virtual char DeserializeChar(ArraySegment<byte> value)
        {
            return BitConverter.ToChar(value.Array, value.Offset);
        }

        protected virtual DateTime DeserializeDateTime(ArraySegment<byte> value)
        {
            return DateTime.FromBinary(BitConverter.ToInt64(value.Array, value.Offset));
        }

        protected virtual double DeserializeDouble(ArraySegment<byte> value)
        {
            return BitConverter.ToDouble(value.Array, value.Offset);
        }

        protected virtual float DeserializeSingle(ArraySegment<byte> value)
        {
            return BitConverter.ToSingle(value.Array, value.Offset);
        }

        protected virtual Decimal DeserializeDecimal(ArraySegment<byte> value)
        {
            using (MemoryStream memoryStream = new MemoryStream(value.Array, value.Offset, value.Count))
            {
                using (BinaryReader binaryReader = new BinaryReader((Stream)memoryStream))
                    return binaryReader.ReadDecimal();
            }
        }

        protected virtual object DeserializeObject(ArraySegment<byte> value)
        {
            using (MemoryStream memoryStream = new MemoryStream(value.Array, value.Offset, value.Count))
                return new BinaryFormatter().Deserialize((Stream)memoryStream);
        }

        private struct CacheItem
        {
            private ArraySegment<byte> data;
            private ushort flags;

            /// <summary>
            /// Initializes a new instance of <see cref="T:CacheItem" />.
            /// </summary>
            /// <param name="flags">Custom item data.</param>
            /// <param name="data">The serialized item.</param>
            public CacheItem(ushort flags, ArraySegment<byte> data)
            {
                this.data = data;
                this.flags = flags;
            }

            /// <summary>
            /// The data representing the item being stored/retireved.
            /// </summary>
            public ArraySegment<byte> Data
            {
                get
                {
                    return this.data;
                }
                set
                {
                    this.data = value;
                }
            }

            /// <summary>Flags set for this instance.</summary>
            public ushort Flags
            {
                get
                {
                    return this.flags;
                }
                set
                {
                    this.flags = value;
                }
            }
        }
    }
}
