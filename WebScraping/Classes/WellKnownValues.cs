using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WebScraping.Classes
{
    public sealed class WellKnownValues
    {
        #region Page elements

        public const string TableForScraping = "//table[@width='640']";

        public const string CurrenciesFieldId = "pjname";

        public const string StartDateId = "erectDate";

        public const string EndDateId = "nothing";

        public const string CurrenciesSelect = "//select[@name='pjname']/option";

        public const string NoRecordsMessage = "sorry, no records！\r\n";

        public const string TrElement = "tr";

        public const string TdElement = "td";

        #endregion

        #region Input formats

        public const string DateInputFormat = "yyyy-MM-dd";

        public const string DateFormatForSaveFile = "yyyy.MM.dd";

        #endregion      

        public const string SelectTheCurrentyOption = "Select the currency";
    }
}
