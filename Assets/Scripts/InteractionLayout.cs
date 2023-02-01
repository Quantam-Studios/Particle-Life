using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class InteractionLayout 
{
	[System.Serializable]
	public struct rowData
	{
		public float[] row;
	}

	public rowData[] rows = new rowData[5];
}
