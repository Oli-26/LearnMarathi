using Vosk;
using System.Text.Json;

namespace LearnMarathi.Services
{
    public interface IVoiceRecognitionService
    {
        Task<VoiceRecognitionResult> RecognizeSpeechAsync(byte[] audioData);
    }

    public class VoiceRecognitionService : IVoiceRecognitionService
    {
        private readonly string _modelPath;
        private static Model? _sharedModel;
        private static readonly object _modelLock = new object();
        private static bool _modelLoaded = false;

        public VoiceRecognitionService(string modelPath)
        {
            _modelPath = modelPath;
            InitializeModel();
        }

        private void InitializeModel()
        {
            if (_modelLoaded)
                return;

            lock (_modelLock)
            {
                if (_modelLoaded)
                    return;

                if (Directory.Exists(_modelPath))
                {
                    Console.WriteLine($"Loading Vosk model from: {_modelPath}");
                    Vosk.Vosk.SetLogLevel(0);
                    _sharedModel = new Model(_modelPath);
                    _modelLoaded = true;
                    Console.WriteLine("Vosk model loaded successfully and cached in memory");
                }
            }
        }

        public async Task<VoiceRecognitionResult> RecognizeSpeechAsync(byte[] audioData)
        {
            return await Task.Run(() =>
            {
                try
                {
                    if (_sharedModel == null)
                    {
                        return new VoiceRecognitionResult
                        {
                            Success = false,
                            Text = "",
                            Confidence = 0,
                            Error = "Vosk model not found. Please download a model from https://alphacephei.com/vosk/models"
                        };
                    }

                    // Create a new recognizer for this request (but reuse the model)
                    using var recognizer = new VoskRecognizer(_sharedModel, 16000.0f);
                    recognizer.SetMaxAlternatives(0);
                    recognizer.SetWords(true);

                    // Process audio in chunks for better recognition
                    int offset = 0;
                    int chunkSize = 8000; // 8KB chunks

                    while (offset < audioData.Length)
                    {
                        int currentChunkSize = Math.Min(chunkSize, audioData.Length - offset);
                        byte[] chunk = new byte[currentChunkSize];
                        Array.Copy(audioData, offset, chunk, 0, currentChunkSize);

                        recognizer.AcceptWaveform(chunk, currentChunkSize);
                        offset += currentChunkSize;
                    }

                    // Get final result after all audio is processed
                    var result = recognizer.FinalResult();
                    var jsonResult = JsonSerializer.Deserialize<VoskResult>(result);

                    var recognizedText = jsonResult?.text ?? "";
                    var confidence = CalculateConfidence(jsonResult);

                    return new VoiceRecognitionResult
                    {
                        Success = !string.IsNullOrEmpty(recognizedText),
                        Text = recognizedText,
                        Confidence = confidence,
                        Error = string.IsNullOrEmpty(recognizedText) ? "No speech detected" : null
                    };
                }
                catch (Exception ex)
                {
                    return new VoiceRecognitionResult
                    {
                        Success = false,
                        Text = "",
                        Confidence = 0,
                        Error = $"Recognition error: {ex.Message}"
                    };
                }
            });
        }

        private double CalculateConfidence(VoskResult? result)
        {
            if (result?.result == null || result.result.Count == 0)
                return 0;

            var avgConfidence = result.result.Average(w => w.conf);
            return avgConfidence;
        }
    }

    public class VoiceRecognitionResult
    {
        public bool Success { get; set; }
        public string Text { get; set; } = "";
        public double Confidence { get; set; }
        public string? Error { get; set; }
    }

    public class VoskResult
    {
        public string? text { get; set; }
        public string? partial { get; set; }
        public List<VoskWord>? result { get; set; }
    }

    public class VoskWord
    {
        public double conf { get; set; }
        public double start { get; set; }
        public double end { get; set; }
        public string? word { get; set; }
    }
}
