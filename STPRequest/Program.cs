using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace STPRequest {
	static class Program {
		/// <summary>
		/// The main entry point for the application.
		/// </summary>
		/// 
		private static string requestsPath = Properties.Settings.Default.RequestFolderPath;

		static void Main() {
			while(true) {
				Thread.Sleep(5000);

				try {
					string[] requests = Directory.GetFiles(requestsPath, "*.req");
					if (requests.Length == 0) {
						Logging.ToLog("Нет новых обращений (*.req)");
						continue;
					}

					foreach (string request in requests) {
						try {
							Logging.ToLog("Обработка: " + request);
							string requestID = Path.GetFileNameWithoutExtension(request);
							string requestContent = File.ReadAllText(request);

							string[] attachments = Directory.GetFiles(requestsPath, requestID + "@*");
							Logging.ToLog("Кол-во вложений: " + attachments.Length);

							List<string> attachmentsToSend = new List<string>();
							foreach (string attachment in attachments) {
								string attachmentToSend = attachment.Replace(requestID + "@", "");
								File.Copy(attachment, attachmentToSend);
								attachmentsToSend.Add(attachmentToSend);
							}

							Mail.SendMail(
								"Обращение в службу технической поддержки",
								requestContent,
								"stp@bzklinika.ru",
								attachmentsToSend.ToArray(),
								false);

							File.Delete(request);

							foreach (string attachment in attachments)
								File.Delete(attachment);

							foreach (string attachmentToSend in attachmentsToSend)
								File.Delete(attachmentToSend);
						} catch (Exception excReq) {
							Logging.ToLog(excReq.Message + Environment.NewLine + excReq.StackTrace);
						}
					}
				} catch (Exception exc) {
					Logging.ToLog(exc.Message + Environment.NewLine + exc.StackTrace);
				}
			}
		}
	}
}
