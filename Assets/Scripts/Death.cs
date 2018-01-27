using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Death : MonoBehaviour {

	[SerializeField]
	private GameObject corpsePrefab;

	private void Die()
	{
		GameObject newCorpse = Instantiate(corpsePrefab, transform.position, transform.rotation);

		// TODO: Make the corpse use an enemy corpse sprite.

		Destroy(gameObject);
	}

}
