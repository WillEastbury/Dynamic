// Binary Data Types
using Dynamic.Logic;

public class ImageField : Field<byte[]>
    {
        public ImageField(byte[] value) { Value = value; }

        public override List<string> ValidationFunction(byte[] value)
        {
            var errors = base.ValidationFunction(value);
            if (!(IsJpeg(value) || IsPng(value)))
                errors.Add("Invalid image format. Only JPEG and PNG are supported.");
            return errors;
        }

        private bool IsJpeg(byte[] data) => data.Length > 3 && data[0] == 0xFF && data[1] == 0xD8;
        private bool IsPng(byte[] data) => data.Length > 8 && data[0] == 0x89 && data[1] == 0x50 && data[2] == 0x4E && data[3] == 0x47;
                public override string GenerateClientValidator(string fieldName)
        {
            var js = $"function Validate{fieldName}() {{\n";
            js += "  // Byte array validation should be handled via file input checks.\n";
            js += "  return '';\n";
            js += "}\n";
            return js;
        }
    }

    public class DownloadField : Field<byte[]>
    {
        public string FileName { get; set; }
        public DownloadField(byte[] value, string fileName) : base()
        {
            Value = value;
            FileName = fileName;
        }
                public override string GenerateClientValidator(string fieldName)
        {
            var js = $"function Validate{fieldName}() {{\n";
            js += "  // Byte array validation should be handled via file input checks.\n";
            js += "  return '';\n";
            js += "}\n";
            return js;
        }
    }