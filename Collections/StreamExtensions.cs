using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OAS.Util.CodeHelpers {
    public static class StreamExtensions {

        public static void Write(this Stream stream, byte[] arr) {
            stream.Write(arr, 0, arr.Length);
        }

    }
}
