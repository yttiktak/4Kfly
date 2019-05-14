using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Rendering;

public class BB {

	// Bobbi Jane aka Roberta Bennett utilities for hex array work.
	// version 1.01 prior version 1.0
	//  added uv3 to the mesh maker, which should report the position of each point in the array
	// to the fragment shader, where lens correction algoritms can use it to, for instance, shift the
	// image in a cell based on its position in the array.
	// First intent for that is to have a playback lens emulator, which will need to know how a viewer
	// is seeing the magnified image, eg, should shift a bit based on viewers direction.
	//

	public Vector3[] directions; 

	public BB ( ) {
	}

	public void MakeHexMesh( Vector3[] translations, 
		ref Mesh theMesh, 
		float textureScale = 0.0f,
		bool pointyPartUp = true
	) {

		Vector3[] oneCellVerts = new Vector3[7];
		oneCellVerts[0] = new Vector3( 0.0f, 0.0f, 0.0f);
		oneCellVerts[1] = new Vector3( 0.0f, -1.0f, 0.0f);
		oneCellVerts[2] = new Vector3( 0.8667f, -0.5f, 0.0f);
		oneCellVerts[3] = new Vector3( 0.8667f, 0.5f, 0.0f);
		oneCellVerts[4] = new Vector3( 0.0f, 1.0f, 0.0f);
		oneCellVerts[5] = new Vector3( -0.8667f, 0.5f, 0.0f);
		oneCellVerts[6] = new Vector3( -0.8667f, -0.5f, 0.0f);

		Vector2[] oneCellUV = new Vector2[7];
		oneCellUV[0] = new Vector2( 0.5f, 0.5f);
		oneCellUV[1] = new Vector2( 0.5f, 0.0f);
		oneCellUV[2] = new Vector2( 0.93333f, 0.250f);
		oneCellUV[3] = new Vector2( 0.93333f, 0.75f);
		oneCellUV[4] = new Vector2( 0.5f, 1.0f);
		oneCellUV[5] = new Vector2( 0.0333f, 0.75f);
		oneCellUV[6] = new Vector2( 0.0333f, 0.25f);

		// I think these rotate in the wrong direction
		Vector3[] oneCellVertsFlatTop = new Vector3[7]; // ??? ok, pointy end to side for flat top.
		oneCellVertsFlatTop[0] = new Vector3( 0.0f, 0.0f, 0.0f);
		oneCellVertsFlatTop[1] = new Vector3( -1.0f, 0.0f, 0.0f);
		oneCellVertsFlatTop[2] = new Vector3( -0.5f, 0.8667f, 0.0f);
		oneCellVertsFlatTop[3] = new Vector3( 0.5f, 0.8667f, 0.0f);
		oneCellVertsFlatTop[4] = new Vector3( 1.0f, 0.0f, 0.0f);
		oneCellVertsFlatTop[5] = new Vector3( 0.5f, -0.8667f, 0.0f);
		oneCellVertsFlatTop[6] = new Vector3( -0.5f, -0.8667f, 0.0f);

		Vector2[] oneCellUVFlatTop = new Vector2[7]; // and swap texture u v, right??
		oneCellUVFlatTop[0] = new Vector2( 0.5f, 0.5f);
		oneCellUVFlatTop[1] = new Vector2(0.0f,  0.5f);
		oneCellUVFlatTop[2] = new Vector2( 0.25f, 0.93333f);
		oneCellUVFlatTop[3] = new Vector2( 0.75f, 0.93333f);
		oneCellUVFlatTop[4] = new Vector2( 1.0f, 0.5f);
		oneCellUVFlatTop[5] = new Vector2( 0.75f, 0.0333f);
		oneCellUVFlatTop[6] = new Vector2( 0.25f, 0.0333f);

		int[] oneTris = {0,1,2,  0,2,3,  0,3,4,   0,4,5,  0,5,6,  0,6,1};
		// odd. The obj format has them start at 1, not zero

		if (theMesh == null) {
			theMesh = new Mesh ();
		}

		Vector3 sideStep = new Vector3(-100.0f,0f,0f);
		int nTot = translations.Length;	
		if (nTot > 1) 
			sideStep = translations [1] - translations [0];
		if (sideStep.x == 0) {
			pointyPartUp = false;
		}

		float spacing = sideStep.magnitude;
		float scale = spacing / Mathf.Sqrt (3.0f);

		theMesh.Clear ();

		Vector3[] myVerts = new Vector3[nTot * 7];
		Vector2[] myUV = new Vector2[nTot * 7];
		Vector2[] myUV2 = new Vector2[nTot * 7]; 
		Vector2[] myUV3 = new Vector2[nTot * 7];
		Vector2[] myUV4 = new Vector2[nTot * 7];	// note center point
		int[] myTris = new int[nTot * 6 * 3];
		Rect bounds = new Rect(translations[0].x,translations[0].y,translations[0].x,translations[0].y);
		foreach (Vector3 tx in translations) {
			if (tx.x < bounds.x)
				bounds.x = tx.x;
			if (tx.y < bounds.y)
				bounds.y = tx.y;
			if (tx.x > bounds.width)
				bounds.width = tx.x;
			if (tx.y > bounds.height)
				bounds.height = tx.y;
		}
		bounds.width -= bounds.x;
		bounds.height -= bounds.y;
		bounds.x += bounds.width * 0.5f; // re-use as centers instead.
		bounds.y += bounds.height * 0.5f;
		float extent = Mathf.Max (bounds.width, bounds.height)+spacing; 
		float textureSpacing = spacing / extent;

		for (int ofn = 0; ofn < translations.Length; ofn++) {
			for (int vn = 0; vn < 7; vn++) {
				// the verticis
				myVerts [ofn * 7 + vn] = translations [ofn] + oneCellVerts [vn] * scale; // not rotating onCell yet
				// the texture. 
				// textureScale = 0 flags to use one full texture per cell
				if (textureScale == 0) {
					myUV [ofn * 7 + vn] = oneCellUV [vn];
				} else { 
					// Note the - for translation, to flip pov. 
					// this for instanced render of subject moved to each position
					myUV [ofn * 7 + vn] = oneCellUV [0] + (oneCellUV [vn] - oneCellUV [0]) * textureScale * textureSpacing; 
					myUV [ofn * 7 + vn].x -= (translations [ofn].x - bounds.x) / extent;
					myUV [ofn * 7 + vn].y -= (translations [ofn].y - bounds.y) / extent;
				}

			//	myUV4 [ofn * 7 + vn] = oneCellUV [0];// + (oneCellUV [vn] - oneCellUV [0]) * textureSpacing; //uv4 gives center of cell in full texture space
				myUV4 [ofn * 7 + vn].x += (translations [ofn].x - bounds.x) / extent;
				myUV4 [ofn * 7 + vn].y += (translations [ofn].y - bounds.y) / extent;

				myUV3 [ofn * 7 + vn] = oneCellUV [0] + (oneCellUV [vn] - oneCellUV [0]) * textureSpacing;
				myUV3 [ofn * 7 + vn].x += (translations [ofn].x - bounds.x) / extent;
				myUV3 [ofn * 7 + vn].y += (translations [ofn].y - bounds.y) / extent;

				myUV2 [ofn * 7 + vn].x = ofn * 1.0f; // mis-use uv2 for slice level of a texture2dArray
			}
			for (int trin = 0; trin < 18; trin++) {
				myTris [ofn * 18 + trin] = oneTris [trin] + 7 * ofn;
			} 
		}
		if (!pointyPartUp) {
			for (int ofn = 0; ofn < translations.Length; ofn++) {
				for (int vn = 0; vn < 7; vn++) {
					myVerts [ofn * 7 + vn] = translations [ofn] + oneCellVertsFlatTop [vn] * scale;
					if (textureScale == 0) {
						myUV [ofn * 7 + vn] = oneCellUVFlatTop [vn];
					} else { 
						myUV [ofn * 7 + vn] = oneCellUVFlatTop [0] + (oneCellUVFlatTop [vn] - oneCellUVFlatTop [0]) * textureScale * textureSpacing; 
						myUV [ofn * 7 + vn].x -= (translations [ofn].x - bounds.x) / extent;
						myUV [ofn * 7 + vn].y -= (translations [ofn].y - bounds.y) / extent;
					}

				//	myUV4 [ofn * 7 + vn] = oneCellUVFlatTop [0];// + (oneCellUV [vn] - oneCellUV [0]) * textureSpacing;
					myUV4 [ofn * 7 + vn].x += (translations [ofn].x - bounds.x) / extent;
					myUV4 [ofn * 7 + vn].y += (translations [ofn].y - bounds.y) / extent;

					myUV3 [ofn * 7 + vn] = oneCellUVFlatTop [0] + (oneCellUVFlatTop [vn] - oneCellUVFlatTop [0]) * textureSpacing;
					// some kind of flip mix up. Try not subtracting, but adding here:
					myUV3 [ofn * 7 + vn].x += (translations [ofn].x - bounds.x) / extent;
					myUV3 [ofn * 7 + vn].y += (translations [ofn].y - bounds.y) / extent;

					myUV2 [ofn * 7 + vn].x = ofn * 1.0f; // mis-use uv2 for slice level of a texture2dArray
				}
				for (int trin = 0; trin < 18; trin++) {
					myTris [ofn * 18 + trin] = oneTris [trin] + 7 * ofn;
				} 
			}
		}
      //  theMesh.indexFormat = IndexFormat.UInt32; not the limiting factor though.
		theMesh.vertices = myVerts;
		theMesh.uv = myUV;
		theMesh.uv2 = myUV2;
		theMesh.uv3 = myUV3;
		theMesh.uv4 = myUV4;
		theMesh.triangles = myTris;
		theMesh.UploadMeshData (false);
	}

