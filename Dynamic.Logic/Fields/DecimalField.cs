namespace Dynamic.Logic;

public class DecimalField : Field<decimal>, IValidatable<decimal>, IEditable, IViewable, ISizable
{
    public DecimalField(decimal ValueIn)
    {
        Value = ValueIn;
    }

    public int? MaxIntegerPlaces { get; set; } = 127;
    public int? MaxDecimalPlaces { get; set; } = 127;

    public override List<string> ValidationFunction(decimal ValueIn)
    {
        var valids = base.ValidationFunction(ValueIn);
        if(MaxSize.HasValue && ValueIn > MaxSize)
            valids.Add( "This value is too big." );
        if(MinSize.HasValue && ValueIn < MinSize)
            valids.Add( "This value is too small." );
        if(MaxIntegerPlaces.HasValue && ValueIn.ToString().Split(".")[0].Length > MaxIntegerPlaces)
            valids.Add( "This value has too many integer places." );
        if(MaxDecimalPlaces.HasValue && ValueIn.ToString().Split(".")[1].Length > MaxDecimalPlaces)
            valids.Add( "This value has too many decimal places." );
        return valids;
    }

    public override string GenerateClientValidator(string fieldName)
    {
        var js = $"function Validate{fieldName}() {{\n";
        js += $"  var value = parseFloat(document.getElementById('{fieldName}').value);\n";
        js += "  if (isNaN(value)) return 'This value must be a decimal number.';\n";
        if (MinSize.HasValue)
        {
            js += $"  if (value < {MinSize}) return 'This value is too small.';\n";
        }
        if (MaxSize.HasValue)
        {
            js += $"  if (value > {MaxSize}) return 'This value is too large.';\n";
        }
        js += "  return '';\n";
        js += "}\n";
        return js;
    }
}
