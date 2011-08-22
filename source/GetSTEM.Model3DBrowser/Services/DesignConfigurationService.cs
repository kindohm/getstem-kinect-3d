using System.Collections.Generic;
using GetSTEM.Model3DBrowser.Models;

namespace GetSTEM.Model3DBrowser.Services
{
    public class DesignConfigurationService : IConfigurationService
    {
        public ModelConfiguration GetModelConfiguration()
        {
            var result = new ModelConfiguration();

            result.LetterModels = new List<LetterModel>();

            var model1 = new LetterModel()
            {
                Color = "Red",
                OffsetX = -2,
                OffsetY = 0,
                OffsetZ = .2
            };

            var model2 = new LetterModel()
            {
                Color = "Blue",
                OffsetX = -1,
                OffsetY = .5,
                OffsetZ = -.2
            };

            var model3 = new LetterModel()
            {
                Color = "Green",
                OffsetX = 1,
                OffsetY = .2,
                OffsetZ = -.1
            };

            var model4 = new LetterModel()
            {
                Color = "Yellow",
                OffsetX = 2,
                OffsetY = 0,
                OffsetZ = .15
            };

            result.LetterModels.Add(model1);
            result.LetterModels.Add(model2);
            result.LetterModels.Add(model3);
            result.LetterModels.Add(model4);

            return result;
        }
    }
}
