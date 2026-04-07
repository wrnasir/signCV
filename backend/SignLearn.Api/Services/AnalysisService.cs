using SignLearn.Api.Models;

namespace SignLearn.Api.Services {

    public class AnalysisService
    {
        private static string labelMapPath = "";
        public AnalysisService()
        {
            LoadModel();
            LoadLabelMap();
        }

        private void LoadModel()
        {
            
        }

        private void LoadLabelMap()
        {
            
        }

        public SignAnalysisResponse Analyze(SignAnalysisRequest req)
        {
            if (req.getLandmarks().length != 63)
            {
                throw new ArgumentException("shii not it fam.");
            }
        }
    }
}