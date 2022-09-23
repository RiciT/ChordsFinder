using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChordsFinder
{
    internal class MidiGenerator
    {
        public struct MidiFormat
        {
            public string note;
            public int octave;
            public double deltaTime;
            public bool isOn;
        }

        static internal void GenerateMidiFile(string fileName, MusicGenerator.NoteAndLength[][] musicData)
        //MusicGenerator.NoteAndLength[] progression, MusicGenerator.NoteAndLength[] melody, MusicGenerator.NoteAndLength[] bassline)
        {
            char[] secondMTrkChunklen = "00000000".ToCharArray(); //DONT CHANGE THIS
            //60 is quarter note - vlv - variable length value - - - this represents note length
            //80 83 00 full note
            const int fullNote = 384;
            string velocity = "64";
            string releaseVelocity = "40";
            //string trackName = " 00 FF 03 10 4D 49 44 49 20 67 65 6E 65 72 61 74 6F 72 20 31"; //optional track name can be used for secret messages -- delta time - FF 03 - length - name
            const string mthdHeader = "4D 54 68 64";
            const string mtrkHeader = "4D 54 72 6B";
            const string noteOn = "9";
            const string noteOff = "8";
            const string startingC = "3C";

            string progToMidiFormat = "";

            int musicDataLength = 0;
            for (int i = 0; i < musicData.Length; i++)
            {
                musicDataLength += musicData[i].Length;
            }

            MidiFormat[] midiActions = new MidiFormat[musicDataLength * 2];
            int[] indeces = new int[musicData.GetLength(0)];
            double[] queueTimes = new double[musicData.GetLength(0)];
            double globalTime = 0;


            for (int i = 0; i < indeces.Length; i++)
            {
                indeces[i] = 0;
                queueTimes[i] = 0;
            }

            #region MidiFormat array generation
            //GENERATING ARRAY TO GENERATE MIDI FROM IT
            //turning on first notes
            for (int i = 0; i < musicData.GetLength(0); i++)
            {
                midiActions[i].note = musicData[i][0].note;
                midiActions[i].octave = musicData[i][0].octave;
                midiActions[i].deltaTime = 0;
                midiActions[i].isOn = true;
                indeces[i] = 1;
                queueTimes[i] = 1.0 / musicData[i][0].length;
            }
            
            for (int i = musicData.GetLength(0); i < midiActions.Length - musicData.GetLength(0); i += 2)
            {
                var temp = queueTimes.ToList().IndexOf(queueTimes.Min());
                midiActions[i].note = musicData[temp][indeces[temp] - 1].note;
                midiActions[i].octave = musicData[temp][indeces[temp] - 1].octave;
                midiActions[i].deltaTime = queueTimes[temp] - globalTime;
                midiActions[i].isOn = false;
                globalTime += midiActions[i].deltaTime;

                midiActions[i + 1].note = musicData[temp][indeces[temp]].note;
                midiActions[i + 1].octave = musicData[temp][indeces[temp]].octave;
                midiActions[i + 1].deltaTime = 0;
                midiActions[i + 1].isOn = true;
                queueTimes[temp] += 1.0 / musicData[temp][indeces[temp]].length;
                indeces[temp]++;
            }
            //turning off last notes
            for (int i = musicData.GetLength(0); i > 0; i--)
            {
                midiActions[midiActions.Length - i].note = musicData[musicData.GetLength(0) - i][musicData[musicData.GetLength(0) - i].Length - 1].note;
                midiActions[midiActions.Length - i].octave = musicData[musicData.GetLength(0) - i][musicData[musicData.GetLength(0) - i].Length - 1].octave;
                midiActions[midiActions.Length - i].deltaTime = (i == musicData.GetLength(0) ?
                    1.0 / musicData[musicData.GetLength(0) - i][musicData[musicData.GetLength(0) - i].Length - 1].length : 0);
                midiActions[midiActions.Length - i].isOn = false;
            }
            #endregion

            //GENERATING MIDI FILE
            for (int i = 0; i < midiActions.Length; i++)
            {
                if (midiActions[i].note.Length > 4)
                {
                    int rootNote = MusicGenerator.FindScale(midiActions[i].note.Substring(0, 2).Contains(" ") ?
                                midiActions[i].note.Substring(0, 1) : midiActions[i].note.Substring(0, 2));
                    //i decided that to glue the chords together better some need to be brought down an octave - i deleted this feautre for now can be put back eventually
                    int noteOffset = rootNote;
                    //current chord meaning the current type of chord without the rootnote
                    int[] currentChord = MusicGenerator.chordTypes[Array.IndexOf(MusicGenerator.chordNames, midiActions[i].note.Substring(0, 2).Contains(" ") ?
                        midiActions[i].note.Substring(2, midiActions[i].note.Length - 2) : midiActions[i].note.Substring(3, midiActions[i].note.Length - 3))];

                    if (midiActions[i].isOn)
                    {
                        //first we need to start all of our notes in the current chord by: delta time - noteOn const - note - velocity
                        for (int j = 0; j < currentChord.Length + 1; j++)
                        {
                            progToMidiFormat += " 00" + " " + noteOn + " " + (Convert.ToInt32(startingC, 16) + noteOffset + midiActions[i].octave * 12).ToString("X") + " " + velocity;
                            if (j < currentChord.Length)
                            {
                                noteOffset += currentChord[j];
                            }
                        }
                    }
                    else
                    {
                        //then we need to stop all the notes in the chord by: delta time - noteOff const - note - release velocity
                        for (int j = 0; j < currentChord.Length + 1; j++)
                        {
                            //we only need to consider the length of the first note because after that the delta time is 0 for stopping the other notes
                            progToMidiFormat += 
                                " " + (midiActions[i].deltaTime != 0 && j == 0 ? VlvConverter.DecToHexVlv((int)(midiActions[i].deltaTime * fullNote)) : VlvConverter.DecToHexVlv(0))
                                + " " + noteOff + " " + (Convert.ToInt32(startingC, 16) + noteOffset + midiActions[i].octave * 12).ToString("X") + " " + releaseVelocity;
                            if (j < currentChord.Length)
                            {
                                noteOffset += currentChord[j];
                            }
                        }
                    }
                }
                else
                {
                    if (midiActions[i].isOn)
                    {
                        progToMidiFormat += " 00" + " " + noteOn + " " + (Convert.ToInt32(startingC, 16) +
                            Array.IndexOf(MusicGenerator.notes, midiActions[i].note) + midiActions[i].octave * 12).ToString("X") + " " + velocity;
                    }
                    else
                    {
                        progToMidiFormat += " " + VlvConverter.DecToHexVlv((int)(midiActions[i].deltaTime * fullNote)) + " " + noteOff + " " + 
                            (Convert.ToInt32(startingC, 16) + Array.IndexOf(MusicGenerator.notes, midiActions[i].note) + midiActions[i].octave * 12).ToString("X") + " " + releaseVelocity;
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