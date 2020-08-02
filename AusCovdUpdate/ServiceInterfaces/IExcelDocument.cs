using System;
using System.Collections.Generic;

using AusCovdUpdate.Model;

namespace AusCovdUpdate.ServiceInterfaces
{
    public interface IExcelDocument
    {

        void OpenDocument (string path);

        void UpdateSpreadsheet (Dictionary<DateTime, Covid19Aus> items);

    }
}
