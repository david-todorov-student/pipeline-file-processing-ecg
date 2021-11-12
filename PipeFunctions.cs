using System;
using System.Collections.Generic;
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
