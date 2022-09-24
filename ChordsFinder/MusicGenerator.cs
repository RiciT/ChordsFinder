using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChordsFinder
{
    internal class MusicGenerator
    {
        internal static string[] notes = new string[] { "C", "C#", "D", "D#", "E", "F", "F#", "G", "G#", "A", "A#", "B" };

        #region scaleTypes

        static int[][] scaleTypes = {
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
            new int[] { 1, 3, 1, 2, 1, 3, 1 },
        };

        static internal string[] scaleNames = { "ionian", "dorian", "phrygian", "lydian", "mixolydian",
            "aeolian", "locrian", "neapolitan", "harmonicMinor", "melodicMinor", "arabic" };

        #endregion

        #region chordTypes

        static internal int[][] chordTypes = {
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

        static internal string[] chordNames = {
            "major", "major 7th", "major 9th",
            "minor", "minor 6th", "minor 7th", "minor 7th b5", "minor 9th",
             "6th", "7th", "7th #5", "9th",
            "diminished", "augmented", "sus2", "sus4", "7th #9 'Hendrix'", "5th 'power'" };

        static int[] simpleChords = { 0, 1, 3, 5, 9 };
        static int[] complexChords = { 0, 1, 3, 4, 5, 8, 9, 12, 13 };
        static int[] myWanted = { 1, 2, 5, 7, 9, 12, 13 };

        #endregion

        public struct NoteAndLength
        {
            public string note;
            public int octave;
            public double length;
            public int channel;
        }

        internal static (List<string> possibleChords, string[] currentScale) GeneratePossibleChords(string key, string scale)
        {
            int scaleType = -1;
            int offset = 0;

            string[] currentScale = new string[7];
            List<string> possibleChords = new List<string>();

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
                //Console.Write(notes[(FindScale(key) + offset) % 12] + " ");
                currentScale[i] = notes[(FindScale(key) + offset) % 12];
                offset += scaleTypes[scaleType][i];
            }

            //Console.WriteLine();

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
                    if (temp.All(b => b.Equals(true)) && myWanted.Contains(j)) //simple chords can be turned off any time
                    {
                        //Console.WriteLine(currentScale[i] + " " + chordNames[j]);
                        possibleChords.Add(currentScale[i] + " " + chordNames[j]);
                    }
                }
            }

            return (possibleChords, currentScale);
        }

        internal static int FindScale(string input)
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

        static internal NoteAndLength[] GenerateProgression(List<string> possibleChords, int channel)
        {
            Random rnd = new Random();
            double[] lengths = { 1, 1, 1, 1 };
            int[] randNumbers = new int[lengths.Length];
            NoteAndLength[] progression = new NoteAndLength[lengths.Length];

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
                progression[i].note = possibleChords[randNumbers[i]];
                progression[i].length = lengths[i];
                progression[i].channel = channel;
            }

            return progression;
        }

        static internal NoteAndLength[] GenerateMelody(string[] currentScale, NoteAndLength[] progression, bool fitMelodyToChords, bool canSameNotesFollow, int channel)
        {
            Random rnd = new Random();
            int[] lengtharr;

            if (fitMelodyToChords)
            {
                List<int>[] lengths = new List<int>[progression.Length];
                for (int i = 0; i < lengths.Length; i++)
                {
                    lengths[i] = new List<int>();
                }
                double[] maxLengths = new double[progression.Length];

                for (int i = 0; i < progression.Length; i++)
                {
                    maxLengths[i] = 1.0 / progression[i].length;
                }
                for (int i = 0; i < maxLengths.Length; i++)
                {
                    while (maxLengths[i] != 0)
                    {
                        int temp = rnd.Next(1, 5);
                        while (1.0 / Math.Pow(2, temp) > maxLengths[i])
                        {
                            temp = rnd.Next(1, 5);
                        }
                        lengths[i].Add((int)Math.Pow(2, temp));
                        maxLengths[i] -= 1.0 / Math.Pow(2, temp);
                    }
                    lengths[i] = lengths[i].OrderBy(x => rnd.Next()).ToList();
                }

                int lengthsLength = 0;
                for (int i = 0; i < lengths.Length; i++)
                {
                    lengthsLength += lengths[i].Count;
                }

                lengtharr = new int[0];
                for (int i = 0; i < lengths.Length; i++)
                {
                    lengtharr = lengtharr.Concat(lengths[i].ToArray()).ToArray();
                }
            }
            else
            {
                List<int> lengths = new List<int>();
                double maxLength = 0;

                for (int i = 0; i < progression.Length; i++)
                {
                    maxLength += 1.0 / progression[i].length;
                }

                while (maxLength != 0)
                {
                    int temp = rnd.Next(1, 5);
                    while (1.0 / Math.Pow(2, temp) > maxLength)
                    {
                        temp = rnd.Next(1, 5);
                    }
                    lengths.Add((int)Math.Pow(2, temp));
                    maxLength -= 1.0 / Math.Pow(2, temp);
                }
                lengths = lengths.OrderBy(x => rnd.Next()).ToList();
                lengtharr = new int[lengths.Count];
                lengtharr = lengths.ToArray();
            }

            int[] randNumbers = new int[lengtharr.Length]; //progressions length times number of notes on a chord
            NoteAndLength[] melody = new NoteAndLength[lengtharr.Length];

            for (int i = 0; i < randNumbers.Length; i++)
            {
                randNumbers[i] = -1;
            }

            for (int i = 0; i < randNumbers.Length; i++)
            {
                if (i == 0)
                    randNumbers[i] = rnd.Next(currentScale.Length);
                else if (i == randNumbers.Length - 1)
                    randNumbers[i] = 0;
                else
                {
                    randNumbers[i] = rnd.Next(currentScale.Length);
                    while (randNumbers[i - 1] == randNumbers[i] && !canSameNotesFollow)
                    {
                        randNumbers[i] = rnd.Next(currentScale.Length);
                    }
                }
            }

            for (int i = 0; i < melody.Length; i++)
            {
                melody[i].note = currentScale[randNumbers[i]];
                melody[i].octave = 2;
                melody[i].length = lengtharr[i];
                melody[i].channel = channel;
            }

            return melody;
        }

        static internal NoteAndLength[] GenerateBassLine(NoteAndLength[] progression, int channel)
        {
            NoteAndLength[] bassLine = new NoteAndLength[progression.Length];

            for (int i = 0; i < bassLine.Length; i++)
            {
                if (i % 2 == 0)
                {
                    int rootOfProgression = FindScale(progression[i].note.Substring(0, 2).Contains(" ") ? progression[i].note.Substring(0, 1) : progression[i].note.Substring(0, 2));
                    int indexOfChordType = Array.IndexOf(chordNames, progression[i].note.Substring(0, 2).Contains(" ") ?
                        progression[i].note.Substring(2, progression[i].note.Length - 2) : progression[i].note.Substring(3, progression[i].note.Length - 3));
                    //assigning the second note of the current chord to bassline[i]
                    bassLine[i].note = notes[(rootOfProgression + chordTypes[indexOfChordType][0]) % 12];
                    bassLine[i].octave = -1;
                    bassLine[i].length = progression[i].length;
                    bassLine[i].channel = channel;
                }
                if (i % 2 == 1)
                {
                    bassLine[i].note = progression[i].note.Substring(0, 2).Contains(" ") ? progression[i].note.Substring(0, 1) : progression[i].note.Substring(0, 2);
                    bassLine[i].octave = -1;
                    bassLine[i].length = progression[i].length;
                    bassLine[i].channel = channel;
                }
            }

            return bassLine;
        }
    }
}