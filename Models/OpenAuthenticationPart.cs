﻿using Orchard.ContentManagement;

namespace NGM.OpenAuthentication.Models {
    public sealed class OpenAuthenticationPart : ContentPart<OpenAuthenticationPartRecord> {
        public string Identifier {
            get { return Record.Identifier; }
            set { Record.Identifier = value; }
        }
    }
}