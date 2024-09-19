namespace TopMarksDevelopment.ExpressionBuilder.Serialization;

using System.Text;

internal static class ValueToByteConverters
{
    public static byte[]? Pack<T>(T? value)
    {
        if (value is string x)
            return Encoding.UTF8.GetBytes(x);

        if (value is int i)
            return BitConverter.GetBytes(i);

        if (value is short s)
            return BitConverter.GetBytes(s);

        if (value is long l)
            return BitConverter.GetBytes(l);

        if (value is uint ui)
            return BitConverter.GetBytes(ui);

        if (value is ushort us)
            return BitConverter.GetBytes(us);

        if (value is ulong ul)
            return BitConverter.GetBytes(ul);

        if (value is byte by)
            return [by];

        if (value is bool b)
            return BitConverter.GetBytes(b);

        if (value is DateTime dt)
            return BitConverter.GetBytes(dt.Ticks);

        if (value is DateTimeOffset dto)
            return BitConverter.GetBytes(dto.Ticks);

        if (value is DateOnly d)
            return BitConverter.GetBytes(d.DayNumber);

        if (value is TimeOnly t)
            return BitConverter.GetBytes(t.Ticks);
        else if (value is float f)
            return BitConverter.GetBytes(f);

        if (value is double db)
            return BitConverter.GetBytes(db);

        if (value is Guid g)
            return g.ToByteArray();

        if (value is decimal dcm)
            return decimal.GetBits(dcm)
                .SelectMany(BitConverter.GetBytes)
                .ToArray();

        if (value is byte[] ba)
            return ba;

        if (value is Enum e)
            return Pack(Enum.GetUnderlyingType(e.GetType()));

        if (value is null)
            return [];

        if (value is IProtoConverter c)
            return c.Pack();

        throw new NotSupportedException();
    }

    public static T? UnPack<T>(byte[] bytes) => (T?)UnPack(typeof(T), bytes);

    internal static object? UnPack(
        Type type,
        byte[]? bytes,
        bool isDeep = false
    )
    {
        if (bytes is null)
            return null!;

        if (type == typeof(string))
            return Encoding.UTF8.GetString(bytes);

        if (type == typeof(int))
            return BitConverter.ToInt32(bytes);

        if (type == typeof(short))
            return BitConverter.ToInt16(bytes);

        if (type == typeof(long))
            return BitConverter.ToInt64(bytes);

        if (type == typeof(uint))
            return BitConverter.ToUInt32(bytes);

        if (type == typeof(ushort))
            return BitConverter.ToUInt16(bytes);

        if (type == typeof(ulong))
            return BitConverter.ToUInt64(bytes);

        if (type == typeof(byte))
            return bytes[0];

        if (type == typeof(bool))
            return BitConverter.ToBoolean(bytes);

        if (type == typeof(DateTime))
            return new DateTime(BitConverter.ToInt64(bytes));

        if (type == typeof(DateTimeOffset))
            return new DateTimeOffset(
                BitConverter.ToInt64(bytes),
                TimeSpan.Zero
            );

        if (type == typeof(DateOnly))
            return new DateOnly().AddDays(BitConverter.ToInt32(bytes));

        if (type == typeof(TimeOnly))
            return new TimeOnly(BitConverter.ToInt64(bytes));

        if (type == typeof(float))
            return BitConverter.ToSingle(bytes);

        if (type == typeof(double))
            return BitConverter.ToDouble(bytes);

        if (type == typeof(Guid))
            return new Guid(bytes);

        if (type == typeof(decimal))
            return new decimal(
                BitConverter.ToInt32(bytes, 0),
                BitConverter.ToInt32(bytes, 4),
                BitConverter.ToInt32(bytes, 8),
                bytes[12] == 1,
                bytes[13]
            );

        if (type == typeof(byte[]))
            return bytes;

        if (type.IsEnum)
            return isDeep
                ? throw new NotImplementedException()
                : Enum.ToObject(
                    type,
                    UnPack(Enum.GetUnderlyingType(type), bytes, true)!
                );

        if (type is IProtoConverter c)
            return c.UnPack(bytes);

        throw new NotSupportedException();
    }
}
