using MarineLaceSpace.Enumerations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace BB.Common.Data.Converters;

public class EnumerationConverter<T> : ValueConverter<T, int> where T : Enumeration
{
    public EnumerationConverter() : base(
        v => v.Id,
        v => GetEnumeration(v))
    {
    }

    private static T GetEnumeration(int id)
    {
        var enumeration = Enumeration.GetAll<T>().FirstOrDefault(e => e.Id == id);

        return enumeration ?? throw new InvalidOperationException(
            $"Could not find an enumeration of type {typeof(T).Name} with ID {id}.");
    }
}