using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using OpenPop.Pop3;
using SenseNet.ContentRepository;
using SenseNet.ContentRepository.Mail;
using SenseNet.ContentRepository.Search;
using SenseNet.ContentRepository.Search.Querying;
using SenseNet.ContentRepository.Storage;
using SenseNet.ContentRepository.Storage.Data;
using SenseNet.ContentRepository.Storage.Search;
using SenseNet.ContentRepository.Storage.Security;
using SenseNet.Diagnostics;
using SenseNet.Workflow;

// ReSharper disable InconsistentNaming

namespace SenseNet.MailProcessing
{
    public class LegacyMailProvider : MailProvider
    {
        //========================================================================== Overrides

        public override bool IsExchangeModeEnabled
        {
            get
            {
                var mpm = Settings.GetValue<MailProcessingMode>(MailHelper.MAILPROCESSOR_SETTINGS, MailHelper.SETTINGS_MODE);
                return mpm == MailProcessingMode.ExchangePull || mpm == MailProcessingMode.ExchangePush;
            }
        }

        public override MailServerCredentials GetPOP3Credentials(string contentListPath)
        {
            var contentList = Node.LoadNode(contentListPath);
            var username = contentList["ListEmail"] as string;

            var pop3Settings = Settings.GetValue<POP3Settings>(
                                   MailHelper.MAILPROCESSOR_SETTINGS, 
                                   MailHelper.SETTINGS_POP3, 
                                   contentListPath) ?? new POP3Settings();

            return new MailServerCredentials
            {
                Username = username,
                Password = pop3Settings.Password
            };
        }

        public override MailMessage[] GetMailMessages(string contentListPath)
        {
            switch (Settings.GetValue(MailHelper.MAILPROCESSOR_SETTINGS, MailHelper.SETTINGS_MODE, contentListPath, MailProcessingMode.ExchangePull))
            {
                case MailProcessingMode.ExchangePull:
                case MailProcessingMode.ExchangePush:
                    throw new SnNotSupportedException("Exchange mail processing modes are not supported here. Use the ExchangeHelper class instead.");
                case MailProcessingMode.POP3:
                    return GetMailMessagesByPOP3(contentListPath);
                default:
                    throw new SnNotSupportedException("Unknown mail processing mode");
            }
        }

        public override void OnListEmailChanged(ContentList list)
        {
            using (new SystemAccount())
            {
                // remove current mail processor workflow
                RemoveWorkflow(list);

                var newEmail = list["ListEmail"] as string;

                // start new workflow + subscription if email is given
                if (!string.IsNullOrEmpty(newEmail))
                    StartSubscription(list);
            }
        }

        //========================================================================== POP3 implementation

        private MailMessage[] GetMailMessagesByPOP3(string contentListPath)
        {
            var messages = new List<MailMessage>();
            var credentials = GetPOP3Credentials(contentListPath);
            var pop3s = Settings.GetValue<POP3Settings>(
                            MailHelper.MAILPROCESSOR_SETTINGS, 
                            MailHelper.SETTINGS_POP3, contentListPath) ?? new POP3Settings();

            using (var client = new Pop3Client())
            {
                try
                {
                    client.Connect(pop3s.Server, pop3s.Port, pop3s.SSL);
                    client.Authenticate(credentials.Username, credentials.Password);
                }
                catch (Exception ex)
                {
                    SnLog.WriteException(ex, "Mail processor workflow error: connecting to mail server " + pop3s.Server + " with the username " + credentials.Username + " failed.");
                    return messages.ToArray();
                }

                int messageCount;

                try
                {
                    messageCount = client.GetMessageCount();
                }
                catch (Exception ex)
                {
                    SnLog.WriteException(ex, "Mail processor workflow error: getting messages failed. Content list: " + contentListPath);
                    return messages.ToArray();
                }

                // Messages are numbered in the interval: [1, messageCount]
                // Most servers give the latest message the highest number
                for (var i = messageCount; i > 0; i--)
                {
                    try
                    {
                        var msg = client.GetMessage(i);
                        var mailMessage = msg.ToMailMessage();
                        messages.Add(mailMessage);
                    }
                    catch (Exception ex)
                    {
                        SnLog.WriteException(ex, "Mail processor workflow error. Content list: " + contentListPath);
                    }
                }

                try
                {
                    client.DeleteAllMessages();
                }
                catch (Exception ex)
                {
                    SnLog.WriteException(ex, "Mail processor workflow error: deleting messages failed. Content list: " + contentListPath);
                }
            }

            SnTrace.Workflow.Write("MailPoller workflow: " + messages.Count + " messages received. Content list: " + contentListPath);

            return messages.ToArray();
        }

