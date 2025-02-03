// Foreign Key Field
using Dynamic.Logic;

public class ForeignKeyField : Field<string>
    {
        public Func<string, bool> ForeignKeyValidator { get; set; }

        public ForeignKeyField(string value, Func<string, bool> foreignKeyValidator) : base()
        {
            ForeignKeyValidator = foreignKeyValidator;
            Value = value;
        }

        public override List<string> ValidationFunction(string value)
        {
            var errors = base.ValidationFunction(value);
            if (!ForeignKeyValidator(value))
            {
                errors.Add("Foreign key value does not exist in the referenced table.");
            }
            return errors;
        }

        public override string RenderEditor()
        {
            return $"<input type='text' value='{Value}' {RenderAttributes()} /><button type='button' onclick='openLookupModal()'>🔍</button>";
        }
                public override string GenerateClientValidator(string fieldName)
        {
            var js = base.GenerateClientValidator(fieldName);
            js += "  // Foreign key validation should be handled via server or asynchronous lookup.\n";
            js += "  return '';\n";
            js += "}\n";
            return js;
        }
    }