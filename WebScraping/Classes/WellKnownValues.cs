using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WebScraping.Classes
{
    public sealed class WellKnownValues
    {
        #region Input fields

        public const string StartDateFieldName = "erectDate";

        public const string EndDateFieldName = "nothing";

        public const string CurrenciesField = "//select[@name='pjname']";

        public const string Button = "//input[@type='button']";

        public const string TableForScraping = "//table[@width='640']";

        #endregion

        #region Input formats

        public const string DateInputFormat = "yyyy-MM-dd";

        #endregion

        #region Page Messages

        public const string WrongWordSumbitMessage = "soryy,wrong search word submit,please check your search word!";

        #endregion
    }
}
