using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FunctionPipelinePrototype
{
    class Program
    {
        static void Main(string[] args)
        {
            var pathToFiles = @"..\..\input_files";
            var fileNames = PipeFunctions.GetFileNames(pathToFiles);
            Console.WriteLine("File names read.");

            fileNames = PipeFunctions.SortFileNames(fileNames);
            Console.WriteLine("File names sorted.");
            
            var pathToMergedFile = pathToFiles + @"\mergedFile.csv";
            
            Console.WriteLine("Beginning file formatting and merging");
            PipeFunctions.ReadAndFormatFiles(fileNames, pathToFiles, pathToMergedFile);

            var pathToSortedFile = pathToFiles + @"\sortedFile.csv";
            var begin = DateTime.Now;
            PipeFunctions.SortFile(pathToMergedFile, pathToSortedFile);
            var end = DateTime.Now;
            var timeDiff = (end - begin).TotalSeconds;

            Console.WriteLine($"Sorting file {pathToMergedFile} finished in {timeDiff} seconds. Result is in file {pathToSortedFile}");

            Console.WriteLine("Filling in the blanks");
            PipeFunctions.CheckAndFillBlanks(pathToSortedFile, pathToSortedFile);

            PipeFunctions.ConvertToECG(pathToSortedFile);
            Console.WriteLine($"Converted file {pathToSortedFile} to ECG.");

            Console.ReadLine();
        }
    }
}
