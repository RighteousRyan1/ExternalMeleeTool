using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace EMTDisplay.CmdConsole;

public class CommandProcessor {
    public struct CommandDesc {
        public Action<string[]> Action;
        public string Desc;
        public string Usage;
    }
    // command map
    readonly Dictionary<string, CommandDesc> _commands = [];

    public void Register(string name, string description, Action<string[]> action, string usage = null) {
        // tolower...?
        usage ??= string.Empty;
        _commands[name] = new() {
            Action = action,
            Desc = description,
            Usage = usage
        };
    }

    public IEnumerable<(string Name, CommandDesc Cmd)> GetRegisteredCommands() {
        return _commands.Select(kvp => (kvp.Key, kvp.Value));
    }

    public void Process(string input) {
        if (string.IsNullOrWhiteSpace(input)) return;

        // split by spaces, preserve text between quotes
        var matches = Regex.Matches(input, @"(?<match>""[^""]*""|\S+)");
        var tokens = matches.Cast<Match>().Select(m => m.Value.Replace("\"", "")).ToArray();

        string cmdName = tokens[0].ToLower();
        string[] args = [.. tokens.Skip(1)];

        if (_commands.TryGetValue(cmdName, out var command)) {
            try {
                Console.ForegroundColor = ConsoleColor.Yellow;
                command.Action(args);
                Console.ForegroundColor = ConsoleColor.White;
            } catch (Exception ex) {
                ShowErrorAt(input, 0, $"Execution error: {ex.Message}");
            }
        }
        else {
            HandleUnknownCommand(input, cmdName);
        }
    }

    private void HandleUnknownCommand(string input, string attempt) {
        ShowErrorAt(input, 0, "Unknown command");

        // "did you mean
        var suggestion = _commands.Keys
            .Select(k => new { Name = k, Dist = GetDistance(attempt, k) })
            .Where(x => x.Dist <= 3) // threshold for similarity
            .OrderBy(x => x.Dist)
            .FirstOrDefault();

        if (suggestion != null) {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine($"Did you mean: {suggestion.Name}?");
            Console.ResetColor();
        }
    }

    public static void ShowErrorAt(string input, int position, string message) {
        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine(input);
        Console.Write(new string(' ', position) + "^");
        Console.WriteLine($" Error: {message}");
        Console.ResetColor();
    }

    // levenschtein
    static int GetDistance(string s, string t) {
        int n = s.Length, m = t.Length;
        int[,] d = new int[n + 1, m + 1];
        for (int i = 0; i <= n; d[i, 0] = i++) ;
        for (int j = 0; j <= m; d[0, j] = j++) ;
        for (int i = 1; i <= n; i++) {
            for (int j = 1; j <= m; j++) {
                int cost = (t[j - 1] == s[i - 1]) ? 0 : 1;
                d[i, j] = Math.Min(Math.Min(d[i - 1, j] + 1, d[i, j - 1] + 1), d[i - 1, j - 1] + cost);
            }
        }
        return d[n, m];
    }
}