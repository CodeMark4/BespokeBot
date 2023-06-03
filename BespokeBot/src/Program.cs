namespace BespokeBot
{
    class Program
    {
        static void Main(string[] args)
        {
            var bot = new BespokeBot();

            try
            {
                bot.RunAsync().GetAwaiter().GetResult();
            }
            catch (Exception e)
            {
                Console.Error.WriteLine(e);
                Environment.Exit(1);
            }
        }
    }
}