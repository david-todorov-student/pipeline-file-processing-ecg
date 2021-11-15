using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FunctionPipelinePrototype
{
    public class PipeFunctions
    {
        public static List<string> getFileNames(string pathToFiles)
        {
            if (Directory.Exists(pathToFiles))
            {
                return Directory.EnumerateFiles(pathToFiles).ToList();
            }
            else
            {
                throw new DirectoryNotFoundException();
            }
        }

        public static List<string> sortFileNames(IEnumerable<string> fileNames)
        {
            return fileNames.OrderBy(fileName => fileName).ToList();
        }

        public static void readAndFormatFiles(IEnumerable<string> fileNames, string pathToSrcFiles, string pathToDestFile)
        {
            foreach (var fileName in fileNames)
            {
                if (File.Exists(fileName))
                {
                    List<string> unformattedLines = File.ReadLines(fileName).ToList();
                    var lines = formatLines(unformattedLines);
                    OverwriteFile(lines, fileName);
                    AppendToFile(fileName, pathToDestFile);

                    Console.WriteLine($"File {fileName} processed and appended to {pathToDestFile}.");
                }
                else
                {
                    throw new FileNotFoundException();
                }
            }
        }

        static string[] formatLines(List<string> unformattedLines)
        {
            var formattedLines = new List<string>();
            var sb = new StringBuilder();
            unformattedLines.ForEach(line =>
            {
                var nums = line.Split(',');
                for (int i = 0; i < nums.Length; i++)
                {
                    if (i % 2 == 0)
                    {
                        sb.Append(nums[i]);
                        sb.Append(",");
                    }
                    else
                    {
                        var y = calculateY(int.Parse(nums[i]));
                        sb.Append(y);
                        formattedLines.Add(sb.ToString());
                        sb.Clear();
                    }
                }
                sb.Clear();
            });

            return formattedLines.ToArray();
        }

        static void OverwriteFile(string[] lines, string pathToFile)
        {
            File.WriteAllLines(pathToFile, lines);
        }

        public static void AppendToFile(string srcFile, string destFile)
        {
            string[] lines = File.ReadLines(srcFile).ToArray();
            File.AppendAllLines(destFile, lines);
        }

        public static void SortFile(string fileToSort, string outputFileName)
        {
            Process process = new Process();
            process.StartInfo.FileName = "cmd.exe";
            process.StartInfo.CreateNoWindow = true;
            process.StartInfo.RedirectStandardInput = true;
            process.StartInfo.RedirectStandardOutput = true;
            process.StartInfo.RedirectStandardError = true;
            process.StartInfo.UseShellExecute = false;
            process.Start();
            process.StandardInput.WriteLine("sort {0} /o {1}", fileToSort, outputFileName);
            process.StandardInput.Flush();
            process.StandardInput.Close();
            process.WaitForExit();
            Console.WriteLine(process.StandardOutput.ReadToEnd());
            Console.WriteLine(process.StandardError.ReadToEnd());
        }

        static List<string> removeTimestamps(string srcFilePath)
        {
            var lines = File.ReadLines(srcFilePath).ToList();
            var newLines = new List<String>();
            lines.ForEach(line =>
            {
                var words = line.Split(',');
                newLines.Add(words[1]);
            });
            return newLines;
        }
        public static string convertToECG(string srcFilePath)
        {
            var newLines = removeTimestamps(srcFilePath);
            var resultFileName = srcFilePath.Replace("csv", "ecg");
            File.WriteAllLines(resultFileName, newLines.ToArray());
            return resultFileName;
        }

        static int calculateY(int x)
        {
            var y = (x / 6) + 511;
            if (y > 1023)
            {
                y = 1023;
            }
            else if (y < 0)
            {
                y = 0;
            }

            return y;
        }
    }
}
