using System.Security.Cryptography;

namespace AzureJarSignTool
{
    public static class AlgorithmTranslator
    {
        public static (string jarName, HashAlgorithm instance) AlgorithmNameToJarPrefixName(HashAlgorithmName algorithm)
        {
            if (algorithm == HashAlgorithmName.MD5)
            {
                return ("MD5", MD5.Create());
            }
            else if (algorithm == HashAlgorithmName.SHA1)
            {
                return ("SHA-1", SHA1.Create());
            }
            else if (algorithm == HashAlgorithmName.SHA256)
            {
                return ("SHA-256", SHA256.Create());
            }
            else if (algorithm == HashAlgorithmName.SHA384)
            {
                return ("SHA-384", SHA384.Create());
            }
            else if (algorithm == HashAlgorithmName.SHA512)
            {
                return ("SHA-512", SHA512.Create());
            }
            return (null, null);
        }
    }
}