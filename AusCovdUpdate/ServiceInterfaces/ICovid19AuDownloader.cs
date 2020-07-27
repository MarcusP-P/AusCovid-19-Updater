using System.Threading.Tasks;

namespace AusCovdUpdate.ServiceInterfaces
{
    public interface ICovid19AuDownloader
    {
        Task DownloadFile ();

        Task DeserialiseDownloadedData ();

    }
}
