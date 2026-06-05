using System.Collections;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;

namespace EPR.Calculator.Service.Function.Utils
{
    public static class ResetObjectUtil
    {
        public static void ResetObject(object resetObject)
        {
            if (resetObject == null) return;
            PropertyInfo[] properties = resetObject.GetType().GetProperties();

            foreach (var property in properties)
            {
                ResetObjectValue(resetObject, property);
            }
        }

        [SuppressMessage(
            "Critical Code Smell",
            "S3776:Cognitive Complexity of methods should not be too high",
            Justification = "Temporaraly suppress - will refactor later.")]
        private static void ResetObjectValue(object resetObject, PropertyInfo property)
        {
            if (!property.CanWrite || (resetObject is IEnumerable)) return;
            if (property.Name == "LeaverDate" && property.GetValue(resetObject)?.ToString() == "Totals") return;
            if (property.Name == "IsTotalRow" || property.Name == "isOverallTotalRow") return;

            Type? propType = property.PropertyType;
            if (propType == typeof(string))
            {
                property.SetValue(resetObject, string.Empty);
            }
            else if (propType == typeof(int) || propType == typeof(double))
            {
                property.SetValue(resetObject, 0);
            }
            else if (propType == typeof(decimal)) { property.SetValue(resetObject, 0m);
            }
            else if (propType is not null && propType.IsValueType)
            {
                object? d = Activator.CreateInstance(propType);
                property.SetValue(resetObject, d);
            }
            else
            {
                object? c = property.GetValue(resetObject);
                if (c != null) { ResetObject(c); }
            }
        }
    }
}
