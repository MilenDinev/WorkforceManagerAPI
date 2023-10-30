namespace WorkforceManager.Services
{
    using System;
    using System.Net;
    using System.Linq;
    using System.Threading.Tasks;
    using System.Collections.Generic;
    using System.Net.Security;
    using System.Security.Cryptography.X509Certificates;
    using Microsoft.Extensions.Options;
    using MimeKit;
    using MailKit.Net.Smtp;
    using MailKit.Security;
    using Models;
    using Services.Interfaces;
    using Data.Entities;
    using Models.Responses.RequestsResponseModels;
    using WorkforceManager.Data.Constants;

    public class EmailNotificationService : IEmailNotificationService
    {
        private readonly SmtpSettings _smtpSettings;

        public EmailNotificationService(IOptions<SmtpSettings> smtpSettings)
        {
            _smtpSettings = smtpSettings.Value;
        }

        public async Task SendSubmitNotification(SubmitRequestResponseModel submitRequestResponseModel, TimeOffRequest request)
        {
            var emailListOfApprovers = request.Approvers.Select(x => x.Email).ToList();
            await SendNotification(emailListOfApprovers, 
                                    string.Format(EmailServiceConstants.SendSubmitNotificationMessage,
                                                    submitRequestResponseModel.RequestType,
                                                    submitRequestResponseModel.RequestId),
                                    submitRequestResponseModel.ToString());
        }

        public async Task SendApprovedNotification(ApprovedRequestResponseModel approveRequestModel, TimeOffRequest request)
        {
            var recipients = new List<string>() { request.Requester.Email };
            await SendNotification(recipients, EmailServiceConstants.ApprovedRequestTopicMessage, approveRequestModel.ToString());
        }

        public async Task SendAutoApprovedNotification(ApprovedRequestResponseModel autoApprovedRequestResponseModel , TimeOffRequest request, List<string> recipients)
        {
            await SendNotification(recipients, 
                                string.Format(EmailServiceConstants.AutoApprovedTopicMessage,
                                                autoApprovedRequestResponseModel.RequestType, 
                                                autoApprovedRequestResponseModel.RequestId), 
                                string.Format(EmailServiceConstants.AutoApprovedBodyMessage,
                                                autoApprovedRequestResponseModel.ToString() + Environment.NewLine));
        }

        public async Task SendRejectedNotification(RejectedRequestResponseModel rejectResponseModel, TimeOffRequest request, int approverId)
        {
            var recipients = request.Approvers.Where(u => u.Id != approverId).Select(x => x.Email).ToList();
            recipients.Add(request.Requester.Email);
            await SendNotification(recipients, 
                                string.Format(EmailServiceConstants.RejectedRequestTopicMessage,
                                                rejectResponseModel.RequestId),
                                rejectResponseModel.ToString());
        }

        public async Task SendMemberAddedToTeamNotification(Team team, User newMember, int requestsCount)
        {
            if (team.Members.Count > 1)
            {
                var recipients = team.Members.Where(u => u.Id != newMember.Id).Select(x => x.Email).ToList();
                await SendNotification(recipients, 
                                        EmailServiceConstants.MemberAddedToTeamOtherMembersTopicMessage,
                                        string.Format(EmailServiceConstants.MemberAddedToTeamOtherMembersBodyMessage,
                                                        newMember.UserName));
            }

            var memberEmail = new List<string>() { newMember.Email };
            await SendNotification(memberEmail, 
                                EmailServiceConstants.MemberAddedToTeamUserTopicMessage, 
                                string.Format(EmailServiceConstants.MemberAddedToTeamUserBodyMessage, team.Id));

            if (team.TeamLeader != null)
            {
                var teamLeadEmail = new List<string>() { team.TeamLeader.Email };
                await SendNotification(teamLeadEmail, 
                                        EmailServiceConstants.MemberAddedToTeamLeaderTopicMessage,
                                        string.Format(EmailServiceConstants.MemberAddedToTeamLeaderBodyMessage,
                                                        newMember.UserName,
                                                        requestsCount));
            }
        }

        public async Task SendMemberRemovedFromTeamNotification(Team team, User member)
        {
            var recipients = team.Members.Where(u => u.Id != member.Id).Select(x => x.Email).ToList();
            if (recipients.Count > 0)
            {
                await SendNotification(recipients, 
                                        EmailServiceConstants.MemberRemovedFromTeamOtherMembersTopicMessage, 
                                        string.Format(EmailServiceConstants.MemberRemovedFromTeamOtherMembersBodyMessage,
                                                        member.UserName,
                                                        team.Title));
            }
            var memberEmail = new List<string>() { member.Email};
            await SendNotification(memberEmail, 
                                    EmailServiceConstants.MemberRemovedFromTeamUserTopicMessage,
                                    string.Format(EmailServiceConstants.MemberRemovedFromTeamUserBodyMessage,
                                                    team.Title));
        }

        public async Task SendNotification(List<string> recipients, string subject, string messageText)
        {
            var message = new MimeMessage();
            var list = new InternetAddressList();

            message.From.Add(MailboxAddress.Parse(_smtpSettings.SenderEmail));
            foreach (string email in recipients)
            {
                list.Add(MailboxAddress.Parse(email));
            }
            message.To.AddRange(list);
            message.Subject = subject;
            message.Body = new TextPart("plain")
            {
                Text = messageText
            };

            var client = new SmtpClient();

            client.ServerCertificateValidationCallback = (object sender,
                X509Certificate certificate,
                X509Chain chain,
                SslPolicyErrors sslPolicyErrors) => true;

            await client.ConnectAsync(_smtpSettings.Server, _smtpSettings.Port, SecureSocketOptions.StartTls);
            await client.AuthenticateAsync(new NetworkCredential(_smtpSettings.SenderEmail, _smtpSettings.Password));
            await client.SendAsync(message);
            await client.DisconnectAsync(true);

            client.Dispose();
        }
    }
}
