using System;
using System.Collections.Generic;

using AusCovdUpdate.Model;

namespace AusCovdUpdate.ServiceInterfaces
{
    public interface IExcelDocument
    {

        void OpenDocument (string path);

        void UpdateDailyData (Dictionary<DateTime, Covid19Aus> items);

        void UpdateInternationalData (Dictionary<DateTime, Dictionary<string, int>> items);
    }
}
