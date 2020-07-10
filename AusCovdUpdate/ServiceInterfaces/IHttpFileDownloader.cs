using System;
using System.IO;
using System.Threading.Tasks;

namespace AusCovdUpdate.ServiceInterfaces
{
    public interface IHttpFileDownloader
    {
        Task<Stream> GetFileStreamAsync (Uri uri);
    }
}
