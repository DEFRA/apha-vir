using System.Text.RegularExpressions;

namespace Apha.VIR.Web.Models
{
    public class Submission
    {
        public static bool IsAVNumberValid(string av)
        {
            // This is for testing when an AV number is exactly right.
            Regex regexObj = new(@"^AV\d{6}-\d{2}\s");
            Regex regexObj2 = new(@"^PD\d{4}-\d{2}\s");
            Regex regexObj3 = new(@"^SI\d{6}-\d{2}\s");
            Regex regexObj4 = new(@"^BN\d{6}-\d{2}\s");

            av = av.ToUpper() + " ";

            return regexObj.IsMatch(av) ||
                   regexObj2.IsMatch(av) ||
                   regexObj3.IsMatch(av) ||
                   regexObj4.IsMatch(av);
        }

        public static bool AVNumberIsValidPotentially(string av)
        {
            // This is for testing when an AV number can be formatted to be valid e.g. AV123-01 is OK.
            var regexObj = new Regex(@"^AV\d{1,6}-\d{1,2}\s");
            var regexObj2 = new Regex(@"^PD\d{1,4}-\d{1,2}\s");
            var regexObj3 = new Regex(@"^SI\d{1,6}-\d{1,2}\s");
            var regexObj4 = new Regex(@"^BN\d{1,6}-\d{1,2}\s");

            av = av.ToUpper() + " ";

            if (regexObj.IsMatch(av))
            {
                return true;
            }
            else if (regexObj2.IsMatch(av) && av.StartsWith("PD"))
            {
                return true;
            }
            else if (regexObj3.IsMatch(av))
            {
                return true;
            }
            else if (regexObj4.IsMatch(av))
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public static string AVNumberFormatted(string avNumber)
        {
            try
            {
                avNumber = avNumber.ToUpper();

                int dashPos = avNumber.IndexOf("-");
                if (dashPos == -1)
                {
                    return avNumber;
                }

                string yearPart = double.Parse(avNumber.Substring(dashPos + 1)).ToString("00");
                string prefPart = avNumber.Substring(0, 2);

                string numPart;
                switch (prefPart)
                {
                    case "AV":
                    case "SI":
                    case "BN":
                        numPart = double.Parse(avNumber.Substring(2, dashPos - 2)).ToString("000000");
                        break;
                    case "PD":
                        numPart = double.Parse(avNumber.Substring(2, dashPos - 2)).ToString("0000");
                        break;
                    default:
                        return avNumber;
                }

                return $"{prefPart}{numPart}-{yearPart}";
            }
            catch
            {
                return avNumber;
            }
        }
    }
}
