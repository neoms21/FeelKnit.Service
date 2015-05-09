namespace FeelKnitService
{
    public static class LogWriter
    {
        static LogWriter()
        {
            //if (!Directory.Exists("Logs"))
            //    Directory.CreateDirectory("Logs");

            //if (!File.Exists("Log.txt"))
            //    File.Create("Log.txt");
        }

        public static void Write(string message)
        {
            //string path = Path.GetDirectoryName((new Uri(Assembly.GetExecutingAssembly().CodeBase)).AbsolutePath);
            //File.AppendAllText(Path.Combine(path, "Log.txt"), string.Format("{0} - {1}\n", DateTime.UtcNow.ToString("F"), message));
        }
    }
}
