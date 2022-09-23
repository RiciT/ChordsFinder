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
Console.WriteLine("The key of the song: ");
string key = Console.ReadLine();
Console.WriteLine("Scales: " + string.Join(", ", MusicGenerator.scaleNames) + "\nThe scale of the song: ");
string scale = Console.ReadLine();

(List<string> possibleChords, string[] currentScale) = MusicGenerator.GeneratePossibleChords(key, scale);
Console.WriteLine("Current Scale:");
Console.WriteLine(string.Join(" ", currentScale.ToList()));
Console.WriteLine("Possible Chords:");
possibleChords.ForEach(Console.WriteLine);

MusicGenerator.NoteAndLength[] progression = MusicGenerator.GenerateProgression(possibleChords);
Console.WriteLine("Chord progression:");
for (int i = 0; i < progression.Length; i++)
{
    Console.WriteLine(progression[i].note);
}

MusicGenerator.NoteAndLength[] melody = MusicGenerator.GenerateMelody(currentScale);
Console.WriteLine("Melody:");
for (int i = 0; i < melody.Length; i++)
{
    Console.Write(melody[i].note + " ");
}

MusicGenerator.NoteAndLength[] bass = MusicGenerator.GenerateBassLine(progression);
Console.WriteLine();
Console.WriteLine("Bass Line:");
for (int i = 0; i < bass.Length; i++)
{
    Console.Write(bass[i].note + " ");
}

MusicGenerator.NoteAndLength[][] musicData = new MusicGenerator.NoteAndLength[3][];
musicData[0] = melody;
musicData[1] = progression;
musicData[2] = bass;
string fileName = Directory.GetParent(Environment.CurrentDirectory).Parent.Parent.Parent.FullName + "\\" + "test.mid";
MidiGenerator.GenerateMidiFile(fileName, musicData);

if (isPlay) PlayMidiFile(fileName);