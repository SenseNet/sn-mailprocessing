using SenseNet.ContentRepository.Mail;
using SenseNet.Tools;

namespace SenseNet.MailProcessing
{
    public static class MailProcessingExtensions
    {
        public static IRepositoryBuilder UseLegacyMailProvider(this IRepositoryBuilder repositoryBuilder)
        {
            repositoryBuilder.UseMailProvider(new LegacyMailProvider());

            return repositoryBuilder;
        }
    }
}
