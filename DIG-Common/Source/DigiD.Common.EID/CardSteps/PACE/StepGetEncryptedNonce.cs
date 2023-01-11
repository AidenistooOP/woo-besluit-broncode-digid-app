// Deze broncode is openbaar gemaakt vanwege een Woo-verzoek zodat deze 
// gericht is op transparantie en niet op hergebruik. Hergebruik van 
// de broncode is toegestaan onder de EUPL licentie, met uitzondering 
// van broncode waarvoor een andere licentie is aangegeven.
//
// Het archief waar dit bestand deel van uitmaakt is te vinden op:
//   https://github.com/MinBZK/woo-besluit-broncode-digid-app
//
// Eventuele kwetsbaarheden kunnen worden gemeld bij het NCSC via:
//   https://www.ncsc.nl/contact/kwetsbaarheid-melden
// onder vermelding van "Logius, openbaar gemaakte broncode DigiD-App" 
//
// Voor overige vragen over dit Woo-besluit kunt u mailen met open@logius.nl
//
// This code has been disclosed in response to a request under the Dutch
// Open Government Act ("Wet open Overheid"). This implies that publication 
// is primarily driven by the need for transparence, not re-use.
// Re-use is permitted under the EUPL-license, with the exception 
// of source files that contain a different license.
//
// The archive that this file originates from can be found at:
//   https://github.com/MinBZK/woo-besluit-broncode-digid-app
//
// Security vulnerabilities may be responsibly disclosed via the Dutch NCSC:
//   https://www.ncsc.nl/contact/kwetsbaarheid-melden
// using the reference "Logius, publicly disclosed source code DigiD-App" 
//
// Other questions regarding this Open Goverment Act decision may be
// directed via email to open@logius.nl
//
﻿using System.Linq;
using System.Threading.Tasks;
using BerTlv;
using DigiD.Common.EID.Enums;
using DigiD.Common.EID.Helpers;
using DigiD.Common.EID.Interfaces;
using DigiD.Common.NFC.Enums;

namespace DigiD.Common.EID.CardSteps.PACE
{
    internal class StepGetEncryptedNonce : IStep
    {
        private readonly Gap _gap;

        /// <summary>
        /// The PCA randomly generates a nonce.
        /// See BSI worked example 3.2, page 16
        /// </summary>
        public StepGetEncryptedNonce(Gap gap)
        {
            _gap = gap;
        }

        public async Task<bool> Execute()
        {
            var command = CommandApduBuilder.GetNonceCommand(_gap.SMContext);
            var response = await CommandApduBuilder.SendAPDU("PACE GetEncryptedNonce", command, _gap.SMContext);

            if  (response.SW == 0x9000)
            {
                return ParseData(response.Data);
            }

            return false;
        }

        private bool ParseData(byte[] data)
        {
            var tlvs = Tlv.ParseTlv(data);
            var tlv = tlvs.FirstOrDefault();

            tlv = tlv?.FindTag("80");

            if (tlv == null)
                return false;

            var password = _gap.Pace.Password;

            if (_gap.Pace.PasswordType == PasswordType.MRZ && _gap.Card.DocumentType != DocumentType.DrivingLicense)
                password = CryptoDesHelper.ComputeSHA1Hash(password);

            var pin = password.Concat(new byte[] { 0x00, 0x00, 0x00, 0x03 }).ToArray();
            var pinBytes = AesHelper.CalculateHash(pin, _gap.Card.MessageDigestAlgorithm, _gap.Card.KeyLength);

            _gap.Pace.DecryptedNonce = AesHelper.AESCBC(false, tlv.Value, pinBytes);

            return true;
        }
    }
}
