using SenseNet.ContentRepository.Mail;
using SenseNet.MailProcessing;
using SenseNet.Tools;

// ReSharper disable once CheckNamespace
namespace SenseNet.Extensions.DependencyInjection
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
