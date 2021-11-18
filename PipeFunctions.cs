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
        public static List<string> GetFileNames(string pathToFiles)
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

        public static List<string> SortFileNames(IEnumerable<string> fileNames)
        {
            return fileNames.OrderBy(fileName => fileName).ToList();
        }

        static string GetFormattedFileOutputName(string inputFileName)
        {
            return inputFileName.Replace("input", "output");
        }

        public static void ReadAndFormatFiles(IEnumerable<string> fileNames, string pathToMergedFile)
        {
            foreach (var fileName in fileNames)
            {
                if (File.Exists(fileName))
                {
                    List<string> unformattedLines = File.ReadLines(fileName).ToList();
                    var formattedLines = FormatLines(unformattedLines);
                    // WriteToFile(formattedLines, fileName);

                    AppendToFile(formattedLines, pathToMergedFile);

                    Console.WriteLine($"File {fileName} processed and appended to {pathToMergedFile}.");
                }
                else
                {
                    throw new FileNotFoundException();
                }
            }
        }

        static string[] FormatLines(List<string> unformattedLines)
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
                        var y = CalculateY(int.Parse(nums[i]));
                        sb.Append(y);
                        formattedLines.Add(sb.ToString());
                        sb.Clear();
                    }
                }

                sb.Clear();
            });

            return formattedLines.ToArray();
        }

        static void WriteToFile(string[] lines, string pathToFile)
        {
            File.WriteAllLines(pathToFile, lines);
        }

        static void AppendToFile(string[] lines, string destFile)
        {
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

        static KeyValuePair<long, int> GetTimestampECGPair(string line)
        {
            var values = line.Split(',');
            return new KeyValuePair<long, int>(long.Parse(values[0]), int.Parse(values[1]));
        }

        public static void CheckAndFillBlanks(string srcFilePath, string destFilePath)
        {
            var lines = File.ReadLines(srcFilePath);
            using (var linesEnumerator = lines.GetEnumerator())
            {
                string currentLine = null;
                string previousLine = null;

                linesEnumerator.MoveNext();
                previousLine = linesEnumerator.Current;

                var linesToAppend = new List<string>();
                linesToAppend.Add(previousLine);

                while (linesEnumerator.MoveNext())
                {
                    currentLine = linesEnumerator.Current;

                    if (AreThereMissingTimestamps(previousLine, currentLine))
                    {
                        if (CalculateDifference(previousLine, currentLine) >= 30000)
                        {
                            var destFileName = GetFileName(linesToAppend[0], destFilePath);
                            File.WriteAllLines(destFileName, linesToAppend.ToArray());
                            ConvertToECG(destFileName);
                            Console.WriteLine($"Converted file {destFileName} to ECG.");
                            linesToAppend.Clear();
                        }
                        else
                        {
                            linesToAppend = GetMissingTimestamps(previousLine, currentLine, linesToAppend);
                        }
                    }

                    previousLine = currentLine;
                    linesToAppend.Add(previousLine);
                }

                var finalDestFileName = GetFileName(linesToAppend[0], destFilePath);
                File.WriteAllLines(GetFileName(linesToAppend[0], destFilePath), linesToAppend.ToArray());
                ConvertToECG(finalDestFileName);
                Console.WriteLine($"Converted file {finalDestFileName} to ECG.");
            }
        }

        static string GetFileName(string line, string destPath)
        {
            var timestamp = GetTimestampECGPair(line).Key;
            var fileName = destPath + @"\ecg_" + timestamp + ".csv";
            return fileName;
        }

        static long CalculateDifference(string previousLine, string currentLine)
        {
            var previousTimeStamp = GetTimestampECGPair(previousLine).Key;
            var currentTimeStamp = GetTimestampECGPair(currentLine).Key;

            var diff = currentTimeStamp - previousTimeStamp;
            return diff;
        }

        static int HowManyMissingTimestamps(string previousLine, string currentLine)
        {
            var diff = CalculateDifference(previousLine, currentLine);
            if (diff % 8 != 0)
            {
                throw new Exception("The difference between timestamps is not dividable by 8.");
            }

            return (int) (diff / 8) - 1;
        }

        static bool AreThereMissingTimestamps(string previousLine, string currentLine)
        {
            var howManyMissing = HowManyMissingTimestamps(previousLine, currentLine);
            return (howManyMissing > 0);
        }

        static List<string> GetMissingTimestamps(string previousLine, string currentLine, List<string> lines)
        {
            var previousTimeStamp = GetTimestampECGPair(previousLine).Key;
            var currentTimeStamp = GetTimestampECGPair(currentLine).Key;

            for (long toAdd = previousTimeStamp + 8; toAdd < currentTimeStamp; toAdd += 8)
            {
                lines.Add(toAdd + ",-1");
            }

            return lines;
        }

        static List<string> RemoveTimestamps(string srcFilePath)
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

        public static string ConvertToECG(string srcFilePath)
        {
            var newLines = RemoveTimestamps(srcFilePath);
            var resultFileName = srcFilePath.Replace("csv", "ecg");
            File.WriteAllLines(resultFileName, newLines.ToArray());
            return resultFileName;
        }

        static int CalculateY(int x)
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