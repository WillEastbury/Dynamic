using System.Text.RegularExpressions;

namespace Dynamic.Logic;

    // String Fields
    public class StringField : Field<string>
    {
        public StringField(string value) { Value = value; }
        public override List<string> ValidationFunction(string value)
        {
            var errors = base.ValidationFunction(value);
            if (MaxSize.HasValue && value.Length > MaxSize) errors.Add("This value is too long.");
            if (MinSize.HasValue && value.Length < MinSize) errors.Add("This value is too short.");
            return errors;
        }
    }

public class EmailStringField : StringField
    {
        public EmailStringField(string value) : base(value)
        {
            ValidationRegex = new Regex(@"^[\\w.-]+@[\\w.-]+\\.[a-zA-Z]{2,6}$");
        }
        public override string RenderEditor() => $"<input type='email' value='{Value}' {RenderAttributes()} />";
    }

    public class MoneyStringField : StringField
    {
        public MoneyStringField(string value) : base(value)
        {
            ValidationRegex = new Regex(@"^[A-Z]{3}\\s\\d+(\\.\\d{2})?$", RegexOptions.Compiled);
        }
        public override string RenderEditor() => $"<input type='text' value='{Value}' {RenderAttributes()} />";
    }

    public class PostCodeField : StringField
    {
        public PostCodeField(string value) : base(value)
        {
            ValidationRegex = new Regex(@"^[A-Z0-9 ]{3,10}$", RegexOptions.Compiled);
        }
    }

    public class CountryField : StringField
    {
        public CountryField(string value) : base(value)
        {
            ValidationRegex = new Regex(@"^[A-Z]{2,3}$", RegexOptions.Compiled);
        }
    }
     public class PhoneNumberField : StringField
    {
        public PhoneNumberField(string value) : base(value)
        {
            ValidationRegex = new Regex(@"^\\+?[0-9]{7,15}$", RegexOptions.Compiled);
        }
        public override string RenderEditor() => $"<input type='tel' value='{Value}' {RenderAttributes()} />";
    }

    public class LongStringField : StringField
    {
        public LongStringField(string value) : base(value)
        {
            MaxSize = 256;
        }
    }

    public class VeryLongStringField : StringField
    {
        public VeryLongStringField(string value) : base(value)
        {
            MaxSize = 1024;
        }
        public override string RenderEditor() => $"<textarea maxlength='{MaxSize}'>{Value}</textarea>";
    }

    public class FirstNameField : StringField
    {
        public FirstNameField(string value) : base(value)
        {
            MaxSize = 40;
            ValidationRegex = new Regex(@"^[A-Za-z]{1,40}$", RegexOptions.Compiled);
        }
    }

    public class LastNameField : StringField
    {
        public LastNameField(string value) : base(value)
        {
            MaxSize = 40;
            ValidationRegex = new Regex(@"^[A-Za-z]{1,40}$", RegexOptions.Compiled);
        }
    }
        // Complex Types
    public class AddressField
    {
        public StringField Line1 { get; set; } = new StringField("") { MaxSize = 40 };
        public StringField Line2 { get; set; } = new StringField("") { MaxSize = 40 };
        public StringField Line3 { get; set; } = new StringField("") { MaxSize = 40 };
        public StringField Line4 { get; set; } = new StringField("") { MaxSize = 40 };
        public PostCodeField PostCode { get; set; }
        public CountryField Country { get; set; }

        public AddressField(string line1, string line2, string line3, string line4, string postCode, string country)
        {
            Line1.Value = line1;
            Line2.Value = line2;
            Line3.Value = line3;
            Line4.Value = line4;
            PostCode = new PostCodeField(postCode);
            Country = new CountryField(country);
        }
    }