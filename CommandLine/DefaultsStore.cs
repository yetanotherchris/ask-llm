using System.Runtime.InteropServices;
using System.Text;
using Microsoft.Extensions.Logging;

namespace AskLlm.CommandLine
{
    public sealed class DefaultsStore
    {
        private readonly ILogger<DefaultsStore> _logger;

        public DefaultsStore(ILogger<DefaultsStore> logger)
        {
            _logger = logger;
        }

        public bool TryStoreDefaults(string? defaults)
        {
            try
            {
                if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                {
                    // On Windows, use user-level environment variables for persistence
                    Environment.SetEnvironmentVariable(EnvironmentVariableNames.Defaults, string.IsNullOrWhiteSpace(defaults) ? null : defaults, EnvironmentVariableTarget.User);
                    return true;
                }
                else
                {
                    // On Linux/macOS, try to write to shell profile for persistence
                    var success = TryWriteToShellProfile(defaults);
                    
                    // Also set for current session
                    Environment.SetEnvironmentVariable(EnvironmentVariableNames.Defaults, string.IsNullOrWhiteSpace(defaults) ? null : defaults, EnvironmentVariableTarget.Process);
                    
                    return success;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to store command defaults.");
                return false;
            }
        }

        public string GetStoredDefaults()
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                // On Windows, try user-level environment variables first, then fall back to process-level
                return Environment.GetEnvironmentVariable(EnvironmentVariableNames.Defaults, EnvironmentVariableTarget.User) ?? 
                       Environment.GetEnvironmentVariable(EnvironmentVariableNames.Defaults, EnvironmentVariableTarget.Process) ?? 
                       string.Empty;
            }
            else
            {
                // On Linux/macOS, try process-level first, then check shell profile
                var processValue = Environment.GetEnvironmentVariable(EnvironmentVariableNames.Defaults, EnvironmentVariableTarget.Process);
                if (!string.IsNullOrEmpty(processValue))
                {
                    return processValue;
                }

                // If not in current process, try to read from shell profile
                return ReadFromShellProfile() ?? string.Empty;
            }
        }

        public string[] MergeWithStoredDefaults(string[] args)
        {
            var defaultsRaw = GetStoredDefaults();
            if (string.IsNullOrWhiteSpace(defaultsRaw))
            {
                return args;
            }

            // Support "askllm here is my prompt" when the defaults have already been set
            string promptOnly = string.Join("", args);
            if (!promptOnly.Contains("--"))
            {
                args = new string[] { "--prompt", $"\"{promptOnly}\"" };
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

        private bool TryWriteToShellProfile(string? defaults)
        {
            try
            {
                var homeDir = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
                if (string.IsNullOrEmpty(homeDir))
                    return false;

                // Try common shell profile files in order of preference
                var profileFiles = new[]
                {
                    Path.Combine(homeDir, ".zshrc"),      // zsh (default on macOS)
                    Path.Combine(homeDir, ".bashrc"),     // bash
                    Path.Combine(homeDir, ".bash_profile"), // bash (macOS)
                    Path.Combine(homeDir, ".profile")     // generic shell profile
                };

                string? targetFile = null;
                foreach (var file in profileFiles)
                {
                    if (File.Exists(file))
                    {
                        targetFile = file;
                        break;
                    }
                }

                // If no profile exists, create .profile
                if (targetFile == null)
                {
                    targetFile = Path.Combine(homeDir, ".profile");
                }

                var envVarLine = $"export {EnvironmentVariableNames.Defaults}=\"{defaults}\"";
                var commentLine = "# ask-llm command defaults";
                
                if (File.Exists(targetFile))
                {
                    var lines = File.ReadAllLines(targetFile).ToList();
                    
                    // Remove existing ask-llm entries
                    for (int i = lines.Count - 1; i >= 0; i--)
                    {
                        if (lines[i].Contains(EnvironmentVariableNames.Defaults) || 
                            lines[i].Contains("# ask-llm command defaults"))
                        {
                            lines.RemoveAt(i);
                        }
                    }

                    // Add new entry if defaults is not empty
                    if (!string.IsNullOrWhiteSpace(defaults))
                    {
                        lines.Add("");
                        lines.Add(commentLine);
                        lines.Add(envVarLine);
                    }

                    File.WriteAllLines(targetFile, lines);
                }
                else if (!string.IsNullOrWhiteSpace(defaults))
                {
                    // Create new profile file
                    File.WriteAllLines(targetFile, new[] { commentLine, envVarLine });
                }

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogDebug(ex, "Failed to write to shell profile");
                return false;
            }
        }

        private string? ReadFromShellProfile()
        {
            try
            {
                var homeDir = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
                if (string.IsNullOrEmpty(homeDir))
                    return null;

                // Try common shell profile files in order of preference
                var profileFiles = new[]
                {
                    Path.Combine(homeDir, ".zshrc"),      // zsh (default on macOS)
                    Path.Combine(homeDir, ".bashrc"),     // bash
                    Path.Combine(homeDir, ".bash_profile"), // bash (macOS)
                    Path.Combine(homeDir, ".profile")     // generic shell profile
                };

                foreach (var file in profileFiles)
                {
                    if (!File.Exists(file))
                        continue;

                    var lines = File.ReadAllLines(file);
                    foreach (var line in lines)
                    {
                        var trimmed = line.Trim();
                        if (trimmed.StartsWith($"export {EnvironmentVariableNames.Defaults}="))
                        {
                            // Extract the value from: export VAR_NAME="value"
                            var equalIndex = trimmed.IndexOf('=');
                            if (equalIndex > 0 && equalIndex < trimmed.Length - 1)
                            {
                                var value = trimmed.Substring(equalIndex + 1);
                                // Remove surrounding quotes if present
                                if (value.Length >= 2 && value.StartsWith("\"") && value.EndsWith("\""))
                                {
                                    value = value.Substring(1, value.Length - 2);
                                }
                                return value;
                            }
                        }
                    }
                }

                return null;
            }
            catch
            {
                return null;
            }
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
