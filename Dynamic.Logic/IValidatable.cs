using System.Text.RegularExpressions;
namespace Dynamic.Logic;

public interface IValidatable<T>
{
    public bool IsNullable {get;set;}
    public bool IsValid(); 
    public Regex? ValidationRegex { get; }
    public List<string> ValidationFunction(T ValueIn);
}
