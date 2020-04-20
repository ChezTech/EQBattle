using System;
using System.IO;
using System.Linq;

namespace EQReadWrite
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine(args.Length);
            if (args.Length != 2)
            {
                Console.Error.WriteLine("Must supply <inputFile> and <outputFile>");
                return;
            }

            var inputFile = args[0];
            var outputFile = args[1];

            new Program().ReadFileAndWriteToOutput(inputFile, outputFile);
        }

        public void ReadFileAndWriteToOutput(string inputFile, string outputFile)
        {
            if (!VerifyArgs(inputFile, outputFile))
                return;

            var totalLines = File.ReadLines(inputFile).Count();

            var logFile = new FileInfo(inputFile);
            var outFile = new FileInfo(outputFile);

            using (var inFs = logFile.Open(FileMode.Open, FileAccess.Read, FileShare.ReadWrite | FileShare.Delete))
            using (var sr = new StreamReader(inFs))
            using (var outFs = outFile.Open(FileMode.Create, FileAccess.Write, FileShare.Read))
            using (var sw = new StreamWriter(outFs))
            {
                Console.WriteLine($"Reading: {logFile.FullName}\nWriting: {outFile.FullName}\nEnter 'q' to quit. Enter a number (e.g. 137) to read that many lines and write to the output file.");

                var inputLineNumber = 0;
                var outputLineNumber = 0;

                while (true)
                {
                    Console.WriteLine($"Currently on line number {inputLineNumber:N0} of {totalLines:N0}. How many lines to read and write? ('q' to quit)");

                    if (!TryReadLineAmount(sr, out int lineAmount))
                    {
                        Console.WriteLine($"Done reading. {inputLineNumber:N0} lines read. {outputLineNumber:N0} written.");
                        return;
                    }

                    bool notEof = TryReadWriteLines(sr, sw, lineAmount, out int linesRead, out int linesWritten);
                    inputLineNumber += linesRead;
                    outputLineNumber += linesWritten;

                    if (!notEof)
                    {
                        Console.WriteLine($"End of file reached. {inputLineNumber:N0} lines read. {outputLineNumber:N0} written.");
                        return;
                    }
                }
            }
        }

        private bool VerifyArgs(string inputFile, string outputFile)
        {
            if (!File.Exists(inputFile))
            {
                Console.Error.WriteLine($"Input file not found: {inputFile}. Please make sure it exists and can be read from.");
                return false;
            }

            if (File.Exists(outputFile))
                Console.WriteLine($"Warning: will overwrite output file: {outputFile}");

            return true;
        }

        private bool TryReadLineAmount(StreamReader sr, out int lineAmount)
        {
            var attempt = 0;
            lineAmount = 0;

            while (attempt < 5)
            {
                var command = Console.ReadLine();

                if (command == "q")
                    return false;

                if (int.TryParse(command, out lineAmount))
                    return true;

                attempt++;
                Console.WriteLine($"Invalid command: {command}. Please enter 'q' to quit, or a valid number.");
            }
            Console.Error.WriteLine("Too many invalid attempts. Exiting program");
            return false;
        }

        private bool TryReadWriteLines(StreamReader sr, StreamWriter sw, int lineAmount, out int linesRead, out int linesWritten)
        {
            string line = null;
            linesRead = 0;
            linesWritten = 0;
            while (linesRead < lineAmount && (line = sr.ReadLine()) != null)
            {
                linesRead++;

                sw.WriteLine(line);
                linesWritten++;
            }

            sw.Flush();
            return (line != null);
        }
    }
}
