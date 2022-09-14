namespace GrokParser.TypeMapper
{
    using System;

    internal static class TypeMapper
    {
        internal static dynamic Map(string type, string value)
        {
            // convert to types int double float bool datetime long datetimeoffset
            switch (type)
            {
                case "int":
                {
                    if (int.TryParse(value, out var result))
                    {
                        return result;
                    }
                    return value;
                }
                case "double":
                {
                    if (double.TryParse(value, out var result))
                    {
                        return result;
                    }
                    return value;
                }
                case "float":
                {
                    if (float.TryParse(value, out var result))
                    {
                        return result;
                    }
                    return value;
                }
                case "bool":
                {
                    if (bool.TryParse(value, out var result))
                    {
                        return result;
                    }
                    return value;
                }
                case "datetime":
                {
                    if (DateTime.TryParse(value, out var result))
                    {
                        return result;
                    }
                    return value;
                }
                case "long":
                {
                    if (long.TryParse(value, out var result))
                    {
                        return result;
                    }
                    return value;
                }
                case "datetimeoffset":
                {
                    if (DateTimeOffset.TryParse(value, out var result))
                    {
                        return result;
                    }
                    return value;
                }
                default:
                {
                    return value;
                }
            }
        }
    }
}
