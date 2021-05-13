using System.Collections.Generic;

namespace Genocs.Integration.MSAzure.Models
{
    public class FormExtractorResult
    {
        public Classification Classification { get; set; }
        public List<dynamic> ContentData { get; set; }
    }
}
