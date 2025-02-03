using System.Text.RegularExpressions;

namespace Dynamic.Logic
{
    // Base Field Class
    public class Field<T> : IValidatable<T>, IEditable, IViewable, ISizable
    {
        private T _value;
        public T Value
        {
            get => _value;
            set
            {
                var errors = ValidationFunction(value);
                if (errors.Count == 0)
                {
                    _value = value;
                }
                else
                {
                    throw new ValidationException($"Invalid value set for {typeof(T).Name} field: {string.Join(", ", errors)}");
                }
            }
        }

        public bool IsNullable { get; set; } = false;
        public Regex? ValidationRegex { get; protected set; } = null;
        public long? MinSize { get; set; } = null;
        public long? MaxSize { get; set; } = 127;

        public virtual List<string> ValidationFunction(T value)
        {
            var errors = BaseValidationFunction(value);
            return errors;
        }

        protected List<string> BaseValidationFunction(T value)
        {
            var errors = new List<string>();

            if (ValidationRegex != null && !ValidationRegex.IsMatch(value?.ToString() ?? ""))
                errors.Add("This value is not valid.");

            if (!IsNullable && value == null)
                errors.Add("This value cannot be null.");

            return errors;
        }

        public virtual string RenderEditor() => $"<input type='text' value='{Value}' {RenderAttributes()} />";
        public virtual string RenderView() => $"<input readonly type='text' value='{Value}' />";
        public virtual string RenderValidator() => "";
        public virtual string RenderBinder() => "";

        protected virtual string RenderAttributes()
        {
            var attributes = $"{(IsNullable ? "" : "required")} ";
            if (MaxSize.HasValue) attributes += $"maxlength='{MaxSize}' ";
            if (MinSize.HasValue) attributes += $"minlength='{MinSize}' ";
            if (ValidationRegex != null) attributes += $"pattern='{ValidationRegex}' ";
            return attributes.Trim();
        }
        public virtual string GenerateClientValidator(string fieldName)
        {
            var js = $"function Validate{fieldName}() {{\n";
            js += $"  var value = document.getElementById('{fieldName}').value;\n";
            if (!IsNullable)
            {
                js += "  if (!value) return 'This value cannot be null.';\n";
            }
            if (ValidationRegex != null)
            {
                js += $"  var regex = /{ValidationRegex}/;\n";
                js += "  if (!regex.test(value)) return 'This value is not valid.';\n";
            }
            if (MinSize.HasValue)
            {
                js += $"  if (value.length < {MinSize}) return 'This value is too short.';\n";
            }
            if (MaxSize.HasValue)
            {
                js += $"  if (value.length > {MaxSize}) return 'This value is too long.';\n";
            }
            js += "  return '';\n";
            js += "}\n";
            return js;
        }        
        public bool IsValid() => ValidationFunction(Value).Count == 0;
    }
}