        //========================================================================== Helper methods

        private static void RemoveWorkflow(ContentList list)
        {
            // check if any workflow is running currently
            var targetPath = RepositoryPath.Combine(list.Path, "Workflows/MailProcess");
            IEnumerable<Node> runningWorkflows;

            if (SearchManager.ContentQueryIsAllowed)
            {
                runningWorkflows = Content.All.DisableAutofilters().Where(
                    c => c.TypeIs("MailProcessorWorkflow") &&
                    (string)c["WorkflowStatus"] == "$1" &&
                    c.InFolder(targetPath)).AsEnumerable().Select(c => c.ContentHandler);
            }
            else
            {
                runningWorkflows =
                    NodeQuery.QueryNodesByTypeAndPathAndProperty(ActiveSchema.NodeTypes["MailProcessorWorkflow"], false,
                                                                 targetPath, false,
                                                                 new List<QueryPropertyData>
                                                                     {
                                                                         new QueryPropertyData
                                                                             {
                                                                                 PropertyName = "WorkflowStatus",
                                                                                 QueryOperator = Operator.Equal,
                                                                                 Value = "1"
                                                                             }
                                                                     }).Nodes;
            }

            foreach (var wfnode in runningWorkflows)
            {
                wfnode.ForceDelete();
            }
        }

        private static void StartSubscription(ContentList list)
        {
            var subscribe = Settings.GetValue<MailProcessingMode>(
                    MailHelper.MAILPROCESSOR_SETTINGS,
                    MailHelper.SETTINGS_MODE,
                    list.Path) == MailProcessingMode.ExchangePush;

            if (subscribe)
            {
                // subscribe to email after saving content. this is done separately from saving the content, 
                // since subscriptionid must be persisted on the content and we use cyclic retrials for that
                ExchangeHelper.Subscribe(list);
            }

            var parent = GetMailProcessorWorkflowContainer(list);
            if (parent == null)
                return;

            // get the workflow to start
            var incomingEmailWorkflow = list.GetReference<Node>("IncomingEmailWorkflow");
            if (incomingEmailWorkflow == null)
                return;

            // set the list as the related content
            var workflowC = Content.CreateNew(incomingEmailWorkflow.Name, parent, incomingEmailWorkflow.Name);
            workflowC["RelatedContent"] = list;

            try
            {
                workflowC.Save();

                InstanceManager.Start(workflowC.ContentHandler as WorkflowHandlerBase);
            }
            catch (Exception ex)
            {
                SnLog.WriteException(ex, categories: ExchangeHelper.ExchangeLogCategory);
            }
        }

        private static Node GetMailProcessorWorkflowContainer(Node contextNode)
        {
            var parent = Node.LoadNode(RepositoryPath.Combine(contextNode.Path, "Workflows/MailProcess"));

            if (parent == null)
            {
                var workflows = Node.LoadNode(RepositoryPath.Combine(contextNode.Path, "Workflows"));
                if (workflows == null)
                {
                    using (new SystemAccount())
                    {
                        workflows = new SystemFolder(contextNode) { Name = "Workflows" };

                        try
                        {
                            workflows.Save();
                        }
                        catch (Exception ex)
                        {
                            SnLog.WriteException(ex, categories: ExchangeHelper.ExchangeLogCategory);
                            return null;
                        }
                    }
                }
                using (new SystemAccount())
                {
                    parent = new Folder(workflows) { Name = "MailProcess" };

                    try
                    {
                        parent.Save();
                    }
                    catch (Exception ex)
                    {
                        SnLog.WriteException(ex, categories: ExchangeHelper.ExchangeLogCategory);
                        return null;
                    }
                }
            }
            return parent;
        }
    }
}
