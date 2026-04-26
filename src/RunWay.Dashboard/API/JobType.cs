namespace Kododo.RunWay.Dashboard.API;

internal sealed record JobType(string Name, string FullName)
{
    public static JobType FromFullTypeName(string fullTypeName)
    {
        var typeNameOnly = fullTypeName.Split(',')[0];
        var lastDotIndex = typeNameOnly.LastIndexOf('.');
        var name = lastDotIndex >= 0 ? typeNameOnly[(lastDotIndex + 1)..] : typeNameOnly;
        return new JobType(name, fullTypeName);
    }
}