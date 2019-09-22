using System.Threading.Tasks;

namespace BoveeBot
{
    class Program
    {
        public static Task Main(string[] args)
            => Startup.RunAsync(args);
    }
}
