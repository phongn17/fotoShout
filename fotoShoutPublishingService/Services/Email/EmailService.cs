using FotoShoutData.Models;
using log4net;
using RazorEngine;
using RazorEngine.Configuration;
using RazorEngine.Templating;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace FotoShoutPublishingService.Services.Email {
    internal class EmailService {
        static ILog _logger = LogManager.GetLogger(typeof(EmailService));

        public string Error { get;  set; }

        public EmailServerAccount EmailServerAccount { get; set; }

        public EmailTemplate EmailTemplate { get; set; }
        
        internal void SendEmails(UserTDO user, EventTDO ev, PhotoAnnotation photoAnnotation, string permalink) {
            Error = "";
            
            ICollection<GuestTDO> guests = photoAnnotation.Guests;
            foreach (GuestTDO guest in guests) {
                SendEmailTo(user, ev, guest, permalink);
            }
        }

        internal void SendEmails(UserTDO user, EventTDO ev, PhotoAnnotation photoAnnotation, ICollection<string> permalinks)
        {
            this.Error = "";
            foreach (GuestTDO guest in (IEnumerable<GuestTDO>)photoAnnotation.Guests)
                this.SendEmailTo(user, ev, guest, permalinks);
        }

        internal TemplateServiceConfiguration GenerateConfigForFileTemplate() {
            TemplateServiceConfiguration config = new TemplateServiceConfiguration();
            config.Resolver = new DelegateTemplateResolver(path => {
                using (StreamReader reader = new StreamReader(path)) {
                    string content = reader.ReadToEnd();
                    return content;
                }
            });

            return config;
        }

        internal void SendEmailTo(UserTDO user, EventTDO ev, GuestTDO guest, ICollection<string> permalinks) {
            if (string.IsNullOrEmpty(guest.Email)) {
                FotoShoutUtils.Log.LogManager.Error(_logger, string.Format("There is no email address stored for {0}.", ((!string.IsNullOrEmpty(guest.FirstName)) ? (guest.FirstName + " ") : "") + ((!string.IsNullOrEmpty(guest.LastName)) ? guest.LastName : guest.GuestId.ToString())));
                return;
            }

            foreach (string permalink in permalinks) {
                SendEmailTo(user, ev, guest, permalink);
            }
        }

        internal void SendEmailTo(UserTDO user, EventTDO ev, GuestTDO guest, string permalink) {
            string receiver = ((!string.IsNullOrEmpty(guest.FirstName)) ? (guest.FirstName + " ") : "") + ((!string.IsNullOrEmpty(guest.LastName)) ? guest.LastName : "");
            if (string.IsNullOrEmpty(receiver))
                receiver = guest.GuestId.ToString();
            FotoShoutUtils.Log.LogManager.Info(_logger, string.Format("Sending the \"{0}\" link to \"{1}\"...", permalink, receiver));

            try {
                string emailContent = "";
                if (EmailTemplate != null)
                    emailContent = GetEmailContent(user, ev, guest, permalink);
                if (string.IsNullOrEmpty(emailContent))
                    emailContent = GenerateDefaultEmailContent(user, ev, guest, permalink);

                using (MailMessage message = new MailMessage()) {
                    message.From = new MailAddress(EmailServerAccount.Username);
                    message.To.Add(new MailAddress(guest.Email));
                    message.Subject = EmailTemplate.EmailSubject;
                    message.IsBodyHtml = true;
                    message.Body = emailContent;
                    message.BodyEncoding = System.Text.Encoding.UTF8;

                    int port = (int)EmailServerAccount.Port;
                    SmtpClient mailClient = (port == 0) ? new SmtpClient(EmailServerAccount.Server) : new SmtpClient(EmailServerAccount.Server, port);
                    bool enableSsl = EmailServerAccount.EnableSSL;
                    try {
                        if (enableSsl) {
                            mailClient.EnableSsl = true;
                            FotoShoutUtils.Log.LogManager.Info(_logger, "Enabled SSL");
                        }
                        //mailClient.DeliveryMethod = SmtpDeliveryMethod.Network;
                        if (!string.IsNullOrEmpty(EmailServerAccount.Username))
                            mailClient.Credentials = string.IsNullOrEmpty(EmailServerAccount.Domain) ?
                                new NetworkCredential(EmailServerAccount.Username, EmailServerAccount.Password) :
                                new NetworkCredential(EmailServerAccount.Username, EmailServerAccount.Password, EmailServerAccount.Domain);
                        else
                            mailClient.UseDefaultCredentials = true;
                        FotoShoutUtils.Log.LogManager.Info(_logger, string.Format("Mail Server: {0}, From: {1} - {2}", EmailServerAccount.Server, message.From.Address, message.From.DisplayName));
                        mailClient.Send(message);
                    }
                    finally {
                        if (mailClient != null)
                            mailClient.Dispose();
                    }
                }
                
                FotoShoutUtils.Log.LogManager.Info(_logger, "Successfully sending email");
            }
            catch (Exception ex) {
                FotoShoutUtils.Log.LogManager.Error(_logger, string.Format("Failed to send the \"{0}\" link to \"{1}\"...", permalink, receiver));
                FotoShoutUtils.Log.LogManager.Error(_logger, ex.ToString());
                Error = ex.Message;
            }
        }

        private string GetEmailContent(UserTDO user, EventTDO ev, GuestTDO guest, string permalink) {
            var model = new {
                Name = !string.IsNullOrEmpty(guest.Salutation) ? (guest.Salutation + " ") : "" + ((!string.IsNullOrEmpty(guest.LastName)) ? guest.LastName : "Sir/Madam"),
                FirstName = !string.IsNullOrEmpty(guest.FirstName) ? guest.FirstName : "",
                LastName = !string.IsNullOrEmpty(guest.LastName) ? guest.LastName : "",
                EventName = !string.IsNullOrEmpty(ev.EventName) ? ev.EventName : "",
                Email = !string.IsNullOrEmpty(guest.Email) ? guest.Email : "",
                UserName = (!string.IsNullOrEmpty(user.FirstName) ? (user.FirstName + " ") : "") + (!string.IsNullOrEmpty(user.LastName) ? user.LastName : ""),
                AccountName = !string.IsNullOrEmpty(user.AccountName) ? user.AccountName : "",
                PermaLink = permalink
            };
            string emailContent = "";
            try {
                emailContent = Razor.Parse(EmailTemplate.EmailContent, model);
            }
            catch (Exception ex) {
                FotoShoutUtils.Log.LogManager.Error(_logger, string.Format("Error: {0} - Publishing service will use the default email content.", ex.ToString()));
            }
            return emailContent;
        }

        private string GenerateDefaultEmailContent(UserTDO user, EventTDO ev, GuestTDO guest, string permalink) {
            var model = new {
                Name = !string.IsNullOrEmpty(guest.Salutation) ? (guest.Salutation + " ") : "" + ((!string.IsNullOrEmpty(guest.LastName)) ? guest.LastName : "Sir/Madam"),
                FirstName = !string.IsNullOrEmpty(guest.FirstName) ? guest.FirstName : "",
                LastName = !string.IsNullOrEmpty(guest.LastName) ? guest.LastName : "",
                EventName = !string.IsNullOrEmpty(ev.EventName) ? ev.EventName : "",
                Email = !string.IsNullOrEmpty(guest.Email) ? guest.Email : "",
                UserName = (!string.IsNullOrEmpty(user.FirstName) ? (user.FirstName + " ") : "") + (!string.IsNullOrEmpty(user.LastName) ? user.LastName : ""),
                AccountName = !string.IsNullOrEmpty(user.AccountName) ? user.AccountName : "",
                PermaLink = permalink
            };
            string template = @"<html>
                                    <head><title>@Model.LastName</title></head>
                                    <body>
                                        Dear @Model.FirstName:<br />
                                        <p>
                                        Thank you for joining us at @Model.EventName. We hope that you enjoy it.
                                        </p>
                                        <p>
                                        Please follow the <a href='@Model.PermaLink'>link</a> to view your photo.
                                        </p>
                                        <br />
                                        <p>
                                        Sincerely,<br />
                                        @Model.UserName - @Model.AccountName
                                        </p>
                                    </body>
                                </html>";
            return Razor.Parse(template, model);
        }

        internal bool IsValid() {
            return (EmailServerAccount != null && !string.IsNullOrEmpty(EmailServerAccount.Server) && !string.IsNullOrEmpty(EmailServerAccount.Username) && !string.IsNullOrEmpty(EmailServerAccount.Password));
        }
    }

}
