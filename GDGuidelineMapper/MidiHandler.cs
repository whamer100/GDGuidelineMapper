using Melanchall.DryWetMidi.Core;
using Melanchall.DryWetMidi.Interaction;

namespace GDGuidelineMapper;

public record Note(byte Number, float Offset);

public class MidiHandler
{
    private readonly MidiFile _midiFile;
    private readonly TempoMap _tempoMap;

    public IEnumerable<Note> Notes
    {
        get
        {
            foreach (var note in _midiFile.GetNotes())
            {
                // if (!te.Event.EventType.Equals(MidiEventType.NoteOn)) continue;

                // var note = (NoteOnEvent)te.Event;
                // note.TimeAs<MetricTimeSpan>(_tempoMap);
                var offset = note.TimeAs<MetricTimeSpan>(_tempoMap).TotalMicroseconds / 1_000_000f;

                yield return new Note(note.NoteNumber, offset);
            }
        }
    }

    public MidiHandler(string midiPath)
    {
        var settings = new ReadingSettings
        {
            UnknownChunkIdPolicy = UnknownChunkIdPolicy.Skip
        };
        _midiFile = MidiFile.Read(midiPath, settings);
        _tempoMap = _midiFile.GetTempoMap();
    }
}