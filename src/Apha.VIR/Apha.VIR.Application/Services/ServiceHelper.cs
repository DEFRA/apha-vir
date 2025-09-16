using Apha.VIR.Core.Entities;
using System.Text;

namespace Apha.VIR.Application.Services
{
    public static class ServiceHelper
    {
        public static string GetCharacteristicNomenclature(IList<IsolateCharacteristicInfo> characteristicList)
        {
            var characteristicNomenclatureList = new StringBuilder();

            // Build nomenclature string from characteristics
            foreach (IsolateCharacteristicInfo item in characteristicList)
            {
                if ((item.CharacteristicDisplay == true) && (!string.IsNullOrEmpty(item.CharacteristicValue)))
                {
                    characteristicNomenclatureList.Append(item.CharacteristicPrefix + item.CharacteristicValue + " ");
                }
            }

            var characteristicNomenclature = characteristicNomenclatureList.ToString().Trim();

            return (string.IsNullOrEmpty(characteristicNomenclature) ? "" : "(" + characteristicNomenclature + ")") ;
        }
    }
}
