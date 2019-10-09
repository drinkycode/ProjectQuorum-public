using UnityEngine;
using System;
using System.Collections;

public class TestSwitchKey : MonoBehaviour
{
	public KeyCode keyCode;
	bool _toggled;

	void Update()
	{
		/*if( Input.GetKeyUp( keyCode ) )
		{
			_toggled = !_toggled;

			ToolMgr.instance.Switch( 9 ); //turn off existing tool

			//set new image
			var imgPair = UIMgr.instance.testImages[ _toggled ? ImageMgr.instance.altImageIndex : ImageMgr.instance.defaultImageIndex ];
			ImageMgr.instance.image = imgPair.photo;
			ImageMgr.instance.groundTruthTex2D = imgPair.groundTruthTex2D;
			ImageMgr.instance.groundTruthString = imgPair.groundTruthTextAsset == null ? null : imgPair.groundTruthTextAsset.text;

			StartCoroutine( Switch() ); //wait a frame and set the new tool
		}//*/
	}

	/*IEnumerator Switch()
	{
		yield return null;
		ToolMgr.instance.Switch( Array.IndexOf( ToolMgr.instance.tools, _toggled ? ToolMgr.instance.altTool : ToolMgr.instance.defaultTool ) );
	}//*/
}
