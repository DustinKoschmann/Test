using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class WorldScript : MonoBehaviour {

	public int Chunks;

	void Start() {
		float posX = -10;
		float posY = 5;
		float newPosX = 0;
		float newPosY = 5;
		System.Random rnd = new System.Random();

		for(int i = 0; i < Chunks; i++) {
			newPosX += rnd.Next(15, 50);
			var rndY = rnd.Next(-2, 3);
			if((newPosY+rndY) > 0) {
				newPosY += rndY;
			}

			CreateChildMeshes(posX, posY, newPosX, newPosY).transform.parent = this.gameObject.transform;
			posX = newPosX;
			posY = newPosY;
		}

		gameObject.layer = 10;

		//MeshFilter[] meshFilters = GetComponentsInChildren<MeshFilter>();
		//CombineInstance[] combine = new CombineInstance[meshFilters.Length];
		//
		//int w = 0;
		//while(w < meshFilters.Length) {
		//    combine[w].mesh = meshFilters[w].sharedMesh;
		//    combine[w].transform = meshFilters[w].transform.localToWorldMatrix;
		//    meshFilters[w].gameObject.SetActive(false);
		//    w++;
		//}
		//
		//var mf = this.gameObject.transform.GetComponent<MeshFilter>();
		//mf.mesh = new Mesh();
		//mf.mesh.CombineMeshes(combine);
		//mf.mesh.RecalculateBounds();
		//mf.mesh.RecalculateNormals();
		//this.gameObject.transform.gameObject.SetActive(true);

	}

	GameObject CreateChildMeshes(float posX, float posY, float newPosX, float newPosY) {
		GameObject gameObj = new GameObject();
		float thickness = 5;
		Material newMat = Resources.Load("GroundMat", typeof(Material)) as Material;
		PhysicMaterial newPhysMat = Resources.Load("GroundPhysMat", typeof(PhysicMaterial)) as PhysicMaterial;

		var meshCollider = gameObj.AddComponent<MeshCollider>();
		var meshFilter = gameObj.AddComponent<MeshFilter>();
		var meshRenderer = gameObj.AddComponent<MeshRenderer>();

		meshCollider.convex = true;

		meshCollider.sharedMesh = CreateMesh(posX, posY, newPosX, newPosY, thickness);
		meshFilter.mesh = meshCollider.sharedMesh;
		meshRenderer.material = newMat;
		meshCollider.material = newPhysMat;

		gameObj.tag = "Ground";

		return gameObj;
	}

	Mesh CreateMesh(float posX, float posY, float newPosX, float newPosY, float thickness) {
		float t = thickness;

		//MeshRenderer meshRenderer = GetComponent<MeshRenderer>();
		Mesh m = new Mesh();

		m.vertices = new Vector3[] {
			//Back Verts
			new Vector3(posX, 0, -t),           //BotL
			new Vector3(newPosX, 0, -t),        //BotR
			new Vector3(newPosX, newPosY, -t),  //TopR
			new Vector3(posX, posY, -t),        //TopL

			//Front Verts
			new Vector3(posX, 0, t),            //BotL
			new Vector3(newPosX, 0, t),         //BotR
			new Vector3(newPosX, newPosY, t),   //TopR
			new Vector3(posX, posY, t),         //TopL
		};

		m.triangles = new int[] {
			//Front
			0, 2, 1,
			0, 3, 2,

			//Back
			4, 5, 6,
			4, 6, 7,

			//Left
			//0, 4, 7,
			//0, 7, 3,

			//Top
			3, 7, 6,
			3, 6, 2,

			//Right
			//6, 1, 2,
			//6, 5, 1,

			//Bottom
			//5, 0, 1,
			//5, 4, 0
		};
		m.RecalculateNormals();
		return m;
	}
}

