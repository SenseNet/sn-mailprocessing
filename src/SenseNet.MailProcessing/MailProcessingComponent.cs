using System;
using SenseNet.ContentRepository;

namespace SenseNet.MailProcessing
{
    public class MailProcessingComponent : SnComponent
    {
        public override string ComponentId => "SenseNet.MailProcessing";
        public override Version SupportedVersion { get; } = new Version(7, 0, 0);
    }
}
