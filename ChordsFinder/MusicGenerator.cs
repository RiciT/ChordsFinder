using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChordsFinder
{
    internal class MusicGenerator
    {
        static string[] notes = new string[] { "C", "C#", "D", "D#", "E", "F", "F#", "G", "G#", "A", "A#", "B" };

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
        };

        static string[] scaleNames = { "ionian", "dorian", "phrygian", "lydian", "mixolydian",
            "aeolian", "locrian", "neapolitan", "harmonicMinor", "melodicMinor" };

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

        #endregion

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
                    if (temp.All(b => b.Equals(true)) && complexChords.Contains(j)) //simple chords can be turned off any time
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

        static internal string[] GenerateProgression(List<string> possibleChords)
        {
            int[] randNumbers = new int[4];
            Random rnd = new Random();

            string[] progression = new string[4];

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

            return progression;
        }
    }
}
