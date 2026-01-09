using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OAS.SystemData {
    public class Clipboard {

        private static String str = "";

        public static void Write(String value) {
            str = value;
        }

        public static String Read() {
            return str;
        }

    }
}
