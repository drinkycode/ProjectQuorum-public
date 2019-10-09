using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class ImageButton : MonoBehaviour
{
	Texture2D _image;
	Texture2D _groundTruthTex2D;
	TextAsset _groundTruthTextAsset;

	/*public void OnClick()
	{
		ImageMgr.instance.image = _image;
		ImageMgr.instance.groundTruthTex2D = _groundTruthTex2D;
		ImageMgr.instance.groundTruthString = _groundTruthTextAsset == null ? null : _groundTruthTextAsset.text;
	}

	public void SetImage( Texture2D image, Texture2D groundTruthTex2D, TextAsset groundTruthTextAsset )
	{
		_image = image;
		_groundTruthTex2D = groundTruthTex2D;
		_groundTruthTextAsset = groundTruthTextAsset;

		GetComponent<RawImage>().texture = _image;
	}//*/
}
