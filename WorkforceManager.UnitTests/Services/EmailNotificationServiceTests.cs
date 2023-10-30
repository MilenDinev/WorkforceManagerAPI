

using AutoMapper;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using Moq;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using WorkforceManager.Models;
using WorkforceManager.Services;
using WorkforceManager.Services.Interfaces;
using WorkforceManager.UnitTests.Mocks;
using Xunit;

namespace WorkforceManager.UnitTests.Services
{
    public class EmailNotificationServiceTests
    {
        private readonly SmtpSettings settings;
        private readonly Mock<IOptions<SmtpSettings>> mockOptions;

        public EmailNotificationServiceTests()
        {
            var config = ConfigInitializer.InitConfig();
            settings = config.GetSection(nameof(SmtpSettings)).Get<SmtpSettings>();
            mockOptions = new Mock<IOptions<SmtpSettings>>();
            mockOptions.Setup(m => m.Value).Returns(settings);
        }

        [Fact]
        public async Task CorrectData_SendNotification_NoException()
        {           
            
            var sut = new EmailNotificationService(mockOptions.Object);

            var recipients = new List<string>() { "recipient@test.mail" };
            string subject = "test subject";
            var message = "test message";
            
            //To find out how to not actually send emails and then uncomment
            //await sut.sendNotification(recipients, subject, message);
        }

        [Fact]
        public async Task EmptyRecipientsList_SendNotifications_ThrowsException()
        {                      
            
            var sut = new EmailNotificationService(mockOptions.Object);

            var recipients = new List<string>();
            var subject = "test subject";
            var message = "This is a test message";

            await Assert.ThrowsAsync<InvalidOperationException>( () =>  sut.SendNotification(recipients, subject, message));
        }

        [Fact]
        public async Task InvalidRecipientEmail_SendNotifications_ThrowsException()
        {
            
            var sut = new EmailNotificationService(mockOptions.Object);

            var recipients = new List<string>() { "invalidEmail"};
            var subject = "test subject";
            var message = "This is a test message";

            await Assert.ThrowsAsync<MailKit.Net.Smtp.SmtpCommandException>( () => sut.SendNotification(recipients, subject, message));
        }

        [Fact]
        public async Task SubjectNotSet_SendNotifications_ThrowsException()
        {
            
            var sut = new EmailNotificationService(mockOptions.Object);

            var recipients = new List<string>();
            string subject = null;
            var message = "This is a test message";

            await Assert.ThrowsAsync<ArgumentNullException>( () => sut.SendNotification(recipients, subject, message));
        }

        [Fact]
        public async Task MessageNotSet_SendNotifications_ThrowsException()
        {
            
            var sut = new EmailNotificationService(mockOptions.Object);

            var recipients = new List<string>();
            var subject = "test subject";
            string message = null;

            await Assert.ThrowsAsync<ArgumentNullException>( () => sut.SendNotification(recipients, subject, message));
        }
    }
}
