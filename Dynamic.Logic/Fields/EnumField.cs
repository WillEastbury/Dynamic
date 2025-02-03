// Enum Field
using Dynamic.Logic;

public class EnumField<TEnum> : Field<string> where TEnum : Enum
{
    public EnumField(string value) : base()
    {
        Value = value;
    }

    public override List<string> ValidationFunction(string value)
    {
        var errors = base.ValidationFunction(value);
        if (!Enum.GetNames(typeof(TEnum)).Contains(value))
        {
            errors.Add("Value is not a valid enum name.");
        }
        return errors;
    }

    public override string RenderEditor()
    {
        var options = string.Join("", Enum.GetNames(typeof(TEnum)).Select(name => $"<option value='{name}' {(name == Value ? "selected" : "")}>{name}</option>"));
        return $"<select {RenderAttributes()}>{options}</select>";
    }

        public override string GenerateClientValidator(string fieldName)
        {
            var allowedEnums = Enum.GetNames(typeof(TEnum)).Select(name => name.Replace("'", "\\'")).ToArray();
            var js = base.GenerateClientValidator(fieldName);
            js += $"  var allowedEnums = ['{string.Join("','", allowedEnums)}'];\n";
            js += "  if (!allowedEnums.includes(value)) return 'Value is not a valid enum name.';\n";
            js += "  return '';\n";
            js += "}\n";
            return js;
        }
}