using Melanchall.DryWetMidi.Multimedia;
using Melanchall.DryWetMidi.Core;

bool isPlay = true;

string[] notes = new string[] { "C", "C#", "D", "D#", "E", "F", "F#", "G", "G#", "A", "A#", "B" };

#region scaleTypes

int[][] scaleTypes = {
    new int[] { 2, 2, 1, 2, 2, 2, 1 }, //ionian = major
    new int[] { 2, 1, 2, 2, 2, 1, 2 },
    new int[] { 1, 2, 2, 2, 1, 2, 2 },
    new int[] { 2, 2, 2, 1, 2, 2, 1 },
    new int[] { 2, 2, 1, 2, 2, 1, 2 },
    new int[] { 2, 1, 2, 2, 1, 2, 2 }, //aeolian = minor
    new int[] { 1, 2, 2, 1, 2, 2, 2 },
    new int[] { 1, 2, 2, 2, 2, 2, 1 },
    new int[] { 2, 1, 2, 2, 1, 3, 1 },
    new int[] { 2, 1, 2, 2, 2, 2, 1 },
};

string[] scaleNames = { "ionian", "dorian", "phrygian", "lydian", "mixolydian",
    "aeolian", "locrian", "neapolitan", "harmonicMinor", "melodicMinor" };

#endregion

#region chordTypes

int[][] chordTypes = {
    new int[] { 4, 3 },         //major
    new int[] { 4, 3, 4 },      //major 7th
    new int[] { 4, 3, 4, 3 },   //major 9th
    new int[] { 3, 4 },         //minor
    new int[] { 3, 4, 2 },      //minor 6th
    new int[] { 3, 4, 3 },      //minor 7th
    new int[] { 3, 3, 4 },      //minor 7th b5
    new int[] { 3, 4, 3, 4 },   //minor 9th
    new int[] { 4, 3, 2 },      //6th
    new int[] { 4, 3, 3 },      //7th
    new int[] { 4, 4, 2 },      //7th #5
    new int[] { 4, 3, 3, 4 },   //9th
    new int[] { 3, 3 },         //diminished
    new int[] { 4, 4 },         //augmented
    new int[] { 2, 5 },         //sus2
    new int[] { 5, 2 },         //sus4
    new int[] { 4, 3, 3, 5 },   //7th #9 'Hendrix'
    //new int[] { 7 },            //5th 'power'
};

string[] chordNames = { 
    "major", "major 7th", "major 9th", 
    "minor", "minor 6th", "minor 7th", "minor 7th b5", "minor 9th",
     "6th", "7th", "7th #5", "9th", 
    "diminished", "augmented", "sus2", "sus4", "7th #9 'Hendrix'", "5th 'power'" };

int[] simpleChords = { 0, 1, 3, 5, 9 };
int[] complexChords = { 0, 1, 3, 4, 5, 8, 9, 12, 13 };

#endregion

string key = Console.ReadLine();
string scale = Console.ReadLine();

int scaleType = -1;
int offset = 0;

string[] currentScale = new string[7];
List<string> possibleChords = new List<string>();

int[] randNumbers = new int[4];
string[] progression = new string[4];

char[] secondMTrkChunklen = "00000000".ToCharArray(); //DONT CHANGE THIS
string length = "80 83 00"; //60 is quarter note - vlv - variable length value - - - this represents note length
string velocity = "64";
string releaseVelocity = "40";
//string trackName = " 00 FF 03 10 4D 49 44 49 20 67 65 6E 65 72 61 74 6F 72 20 31"; //optional track name can be used for secret messages -- delta time - FF 03 - length - name
const string mthdHeader = "4D 54 68 64";
const string mtrkHeader = "4D 54 72 6B";
const string noteOn = "90";
const string noteOff = "80";
const string startingC = "3C";

string progToMidiFormat = "";
string fileName = Directory.GetParent(Environment.CurrentDirectory).Parent.Parent.Parent.FullName + "\\" + "test.mid";

Random rnd = new Random();

//finding the scale by its name
for (int i = 0; i < scaleNames.Length; i++)
{
    if (scale == scaleNames[i])
    {
        scaleType = i;
        break;
    }
}

//finding the index of the user-given key
//and also getting all the notes of the given scale
for (int i = 0; i < 7; i++)
{
    Console.Write(notes[(FindScale(key) + offset) % 12] + " ");
    currentScale[i] = notes[(FindScale(key) + offset) % 12];
    offset += scaleTypes[scaleType][i];
}

Console.WriteLine();

//getting all the usable chords in the specific scale 
//given the notes of it
for (int i = 0; i < currentScale.Length; i++)
{
    for (int j = 0; j < chordTypes.GetLength(0); j++)
    {
        bool[] temp = new bool[chordTypes[j].Length + 1];
        int chordOffset = 0;
        for (int n = 0; n < chordTypes[j].Length + 1; n++)
        {
            temp[n] = currentScale.Any(s => s.Equals(notes[(FindScale(currentScale[i]) + chordOffset) % 12]));
            if (n < chordTypes[j].Length)
            {
                chordOffset += chordTypes[j][n];
            }
        }
        if (temp.All(b => b.Equals(true)) && complexChords.Contains(j)) //simple chords can be turned off any time
        {
            Console.WriteLine(currentScale[i] + " " + chordNames[j]);
            possibleChords.Add(currentScale[i] + " " + chordNames[j]);
        }
    }
}

//filling up randNumbers array with -1s so that its default value wouldnt be 0s
for (int i = 0; i < randNumbers.Length; i++)
{
    randNumbers[i] = -1;
}

