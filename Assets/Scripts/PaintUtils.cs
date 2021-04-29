using System.Collections.Generic;
using UnityEngine;

public static class PaintUtils
{
	public static IEnumerable<float> DrawLine(Vector2 start, Vector2 end, float stepLength)
	{
		float dist = Vector2.Distance(start, end);
		int stepCount = Mathf.FloorToInt(dist / stepLength);
		float position = 0f;
    
		yield return position;
        	
		for (int i = 1; i < stepCount; i++)
		{
			yield return position;
			position = (float)i / stepCount;
		}
    
		// Leave last point to avoid overlapping:
		//yield return 1f;
	}

	public static Vector2 PointOnQuadraticCurve(Vector2 start, Vector2 control, Vector2 end, float a) {
		float f1 = (1 - a) * (1 - a);
		float f2 = 2 * a * (1 - a);
		float f3 = a * a;
            
		return start * f1 + control * f2 + end * f3;
	}
}