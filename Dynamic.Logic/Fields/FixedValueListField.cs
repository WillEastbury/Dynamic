// Fixed Value List Field
using Dynamic.Logic;

public class FixedValueListField : Field<string>
{
    public List<string> AllowedValues { get; set; }

    public FixedValueListField(string value, List<string> allowedValues) : base()
    {
        AllowedValues = allowedValues;
        Value = value;
    }

    public override List<string> ValidationFunction(string value)
    {
        var errors = base.ValidationFunction(value);
        if (!AllowedValues.Contains(value))
        {
            errors.Add("Value is not in the list of allowed values.");
        }
        return errors;
    }

    public override string RenderEditor()
    {
        var options = string.Join("", AllowedValues.Select(v => $"<option value='{v}' {(v == Value ? "selected" : "")}>{v}</option>"));
        return $"<select {RenderAttributes()}>{options}</select>";
    }

    public override string GenerateClientValidator(string fieldName)
    {
        var js = base.GenerateClientValidator(fieldName);
        var allowedValuesEscaped = AllowedValues.Select(v => v.Replace("'", "\\'")).ToArray();
        js += $"  var allowedValues = ['{string.Join("','", allowedValuesEscaped)}'];\n";
        js += "  if (!allowedValues.includes(value)) return 'Value is not in the list of allowed values.';\n";
        js += "  return '';\n";
        js += "}\n";
        return js;
    }
}