using asknvl.logger;
using Avalonia.Threading;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;

namespace cheatbot.ViewModels
{
    public class loggerVM : ViewModelBase, ILogger
    {
        #region const
        string logFolder = "logs";
        #endregion

        #region vars
        Queue<LogMessage> logMessages = new Queue<LogMessage>();
        System.Timers.Timer timer = new System.Timers.Timer();
        string filePath;
        #endregion

        #region properties
        bool disableFileOutput;
        public bool DisableFileOutput
        {
            get => disableFileOutput;
            set
            {
                if (value)
                    timer.Stop();
                else
                    timer.Start();

                disableFileOutput = value;
            }
        }
        public ObservableCollection<LogMessage> Messages { get; set; } = new();        
        #endregion

        public loggerVM()
        {
            var fileDirectory = Path.Combine(Directory.GetCurrentDirectory(), logFolder);
            if (!Directory.Exists(fileDirectory))
                Directory.CreateDirectory(fileDirectory);

            filePath = Path.Combine(fileDirectory, $"cheat.log");

            if (File.Exists(filePath))
                File.Delete(filePath);

            timer.Interval = 1000;
            timer.AutoReset = true;
            timer.Elapsed += Timer_Elapsed;
            timer.Start();

        }

        #region private
        private void Timer_Elapsed(object? sender, System.Timers.ElapsedEventArgs e)
        {
            appendLogFile();
        }

        void appendLogFile()
        {
            try
            {

                using (StreamWriter sw = File.AppendText(filePath))
                {
                    while (logMessages.Count > 0)
                    {
                        LogMessage message = logMessages.Dequeue();
                        if (message != null)
                            sw.WriteLine(message.ToString());
                    }
                }

            }
            catch (Exception ex)
            {

            }
        }
        #endregion

        #region helpers
        void post(LogMessage message)
        {
            Dispatcher.UIThread.InvokeAsync(() =>
            {
                Messages.Add(message);
            });

            if (!DisableFileOutput)
                logMessages.Enqueue(message);
        }
        #endregion

        #region public
        public void dbg(string tag, string text)
        {
            var message = new LogMessage(LogMessageType.dbg, tag, text);
            post(message);
        }

        public void err(string tag, string text)
        {
            var message = new LogMessage(LogMessageType.err, tag, text);
            post(message);
        }

        public void inf(string tag, string text)
        {
            var message = new LogMessage(LogMessageType.inf, tag, text);
            post(message);
        }

        public void inf_urgent(string tag, string text)
        {
            var message = new LogMessage(LogMessageType.inf, tag, text);
            post(message);
        }
        #endregion
    }
}
