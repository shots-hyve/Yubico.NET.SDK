// Copyright 2021 Yubico AB
//
// Licensed under the Apache License, Version 2.0 (the "License").
// You may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//     http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

using System;
using System.Security.Cryptography.X509Certificates;
using Yubico.YubiKey.TestUtilities;
using Xunit;

namespace Yubico.YubiKey.Piv
{
    public class PivSessionAttestationTests
    {
        [Fact]
        public void CreateAttest_BadSlot_ThrowsArgException()
        {
            var yubiKey = new HollowYubiKeyDevice();
            yubiKey.FirmwareVersion.Major = 4;
            yubiKey.FirmwareVersion.Minor = 3;

            using (var pivSession = new PivSession(yubiKey))
            {
                _ = Assert.Throws<ArgumentException>(() => pivSession.CreateAttestationStatement(0x88));
            }
        }

        [Fact]
        public void CreateAttest_BadVersion_ThrowsNotSupportedException()
        {
            var yubiKey = new HollowYubiKeyDevice();
            yubiKey.FirmwareVersion.Major = 4;
            yubiKey.FirmwareVersion.Minor = 2;

            using (var pivSession = new PivSession(yubiKey))
            {
                _ = Assert.Throws<NotSupportedException>(() => pivSession.CreateAttestationStatement(0x9A));
            }
        }

        [Fact]
        public void GetAttest_BadVersion_ThrowsNotSupportedException()
        {
            var yubiKey = new HollowYubiKeyDevice();
            yubiKey.FirmwareVersion.Major = 4;
            yubiKey.FirmwareVersion.Minor = 2;

            using (var pivSession = new PivSession(yubiKey))
            {
                _ = Assert.Throws<NotSupportedException>(() => pivSession.GetAttestationCertificate());
            }
        }

        [Fact]
        public void ReplaceAttest_BadVersion_ThrowsNotSupportedException()
        {
            var yubiKey = new HollowYubiKeyDevice();
            yubiKey.FirmwareVersion.Major = 4;
            yubiKey.FirmwareVersion.Minor = 2;

            var privateKey = new PivPrivateKey();
            var cert = new X509Certificate2();
            using (var pivSession = new PivSession(yubiKey))
            {
                _ = Assert.Throws<NotSupportedException>(() => pivSession.ReplaceAttestationKeyAndCertificate(privateKey, cert));
            }
        }

        [Fact]
        public void ReplaceAttest_NullKey_ThrowsException()
        {
            var yubiKey = new HollowYubiKeyDevice();
            yubiKey.FirmwareVersion.Major = 4;
            yubiKey.FirmwareVersion.Minor = 3;
            bool isValid = SampleKeyPairs.GetMatchingKeyAndCert(out X509Certificate2 cert, out _);
            Assert.True(isValid);

            using (var pivSession = new PivSession(yubiKey))
            {
#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.
                _ = Assert.Throws<ArgumentNullException>(() => pivSession.ReplaceAttestationKeyAndCertificate(null, cert));
#pragma warning restore CS8625 // testing a null input.
            }
        }

        [Fact]
        public void ReplaceAttest_NullCert_ThrowsException()
        {
            var yubiKey = new HollowYubiKeyDevice();
            yubiKey.FirmwareVersion.Major = 4;
            yubiKey.FirmwareVersion.Minor = 3;
            bool isValid = SampleKeyPairs.GetMatchingKeyAndCert(out _, out PivPrivateKey privateKey);
            Assert.True(isValid);

            using (var pivSession = new PivSession(yubiKey))
            {
#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.
                _ = Assert.Throws<ArgumentNullException>(() => pivSession.ReplaceAttestationKeyAndCertificate(privateKey, null));
#pragma warning restore CS8625 // testing a null input.
            }
        }

        [Fact]
        public void ReplaceAttest_Rsa1024_ThrowsException()
        {
            var yubiKey = new HollowYubiKeyDevice(true);
            yubiKey.FirmwareVersion.Major = 4;
            yubiKey.FirmwareVersion.Minor = 3;

            BadAttestationPairs.GetPair(
                BadAttestationPairs.KeyRsa1024CertValid, out string privateKeyPem, out string certPem);

            var priKey = new KeyConverter(privateKeyPem.ToCharArray());
            PivPrivateKey pivPrivateKey = priKey.GetPivPrivateKey();

            char[] certChars = certPem.ToCharArray();
            byte[] certDer = Convert.FromBase64CharArray(certChars, 27, certChars.Length - 52);
            var certObj = new X509Certificate2(certDer);

            using (var pivSession = new PivSession(yubiKey))
            {
                var simpleCollector = new SimpleKeyCollector(false);
                pivSession.KeyCollector = simpleCollector.SimpleKeyCollectorDelegate;

                _ = Assert.Throws<ArgumentException>(() => pivSession.ReplaceAttestationKeyAndCertificate(pivPrivateKey, certObj));
            }
        }

        [Theory]
        [InlineData(BadAttestationPairs.KeyRsa2048CertVersion1)]
        [InlineData(BadAttestationPairs.KeyEccP256CertVersion1)]
        [InlineData(BadAttestationPairs.KeyEccP384CertVersion1)]
        public void ReplaceAttest_Version1Cert_ThrowsException(int whichPair)
        {
            var yubiKey = new HollowYubiKeyDevice(true);
            yubiKey.FirmwareVersion.Major = 4;
            yubiKey.FirmwareVersion.Minor = 3;

            BadAttestationPairs.GetPair(whichPair, out string privateKeyPem, out string certPem);

            var priKey = new KeyConverter(privateKeyPem.ToCharArray());
            PivPrivateKey pivPrivateKey = priKey.GetPivPrivateKey();

            char[] certChars = certPem.ToCharArray();
            byte[] certDer = Convert.FromBase64CharArray(certChars, 27, certChars.Length - 52);
            var certObj = new X509Certificate2(certDer);

            using (var pivSession = new PivSession(yubiKey))
            {
                var simpleCollector = new SimpleKeyCollector(false);
                pivSession.KeyCollector = simpleCollector.SimpleKeyCollectorDelegate;

                _ = Assert.Throws<ArgumentException>(() => pivSession.ReplaceAttestationKeyAndCertificate(pivPrivateKey, certObj));
            }
        }

        [Fact]
        public void ReplaceAttest_BigName_ThrowsException()
        {
            var yubiKey = new HollowYubiKeyDevice(true);
            yubiKey.FirmwareVersion.Major = 4;
            yubiKey.FirmwareVersion.Minor = 3;

            BadAttestationPairs.GetPair(
                BadAttestationPairs.KeyRsa2048CertBigName, out string privateKeyPem, out string certPem);

            var priKey = new KeyConverter(privateKeyPem.ToCharArray());
            PivPrivateKey pivPrivateKey = priKey.GetPivPrivateKey();

            char[] certChars = certPem.ToCharArray();
            byte[] certDer = Convert.FromBase64CharArray(certChars, 27, certChars.Length - 52);
            var certObj = new X509Certificate2(certDer);

            using (var pivSession = new PivSession(yubiKey))
            {
                var simpleCollector = new SimpleKeyCollector(false);
                pivSession.KeyCollector = simpleCollector.SimpleKeyCollectorDelegate;

                _ = Assert.Throws<ArgumentException>(() => pivSession.ReplaceAttestationKeyAndCertificate(pivPrivateKey, certObj));
            }
        }
    }
}
