using System.Text;
using HashLib;
using System.IO;

namespace CoreTools {
    public class Hash {

        public static Encoding DefaultEncoding = Encoding.UTF8;
        public static IHash DefaultHashAlgorithm = HashFactory.Crypto.CreateSHA256();
        public delegate byte[] CalculateDelegateBytes(byte[] Input, IHash Algorithm);
        public delegate byte[] CalculateDelegateFile(FileStream Stream, IHash Algorithm);

        public IHash HashAlgorithm { get; set; } = DefaultHashAlgorithm;

        public byte[] Calculate(byte[] Input, IHash Algorithm) {
            return Algorithm.ComputeBytes(Input).GetBytes();
        }

        public byte[] Calculate(FileStream Stream, IHash Algorithm) {
            return Algorithm.ComputeStream(Stream).GetBytes();
        }
    }
}
