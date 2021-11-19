using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FunctionPipelinePrototype
{
    class Program
    {
        static async Task Main(string[] args)
        {
            var pathToInputFiles = @"..\..\input_files";
            var pathToOutputFiles = @"..\..\output_files";
            var fileNames = PipeFunctions.GetFileNames(pathToInputFiles);
            Console.WriteLine("File names read.");

            fileNames = PipeFunctions.SortFileNames(fileNames);
            Console.WriteLine("File names sorted.");
            
            var pathToMergedFile = pathToOutputFiles + @"\mergedFile.csv";
            
            Console.WriteLine("Beginning file formatting and merging");
            await PipeFunctions.ReadAndFormatFiles(fileNames, pathToMergedFile);
            
            var pathToSortedFile = pathToOutputFiles + @"\sortedFile.csv";
            var begin = DateTime.Now;
            PipeFunctions.SortFile(pathToMergedFile, pathToSortedFile);
            var end = DateTime.Now;
            var timeDiff = (end - begin).TotalSeconds;
            
            Console.WriteLine($"Sorting file {pathToMergedFile} finished in {timeDiff} seconds. Result is in file {pathToSortedFile}");
            
            Console.WriteLine("Filling in the blanks");
            await PipeFunctions.CheckAndFillBlanksAsync(pathToSortedFile, pathToOutputFiles);
            
            Console.WriteLine("\nFINISHED!");
            Console.ReadLine();
        }
    }
}
