using GDGuidelineMapper;
#if DEBUG
using System.Text.Json;
#endif

// TODO:
//  - Write a good readme
//  - Create a roadmap of features

var opt = new Cli(args).Options;

var importer = new Importer(opt);
importer.Run();

#if DEBUG
Logma.Debug("Hello, World!");
Logma.Debug("Parsed arguments:");
Logma.Debug(JsonSerializer.Serialize(opt, new JsonSerializerOptions{WriteIndented = true, IncludeFields = true}));
#endif