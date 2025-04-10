using LpjGuess.Runner.Models;
using LpjGuess.Runner.Parsers;

const string appName = "lpjg-experiment";

if (args.Length == 0)
{
    Console.Error.WriteLine($"Usage: {appName} <config-file>");
    return 1;
}

string inputFile = args[0];
IParser parser = new TomlParser();
Configuration config = parser.Parse(inputFile);
SimulationGenerator generator = new SimulationGenerator(config);

CancellationTokenSource cancellation = new CancellationTokenSource();
Console.CancelKeyPress += (_, args) =>
{
    cancellation.Cancel();

    // Setting args.Cancel to false causes the application to exit when this
    // handler returns, which is what we want since we've cancelled the
    // simulations.
    args.Cancel = false;
};

try
{
    IEnumerable<Job> jobs = await generator.GenerateAllJobsAsync(cancellation.Token);
    JobManager jobManager = new JobManager(config, jobs);
    await jobManager.RunAllAsync(cancellation.Token);
    return 0;
}
catch (OperationCanceledException)
{
    Console.Error.WriteLine("Cancelled.");
    return 1;
}
catch (Exception error)
{
    Console.Error.WriteLine($"Error: {error.Message}");
    return 1;
}
