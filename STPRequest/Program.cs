using System;
using System.Collections.Generic;
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
		private static EventSystem eventSystem;
		private static Thread thread;

		static void Main() {
			if (Environment.UserInteractive) {
				Start();

				Console.WriteLine("Press any key to stop...");
				Console.ReadKey(true);

				Stop();
			} else
				using (Service service = new Service())
					ServiceBase.Run(service);
		}

		public class Service : ServiceBase {
			public Service() { }
			protected override void OnStart(string[] args) { Start(); }
			protected override void OnStop() { Stop(); }
		}

		private static void Start() {
			Logging.ToLog("----- Запуск");

			eventSystem = new EventSystem();
			thread = new Thread(eventSystem.CheckNewRequests) { IsBackground = true };
			thread.Start();
		}

		private static void Stop() {
			Logging.ToLog("----- Завершение");

			eventSystem.Stop();
			thread.Abort();
		}
	}
}
