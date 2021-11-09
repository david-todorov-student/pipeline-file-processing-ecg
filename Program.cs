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
            var pathToFiles = "../../input_files";
            var fileNames = PipeFunctions.getFileNames(pathToFiles);
            fileNames = PipeFunctions.sortFileNames(fileNames);

        }
    }
}
