using System;
using System.Diagnostics;
using System.IO;
using Newtonsoft.Json.Linq;

class Program
{
    static void Main()
    {
        Console.Title = "CurForge. Made by DarkV.";

        string propertiesFilePath = @"C:\Users\" + Environment.UserName + @"\AppData\Roaming\.tlauncher\tlauncher-2.0.properties";
        string basePath = @"C:\Users\" + Environment.UserName + @"\curseforge\minecraft\Instances";

        string[] folders = Directory.GetDirectories(basePath);
        Console.WriteLine("Folders in the path:");
        for (int i = 0; i < folders.Length; i++)
        {
            Console.WriteLine($"{i + 1}. {Path.GetFileName(folders[i])}");
        }

        Console.WriteLine("Enter number to folder you want to select:");
        int selection = int.Parse(Console.ReadLine()) - 1;
        string selectedPath = folders[selection];

        string manifestPath = Path.Combine(selectedPath, "manifest.json");
        string manifestJson = File.ReadAllText(manifestPath);
        JObject manifest = JObject.Parse(manifestJson);

        string forgePath = manifest["minecraft"]["modLoaders"][0]["id"].ToString();
        string minecraftPath = manifest["minecraft"]["version"].ToString();

        Console.Clear();
        Console.WriteLine("Forge Path: " + forgePath);
        Console.WriteLine("Minecraft Path: " + minecraftPath);

        string versionsPath = Path.Combine(selectedPath, "versions");
        string installVersionsPath = @"C:\Users\" + Environment.UserName + @"\curseforge\minecraft\Install\versions";

        MoveFolder(Path.Combine(installVersionsPath, forgePath), Path.Combine(versionsPath, forgePath));
        MoveFolder(Path.Combine(installVersionsPath, minecraftPath), Path.Combine(versionsPath, minecraftPath));

        string librariesPath = Path.Combine(selectedPath, "libraries");
        string installLibrariesPath = @"C:\Users\" + Environment.UserName + @"\curseforge\minecraft\Install\libraries";

        MoveFolder(installLibrariesPath, librariesPath);

        Console.WriteLine("\nProcess completed.");

        SetPropertyValue(propertiesFilePath, "minecraft.gamedir", EncodePath(selectedPath));

        if (GetPropertyValue(propertiesFilePath, "minecraft.gamedir") == EncodePath(selectedPath)) {
            Console.WriteLine("Select a version like that: " + forgePath);
            SetPropertyValue(propertiesFilePath, "login.version.game", forgePath);

            Thread.Sleep(2000);
            Process.Start(@"C:\Users\" + Environment.UserName + @"\AppData\Roaming\.minecraft\TLauncher.exe");
        }
        else
        {
            Console.WriteLine("Auto-Tlaucnher function error. Set path manually - \"" + selectedPath + "\"");
        }

        Console.ReadLine();
    }

    static void MoveFolder(string sourcePath, string destinationPath)
    {
        Directory.CreateDirectory(destinationPath);

        string[] files = Directory.GetFiles(sourcePath, "*", SearchOption.AllDirectories);
        int totalFiles = files.Length;
        int filesMoved = 0;

        Console.WriteLine($"Moving files from '{sourcePath}' to '{destinationPath}':");

        foreach (string file in files)
        {
            string fileName = Path.GetFileName(file);
            string relativePath = file.Replace(sourcePath, string.Empty).TrimStart(Path.DirectorySeparatorChar);
            string destinationFilePath = Path.Combine(destinationPath, relativePath);

            Directory.CreateDirectory(Path.GetDirectoryName(destinationFilePath));
            File.Move(file, destinationFilePath);

            filesMoved++;
            double progressPercentage = (double)filesMoved / totalFiles * 100;

            Console.SetCursorPosition(0, Console.CursorTop);
            Console.Write($"Progress: {progressPercentage:F2}%");
        }

        Directory.Delete(sourcePath, recursive: true);
        Console.WriteLine();
    }
    static string EncodePath(string path)
    {
        return path.Replace("\\", "\\\\");
    }
    static string GetPropertyValue(string filePath, string key)
    {
        string value = null;
        string[] lines = File.ReadAllLines(filePath);

        foreach (string line in lines)
        {
            if (!string.IsNullOrWhiteSpace(line) && !line.StartsWith("#"))
            {
                string[] parts = line.Split('=');
                if (parts.Length >= 2 && parts[0].Trim() == key)
                {
                    value = parts[1].Trim();
                    break;
                }
            }
        }

        return value;
    }

    static void SetPropertyValue(string filePath, string key, string value)
    {
        List<string> updatedLines = new List<string>();
        string[] lines = File.ReadAllLines(filePath);

        foreach (string line in lines)
        {
            if (!string.IsNullOrWhiteSpace(line) && !line.StartsWith("#"))
            {
                string[] parts = line.Split('=');
                if (parts.Length >= 2 && parts[0].Trim() == key)
                {
                    updatedLines.Add(key + "=" + value);
                }
                else
                {
                    updatedLines.Add(line);
                }
            }
            else
            {
                updatedLines.Add(line);
            }
        }

        File.WriteAllLines(filePath, updatedLines);
    }
}
