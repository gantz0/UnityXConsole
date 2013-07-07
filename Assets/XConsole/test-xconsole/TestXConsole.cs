using UnityEngine;
using System.Collections;

public class TestXConsole : MonoBehaviour {

	void Start () {
//		XConsole.XCon.Inst.InitMathDatas(
//			new XConsole.MatchData{patthen= "LogNormal", color= Color.green}
//		);
//		
		DDD.LogNormal("DDD.LogNormal");
		DDD.LogWarning("DDD.LogWarning");
		DDD.LogError("DDD.LogError");
	}
}
