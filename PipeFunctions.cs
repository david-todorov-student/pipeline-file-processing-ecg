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
        public static IEnumerable<string> getFileNames(string pathToFiles)
        {
            if (Directory.Exists(pathToFiles))
            {
                return Directory.EnumerateFiles(pathToFiles);
            }
            else
            {
                throw new DirectoryNotFoundException();
            }
        }

        public static IEnumerable<string> sortFileNames(IEnumerable<string> fileNames)
        {
            return fileNames.OrderBy(fileName => fileName);
        }

        public static void readFiles(IEnumerable<string> fileNames, string pathToDirectory)
        {
            foreach (var fileName in fileNames)
            {
                var fullPath = pathToDirectory + "/" +fileName;
                if (File.Exists(fullPath))
                {
                    File.ReadLines(fullPath);
                }
                else
                {
                    throw new FileNotFoundException();
                }
            }
        }
    }
}
