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


        public static async Task ReadAndFormatFiles(IEnumerable<string> fileNames, string pathToMergedFile)
        {
            using (var streamWriter = new StreamWriter(pathToMergedFile, true))
            {
                foreach (var fileName in fileNames)
                {
                    if (File.Exists(fileName))
                    {
                        Console.WriteLine($"Processing {fileName} and appending to {pathToMergedFile}.");
                        await FormatLinesAndAppendToMergedFile(fileName, streamWriter);
                    }
                    else
                    {
                        throw new FileNotFoundException();
                    }
                }
            }
        }

        static async Task FormatLinesAndAppendToMergedFile(string inputFileName, StreamWriter mergedFileStreamWriter)
        {
            using (var streamReader = new StreamReader(inputFileName))
            {
                string line;
                var sb = new StringBuilder();
                while ((line = await streamReader.ReadLineAsync()) != null)
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
                            await mergedFileStreamWriter.WriteLineAsync(sb.ToString());
                            sb.Clear();
                        }
                    }

                    sb.Clear();
                }
            }
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

        public static async Task CheckAndFillBlanksAsync(string srcFilePath, string destDirPath)
        {
            using (StreamReader streamReader = new StreamReader(srcFilePath))
            {
                string currentLine = null;
                string previousLine = null;

                previousLine = await streamReader.ReadLineAsync();
                var destFileName = GetFileName(previousLine, destDirPath);

                var streamWriter = new StreamWriter(destFileName, true);

                while ((currentLine = await streamReader.ReadLineAsync()) != null)
                {
                    if (AreThereMissingTimestamps(previousLine, currentLine))
                    {
                        if (CalculateDifference(previousLine, currentLine) >= 30000)
                        {
                            await streamWriter.FlushAsync();
                            streamWriter.Dispose(); 
                            await ConvertToEcg(destFileName);
                            destFileName = GetFileName(currentLine, destDirPath);
                            streamWriter = new StreamWriter(destFileName, true);
                        }
                        else
                        {
                            await GetMissingTimestamps(previousLine, currentLine, streamWriter);
                        }
                    }

                    previousLine = currentLine;
                    await streamWriter.WriteLineAsync(previousLine);
                }

                await streamWriter.FlushAsync();
                streamWriter.Dispose();
                await ConvertToEcg(destFileName);
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

        static async Task GetMissingTimestamps(string previousLine, string currentLine, StreamWriter streamWriter)
        {
            var previousTimeStamp = GetTimestampECGPair(previousLine).Key;
            var currentTimeStamp = GetTimestampECGPair(currentLine).Key;

            for (long toAdd = previousTimeStamp + 8; toAdd < currentTimeStamp; toAdd += 8)
            {
                await streamWriter.WriteLineAsync(toAdd + ",-1");
            }
        }

        static string RemoveTimestamp(string line)
        {
            var words = line.Split(',');
            return words[1];
        }

        public static async Task<string> ConvertToEcg(string srcFilePath)
        {
            var resultFileName = srcFilePath.Replace("csv", "ecg");

            using (var streamReader = new StreamReader(srcFilePath))
            using (var streamWriter = new StreamWriter(resultFileName, true))
            {
                string line;
                while ((line = await streamReader.ReadLineAsync()) != null)
                {
                    await streamWriter.WriteLineAsync(RemoveTimestamp(line));
                }
            }

            Console.WriteLine($"Converted file {srcFilePath} to ECG.");
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