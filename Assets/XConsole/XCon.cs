using UnityEngine;
using UnityEditor;

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

using UnityEditorInternal;

namespace XConsole {
	public enum LOG_TYPE {
		NORMAL,
		WARNING,
		ERROR
	}
	
	[Serializable]
	public class LogData {
		public string message;
		public string fpath;
		public int fline;
		public LOG_TYPE log_type;
	}
	
	public class MatchData {
		public string patthen;
		public Color color;
	}
	
	[ExecuteInEditMode]
	public class XCon : EditorWindow {
		static XCon _inst = null;
		
		public static XCon Inst {
			get {
				if (_inst == null)
					_inst = OpenWindow();
				return _inst;
			}
		}
	
		List<LogData> logs = new List<LogData>();
		MatchData[] match_datas = null;
		
		[MenuItem("XConsole/Open %&x")]
		public static void Open() {
			if (_inst == null)
				_inst = OpenWindow();
		}
		
		static XCon OpenWindow() {
			var window = EditorWindow.GetWindow<XCon>();
			window.title = "XConsole";
			return window;
		}
		
		static void CloseWindow() {
			if (_inst != null) {
				_inst.Close();
				_inst = null;
			}
		}
		
		public void InitMathDatas(params MatchData[] match_datas) {
			this.match_datas = match_datas;
		}
		
		public void Log(LogData logdata) {
		
			logs.Add(logdata);
			
			if (this.is_scroll_down) {
				this.scroll_pos.y = Mathf.Infinity;
			}
			
			switch (logdata.log_type) {
			case LOG_TYPE.NORMAL:
				this.normal_log_cnt++;
				break;
			case LOG_TYPE.WARNING:
				this.warning_log_cnt++;
				break;
			case LOG_TYPE.ERROR:
				{
				this.error_log_cnt++;
				
				if (this.is_error_paused)
					UnityEngine.Debug.Break();
				break;
				}
			}
		}
		
		void clearLogDatas() {
			logs.Clear();
			this.normal_log_cnt = 0;
			this.warning_log_cnt = 0;
			this.error_log_cnt = 0;
		}
		
		// check logs
		int normal_log_cnt = 0;
		int warning_log_cnt = 0;
		int error_log_cnt = 0;
		
		bool is_show_normal_log = true;
		bool is_show_warning_log = true;
		bool is_show_error_log = true;
		
		List<LogData> getWannaShowLog() {
			
			List<LogData> ret = logs.ToList();
			
			if (!this.is_show_normal_log)  ret.RemoveAll(l => l.log_type == LOG_TYPE.NORMAL);
			if (!this.is_show_warning_log) ret.RemoveAll(l => l.log_type == LOG_TYPE.WARNING);
			if (!this.is_show_error_log)   ret.RemoveAll(l => l.log_type == LOG_TYPE.ERROR);
			
			if (this.filter_text != string.Empty) {
				if (this.is_regexp) {
					return ret.FindAll(c => System.Text.RegularExpressions.Regex.IsMatch(c.message, this.filter_text));
				}
				else {
					return ret.FindAll(c => c.message.Contains(this.filter_text));
				}
			}
			return ret;
		}
		
// ========================================================================================================
// Vars
//
		// color
		Color color_normal = Color.white;
		Color color_error = new Color(0.93f, 0.40f, 0.40f, 1);
		Color color_warning = new Color(0.93f, 0.93f, 0.40f, 1);
		
		// ui
		bool is_error_paused = false;
		
		// scroll
		bool is_scroll_down = false;
		Vector2 scroll_pos = new Vector2();
		
		// filter
		string filter_text = string.Empty;
		bool is_regexp = false;
		
// ========================================================================================================
// Layout
//
		
		bool isPressedSelectionCopyCmd(UnityEngine.Event evt) {
			// Ctrl + C
			return (evt.control && evt.type == EventType.keyUp && evt.keyCode == KeyCode.C);
		}
		
		bool isPressedFullCopyCmd(UnityEngine.Event evt) {
			// Ctrl + F
			return (evt.control && evt.type == EventType.keyUp && evt.keyCode == KeyCode.F);
		}
		
		bool isCloseKeyEvent(UnityEngine.Event evt) {
			// Ctrl + Q
			return (evt.control && evt.type == EventType.keyUp && evt.keyCode == KeyCode.Q);
		}
		
		void onSelectedCopy() {
//			Debug.Log("Xcon: Selected Copy");
			var text_editor = (TextEditor)GUIUtility.GetStateObject(typeof(TextEditor), GUIUtility.keyboardControl);
			EditorGUIUtility.systemCopyBuffer = text_editor.SelectedText;
		}
		
