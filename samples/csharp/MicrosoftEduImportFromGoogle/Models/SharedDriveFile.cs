using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MicrosoftEduImportFromGoogle.Models
{
    internal class SharedDriveFile : Material
    {
        public DriveFile DriveFile { get; set; }
        public string ShareMode { get; set; }
    }
}
