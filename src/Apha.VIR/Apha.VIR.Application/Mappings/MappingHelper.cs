namespace Apha.VIR.Application.Mappings
{
    public static class MappingHelper
    {
        public static string ToYesNo(bool? value)
        {
            if (value.HasValue)
                return value.Value ? "Yes" : "No";
            else
                return "No";
        }

        public static string ToDateStringFormat(DateTime? date)
        {
            if (date.HasValue)
                return date.Value.ToString("dd/MM/yyyy");
            else
                return "";
        }
    }
}
