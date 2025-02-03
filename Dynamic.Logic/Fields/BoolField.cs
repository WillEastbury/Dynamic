// Boolean Fields
using Dynamic.Logic;

public class YesNoField : Field<bool>
    {
        public YesNoField(bool value)
        {
            IsNullable = false;
            Value = value;
        }

        public override string RenderEditor() => $"<input type='checkbox' {(Value ? "checked" : "")} />";
        public override string GenerateClientValidator(string fieldName)
        {
            var js = $"function Validate{fieldName}() {{\n";
            js += $"  var value = document.getElementById('{fieldName}').checked;\n";
            js += "  return '';\n";
            js += "}\n";
            return js;
        }
    }

    public class YesNoNotSureField : Field<bool?>
    {
        public YesNoNotSureField(bool? value)
        {
            IsNullable = true;
            Value = value;
        }

        public override string RenderEditor()
        {
            return $@"
                <select>
                    <option value='' {(Value == null ? "selected" : "")}>Not Sure</option>
                    <option value='true' {(Value == true ? "selected" : "")}>Yes</option>
                    <option value='false' {(Value == false ? "selected" : "")}>No</option>
                </select>";
        }
    }