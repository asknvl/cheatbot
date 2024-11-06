using cheatbot.ViewModels;
using ReactiveUI;
using System;

namespace asknvl.logger
{
    public enum LogMessageType
    {
        dbg,
        err,
        inf,
        inf_urgent
    }
    public class LogMessage : ViewModelBase
    {
        LogMessageType type;
        public LogMessageType Type
        {
            get => type;
            set => this.RaiseAndSetIfChanged(ref type, value);
        }

        string tag;
        public string TAG
        {
            get => tag;
            set => this.RaiseAndSetIfChanged(ref tag, value);
        }

        string text;
        public string Text
        {
            get => text;
            set => this.RaiseAndSetIfChanged(ref text, value);
        }

        string date;
        public string Date
        {
            get => date;
            set => this.RaiseAndSetIfChanged(ref date, value);
        }

        public LogMessage(LogMessageType type, string tag, string text)
        {
            TAG = tag;
            Type = type;
            Text = text;
            Date = DateTime.Now.ToString();
        }

        public override string ToString()
        {
            return $"{Date} {Type} {TAG} > {Text}";
        }

        public string ToFiltered()
        {
            return $"{Type}{TAG}{Text}".ToLower();
        }
    }

}