	public void MakeCameraIconHexMesh( Vector3[] translations, ref Mesh theMesh) {
		Vector3[] oneCellVerts = new Vector3[7];
		oneCellVerts[0] = new Vector3( 0.0f, 0.0f, 0.0f);
		oneCellVerts[1] = new Vector3( 0.0f, -1.0f, 0.0f);
		oneCellVerts[2] = new Vector3( 0.8667f, -0.5f, 0.0f);
		oneCellVerts[3] = new Vector3( 0.8667f, 0.5f, 0.0f);
		oneCellVerts[4] = new Vector3( 0.0f, 1.0f, 0.0f);
		oneCellVerts[5] = new Vector3( -0.8667f, 0.5f, 0.0f);
		oneCellVerts[6] = new Vector3( -0.8667f, -0.5f, 0.0f);

		Vector2[] oneCellUV = new Vector2[7];
		oneCellUV[0] = new Vector2( 0.5f, 0.5f);
		oneCellUV[1] = new Vector2( 0.5f, 0.0f);
		oneCellUV[2] = new Vector2( 0.93333f, 0.250f);
		oneCellUV[3] = new Vector2( 0.93333f, 0.75f);
		oneCellUV[4] = new Vector2( 0.5f, 1.0f);
		oneCellUV[5] = new Vector2( 0.0333f, 0.75f);
		oneCellUV[6] = new Vector2( 0.0333f, 0.25f);

		int[] oneTris = {0,1,2,  0,2,3,  0,3,4,   0,4,5,  0,5,6,  0,6,1};

		if (theMesh == null) {
			theMesh = new Mesh ();
		}
		theMesh.Clear ();

		int nTot = translations.Length;	
		float scale = 1.0f / Mathf.Sqrt (3.0f);

		Vector3[] myVerts = new Vector3[nTot * 7];
		Vector2[] myUV = new Vector2[nTot * 7];
		Vector2[] myUV2 = new Vector2[nTot * 7];
		int[] myTris = new int[nTot * 6 * 3]; // make em two sided, just draw second batch of tris ccw

		for (int ofn = 0; ofn < translations.Length; ofn++) {
			for (int vn = 0; vn < 7; vn++) {
				// the verticis
				myVerts [ofn * 7 + vn] = translations [ofn] + oneCellVerts [vn] * scale; 
				if (vn == 0) { // make center a bump
					myVerts [ofn * 7 + vn].z += scale * 0.5f;
				}
				myUV [ofn * 7 + vn] = oneCellUV [vn];
				myUV2 [ofn * 7 + vn].x = ofn * 1.0f; // mis-use uv2 for slice level of a texture2dArray if desired
			}
			for (int trin = 0; trin < 18; trin++) {
				myTris [ofn * 18 + trin] = oneTris [trin] + 7 * ofn;
			} 
		}
		theMesh.vertices = myVerts;
		theMesh.uv = myUV;
		theMesh.uv2 = myUV2;
		theMesh.triangles = myTris;
		theMesh.UploadMeshData (false);
	}

