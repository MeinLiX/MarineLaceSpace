using System.Reflection;

namespace MarineLaceSpace.DTO.Validation
{
    public class ValidationAssemblyMarker
    {
        public static Assembly Assembly => typeof(ValidationAssemblyMarker).Assembly;
    }
}
