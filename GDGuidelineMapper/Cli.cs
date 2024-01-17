using CommandLine;
using CommandLine.Text;

namespace GDGuidelineMapper;

public class Cli
{
    private class InternalOptions
    {
        // i/o
        [Option("input", Required = true,
            HelpText = "Midi file to import.")]
        public string? MidiPath { get; set; }
        [Option("output", Required = true,
            HelpText = "Destination level to import into.")]
        public string? LevelName { get; set; }
        [Option("revision", Default = 0,
            HelpText = "Level revision.")]
        public int Revision { get; set; }
        // options
        [Option("orange", Default = Constants.DefaultOrangeId,
            HelpText = "Midi note id to use for Orange lines. (minimum 0, maximum 127)")]
        public byte OrangeId { get; set; }
        [Option("yellow", Default = Constants.DefaultYellowId,
            HelpText = "Midi note id to use for Yellow lines. (minimum 0, maximum 127)")]
        public byte YellowId { get; set; }
        [Option("green", Default = Constants.DefaultGreenId,
            HelpText = "Midi note id to use for Green lines. (minimum 0, maximum 127)")]
        public byte GreenId { get; set; }
        // etc
        [Option("dryrun", Default = false,
            HelpText = "Run but dont save.")]
        public bool DryRun { get; set; }
        [Option("verbose", Default = false,
            HelpText = "Set output to verbose messages.")]
        public bool Verbose { get; set; }
        [Option("nobackup", Default = false,
            HelpText = "Disable backing up of game data before touching anything.")]
        public bool DisableBackup { get; set; }
    }

    public struct ProgramOptions
    {
        public string MidiPath { get; set; }
        public string LevelName { get; set; }
        public int Revision { get; set; }
        public byte OrangeId { get; set; }
        public byte YellowId { get; set; }
        public byte GreenId { get; set; }
        public bool DryRun { get; set; }
        public bool Verbose { get; set; }
        public bool DisableBackup { get; set; }
    }

    public ProgramOptions Options;

    public Cli(string[] args)
    {
        var parser = new Parser(with => with.HelpWriter = null);
        var parserResult = parser.ParseArguments<InternalOptions>(args);

        parserResult
            .WithParsed(o =>
            {
                Options.MidiPath = o.MidiPath!;
                Options.LevelName = o.LevelName!;
                Options.Revision = o.Revision;
                Options.OrangeId = o.OrangeId;
                Options.YellowId = o.YellowId;
                Options.GreenId = o.GreenId;
                Options.DryRun = o.DryRun;
                Options.Verbose = o.Verbose;
                if (o.Verbose)
                    Logma.IncreaseVerbosity();
                Options.DisableBackup = o.DisableBackup;
            })
            .WithNotParsed(errs => DisplayHelp(parserResult, errs));
    }

    private static void DisplayHelp<T>(ParserResult<T> result, IEnumerable<Error> errs)
    {
        HelpText helpText;
        if (errs.IsVersion())  //check if error is version request
            helpText = HelpText.AutoBuild(result);
        else
        {
            helpText = HelpText.AutoBuild(result, h =>
            {
                h.AdditionalNewLineAfterOption = false;
                h.Heading = $"{AppDomain.CurrentDomain.FriendlyName} v{Constants.Version}";
                h.Copyright = "Copyright (c) 2024 whamer100";
                return HelpText.DefaultParsingErrorsHandler(result, h);
            }, e => e);
        }
        Console.WriteLine(helpText);
        Environment.Exit(1);
    }
}