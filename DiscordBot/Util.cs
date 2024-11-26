
namespace DiscordBot
{
    public static class Util
    {
        public static int StringMatchDistance(string s, string t) // Stolen From the internet
        {
            s = s.ToUpper();
            t = t.ToUpper();

            int n = s.Length;
            int m = t.Length;
            int[,] d = new int[n + 1, m + 1];

            if (n == 0)
            {
                return m;
            }

            if (m == 0)
            {
                return n;
            }

            for (int i = 0; i <= n; d[i, 0] = i++)
            {
            }

            for (int j = 0; j <= m; d[0, j] = j++)
            {
            }

            for (int i = 1; i <= n; i++)
            {
                for (int j = 1; j <= m; j++)
                {
                    int cost = (t[j - 1] == s[i - 1]) ? 0 : 1;

                    d[i, j] = Math.Min(
                        Math.Min(d[i - 1, j] + 1, d[i, j - 1] + 1),
                        d[i - 1, j - 1] + cost);
                }
            }

            return d[n, m];
        }

        public static float StringMatchPercentage(string s, string t) => MathF.Max(t.Length - StringMatchDistance(s, t), 0) / MathF.Max(t.Length, 1);

        public static string[] GetArguements(string source, int args)
        {
            if (args == 0)
            {
                return Array.Empty<string>();
            }
            else if (args == 1)
            {
                return [source];
            }

            string[] result = new string[args];
            string[] split = source.Split(' ');

            for (int i = 0; i < args - 1 && i < split.Length; i++)
            {
                result[i] = split[i];
            }

            int times = split.Length - args + 1;
            int spaceIndex = source.Length - 1;
            while (times > 0)
            {
                spaceIndex--;
                if (source[spaceIndex] == ' ')
                {
                    times--;
                }
            }

            result[^1] = source.Substring(spaceIndex + 1);

            return result;
        }
    }
}
