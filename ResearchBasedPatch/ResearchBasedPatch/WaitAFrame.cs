using System.Collections;
using UnityEngine;

class WaitAFrame : MonoBehaviour
{
	public void Initialize (ResearchBasedPatch modClass)
	{
		StartCoroutine(WaitTheFrame(modClass));
	}

	IEnumerator WaitTheFrame (ResearchBasedPatch modClass)
	{
		yield return null;
		modClass.ForceLoadLast();
		Destroy(gameObject);
	}
}
