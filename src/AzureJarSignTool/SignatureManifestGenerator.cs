using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Security.Cryptography;
using System.Threading.Tasks;

namespace AzureJarSignTool
{
    public class SignatureManifestGenerator : IDisposable
    {
        private readonly HashAlgorithmName _digestAlgorithm;
        private readonly HashAlgorithm _hashInstance;
        private readonly string _jarHashName;
        private readonly ConcurrentDictionary<string, byte[]> _files;
        private readonly ZipArchive _archive;

        public SignatureManifestGenerator(string jarPath, HashAlgorithmName digestAlgorithm)
        {
            _archive = ZipFile.Open(jarPath, ZipArchiveMode.Update);
            _files = new ConcurrentDictionary<string, byte[]>();
            _digestAlgorithm = digestAlgorithm;
            (_jarHashName, _hashInstance) = AlgorithmTranslator.AlgorithmNameToJarPrefixName(digestAlgorithm);
            if (_jarHashName == null || _hashInstance == null)
            {
                throw new ArgumentException("Invalid hash algorithm", nameof(digestAlgorithm));
            }
        }

        public (int major, int minor) Version { get; } = (1, 0);

        public string CreatedBy { get; set; } = "1.0 (AzureJarSignTool)";

        private byte[] GetManifestDigest()
        {
            var entry = _archive.GetEntry("META-INF/MANIFEST.MF");
            if (entry == null)
            {
                return null;
            }
            return GetEntryDigest(entry);
        }

        public async Task WriteAsync(TextWriter writer)
        {
            var manifestDigest = GetManifestDigest();
            if (manifestDigest == null)
            {
                throw new InvalidOperationException("Jar is missing a manifest.");
            }
            await writer.WriteLineAsync($"Signature-Version: {Version.major}.{Version.minor}");
            await writer.WriteLineAsync($"{_jarHashName}-Manifest-Digest: {Convert.ToBase64String(manifestDigest)}");
            await writer.WriteLineAsync($"Created-By: 1.0 (AzureJarSignTool)");
            await writer.WriteLineAsync();

            foreach(var entry in _archive.Entries)
            {
                if (entry.FullName.StartsWith("META-INF/") || entry.Length == 0)
                {
                    continue;
                }
                var entryDigest = GetEntryDigest(entry);
                await writer.WriteLineAsync($"Name: {entry.FullName}");
                await writer.WriteLineAsync($"{_jarHashName}-Digest: {Convert.ToBase64String(entryDigest)}");
                await writer.WriteLineAsync();
            }
            await writer.WriteLineAsync();
        }

        private byte[] GetEntryDigest(ZipArchiveEntry entry)
        {
            using (var stream = entry.Open())
            {
                return _hashInstance.ComputeHash(stream);
            }
        }

        public void Dispose()
        {
            _hashInstance.Dispose();
            _archive.Dispose();
        }
    }
}
