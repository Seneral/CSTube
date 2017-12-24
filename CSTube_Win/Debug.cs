using System.Windows.Controls;

namespace CSTube_Win
{
	public static class Debug
	{
		public static Logger logHandler;
		private static bool doLog = true;

		static Debug()
		{ // Set default logHandler to Console.WriteLine
			logHandler = new ConsoleLogger();
			doLog = true;
		}

		public static void SetLogger(Logger logger = null)
		{ logHandler = logger; }

		public static void SetLogState(bool log)
		{ doLog = log; }

		public static void Log(string message)
		{ if (doLog) logHandler?.Log(message); }

		public static void ClearLog()
		{ if (doLog) logHandler?.ClearLog(); }
	}

	public class UILogger : Logger
	{
		public TextBox logPanel;

		public UILogger(TextBox log) : base()
		{
			logPanel = log;
		}

		public override void Log(string message)
		{
			logPanel.Text += "\r\n" + getTimePrefix() + message.Replace("\n", "\r\n");
		}

		public override void ClearLog()
		{
			base.ClearLog();
			logPanel.Text = "";
		}
	}
}
