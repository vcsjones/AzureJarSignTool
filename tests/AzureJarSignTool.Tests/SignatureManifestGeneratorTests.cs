using System;
using System.IO;
using System.Security.Cryptography;
using System.Threading.Tasks;
using Xunit;

namespace AzureJarSignTool.Tests
{
    public class SignatureManifestGeneratorTests
    {
        private static string SampleJar = Path.Combine("sample", "jfxswt.jar");

        [Fact]
        public async Task ShouldGenerateManifestFile()
        {
            using(var generator = new SignatureManifestGenerator(SampleJar, HashAlgorithmName.SHA256))
            {
                await generator.WriteAsync(Console.Out);
            }
        } 
    }
}
