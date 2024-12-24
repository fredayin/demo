using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common.BLL
{
    public interface IStreamReader
    {
        StreamReader GetReader(Stream stream);
    }

    public class FileStreamReader : IStreamReader
    {
        public StreamReader GetReader(Stream stream)
        {
            return new StreamReader(stream);
        }
    }
}
