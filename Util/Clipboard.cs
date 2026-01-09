namespace Haruka.Common.Util;

public class Clipboard {

    private static String str = "";

    public static void Write(String value) {
        str = value;
    }

    public static String Read() {
        return str;
    }

}