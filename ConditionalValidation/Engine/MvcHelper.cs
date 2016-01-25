using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Net.Mime;
using System.Web;

namespace ConditionalValidation {

    public class MvcHelper {

        internal MvcHelper() { }

        /// <summary>
        /// Отправка почтовое сообщение
        /// </summary>
        /// <param name="mailFrom">адрес от</param>
        /// <param name="mailTo">адрес кому</param>
        /// <param name="mailSubject">предмет сообщения (заголовок)</param>
        /// <param name="mailBody">текст сообщения</param>
        /// <param name="mailSmtpServer">почтовый сервер</param>
        /// <param name="FileName">наименование файла</param>
        /// <returns>успех/неудача</returns>
        public static bool SendMail(string mailFrom, string mailTo, string mailSubject, string mailBody, IEnumerable<HttpPostedFileBase> files, bool isHtml = true) {
            bool _mailSended;
            try {
                MailMessage message = new MailMessage(mailFrom, mailTo, mailSubject, mailBody);
                if (files != null) {
                    foreach (HttpPostedFileBase file in files) {
                        Attachment data = new Attachment(file.InputStream, file.ContentType);
                        ContentDisposition disposition = data.ContentDisposition;
                        disposition.CreationDate = System.IO.File.GetCreationTime(file.FileName);
                        disposition.ModificationDate = System.IO.File.GetLastWriteTime(file.FileName);
                        disposition.ReadDate = System.IO.File.GetLastAccessTime(file.FileName);
                        message.Attachments.Add(data);
                    }
                }
                message.BodyEncoding = System.Text.Encoding.UTF8;
                message.SubjectEncoding = System.Text.Encoding.UTF8;
                message.IsBodyHtml = isHtml;

                SmtpClient client = new SmtpClient("localhost");
                client.Credentials = CredentialCache.DefaultNetworkCredentials;
                client.Send(message);
                _mailSended = true;
            }
            catch (Exception ex) {
                _mailSended = false;
                string inner = ex.InnerException != null ? inner = ex.InnerException.Message : "нет";
            }
            return _mailSended;
        }

        /// <summary>
        /// Перемещение (переименование) папки
        /// </summary>
        /// <param name="currentName">текущее название папки вместе с относительным путем</param>
        /// <param name="newName">новое название папки вместе с относительным путем</param>
        /// <returns>удача/неудача</returns>
        public static bool RenameFolder(string currentName, string newName) {
            if (string.IsNullOrEmpty(currentName)) throw new ArgumentNullException("Переименование невозможно из-за отсутствия параметра", "currentFolderName");
            if (string.IsNullOrEmpty(newName)) throw new ArgumentNullException("Переименование невозможно из-за отсутствия параметра", "newFolderName");

            if (currentName.ToLower().Equals(newName.ToLower())) return true;
            string dir = HttpContext.Current.Server.MapPath(currentName);
            string newdir = HttpContext.Current.Server.MapPath(newName);
            if (Directory.Exists(dir)) {
                Directory.Move(dir, newdir);
                return true;
            }
            else return false;
        }

        /// <summary>
        /// Удаление файла
        /// </summary>
        /// <param name="filename">имя файла</param>
        /// <param name="pathfile">путь до файла без тильды и слеша вначале</param>
        /// <returns>удача/неудача</returns>
        public static bool DeleteFile(string filename, string pathfile) {
            if (string.IsNullOrEmpty(filename)) throw new ArgumentNullException("Удаление невозможно из-за отсутствия параметра", "filename");
            if (string.IsNullOrEmpty(pathfile)) throw new ArgumentNullException("Удаление невозможно из-за отсутствия параметра", "pathfile");
            string dir = HttpContext.Current.Server.MapPath(string.Concat("~/", pathfile));
            if (Directory.Exists(dir)) {
                if (File.Exists(dir + "/" + filename)) {
                    File.Delete(dir + "/" + filename);
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Удаление файла
        /// </summary>
        /// <param name="filename">имя файла</param>
        /// <param name="pathfile">путь до файла без тильды и слеша вначале</param>
        /// <returns>удача/неудача</returns>
        public static bool DeleteFile(string filenameWithPath) {
            if (string.IsNullOrEmpty(filenameWithPath)) throw new ArgumentNullException("Удаление невозможно из-за отсутствия параметра", "filename");
            string dir = HttpContext.Current.Server.MapPath(string.Concat("~/", filenameWithPath));
            if (File.Exists(dir)) {
                File.Delete(filenameWithPath);
                return true;
            }
            return false;
        }

        /// <summary>
        /// Удаление папки и вложенных файлов
        /// </summary>
        /// <param name="filename">имя файла</param>
        /// <param name="pathfile">путь до файла без тильды и слеша вначале</param>
        /// <returns>удача/неудача</returns>
        public static bool DeleteFolder(string folderName) {
            if (string.IsNullOrEmpty(folderName)) throw new ArgumentNullException("Удаление невозможно из-за отсутствия параметра", "folderName");
            string dir = HttpContext.Current.Server.MapPath(string.Concat("~/uploads/", folderName));
            if (Directory.Exists(dir)) {
                foreach (string item in Directory.GetFiles(dir)) {
                    File.Delete(item);
                }
                Directory.Delete(dir);
                return true;
            }
            return false;
        }

        /// <summary>
        /// Читать Coockie
        /// </summary>
        /// <param name="userName"></param>
        /// <param name="userEmail"></param>
        public static void GetCookie(out string userName, out string userEmail) {
            userName = string.Empty;
            userEmail = string.Empty;
            if ((HttpContext.Current.Request.Cookies["sitecookies"] != null)
                && (HttpContext.Current.Request.Cookies["sitecookies"]["Name"] != null)
                && (HttpContext.Current.Request.Cookies["sitecookies"]["Email"] != null)) {
                userName = HttpContext.Current.Request.Cookies["sitecookies"]["Name"].ToString();
                userEmail = HttpContext.Current.Request.Cookies["sitecookies"]["Email"].ToString();
            }
        }

        /// <summary>
        /// Записать Coockie
        /// </summary>
        /// <param name="userName"></param>
        /// <param name="userEmail"></param>
        public static void SetCookie(string userName, string userEmail) {
            HttpCookie cookie = new HttpCookie("sitecookies");
            cookie["Name"] = userName;
            cookie["Email"] = userEmail;
            cookie.Expires = DateTime.Now.AddDays(30);
            HttpContext.Current.Response.Cookies.Add(cookie);
        }

        /// <summary>
        /// Вызвращает количество файлов в папке
        /// </summary>
        /// <param name="productId"></param>
        /// <returns></returns>
        public static int GetFilesCount(int productId) {
            if (productId == 0) throw new ArgumentNullException("productId");
            string filepath = HttpContext.Current.Server.MapPath(string.Concat("~/Uploads/", productId.ToString()));
            if (!Directory.Exists(filepath)) {
                Directory.CreateDirectory(filepath);
            }
            DirectoryInfo dir = new DirectoryInfo(filepath);
            return dir.GetFiles().Count();
        }
    }
}