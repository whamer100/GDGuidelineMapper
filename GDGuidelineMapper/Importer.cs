using GeometryDashAPI.Data;
using GeometryDashAPI.Data.Enums;
using GeometryDashAPI.Levels;
using GeometryDashAPI.Levels.Enums;
using GeometryDashAPI.Levels.GameObjects;
using GeometryDashAPI.Levels.GameObjects.Default;
using GeometryDashAPI.Levels.GameObjects.Triggers;
using GeometryDashAPI.Levels.Structures;
using Melanchall.DryWetMidi.Core;
using ProgramOptions = GDGuidelineMapper.Cli.ProgramOptions;
using Color = GeometryDashAPI.Levels.Color;
using Hsv = GeometryDashAPI.Levels.Hsv;

#if DEBUG
using System.Text.Json;
#endif

namespace GDGuidelineMapper;

public class Importer
{
    private readonly ProgramOptions _opt;

    public Importer(ProgramOptions options)
    {
        _opt = options;

        if (!File.Exists(_opt.MidiPath))
        {
            Logma.Fatal($"File \"{_opt.MidiPath}\" does not exist!");
            Environment.Exit(1);
        }
    }

    public void Run()
    {
        if (!_opt.DisableBackup)  // only back up if not set
            BackupData();

        var local = LocalLevels.LoadFile();
        if (!local.LevelExists(_opt.LevelName))
        {
            Logma.Fatal($"Level \"{_opt.LevelName}\" does not exist!");
            Environment.Exit(1);
        }

        var levelEntry = local.GetLevel(_opt.LevelName, _opt.Revision);
        Logma.Info($"Loading level \"{levelEntry.Name} by {levelEntry.AuthorName}\" [Song id: {levelEntry.MusicId}]");
        var level = levelEntry.LoadLevel();
        var duration = TimeSpan.Zero;
        var blockCount = level.CountBlock;
        if (blockCount > 0)
            duration = level.Duration;
        Logma.Debug($"bv:{levelEntry.BinaryVersion},r:{levelEntry.Revision},d:{duration},b:{blockCount},c:{level.CountColor}");

        var offset = level.Options.MusicOffset;
        var mh = new MidiHandler(_opt.MidiPath);
        foreach (var note in mh.Notes)
        {
            GuidelineColor guidelineColor;
            if (note.Number == _opt.OrangeId)
                guidelineColor = GuidelineColor.Orange;
            else if (note.Number == _opt.YellowId)
                guidelineColor = GuidelineColor.Yellow;
            else if (note.Number == _opt.GreenId)
                guidelineColor = GuidelineColor.Green;
            else
            {
                Logma.Warn($"{note} not recognized! Skipping...");
                continue;
            }

            // Console.WriteLine(note);
            level.Guidelines.Add(new Guideline
            {
                Timestamp = note.Offset + offset,
                Color = guidelineColor
            });
        }

#if DEBUG
        Console.WriteLine("Guidelines: " +
            JsonSerializer.Serialize(level.Guidelines, new JsonSerializerOptions{IncludeFields = true})
        );
#endif

        Logma.Info("Saving data...");
        if (_opt.DryRun) return;
        local.GetLevel(_opt.LevelName, revision: _opt.Revision).SaveLevel(level);
        local.Save();
    }

    private void PrintBlock(IBlock block)
    {
        Console.WriteLine($"{block} =>");
        if (block.GetType() == typeof(ColorTrigger))
        {
            var b = (ColorTrigger)block;
            Console.WriteLine($"\tcolor id => {b.ColorId}");
            Console.WriteLine($"\tfade => {b.FadeTime}");
            Console.WriteLine($"\trgb => {{ {b.Red}, {b.Green}, {b.Blue} }}");
        }

        foreach (var wl in block.WithoutLoaded)
        {
            var wlParts = wl.Split(',');
            if (wlParts[0].Equals("43"))
            {
                var hsv = Hsv.Parse(wlParts[1]);
                Console.WriteLine($"\t{wl} => {SerializeHsv(hsv)}");
            }
            else
                Console.WriteLine($"\t{wl}");
        }
        Console.WriteLine("");
    }

    private string SerializeHsv(Hsv hsv)
    {
        return $"{{ Hue: {hsv.Hue}, Sat: {hsv.Saturation}, Val: {hsv.Brightness} }}";
    }

    private static void BackupData()
    {
        Logma.Info("Backing up game data...");
        var backupDirectory = Path.Combine(Directory.GetCurrentDirectory(), "backups");
        Logma.Debug($"Writing to {backupDirectory}");
        Directory.CreateDirectory(backupDirectory);
        var backupTimestamp = DateTime.Now.ToString("yyyy'-'MM'-'dd'T'HH'_'mm'_'ss");

        var gmPath = GameData.ResolveFileName(GameDataType.GameManager);
        var llPath = GameData.ResolveFileName(GameDataType.LocalLevels);

        File.Copy(gmPath, Path.Combine(backupDirectory, $"CCGameManager_{backupTimestamp}.dat"));
        File.Copy(llPath, Path.Combine(backupDirectory, $"CCLocalLevels_{backupTimestamp}.dat"));
    }
}