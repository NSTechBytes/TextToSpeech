using System;
using System.Runtime.InteropServices;
using System.Speech.Synthesis;
using Rainmeter;

namespace TextToSpeechPlugin
{
    public class Measure
    {
        private string _textToSpeak = "Hello, world!";
        private string _voiceName = "Microsoft David Desktop";  // Default voice set to "David"
        private SpeechSynthesizer _synthesizer;

        // Constructor: Initializes the Speech Synthesizer
        public Measure()
        {
            _synthesizer = new SpeechSynthesizer();
            SetVoice(_voiceName);  // Set the default voice
        }

        // Reloads settings from the Rainmeter INI file
        internal void Reload(Rainmeter.API api, ref double maxValue)
        {
            // Read the text parameter from the INI file (default "Hello, world!")
            _textToSpeak = api.ReadString("Text", "Hello, world!");

            // Read the voice parameter from the INI file (default "Microsoft David Desktop")
            _voiceName = api.ReadString("Voice", "Microsoft David Desktop");

            SetVoice(_voiceName);  // Update voice based on the skin's configuration
        }

        // Executes when the text-to-speech command is triggered
        internal void Execute()
        {
            _synthesizer.SpeakAsync(_textToSpeak);
        }

        // Set the voice for the SpeechSynthesizer
        private void SetVoice(string voiceName)
        {
            try
            {
                foreach (var voice in _synthesizer.GetInstalledVoices())
                {
                    if (voice.VoiceInfo.Name.Equals(voiceName, StringComparison.OrdinalIgnoreCase))
                    {
                        _synthesizer.SelectVoice(voiceName);
                        return;
                    }
                }
                // If the specified voice is not found, fallback to the default system voice
                _synthesizer.SelectVoice("Microsoft David Desktop");
            }
            catch (Exception ex)
            {
                // In case of error, log it and use the default voice
                System.Diagnostics.Debug.WriteLine("Error setting voice: " + ex.Message);
                _synthesizer.SelectVoice("Microsoft David Desktop");
            }
        }

        // Return the text to show in the Rainmeter skin
        internal string GetString()
        {
            return _textToSpeak;
        }
    }

    // The plugin class that hooks into Rainmeter
    public static class Plugin
    {
        // Handle for the measure object
        private static IntPtr _measureHandle;

        [DllExport]
        public static void Initialize(ref IntPtr data, IntPtr rm)
        {
            // Allocate the measure object
            data = GCHandle.ToIntPtr(GCHandle.Alloc(new Measure()));
        }

        [DllExport]
        public static void Finalize(IntPtr data)
        {
            // Free the allocated memory when the plugin is finalized
            GCHandle.FromIntPtr(data).Free();
        }

        [DllExport]
        public static void Reload(IntPtr data, IntPtr rm, ref double maxValue)
        {
            // Reload the measure object with updated settings
            Measure measure = (Measure)GCHandle.FromIntPtr(data).Target;
            measure.Reload(new Rainmeter.API(rm), ref maxValue);
        }

        [DllExport]
        public static void ExecuteBang(IntPtr data)
        {
            // Execute the text-to-speech functionality when triggered
            Measure measure = (Measure)GCHandle.FromIntPtr(data).Target;
            measure.Execute();
        }

        [DllExport]
        public static IntPtr GetString(IntPtr data)
        {
            // Return the text string from the measure
            Measure measure = (Measure)GCHandle.FromIntPtr(data).Target;
            return Marshal.StringToHGlobalUni(measure.GetString());
        }

        [DllExport]
        public static double Update(IntPtr data)
        {
            // Just return 0 as it's not needed for text-to-speech
            return 0.0;
        }
    }
}
