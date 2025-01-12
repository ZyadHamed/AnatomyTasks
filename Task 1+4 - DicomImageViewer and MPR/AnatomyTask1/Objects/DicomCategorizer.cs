using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AnatomyTask1.Objects
{
    public class DicomCategorizer
    {
        public static Dictionary<string, List<string>> CategoryMap = new Dictionary<string, List<string>>()
    {
        { "Patient Info", new List<string> { "0010"} },
        { "Equipment Info", new List<string> { "0008" } }
        };

        public static Dictionary<string, List<DicomTagDTO>> CategorizeTags(List<DicomTagDTO> dicomTags)
        {
            var categorizedTags = new Dictionary<string, List<DicomTagDTO>>();

            foreach (var tag in dicomTags)
            {
                // Find the category for the tag
                foreach (var category in CategoryMap)
                {
                    string s = tag.TagID.Substring(1, 4);
                    if (category.Value.Contains(tag.TagID.Substring(1, 4)))
                    {
                        if (!categorizedTags.ContainsKey(category.Key))
                            categorizedTags[category.Key] = new List<DicomTagDTO>();

                        categorizedTags[category.Key].Add(tag);
                        break;
                    }
                }
            }

            return categorizedTags;
        }
    }
}
