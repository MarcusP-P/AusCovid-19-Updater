using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using AusCovdUpdate.Model;

namespace AusCovdUpdate.ServiceInterfaces
{
    public interface ICovid19AuDownloader
    {
        Task DownloadFile ();

        Task<Dictionary<DateTime, Covid19Aus>> DeserialiseDownloadedData ();

    }
}
