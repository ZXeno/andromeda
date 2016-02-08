﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;

namespace Andromeda.Infrastructure
{
    public class Logger
    {
        private static string _logFilePath;
        private static string _logFileName;
        private static string _fullLogPath;

        private static Queue<string> _queue = new Queue<string>();
        private static AutoResetEvent _hasNewItems = new AutoResetEvent(false);
        private static volatile bool _waiting = false;
        
        public Logger()
        {
            _logFilePath = Program.UserFolder + "\\logs";
            _logFileName = DateTime.Today.Month + "_" + DateTime.Today.Day + "_" + DateTime.Today.Year + "_log.txt";

            _fullLogPath = _logFilePath + "\\" + _logFileName;

            ValidateLogDirectoryExists();

            if (!File.Exists(_fullLogPath))
            {
                WriteToTextFile.CreateNewLogFile(_fullLogPath);
            }

            Thread loggingThread = new Thread(new ThreadStart(ProcessQueue));
            loggingThread.IsBackground = true;
            loggingThread.Start();

            Log("Logger initiated.");
        }

        public static void Log(string line)
        {
            lock (_queue)
            {
                _queue.Enqueue(DateTime.Now + " " + line);
            }
            _hasNewItems.Set();
        }

        void ProcessQueue()
        {
            while (true)
            {
                _waiting = true;
                _hasNewItems.WaitOne(10000, true);
                _waiting = false;

                Queue<string> queueCopy;
                lock (_queue)
                {
                    queueCopy = new Queue<string>(_queue);
                    _queue.Clear();
                }

                StringBuilder sb = new StringBuilder();

                foreach (var line in queueCopy)
                {
                    sb.AppendLine(line);
                }

                WriteToTextFile.WriteToLogFile(_fullLogPath, sb.ToString());
                queueCopy.Clear();
            }
        }

        public void Flush()
        {
            while (!_waiting)
            {
                Thread.Sleep(1);
            }
        }

        private static void ValidateLogDirectoryExists()
        {
            try
            {
                if (!Directory.Exists(_logFilePath))
                {
                    Directory.CreateDirectory(_logFilePath);
                }
            }
            catch (Exception ex)
            {
                ResultConsole.Instance.AddConsoleLine("There was an exception when validating or creating the folder used for logging.");
                ResultConsole.Instance.AddConsoleLine(ex.Message);
            }
        }
        
    }
}