		void onFullCopy() {
//			Debug.Log("Xcon: Full Copy");
			var text_editor = (TextEditor)GUIUtility.GetStateObject(typeof(TextEditor), GUIUtility.keyboardControl);
			EditorGUIUtility.systemCopyBuffer = text_editor.content.text;
		}
		
		void onClose() {
//			Debug.Log("Xcon: Close");
			CloseWindow();
		}
		
		Color getColorFromLogType(LOG_TYPE log_type) {
			switch (log_type) {
			case LOG_TYPE.NORMAL : return this.color_normal;
			case LOG_TYPE.WARNING: return this.color_warning;
			case LOG_TYPE.ERROR  : return this.color_error;
			default              : return Color.white;
			}
		}
		
		void OnGUI() {
			/// Event Handler
			if (isPressedSelectionCopyCmd(Event.current)) onSelectedCopy();
			if (isPressedFullCopyCmd(Event.current))      onFullCopy();
			if (isCloseKeyEvent(Event.current))           onClose();
			
			/// | Clear log | Save log |
			/// | Clear on Play |Error Pause| Scroll |
			/// | normal | warn | error |
			GUILayout.BeginHorizontal(); {
				
				if (GUILayout.Button("Clear log", EditorStyles.toolbarButton)) clearLogDatas();
//				if (GUILayout.Button("Save", EditorStyles.toolbarButton)) { }
				
				GUILayout.FlexibleSpace();
				
//				if (GUILayout.Toggle(true, "Clear on Play", EditorStyles.toolbarButton)) { }
				
				this.is_error_paused = GUILayout.Toggle(this.is_error_paused, "Error Pause", EditorStyles.toolbarButton);
				this.is_scroll_down = GUILayout.Toggle(this.is_scroll_down, "Scroll", EditorStyles.toolbarButton);
				
				GUILayout.FlexibleSpace();
				
				
				GUI.color = Color.white;
				this.is_show_normal_log  = GUILayout.Toggle(this.is_show_normal_log, "N: " + this.normal_log_cnt, EditorStyles.toolbarButton);
				
				GUI.color = Color.yellow;
				this.is_show_warning_log = GUILayout.Toggle(this.is_show_warning_log, "W: " + this.warning_log_cnt, EditorStyles.toolbarButton);
				
				GUI.color = Color.red;
				this.is_show_error_log = GUILayout.Toggle(this.is_show_error_log, "E: " + this.error_log_cnt, EditorStyles.toolbarButton);
				
				GUI.color = Color.white;
				
			} GUILayout.EndHorizontal();
			
			
			/// logs
			GUILayout.BeginVertical(); {
				
				this.scroll_pos = GUILayout.BeginScrollView(this.scroll_pos, GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(true)); {
					
					var show_log = getWannaShowLog();
					
					for (int i = 0; i < show_log.Count; ++i) {
						
						GUILayout.BeginHorizontal(); {
							
							var log = show_log[i];
							
							if (match_datas == null) {
								GUI.color = getColorFromLogType(log.log_type);
							}
							else {
								foreach (MatchData md in match_datas) {
									if (System.Text.RegularExpressions.Regex.IsMatch(log.message, md.patthen)) {
										GUI.color = md.color;
										break;
									}
								}
							}
							
							GUILayout.TextArea(log.message, EditorStyles.textField, GUILayout.ExpandWidth(true));
							
							GUI.color = Color.white;
							
							if (GUILayout.Button("open", GUILayout.MaxWidth(45))) {
								InternalEditorUtility.OpenFileAtLineExternal(log.fpath, log.fline);
							}
							
						} GUILayout.EndHorizontal();
					}
					
				} GUILayout.EndScrollView();
				
			} GUILayout.EndVertical();

			
			/// [ clear ][ regexp ][ &search ]
			GUI.color = Color.white;			
			GUI.backgroundColor = Color.white;
			
			GUILayout.BeginHorizontal(GUILayout.MinHeight(20)); {
				
				if (GUILayout.Button("Clear", EditorStyles.miniButtonLeft)) {
					this.filter_text = string.Empty;
				}
				
				this.is_regexp = GUILayout.Toggle(this.is_regexp, "regexp", EditorStyles.miniButtonRight);
				this.filter_text = GUILayout.TextField(this.filter_text, GUI.skin.FindStyle("ToolbarSeachTextField"), GUILayout.MaxWidth(float.MaxValue));
				
			} GUILayout.EndHorizontal();
		}
	}
}