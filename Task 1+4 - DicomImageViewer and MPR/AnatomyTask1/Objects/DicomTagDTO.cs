namespace AnatomyTask1.Objects
{
    public class DicomTagDTO
    {
        public string TagID { get; set; } // e.g., "(0002,0001)"
        public string VR { get; set; }    // e.g., "OB"
        public string TagName { get; set; } // e.g., "FileMetaInformationVersion"
        public string Value { get; set; }   // e.g., "binary data"
    }
}
