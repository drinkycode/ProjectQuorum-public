using UnityEngine;
using System.Collections;

[RequireComponent(typeof(MeshRenderer))]
public class AssignImageFromMgr : MonoBehaviour
{
	void Update()
	{
		//if( ImageMgr.instance.dirty ) { AssignImage(); }
	}

	/*void AssignImage()
	{
		var imgMgr = ImageMgr.instance;
		if( imgMgr == null ) { return; }

		transform.parent.localScale = imgMgr.GetOneScale(); //scale parent instead to autoscale everythin else

		var renderer = GetComponent<MeshRenderer>();
		renderer.material.SetTexture( "_MainTex", imgMgr.image );

		var panMgr = Camera.main.GetComponent<PanController>();
		if( panMgr != null )
		{
			panMgr.panSlider.value = 0;

			var screenRatio = ((float)Screen.width) / ((float)Screen.height);
			var imgRatio = ((float)imgMgr.image.width) / ((float)imgMgr.image.height);
			panMgr.range = ( transform.lossyScale.x - screenRatio ) * 0.5f / imgRatio;
		}
	}//*/
}
