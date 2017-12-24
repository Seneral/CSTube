using System;
using System.Text;

namespace CSTube
{
	public static class CSTube
	{
		public static Logger logHandler;
		private static bool doLog = true;

		static CSTube()
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
}

public abstract class Logger
{
	public DateTime logStart;

	public Logger()
	{
		logStart = DateTime.Now;
	}

	protected string getTimePrefix()
	{
		TimeSpan time = DateTime.Now.Subtract(logStart);
		return time.ToString(@"mm\:ss\:fff") + ": ";
	}

	public abstract void Log(string message);

	public virtual void ClearLog()
	{
		logStart = DateTime.Now;
	}
}

public class ConsoleLogger : Logger
{
	public override void Log(string message)
	{
		Console.WriteLine(getTimePrefix() + message);
	}
}

public class PostLogger : Logger
{
	private StringBuilder logStack;
	public Logger logHandler;

	public PostLogger()
	{
		logStack = new StringBuilder();
	}

	public void SetEmbeddedLogger(Logger embeddedLogHandler)
	{
		logHandler = embeddedLogHandler;
		logStart = logHandler != null ? logHandler.logStart : DateTime.Now;
	}

	public override void Log(string message)
	{
		logStack.Append("\n" + getTimePrefix() + message);
	}

	public override void ClearLog()
	{
		logStart = DateTime.Now;
		logHandler.ClearLog();
	}

	public void PostLog()
	{
		if (logHandler != null)
			logHandler.Log("Post Log: \n" + logStack.ToString());
		logStack.Clear();
	}
}
