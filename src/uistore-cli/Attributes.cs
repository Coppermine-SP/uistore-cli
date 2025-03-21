namespace CloudInteractive.UniFiStore;

[AttributeUsage(AttributeTargets.Method)]
public class CommandAttribute(string name) : Attribute
{
    public string Name { get; } = name;
    public string Description { get; set; } = "";
}

[AttributeUsage(AttributeTargets.Parameter)]
public class OptionAttribute(string name) : Attribute
{
    public string Name { get; } = name;
    public string Description { get; set; } = "";
    public bool IsRequired { get; set; } = false;
}
