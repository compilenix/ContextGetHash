using System;
using System.IO;

namespace CoreTools {
    public static class IOutils {
        public static FileStream FileCanRead(string FilePath) {
            return File.Open(FilePath, FileMode.Open, FileAccess.Read);
        }
    }
}