//generating 4 random numbers that will be used as indexes for our chord progression
//important thing to note is that after the first we check if the random index has already been used
//so that we can avoid duplicates in our chord progression
//(we also need to avoid an infinite loop if there arent enough chords for the given length of our chord progression)
for (int i = 0; i < randNumbers.Length; i++)
{
    int newRand = rnd.Next(0, possibleChords.Count);
    if (i == 0)
    {
        randNumbers[i] = newRand;
    }
    else
    {
        while (randNumbers.Contains(newRand) && (possibleChords.Count - (i + 1)) > 0)
        {
            newRand = rnd.Next(0, possibleChords.Count);
        }
        randNumbers[i] = newRand;
    }
}

//assigning the random chords to the progression
for (int i = 0; i < progression.Length; i++)
{
    progression[i] = possibleChords[randNumbers[i]];
}

//GENERATING MIDI FILE
for (int i = 0; i < progression.Length; i++)
{
    int rootNote = FindScale(progression[i].Substring(0, 2).Contains(" ") ?
            progression[i].Substring(0, 1) : progression[i].Substring(0, 2));
    //i decided that to glue the chords together better some need to be brought down an octave
    int noteOffset = rootNote - (rootNote > 7 ? 12 : 0);
    //current chord meaning the current type of chord without the rootnote
    int[] currentChord = chordTypes[Array.IndexOf(chordNames, progression[i].Substring(0, 2).Contains(" ") ?
        progression[i].Substring(2, progression[i].Length - 2) : progression[i].Substring(3, progression[i].Length - 3))];

    //first we need to start all of our notes in the current chord by: delta time - noteOn const - note - velocity
    for (int j = 0; j < currentChord.Length + 1; j++)
    {
        progToMidiFormat += " 00" + " " + noteOn + " " + (Convert.ToInt32(startingC, 16) + noteOffset).ToString("X") + " " + velocity;
        if (j < currentChord.Length) 
        {
            noteOffset += currentChord[j];
        }
    }

    //resetting note offset
    noteOffset = rootNote - (rootNote > 7 ? 12 : 0);

    //then we need to stop all the notes in the chord by: delta time - noteOff const - note - release velocity
    for (int j = 0; j < currentChord.Length + 1; j++)
    {
        //we only need to consider the length of the first note because after that the delta time is 0 for stopping the other notes
        progToMidiFormat += " " + (j == 0 ? length : "00") + " " + noteOff + " " + (Convert.ToInt32(startingC, 16) + noteOffset).ToString("X") + " " + releaseVelocity;
        if (j < currentChord.Length)
        {
            noteOffset += currentChord[j];
        }
    }
}

//need to remove the first element because its just an empty space
progToMidiFormat = progToMidiFormat.Remove(0, 1);
int index = 0;
//counting how long the second track chunk is converting it
//to hex and putting into the secondMTrkChunklen char array
for (int i = secondMTrkChunklen.Length - ((progToMidiFormat.Length + 12 + 1) / 3).ToString("X").Length; i < secondMTrkChunklen.Length; i++)
{
    secondMTrkChunklen[i] = ((progToMidiFormat.Length + 12 + 1) / 3).ToString("X")[index];
    index++;
}

//putting together the final midi file by using - first:
//header - length - settings
//first track chunk - length - different settings - end chunk(00(dt) FF 2F 00)
//second track chunk - length - all note data - end chunk
Byte[] data =
    HexToByteArr(mthdHeader + " 00 00 00 06" + " 00 01 00 02 00 60").Concat(
    HexToByteArr(mtrkHeader + " 00 00 00 13" + " 00 FF 51 03 07 0A E2" + " 00 FF 58 04 04 02 18 08" + " 00 FF 2F 00")).Concat(
    HexToByteArr(mtrkHeader)).Concat(
    HexToByteArr(string.Join(string.Empty, new string(secondMTrkChunklen).Select((x, i) => i > 0 && i % 2 == 0 ? string.Format(" {0}", x) : x.ToString()))).Concat(
    HexToByteArr(progToMidiFormat)).Concat(
    HexToByteArr("00 FF 2F 00"))).ToArray();

//writing the midi data into a midi file
using (var fs = new FileStream(fileName, FileMode.Create, FileAccess.Write))
{
    fs.Write(data, 0, data.Length);
}

Console.WriteLine("Chord progression:");
Console.WriteLine(progression[0]);
Console.WriteLine(progression[1]);
Console.WriteLine(progression[2]);
Console.WriteLine(progression[3]);

//the only dependencies this program has
//we use Melanchall.DryWetMidi to play the midi file once we wrote everything on the screen
//and we use the default microsoft wavetable synth as sound
if (isPlay)
{
    var midiFile = MidiFile.Read(fileName);

    using (var outputDevice = OutputDevice.GetByName("Microsoft GS Wavetable Synth"))
    {
        midiFile.Play(outputDevice);
    }
}

#region functions

int FindScale(string input)
{
    for (int i = 0; i < notes.Length; i++)
    {
        if (input == notes[i])
        {
            return i;
        }
    }
    return -1;
}

//the input format must be - "XX XX XX XX XX XX XX"
List<Byte> HexToByteArr(string input)
{
    List<Byte> result = new List<byte>();

    for (int i = 0; i < input.Length; i += 3)
    {
        result.Add(Byte.Parse(input[i].ToString() + input[i + 1].ToString(), System.Globalization.NumberStyles.HexNumber));
    }
    return result;
}

#endregion