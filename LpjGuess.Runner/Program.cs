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
Instructions input = parser.Parse(inputFile);

CancellationTokenSource cancellation = new CancellationTokenSource();
Console.CancelKeyPress += (_, args) =>
{
	cancellation.Cancel();

	// Setting args.Cancel to false causes the application to exit when this
	// event handler exits. We need to continue execution in order to kill any
	// potential child processes.
	args.Cancel = true;
};

IEnumerable<Job> jobs = input.GenerateAllJobs(cancellation.Token);
JobManager jobManager = new JobManager(input.Settings);
await jobManager.RunAllAsync(jobs, cancellation.Token);

return 0;
