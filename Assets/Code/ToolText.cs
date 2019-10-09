using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class ToolText : MonoBehaviour
{
	static public ToolText instance;

	public Text textField;

	void Awake()
	{
		instance = this;
	}

	public void Log( string log )
	{
		textField.text = log;
	}
}
