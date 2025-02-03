namespace Dynamic.Logic;

public class IntegerField : Field<int>, IValidatable<int>, IEditable, IViewable, ISizable
{
    public IntegerField(int ValueIn)
    {
        Value = ValueIn;
    }
    public override List<string> ValidationFunction(int ValueIn)
    {
        var valids = base.ValidationFunction(ValueIn);
        if(MaxSize.HasValue && ValueIn > MaxSize)
            valids.Add( "This value is too big." );

        if(MinSize.HasValue && ValueIn < MinSize)
            valids.Add( "This value is too small." );
        return valids;
    }
    public override string GenerateClientValidator(string fieldName)
    {
        var js = $"function Validate{fieldName}() {{\n";
        js += $"  var value = parseInt(document.getElementById('{fieldName}').value);\n";
        js += "  if (isNaN(value)) return 'This value must be an integer.';\n";
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
