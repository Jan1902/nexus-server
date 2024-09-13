using Nexus.Start;

var server = ServerBuilder.BuildServer();

var cts = new CancellationTokenSource();

Console.CancelKeyPress += (sender, args) =>
{
    cts.Cancel();
    args.Cancel = true;
};

await server.RunAsync(cts.Token);