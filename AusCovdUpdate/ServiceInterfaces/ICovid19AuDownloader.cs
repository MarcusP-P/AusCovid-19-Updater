using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace AusCovdUpdate.ServiceInterfaces
{
    interface ICovid19AuDownloader
    {
        Task DownloadFile ();
    }
}
