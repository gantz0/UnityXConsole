using UnityEngine;
using System.Collections;

using System.Diagnostics;

using XConsole;

public class DDD {
	public static void LogNormal(string message) {
		_Log(message, LOG_TYPE.NORMAL);
	}
	
	public static void LogWarning(string message) {
		_Log(message, LOG_TYPE.WARNING);
	}
	
	public static void LogError(string message) {
		_Log(message, LOG_TYPE.ERROR);
	}
	
	static void _Log(string message, LOG_TYPE log_type) {
		var st = new StackTrace(true);
		var sf = st.GetFrame(2);
		
		var fpath = sf.GetFileName();
		var fline = sf.GetFileLineNumber();
		
		XCon.Inst.Log(new LogData{message= message, fpath= fpath, fline= fline, log_type= log_type});
	}
}
