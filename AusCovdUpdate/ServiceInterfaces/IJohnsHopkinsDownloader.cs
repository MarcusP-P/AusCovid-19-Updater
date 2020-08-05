using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AusCovdUpdate.ServiceInterfaces
{
    public interface IJohnsHopkinsDownloader
    {
        Task DownloadFile ();

        Task<Dictionary<DateTime, Dictionary<string, int>>> DeserialiseDownloadedData ();
    }
}
