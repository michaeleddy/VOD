    using System;
    using System.Runtime.CompilerServices;

namespace VOD.Lib.Libs
{
    public abstract class EndianBitConverter
    {
        public static EndianBitConverter LittleEndian { get; } = new LittleEndianBitConverter();
        public static EndianBitConverter BigEndian { get; } = new BigEndianBitConverter();
        public abstract bool IsLittleEndian { get; }
        public byte[] GetBytes(bool value)
        {
            return new byte[] { value ? (byte)1 : (byte)0 };
        }
        public byte[] GetBytes(char value)
        {
            return this.GetBytes((short)value);
        }
        public byte[] GetBytes(double value)
        {
            long val = BitConverter.DoubleToInt64Bits(value);
            return this.GetBytes(val);
        }
        public abstract byte[] GetBytes(short value);
        public abstract byte[] GetBytes(int value);
        public abstract byte[] GetBytes(long value);
        public byte[] GetBytes(float value)
        {
            int val = new SingleConverter(value).GetIntValue();
            return this.GetBytes(val);
        }
        [CLSCompliant(false)]
        public byte[] GetBytes(ushort value)
        {
            return this.GetBytes((short)value);
        }
        [CLSCompliant(false)]
        public byte[] GetBytes(uint value)
        {
            return this.GetBytes((int)value);
        }
        [CLSCompliant(false)]
        public byte[] GetBytes(ulong value)
        {
            return this.GetBytes((long)value);
        }
        public bool ToBoolean(byte[] value, int startIndex)
        {
            this.CheckArguments(value, startIndex, 1);
            return value[startIndex] != 0;
        }
        public char ToChar(byte[] value, int startIndex)
        {
            return (char)this.ToInt16(value, startIndex);
        }
        public double ToDouble(byte[] value, int startIndex)
        {
            long val = this.ToInt64(value, startIndex);
            return BitConverter.Int64BitsToDouble(val);
        }
        public abstract short ToInt16(byte[] value, int startIndex);
        public abstract int ToInt32(byte[] value, int startIndex);
        public abstract long ToInt64(byte[] value, int startIndex);
        public float ToSingle(byte[] value, int startIndex)
        {
            int val = this.ToInt32(value, startIndex);
            return new SingleConverter(val).GetFloatValue();
        }
        [CLSCompliant(false)]
        public ushort ToUInt16(byte[] value, int startIndex)
        {
            return (ushort)this.ToInt16(value, startIndex);
        }
        [CLSCompliant(false)]
        public uint ToUInt32(byte[] value, int startIndex)
        {
            return (uint)this.ToInt32(value, startIndex);
        }
        [CLSCompliant(false)]
        public ulong ToUInt64(byte[] value, int startIndex)
        {
            return (ulong)this.ToInt64(value, startIndex);
        }
#if !NET40
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        internal void CheckArguments(byte[] value, int startIndex, int byteLength)
        {
            if (value == null)
                throw new ArgumentNullException(nameof(value));
            if ((uint)startIndex > value.Length - byteLength)
                throw new ArgumentOutOfRangeException(nameof(value));
        }
    }
}