	// overload?? whats that?
	public int MakeTranslations( float spacing, int nWide, int nTall, ref Vector3[] translations) {
		int[] toss ={};
		return MakeTranslationsIndexed (spacing, nWide, nTall, ref translations, ref toss);
	}
// pointy part up makes wrong rotation triangles!!
	public int MakeTranslationsIndexed( float spacing, int nWide, int nTall, ref Vector3[] translations, ref int[] index) {
		translations = null; // !!! check if already allocated, and dispose of it!!! 

		int[] zigzag = { 0, 3, 5, 1, 3, 6, 1, 4, 6, 2, 4, 0, 2, 5 }; // ih even wide
	//	int[] zigzag = { 0, 2, 5, 0, 3, 5, 1, 3, 6, 1, 4, 6, 2, 4 }; // ih odd wide

		if (nWide % 2 == 0 ) 
			nWide = nWide + 1;
		if (nTall % 2 == 0)
			nTall = nTall + 1;

		int ihmid = (nTall - 1) / 2;
		int iwmid = (nWide - 1) / 2;

		int nTotal = nTall * (nWide + 1); // ntx * (nWide + 1) + ntn * nWide;
		translations = new Vector3[nTotal];
		index = new int[nTotal];

		bool pointyPartUp = true;
		if (spacing < 0) {
			pointyPartUp = false;
			spacing = Mathf.Abs (spacing);
		}

		Vector3 sideTranslateStep;
		Vector3 upTranslateStep; 
		Vector3 noTranslation; 
		if (pointyPartUp) {
			sideTranslateStep = new Vector3 (-1.0f * spacing, 0.0f, 0.0f);
			upTranslateStep = new Vector3 (0.0f, -1.0f * spacing * 0.5f * Mathf.Sqrt (3.0f), 0.0f);
			noTranslation = new Vector3 (0f, 0f, 0f);
		} else {
			sideTranslateStep = new Vector3 (0.0f, -1.0f * spacing,  0.0f);
			upTranslateStep = new Vector3 (-1.0f * spacing * 0.5f * Mathf.Sqrt (3.0f), 0.0f, 0.0f);
			noTranslation = new Vector3 (0f, 0f, 0f);
		}

		directions = new Vector3[6];
		directions [0] = sideTranslateStep;
		directions [1] = 0.5f * sideTranslateStep + upTranslateStep;
		directions [2] = -0.5f * sideTranslateStep + upTranslateStep;
		directions [3] = -1.0f * sideTranslateStep;
		directions [4] = -0.5f * sideTranslateStep - upTranslateStep;
		directions [5] =  0.5f * sideTranslateStep - upTranslateStep;

		int ins = 0;
		for (int ih = 0; ih < nTall; ih++) {
			int nWideHere = nWide + (ih-ihmid+nTall-1) % 2;
			for (int iw = 0; iw < nWideHere; iw++) {
				translations [ins] = noTranslation;
				if ((ih-ihmid) % 2 == 0) {// nWide, so start normal
					translations [ins] +=  1.0f * (iw-iwmid) * sideTranslateStep;
				} else {				// nWide+1, so shift left half step
					translations [ins] +=  1.0f * (iw-iwmid - 0.5f) * sideTranslateStep;
				}
				translations [ins] += (ih-ihmid) * 1.0f * upTranslateStep;
				index [ins] = (zigzag [ih % 14] + iw) % 7;
				ins += 1;
			}
		}
		/**
		if !pointyPartUp, sideStep.x = 0. So translations[1].x-translations[0].x = 0
		 */
		return nTotal;
	}


}
