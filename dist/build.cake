///////////////////////////////////////////////////////////////////////////////
// ARGUMENTS
///////////////////////////////////////////////////////////////////////////////

var target = Argument("target", "help");
var configuration = Argument("configuration", "Release");
var useLinker = !HasArgument("prevent-linker");
var cleanBuild = HasArgument("clean");
var acceptAllPrompts = HasArgument("y") || Console.IsInputRedirected;
if (acceptAllPrompts)
{
    Warning("The argument -y was found or could not read stdin.");
}

var outputBuild = Argument<string>("o", null) ?? Argument("output", "./output");

var isDryRun = IsDryRun();
if(isDryRun)
{
    Error("No support for dry runs yet.");
    Prompt("Continue?");
}

var rids = new string[] {
    //https://docs.microsoft.com/en-us/dotnet/core/rid-catalog
    "linux-x64",
    // "win-x64",
    // "osx-64"
};

///////////////////////////////////////////////////////////////////////////////
// SETUP / TEARDOWN
///////////////////////////////////////////////////////////////////////////////
string tempOutputBuild = "/tmp/wassabi-build/";

Setup(ctx =>
{
    Information("Build output: {0}", outputBuild);

    if (cleanBuild)
    {
        Information("Clean build triggered by --clean argument");
        Information("Cleaning up output directory...");
        CleanDirectory(outputBuild);
    }
});
TaskSetup(setupContext =>
{
    switch (setupContext.Task.Name)
    {
        case "publish:linker":
            Information("Generating a temp directory to start the linking proccess");
            CopyDirectory("../", tempOutputBuild);
            break;
    }
});

TaskTeardown(setupContext =>
{
    switch (setupContext.Task.Name)
    {
        case "publish:linker":
            Information("Deleting temp build directory");
            DeleteDirectory(tempOutputBuild, new DeleteDirectorySettings
            {
                Force = true,
                Recursive = true
            });
            break;
    }
});

public long DirectorySize(DirectoryPath dir)
{
    return System.IO.Directory.EnumerateFiles(dir.FullPath, "*", SearchOption.AllDirectories)
        .Select(f => FileSize(f))
        .Sum();
}

public double DirectorySizeInMB(DirectoryPath dir)
{
    var sizeBytes = DirectorySize(dir);
    double inMb = (sizeBytes / 1024D) / 1024D;
    return Math.Round(inMb, 2, MidpointRounding.ToEven);
}

public bool Prompt(string question)
{
    if (acceptAllPrompts)
    {
        Console.Write(question);
        Warning(" [AUTO] Y");
        return true;
    }

    ConsoleKey consoleKey;
    do
    {
        Console.Write(question);
        Console.WriteLine(" [Y/n]");
        consoleKey = Console.ReadKey(true).Key;
        if (consoleKey == ConsoleKey.Y || consoleKey == ConsoleKey.Enter)
        {
            return true;
        } else if (consoleKey == ConsoleKey.N)
        {
            return false;
        }
    } while (true);
}

///////////////////////////////////////////////////////////////////////////////
// TASKS
///////////////////////////////////////////////////////////////////////////////

Task("publish:selfcontained")
    .DoesForEach(rids,
    (rid) => {
        var outputDir = MakeAbsolute(
            new DirectoryPath(outputBuild)
                .Combine("publish")
                .Combine(rid)
        );
        var settings = new DotNetCorePublishSettings
        {
            SelfContained = true,
            Configuration = configuration,
            Force = true,
            Runtime = rid,
            OutputDirectory = outputDir,
            MSBuildSettings = new DotNetCoreMSBuildSettings{
                NoLogo = true
            }
        };
        
        var linkerOptValue = new string[] { useLinker.ToString() };
        settings.MSBuildSettings.Properties.Add("LinkDuringPublish", linkerOptValue);
        settings.MSBuildSettings.Properties.Add("ShowLinkerSizeComparison", linkerOptValue);

        DotNetCorePublish("../WalletWasabi.Gui", settings);

        Information("Output size: {0} {1}MB", outputDir, DirectorySizeInMB(outputDir));
    });

Task("publish-final")
    .IsDependentOn("publish:selfcontained")
    .Does(() => {
    });

Task("help")
    .Does(() => {
        Prompt("Test?");
        Information("Hello Cake!");
    });

RunTarget(target);