using System.Diagnostics;
using System.Collections.Generic;

namespace PCAapp {
	class log {
		public string label;
		public Stopwatch timer;
		public System.TimeSpan time;
		public log(string name) {
			timer = new Stopwatch();
			timer.Start();
			label = name;
			logging.logs.Add(this);
		}
		public void end() {
			timer.Stop();
			time = timer.Elapsed;
		}
	}
	
	static class logging {
		public static List<log> logs = new List<log>();
		public static void print() {
			foreach (log l in logs) {
				Debug.WriteLine(l.label + " : " + l.time.ToString());
			}
		}
	}
}