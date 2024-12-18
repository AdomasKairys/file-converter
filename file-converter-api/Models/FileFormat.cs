namespace file_converter_api.Models
{
    [System.Serializable]
    public class FileFormat
    {
        public required string Format { get; set; }
        public required string Mime { get; set; }
        public bool IsReadable {  get; set; }
        public bool IsWritable { get; set; }
    }
}
