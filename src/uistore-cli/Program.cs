using System.Reflection;

namespace CloudInteractive.UniFiStore;

internal static class Program
{
    public static StoreFront? StoreFront = null;
    private static readonly Dictionary<string, MethodInfo> Commands = new(StringComparer.OrdinalIgnoreCase);
    static void Main()
    {
        var methods = Assembly.GetExecutingAssembly()
            .GetTypes()
            .SelectMany(t => t.GetMethods(BindingFlags.Public | BindingFlags.Static))
            .Where(m => m.GetCustomAttribute<CommandAttribute>() != null);

        foreach (var method in methods)
        {
            var attr = method.GetCustomAttribute<CommandAttribute>();
            Commands[attr!.Name] = method;
        }
        
        //Use Alternative Screen Buffer
        Console.WriteLine("\u001b[?1049hWelcome to Ubiquiti Store CLI.\nType 'help' for available commands. Type 'exit' to quit.\n");
        while (true)
        {
            Console.Write("> ");
            var input = Console.ReadLine();
            if (string.IsNullOrWhiteSpace(input))
                continue;

            if (input.Equals("exit", StringComparison.OrdinalIgnoreCase)) break;
            if (input.Equals("help", StringComparison.OrdinalIgnoreCase))
            {
                ShowHelp();
                continue;
            }
            var tokens = Tokenize(input);
            if (tokens.Count == 0) continue;
            
            var commandName = tokens[0];
            tokens.RemoveAt(0);
            if (!Commands.TryGetValue(commandName, out MethodInfo? method))
            {
                Console.WriteLine($"Unknown command: {commandName}\n");
                continue;
            }

            try
            {
                var parameters = method.GetParameters();
                var arg = new object[parameters.Length];

                var options = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
                var positional = new List<string>();

                for (int i = 0; i < tokens.Count; i++)
                {
                    var token = tokens[i];
                    if (token.StartsWith("--"))
                    {
                        string optionName;
                        string optionValue;
                        if (token.Contains("="))
                        {
                            var parts = token.Split('=', 2);
                            optionName = parts[0].Substring(2);
                            optionValue = parts[1];
                        }
                        else
                        {
                            optionName = token.Substring(2);
                            if (i + 1 < tokens.Count && !tokens[i + 1].StartsWith("--"))
                            {
                                optionValue = tokens[i + 1];
                                i++;
                            }
                            else optionValue = "true";
                        }

                        options[optionName] = optionValue;
                    }
                    else positional.Add(token);
                }

                int posIndex = 0;
                for (int i = 0; i < parameters.Length; i++)
                {
                    var param = parameters[i];
                    var optionAttr = param.GetCustomAttribute<OptionAttribute>();
                    if (optionAttr != null)
                    {
                        if (options.TryGetValue(optionAttr.Name, out string? value))
                            arg[i] = Convert.ChangeType(value, param.ParameterType);
                        else if (optionAttr.IsRequired)
                            throw new ArgumentException($"Missing required option: --{optionAttr.Name}");
                        else
                            arg[i] = (param.HasDefaultValue ? param.DefaultValue : GetDefault(param.ParameterType)) ?? String.Empty;
                    }
                    else
                    {
                        if (posIndex < positional.Count)
                        {
                            arg[i] = Convert.ChangeType(positional[posIndex], param.ParameterType);
                            posIndex++;
                        }
                        else if (param.HasDefaultValue) arg[i] = param.DefaultValue ?? String.Empty;
                        else throw new ArgumentException($"Missing required parameter: {param.Name}");
                    }
                }

                method.Invoke(null, arg);
                Console.WriteLine();
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error: " + ex.Message);
            }
        }
        Console.Write("\u001b[?1049l");
    }
    
    private static void ShowHelp()
    {
        Console.WriteLine("Available commands:");
        foreach (var kv in Commands)
        {
            var attr = kv.Value.GetCustomAttribute<CommandAttribute>();
            Console.WriteLine($"- {attr?.Name}: {attr?.Description}");

            var parameters = kv.Value.GetParameters();
            foreach (var param in parameters)
            {
                var optionAttr = param.GetCustomAttribute<OptionAttribute>();
                if (optionAttr != null)
                {
                    Console.WriteLine($"   --{optionAttr.Name} (option): {optionAttr.Description}" + (optionAttr.IsRequired ? " [Required]" : ""));
                }
                else
                {
                    Console.WriteLine($"   {param.Name} (positional)");
                }
            }
        }
        
        Console.WriteLine();
    }
    
    private static object? GetDefault(Type t) => t.IsValueType ? Activator.CreateInstance(t) : null;
    private static List<string> Tokenize(string input)
    {
        var tokens = new List<string>();
        var currentToken = "";
        bool inQuotes = false;
        foreach (var ch in input)
        {
            if (ch == '"')
            {
                inQuotes = !inQuotes;
                continue;
            }
            if (char.IsWhiteSpace(ch) && !inQuotes)
            {
                if (!string.IsNullOrEmpty(currentToken))
                {
                    tokens.Add(currentToken);
                    currentToken = "";
                }
            }
            else currentToken += ch;
        }
        if (!string.IsNullOrEmpty(currentToken)) tokens.Add(currentToken);
        return tokens;
    }
    
}
