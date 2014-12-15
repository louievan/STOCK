using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;

namespace StockPicker.Utils
{
    class CommonUtils
    {
        private const string DATA_DIRECTORY = @"C:\StockData\";
        private const string REPORT_DIRECTORY = @"C:\StockData\Reports\";

        public static string generateStockFilePath(string code)
        {
            return DATA_DIRECTORY + code + ".xml";
        }

        public static string generateReportFilePath()
        {
            string now = DateTime.Now.ToShortDateString().Replace('/', '-');
            return REPORT_DIRECTORY + now + ".csv";
        }

        public static void sendMail()
        {
            MailMessage mail = new MailMessage();
            Encoding encoding = Encoding.Default;

            MailAddress from = new MailAddress("louievan@163.com", "louievan", encoding);
            MailAddress to = new MailAddress("124458864@qq.com", "L", encoding);

            mail.From = from;
            mail.To.Add(to);
            mail.Subject = "Daily Report " + DateTime.Now.ToShortDateString().Replace('/', '-');
            mail.IsBodyHtml = false;
            mail.Body = "";
            mail.Priority = MailPriority.Normal;
            mail.BodyEncoding = encoding;
            mail.Attachments.Add(new Attachment(REPORT_DIRECTORY + DateTime.Now.ToShortDateString().Replace('/', '-') + ".csv"));

            SmtpClient smtp = new SmtpClient();
            smtp.Host = "smtp.163.com";
            smtp.Port = 25;

            smtp.Credentials = new NetworkCredential("louievan@163.com", "fj58445809");

            try
            {
                smtp.Send(mail);
            }
            catch (System.Exception ex)
            {
                Console.WriteLine(ex.Message);
            }

        }
    }
}
