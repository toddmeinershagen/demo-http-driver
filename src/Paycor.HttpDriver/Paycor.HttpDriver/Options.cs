using CommandLine;

namespace Paycor.HttpDriver
{
    public class Options
    {
        [Option('i', "intervals", Default = 30, Required = false, HelpText = "Number of intervals that the driver should run before exiting.")]
        public int NumberOfIntervals { get; set; }
        [Option('r', "request-per-interval", Default = 10, Required = false, HelpText = "Number of requests that driver should issue per interval.")]
        public int NumberOfRequestsPerInterval { get; set; }
        [Option('m', "minutes-per-interval", Default = 1, Required = false, HelpText = "Number of minutes that driver should wait between intervals.")]
        public double NumberOfMinutesBetweenIntervals { get; set; }
    }
}
