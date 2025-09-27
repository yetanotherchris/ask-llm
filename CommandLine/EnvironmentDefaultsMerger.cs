using System.Text;

namespace AskLlm.CommandLine
{
    public sealed class EnvironmentDefaultsMerger
    {
        public string GetStoredArgs()
        {
            return Environment.GetEnvironmentVariable(EnvironmentVariableNames.Defaults, EnvironmentVariableTarget.User);
        }

        public string[] MergeWithStoredDefaults(string[] args)
        {
            var defaultsRaw = GetStoredArgs();
            if (string.IsNullOrWhiteSpace(defaultsRaw))
            {
                return args;
            }

            var defaultArguments = SplitArguments(defaultsRaw);
            if (defaultArguments.Count == 0)
            {
                return args;
            }

            var providedOptions = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            foreach (var argument in args)
            {
                if (IsOption(argument))
                {
                    providedOptions.Add(argument);
                }
            }

            var mergedArguments = new List<string>(args);

            for (var i = 0; i < defaultArguments.Count; i++)
            {
                var token = defaultArguments[i];
                if (!IsOption(token))
                {
                    continue;
                }

                if (string.Equals(token, "--prompt", StringComparison.OrdinalIgnoreCase))
                {
                    if (i + 1 < defaultArguments.Count && !IsOption(defaultArguments[i + 1]))
                    {
                        i++;
                    }

                    continue;
                }

                var hasValue = i + 1 < defaultArguments.Count && !IsOption(defaultArguments[i + 1]);
                if (!hasValue)
                {
                    continue;
                }

                if (providedOptions.Contains(token))
                {
                    i++;
                    continue;
                }

                mergedArguments.Add(token);
                mergedArguments.Add(defaultArguments[i + 1]);
                i++;
            }

            return mergedArguments.ToArray();
        }

        private static bool IsOption(string value)
        {
            return value.StartsWith("--", StringComparison.Ordinal);
        }

        private static List<string> SplitArguments(string commandLine)
        {
            var results = new List<string>();
            var current = new StringBuilder();
            var inQuotes = false;
            var escapeNext = false;

            foreach (var character in commandLine)
            {
                if (escapeNext)
                {
                    current.Append(character);
                    escapeNext = false;
                    continue;
                }

                if (character == '\\')
                {
                    escapeNext = true;
                    continue;
                }

                if (character == '"')
                {
                    inQuotes = !inQuotes;
                    continue;
                }

                if (char.IsWhiteSpace(character) && !inQuotes)
                {
                    if (current.Length > 0)
                    {
                        results.Add(current.ToString());
                        current.Clear();
                    }

                    continue;
                }

                current.Append(character);
            }

            if (escapeNext)
            {
                current.Append('\\');
            }

            if (current.Length > 0)
            {
                results.Add(current.ToString());
            }

            return results;
        }
    }
}