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
﻿using System;
using System.Threading.Tasks;
using DigiD.Common.Interfaces;
using DigiD.Common.Mobile.BaseClasses;
using DigiD.Common.RDA.ViewModels;
using DigiD.Common.Services;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace DigiD.UI.Pages
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class AP109 : BaseContentPage
    {
        public AP109()
        {
            InitializeComponent();
        }

        public ScrollOrientation ScrollOrientation { get; set; } = ScrollOrientation.Vertical;

        protected async void txt_Completed(object sender, EventArgs e)
        {
            if (sender == null) throw new ArgumentNullException(nameof(sender));
            if (sender is ICustomEntry cef && BindingContext is AP109ViewModel vm)
            {
                var name = cef.AutomationId;
                vm.Validate(name);
                if (!cef.IsValid && DependencyService.Get<IA11YService>().IsInVoiceOverMode())
                    await InformUser(cef, cef.ErrorText);
            }
        }

        private async Task InformUser(ICustomEntry entryfield, string errorText, bool longDelay = false)
        {
            if (DependencyService.Get<IA11YService>().IsInVoiceOverMode())
            {
                await Task.Delay(100);
                entryfield.Focus();
                // vertraging voldoende voor uitspreken bij invalid geb.datum inclusief invoer vd gebruiker
                await DependencyService.Get<IA11YService>().Speak(errorText, longDelay ? 6000 : 3000);
            }
        }
    }
}
