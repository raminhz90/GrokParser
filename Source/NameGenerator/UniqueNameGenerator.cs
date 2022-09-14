namespace GrokParser.NameGenerator
{
    using System.Text;

    internal static class UniqueNameGenerator
    {
        internal static string GenerateUniqueName(int patternId, int id, int num)
        {
            var builder = new StringBuilder();
            while (patternId > 0)
            {
                var remainder = patternId % 25;
                _ = builder.Append((char)(remainder + 97));
                patternId /= 25;
            }
            _ = builder.Append('Z');
            while (id > 0)
            {
                var remainder = id % 25;
                _ = builder.Append((char)(remainder + 97));
                id /= 25;
            }
            _ = builder.Append('Z');
            while (num > 0)
            {
                var remainder = num % 25;
                _ = builder.Append((char)(remainder + 97));
                num /= 25;
            }
            return builder.ToString();
        }
    }
}
