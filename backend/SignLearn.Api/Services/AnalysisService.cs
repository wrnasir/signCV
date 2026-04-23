using System.Text.Json;
using Microsoft.ML.OnnxRuntime;
using Microsoft.ML.OnnxRuntime.Tensors;
using SignLearn.Api.DTOs;

namespace SignLearn.Api.Services {

    public class AnalysisService
    {
        private readonly string _labelMapPath = Path.Combine("wwwroot", "models", "label_map.json");
        private readonly string _modelPath = Path.Combine("wwwroot", "models", "asl_classifier.onnx");
        private Dictionary<int, string> _labelMap = new();
        private InferenceSession _session = null!;

        public AnalysisService()
        {
            LoadModel();
            LoadLabelMap();
        }

        private void LoadModel()
        {
            _session = new InferenceSession(_modelPath);
        }

        private void LoadLabelMap()
        {
            string json = File.ReadAllText(_labelMapPath);
            var rawMap = JsonSerializer.Deserialize<Dictionary<string, string>>(json);

            if (rawMap == null)
            {
                throw new Exception("Failed to deserialize label map");
            }

            _labelMap = rawMap.ToDictionary(k => int.Parse(k.Key), k => k.Value);
        }

        public AnalysisResponse Analyze(AnalysisRequest req)
        {
            if (req.Landmarks.Length != 63)
            {
                throw new ArgumentException($"Expected 63 landmarks, received {req.Landmarks.Length}");
            }

            // Create a tensor from the landmarks array
            var inputTensor = new DenseTensor<float>(req.Landmarks, new[] { 1, 63 });

            // Wrap it with the input name the model expects
            var inputs = new List<NamedOnnxValue>
            {
                NamedOnnxValue.CreateFromTensor("input", inputTensor)
            };

            // Run inference
            using var results = _session.Run(inputs);

            // Get predicted class index
            var predictedLabel = results.First().AsEnumerable<long>().First();

            // Get confidence scores per class
            // var probabilities = results.Last().AsEnumerable<float>().ToArray();
            // float confidence = probabilities.Max();

            // Look up the letter
            string recognizedSign = _labelMap[(int)predictedLabel];

            // Build and return response
            return new AnalysisResponse
            {
                RecognizedSign = recognizedSign,
                //Confidence = confidence
                Confidence = 1.0f  // hardcode for now
            };
        }
    }
}