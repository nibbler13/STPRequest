using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;

namespace STPRequest {
	class EventSystem {
		private Timer timer;
		private string requestsPath = Properties.Settings.Default.RequestFolderPath;
		private static bool isCompleted = true;

		public void CheckNewRequests() {
			Logging.ToLog("Проверка наличия новых обращений");
			timer = new Timer(5000);
			timer.Elapsed += Timer_Elapsed;
			timer.AutoReset = true;
			timer.Start();
		}

		public void Stop() {
			if (timer != null)
				timer.Stop();
		}

		private void Timer_Elapsed(object sender, ElapsedEventArgs e) {
			if (!isCompleted)
				return;

			isCompleted = false;

			try {
				string[] requests = Directory.GetFiles(requestsPath, "*.req");
				if (requests.Length == 0) {
					Logging.ToLog("Нет новых обращений (*.req)");
					isCompleted = true;
					return;
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

			isCompleted = true;
		}
	}
}
