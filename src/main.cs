using System.Net;
using System.Net.Sockets;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text.RegularExpressions;
// You can use print statements as follows for debugging, they'll be visible when running tests.
// Console.WriteLine("Logs from your program will appear here!");

// Uncomment this line to pass the first stage

bool run = true;
int returncode = 0;
List<string> builtinCommands = ["exit", "echo", "type", "pwd", "cd"];
string PATH = Environment.GetEnvironmentVariable("PATH") ?? "";
string HOME = Environment.GetEnvironmentVariable("HOME") ?? "";
char delimiter = ':';
if (Environment.OSVersion.Platform == PlatformID.Win32NT)
{
    delimiter = ';';
}
var paths = PATH.Split(delimiter);

while (run)
{
    Console.Write("$ ");
    // Wait for user input
    var userInput = Console.ReadLine();
    if (userInput != null)
    {
        var parts = userInput.Split(' ');
        var command = parts[0];
        var commandType = GetCommandType(command);
        if (commandType == CommandType.Builtin)
        {
            run = BuiltInCommandRunner(parts);
        }
        else if (commandType == CommandType.External)
        {
            run = ExternalCommandRunner(parts, new string[]{});
        }
        else
        {
            Console.WriteLine($"{command}: command not found");
        }
    }
}


return returncode;

bool BuiltInCommandRunner(string[]? command)
{
    bool run = true;
    switch (command[0])
    {
        case "exit":
            returncode = int.Parse(command[1]);
            run = false;
            break;
        case "type":
            var commandType = GetCommandType(command[1]);
            if (commandType == CommandType.Builtin)
            {
                Console.WriteLine($"{command[1]} is a shell builtin");
            }
            else if (commandType == CommandType.External)
            {
                Console.WriteLine($"{command[1]} is {GetExecutable(command[1])}");
            }
            else
            {
                Console.WriteLine($"{command[1]}: not found");
            }
            break;
        case "echo":
            var text = string.Join(" ", command.Skip(1));
            Console.WriteLine(text);
            break;
        case "pwd":
            string currentDirectory = System.IO.Directory.GetCurrentDirectory();
            Console.WriteLine(currentDirectory);
            break;
        case "cd":
            var cmd = string.Join(" ", command.Skip(1));
            if(cmd == "~")
            {
                cmd = HOME;
            }
            HandleCd(cmd);
            break;
    }
    return run;
}

void HandleCd(string commandInput)
{
    if (commandInput == null)
    {
        return;
    }
    if (!Directory.Exists(commandInput))
    {
        Console.WriteLine($"{commandInput}: No such file or directory");
        return;
    }
    Directory.SetCurrentDirectory(commandInput);
}

bool ExternalCommandRunner(string[]? command , string[]? arguments)
{
    var exe = GetExecutable(command[0]);
    using var process = new Process();
    process.StartInfo.FileName = exe;
    process.StartInfo.Arguments = string.Join(" ", command.Skip(1).ToArray());
    process.Start();
    return true;
}

CommandType GetCommandType(string command)
{
    if (builtinCommands.Contains(command))
    {
        return CommandType.Builtin;
    }
    if (GetExecutable(command) != null)
    {
        return CommandType.External;
    }
    return CommandType.notFound;
}

string? GetExecutable(string command)
{
    string? path = null;
    bool found = false;
    int i = 0;
    while (!found && i < paths.Length)
    {
        string testPath = Path.Combine(paths[i], command);
        if (File.Exists(testPath))
        {
            path = testPath;
            found = true;
        }
        i++;
    }
    return path;
}
void ShowNoArgumentsMessage() => Console.WriteLine("No arguments specified");

string[] ParseInput(string input) {
  ArgumentNullException.ThrowIfNull(input);
  var regex = new Regex(@"'([^']*)'|(\S+)");
  var matches = regex.Matches(input);
  return matches
      .Select(match => match.Groups[1].Success ? match.Groups[1].Value
                                               : match.Groups[2].Value)
      .ToArray();
}
enum CommandType { notFound, Builtin, External }

