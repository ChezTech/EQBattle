using System;
using System.IO;
using System.Linq;
using System.Threading;

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
                    Console.WriteLine($"Currently on line number {inputLineNumber:N0} of {totalLines:N0}. How many lines to read and write (command[1] = delay (ms), command[2] = repeat count? ('q' to quit)");

                    if (!TryReadLineInfo(out LineArgs lineInfo))
                    {
                        Console.WriteLine($"Done reading. {inputLineNumber:N0} lines read. {outputLineNumber:N0} written.");
                        return;
                    }

                    bool notEof = TryReadWriteLines(sr, sw, lineInfo, out int linesRead, out int linesWritten);
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

        private struct LineArgs
        {
            public int LineAmount;
            public int DelayMs;
            public int ReadCount;
        }

        private bool TryReadLineInfo(out LineArgs lineInfo)
        {
            var attempt = 0;
            lineInfo.LineAmount = 0;
            lineInfo.DelayMs = 0;
            lineInfo.ReadCount = 0;

            do
            {
                var command = Console.ReadLine().Trim();

                if (command == "q")
                    return false;

                var commandArgs = command.Split(' ');

                if (TryParseCommandArgs(commandArgs, out lineInfo))
                    return true;

                attempt++;
                Console.WriteLine($"Invalid command: '{command}'. Please enter 'q' to quit, or a valid number or two or three.");

            } while (attempt < 5);

            Console.Error.WriteLine("Too many invalid attempts. Exiting program");
            return false;
        }

        /// <summary>
        /// Parse command args. If supplied, must be valid. If not supplied will default.
        /// </summary>
        private bool TryParseCommandArgs(string[] commandArgs, out LineArgs lineInfo)
        {
            lineInfo.LineAmount = 0;
            lineInfo.DelayMs = 0;
            lineInfo.ReadCount = 1;

            if (commandArgs.Length >= 3)
            {
                if (!int.TryParse(commandArgs[2], out lineInfo.ReadCount))
                    return false;
                else if (lineInfo.ReadCount <= 0 || lineInfo.ReadCount > 100)
                    lineInfo.ReadCount = 1;
            }
            else
                lineInfo.ReadCount = 1;

            if (commandArgs.Length >= 2)
            {
                if (!int.TryParse(commandArgs[1], out lineInfo.DelayMs))
                    return false;
                else if (lineInfo.DelayMs < 0 || lineInfo.DelayMs > 5000)
                    lineInfo.DelayMs = 0;
            }
            else
                lineInfo.DelayMs = 0;

            if (commandArgs.Length >= 1)
            {
                if (!int.TryParse(commandArgs[0], out lineInfo.LineAmount))
                    return false;
                else if (lineInfo.LineAmount <= 0)
                    lineInfo.LineAmount = 1;

            }
            else
                lineInfo.LineAmount = 1;

            return true;
        }

        private bool TryReadWriteLines(StreamReader sr, StreamWriter sw, LineArgs lineInfo, out int linesRead, out int linesWritten)
        {
            string line = null;
            linesRead = 0;
            linesWritten = 0;
            var totalLinesToRead = lineInfo.LineAmount * lineInfo.ReadCount;

            while (linesRead < totalLinesToRead && (line = sr.ReadLine()) != null)
            {
                if (linesRead > 0 && linesRead % lineInfo.LineAmount == 0)
                    Thread.Sleep(lineInfo.DelayMs);

                linesRead++;

                sw.WriteLine(line);
                linesWritten++;
            }

            sw.Flush();
            return (line != null);
        }
    }
}
