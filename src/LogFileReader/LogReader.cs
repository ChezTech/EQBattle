using System;
using System.IO;
using System.Threading;

namespace LogFileReader
{
    public class LogReader : IDisposable
    {
        // Thanks: https://stackoverflow.com/a/24993767

        FileSystemWatcher _fsw = new FileSystemWatcher();
        AutoResetEvent _readEvent = new AutoResetEvent(false);
        bool _cancelRequested = false;

        public FileInfo LogFile { get; set; }

        public LogReader(string logFilePathName)
        {
            LogFile = new FileInfo(logFilePathName);

            if (!LogFile.Exists)
                throw new FileNotFoundException("File does not exist", logFilePathName);

            _fsw.Path = LogFile.DirectoryName;
            _fsw.Filter = LogFile.Name;
            _fsw.Changed += _fsw_Changed;
        }

        private void _fsw_Changed(object sender, FileSystemEventArgs e)
        {
            // We're only watching one file, event args will always be the same, this is pretty much telling us the file changed and we need to keep reading
            _readEvent.Set();
        }

        public event EventHandler<LineReadArgs> LineRead;
        public event EventHandler EoFReached;

        public void StartReading()
        {
            _cancelRequested = false;
            StartWatching();
            OpenFileToRead();
        }

        public void StopReading()
        {
            _cancelRequested = true;
            StopWatching();
        }

        private void OpenFileToRead()
        {
            // First, read the whole file till the end (maybe later, we can just jump backwards till we find a good starting point, e.g. 2h ago)
            // Once we hit the end, start the FSW to monitor

            using (var fs = LogFile.Open(FileMode.Open, FileAccess.Read, FileShare.ReadWrite | FileShare.Delete))
            using (var sr = new StreamReader(fs))
            {
                string logLine;
                while (!_cancelRequested)
                {
                    logLine = sr.ReadLine();

                    if (logLine != null)
                        RaiseReadLine(logLine);
                    else
                    {
                        RaiseEof();
                        _readEvent.WaitOne(1000);
                    }
                }
            }
        }

        private void StartWatching()
        {
            _fsw.EnableRaisingEvents = true;
        }

        private void StopWatching()
        {
            _fsw.EnableRaisingEvents = false;
        }

        private void RaiseReadLine(string logLine)
        {
            LineRead?.Invoke(this, new LineReadArgs(logLine));
        }

        private void RaiseEof()
        {
            EoFReached?.Invoke(this, new EventArgs());
        }

        #region IDisposable Support
        private bool disposedValue = false; // To detect redundant calls

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    // TODO: dispose managed state (managed objects).
                    _fsw.Dispose();
                }

                // TODO: free unmanaged resources (unmanaged objects) and override a finalizer below.
                // TODO: set large fields to null.

                disposedValue = true;
            }
        }

        // TODO: override a finalizer only if Dispose(bool disposing) above has code to free unmanaged resources.
        // ~LogReader()
        // {
        //   // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
        //   Dispose(false);
        // }

        // This code added to correctly implement the disposable pattern.
        public void Dispose()
        {
            // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
            Dispose(true);
            // TODO: uncomment the following line if the finalizer is overridden above.
            // GC.SuppressFinalize(this);
        }
        #endregion

    }
}
