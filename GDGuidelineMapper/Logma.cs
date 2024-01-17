namespace GDGuidelineMapper;

public static class Logma
{
    public static bool Verbose;

    public static void IncreaseVerbosity()
    {
        Verbose = true;
        Debug("Debug logging has been enabled.");
    }
    public static void Log(string fmt)
    {
        Write($"[INFO] {fmt}");
        Console.ResetColor();
    }
    public static void Info(string fmt)
    {
        Write($"[INFO] {fmt}");
        Console.ResetColor();
    }
    public static void Warn(string fmt)
    {
        Console.ForegroundColor = ConsoleColor.Yellow;
        Write($"[WARN] {fmt}");
        Console.ResetColor();
    }
    public static void Fatal(string fmt)
    {
        Console.ForegroundColor = ConsoleColor.Red;
        Write($"[FATAL] {fmt}");
        Console.ResetColor();
    }
    public static void Debug(string fmt)
    {
        if (!Verbose) return;
        Console.ForegroundColor = ConsoleColor.DarkGray;
        Write($"[DEBUG] {fmt}");
        Console.ResetColor();
    }

    private static readonly StreamWriter LogWriter;
    private static void Write(string m)
    {
        Console.WriteLine(m);
        LogWriter.WriteLine(m);
    }

    static Logma()
    {
        Directory.CreateDirectory(Path.Combine(Directory.GetCurrentDirectory(), "logs"));
        var logOutputLocation = Path.Combine(Directory.GetCurrentDirectory(), "logs", DateTime.Now.ToString("yyyy'-'MM'-'dd'T'HH'_'mm'_'ss") + ".log");
        LogWriter = File.AppendText(logOutputLocation);
        LogWriter.AutoFlush = true;
    }
}