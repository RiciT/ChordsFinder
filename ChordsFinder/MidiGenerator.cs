using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChordsFinder
{
    internal class MidiGenerator
    {
        static internal void GenerateMidiFile(string[] progression, string fileName)
        {
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



            //GENERATING MIDI FILE
            for (int i = 0; i < progression.Length; i++)
            {
                int rootNote = MusicGenerator.FindScale(progression[i].Substring(0, 2).Contains(" ") ?
                        progression[i].Substring(0, 1) : progression[i].Substring(0, 2));
                //i decided that to glue the chords together better some need to be brought down an octave
                int noteOffset = rootNote - (rootNote > 7 ? 12 : 0);
                //current chord meaning the current type of chord without the rootnote
                int[] currentChord = MusicGenerator.chordTypes[Array.IndexOf(MusicGenerator.chordNames, progression[i].Substring(0, 2).Contains(" ") ?
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
        }

        /// <summary>
        /// Generates byte array from hex string bytes
        /// </summary>
        /// <param name="input">hex string input - the input format must be - "XX XX XX XX XX XX XX"</param>
        /// <returns></returns>
        static List<Byte> HexToByteArr(string input)
        {
            List<Byte> result = new List<byte>();

            for (int i = 0; i < input.Length; i += 3)
            {
                result.Add(Byte.Parse(input[i].ToString() + input[i + 1].ToString(), System.Globalization.NumberStyles.HexNumber));
            }
            return result;
        }

    }
}
