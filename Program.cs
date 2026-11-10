using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace CsvToPhpConverter
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Enter file name (without .csv): ");
            string fileName = Console.ReadLine();

            string inputFile = fileName + ".csv";

            string baseDirectory = AppDomain.CurrentDomain.BaseDirectory;

            string[] foundFiles = Directory.GetFiles(baseDirectory, inputFile, SearchOption.AllDirectories);

            string inputPath = "";
            string outputPath = "";

            if (foundFiles.Length > 0)
            {
                Console.WriteLine($"File {fileName} has found.");
                inputPath = foundFiles[0];
                outputPath = fileName + ".php";
            }
            else
            {
                Console.WriteLine($"File {fileName} has not found. Press any key to close the console.");
                Console.ReadKey();
                return;
            }

            Console.WriteLine("Enter array name (this should be conforms to snakeCase or CamelCase syntax)");
            string arrayName = Console.ReadLine();


            // 1. Reading and parsing CSV
            var data = ParseCsv(inputPath);

            // 2. Generating PHP code
            string phpCode = GeneratePhpArray(data, arrayName);

            // 3. Saving to a file
            File.WriteAllText(outputPath, phpCode, Encoding.UTF8);

            Console.WriteLine($"Sucsess! File saved as {outputPath}. Press any key to close the console.");
            Console.ReadKey();
        }

        // --- STAGE 1: PARSING ---
        static List<Dictionary<string, object>> ParseCsv(string path)
        {
            var lines = File.ReadAllLines(path);
            var headers = ParseLine(lines[0]); // Titles
            var result = new List<Dictionary<string, object>>();

            for (int i = 1; i < lines.Length; i++)
            {
                if (string.IsNullOrWhiteSpace(lines[i])) continue;

                var values = ParseLine(lines[i]);
                var row = new Dictionary<string, object>();

                for (int j = 0; j < headers.Count; j++)
                {
                    row[headers[j]] = ParseValue(values[j]);
                }
                result.Add(row);
            }
            return result;
        }

        // A simple CSV parser that handles quotes
        static List<string> ParseLine(string line)
        {
            var result = new List<string>();
            bool inQuotes = false;
            StringBuilder current = new StringBuilder();

            foreach (char c in line)
            {
                if (c == '"') inQuotes = !inQuotes;
                else if (c == ';' && !inQuotes)
                {
                    result.Add(current.ToString().Trim());
                    current.Clear();
                }
                else current.Append(c);
            }
            result.Add(current.ToString().Trim());
            return result;
        }

        // Value parser (determines whether it is an array or a scalar)
        static object ParseValue(string value)
        {
            // If there is an array separator
            if (value.Contains('|'))
            {
                var items = value.Split('|');

                // Let's check: is this an associative array (it contains ':') or an indexed one?
                if (items.Any(item => item.Contains(':')))
                {
                    var dict = new Dictionary<string, object>();
                    foreach (var item in items)
                    {
                        var parts = item.Split(':');
                        if (parts.Length == 2)
                        {
                            // Attempt to convert the value to an int; otherwise, leave it as a string.
                            if (int.TryParse(parts[1], out int num))
                            {
                                // Attempt to convert the value to an int; otherwise, leave it as a string.
                                dict[parts[0]] = num;
                            }
                            else
                            {
                                // Attempt to convert the value to an int; otherwise, leave it as a string.
                                dict[parts[0]] = parts[1];
                            }
                        }
                    }
                    return dict;
                }
                else
                {
                    return items.ToList(); // A simple list of strings
                }
            }

            // If it is not an array, attempt to convert it into a number.
            if (int.TryParse(value, out int intVal)) return intVal;
            return value;
        }

        // --- STAGE 2: PHP GENERATION ---
        static string GeneratePhpArray(List<Dictionary<string, object>> data, string varName)
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("<?php\n");
            sb.AppendLine($"${varName} = [");

            foreach (var row in data)
            {
                sb.AppendLine("    [");
                foreach (var kvp in row)
                {
                    string key = FormatPhpString(kvp.Key);
                    string val = FormatPhpValue(kvp.Value, 2); // 2 levels of indentation
                    sb.AppendLine($"        {key} => {val},");
                }
                sb.AppendLine("    ],");
            }

            sb.AppendLine("];");
            sb.AppendLine($"\nprint_r(${varName});");
            sb.AppendLine("\n?>");
            return sb.ToString();
        }

        static string FormatPhpValue(object value, int indentLevel)
        {
            string indent = new string(' ', indentLevel * 4);
            string nextIndent = new string(' ', (indentLevel + 1) * 4);

            if (value is Dictionary<string, object> dict)
            {
                var sb = new StringBuilder();
                sb.AppendLine("[");
                foreach (var kvp in dict)
                {
                    sb.AppendLine($"{nextIndent}{FormatPhpString(kvp.Key)} => {FormatPhpValue(kvp.Value, indentLevel + 1)},");
                }
                sb.Append($"{indent}]");
                return sb.ToString();
            }

            if (value is List<string> list)
            {
                var items = list.Select(i => FormatPhpString(i));
                return "[" + string.Join(", ", items) + "]";
            }

            if (value is int) return value.ToString();

            return FormatPhpString(value.ToString());
        }

        static string FormatPhpString(string str)
        {
            // Escape single quotes and wrap in them
            return "'" + str.Replace("'", "\\'") + "'";
        }
    }
}