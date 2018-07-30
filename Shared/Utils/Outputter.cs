using System;
using System.Collections.Concurrent;
using System.IO;
using System.Threading;

namespace SyntetiskTestdataGen.Shared.Utils
{
    public class Outputter
    {
        public static string LogFilePath = null;
        private static BlockingCollection<string> m_Queue = new BlockingCollection<string>();

        private static StreamWriter _sw;
        private static FileStream _fs;
        private static bool _wasClosed;

        static Outputter()
        {
            var thread = new Thread(
                () =>
                {
                    while (true)
                    {
                        var msg = m_Queue.Take();
                        Console.WriteLine(msg);

                        if (!string.IsNullOrEmpty(LogFilePath) && !_wasClosed)
                        {
                            InitIfNotDone();
                            _sw.WriteLine(msg);
                        }
                    }
                });

            thread.IsBackground = true;
            thread.Start();
        }

        public static void Close()
        {
            if (_sw != null)
            {
                _sw.Close();
                _fs.Close();
            }
            _wasClosed = true;
        }

        private static void InitIfNotDone()
        {
            if (_sw == null && !string.IsNullOrEmpty(LogFilePath))
            {
                _fs = File.Open(LogFilePath, FileMode.CreateNew);
                _sw = new StreamWriter(_fs);
            }
        }

        public static void WriteLine(string value)
        {
            m_Queue.Add(value);
        }
    }
}
