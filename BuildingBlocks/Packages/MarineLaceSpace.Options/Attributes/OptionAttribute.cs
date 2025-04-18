namespace MarineLaceSpace.Options.Attributes;

[AttributeUsage(AttributeTargets.Class)]
public class OptionAttribute(string sectionName) : Attribute
{
    public string SectionName { get; } = sectionName;
}
