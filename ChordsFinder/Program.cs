using ChordsFinder;
using Melanchall.DryWetMidi.Multimedia;
using Melanchall.DryWetMidi.Core;

/// <summary>
/// Play a Midi File
/// </summary>
/// <remarks>
/// the only dependencies this program has
/// we use Melanchall.DryWetMidi to play the midi file once we wrote everything on the screen
/// and we use the default microsoft wavetable synth as sound
/// </remarks>
void PlayMidiFile(string fileName)
{
    var midiFile = MidiFile.Read(fileName);

    using (var outputDevice = OutputDevice.GetByName("Microsoft GS Wavetable Synth"))
    {
        midiFile.Play(outputDevice);
    }
}

bool isPlay = true;
string key = Console.ReadLine();
string scale = Console.ReadLine();

(List<string> possibleChords, string[] currentScale) = MusicGenerator.GeneratePossibleChords(key, scale);
Console.WriteLine("currentScale:");
currentScale.ToList().ForEach(Console.WriteLine);
Console.WriteLine("possibleChords:");
possibleChords.ForEach(Console.WriteLine);

string[] progression = MusicGenerator.GenerateProgression(possibleChords);
Console.WriteLine("Chord progression:");
progression.ToList().ForEach(Console.WriteLine);

string fileName = Directory.GetParent(Environment.CurrentDirectory).Parent.Parent.Parent.FullName + "\\" + "test.mid";
MidiGenerator.GenerateMidiFile(progression, fileName);

if (isPlay) PlayMidiFile(fileName);
