using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum VisionGridArea { 
	Neutral,
	Team1, 
	Team2, 
	Team1_Agent, 
	Team2_Agent 
}

public interface VisionGridData
{
	int Width { get; }
	int Height { get; }
	VisionGridArea GetValue(int _x, int _y);
}

public class VisionGridDisplay : MonoBehaviour
{
	MeshRenderer m_MeshRenderer;
	MeshFilter m_MeshFilter;
	Mesh m_Mesh;

	VisionGridData m_VisionGridData;

	[SerializeField]
	Material m_Material;

	[System.Serializable]
	struct MapColor
	{
		public VisionGridArea visionGridArea;
		public Color color;
	}
	[SerializeField]
	MapColor[] m_MapColor;

	Dictionary<VisionGridArea, Color> m_ColorDictionary;


	Color[] m_Colors;

	private void Start()
	{
		m_ColorDictionary = new Dictionary<VisionGridArea, Color>();

		foreach (MapColor mapColor in m_MapColor)
		{
			m_ColorDictionary.Add(mapColor.visionGridArea, mapColor.color);
		}
	}

	private void Update()
	{
		for (int yIdx = 0; yIdx < m_VisionGridData.Height; ++yIdx)
		{
			for (int xIdx = 0; xIdx < m_VisionGridData.Width; ++xIdx)
			{
				Color c;

				switch(m_VisionGridData.GetValue(xIdx, yIdx))
                {
					case VisionGridArea.Team1: 
						c = m_ColorDictionary[VisionGridArea.Team1];
						break;
					case VisionGridArea.Team2:
						c = m_ColorDictionary[VisionGridArea.Team2];
						break;
					case VisionGridArea.Team1_Agent:
						c = m_ColorDictionary[VisionGridArea.Team1_Agent];
						break;
					case VisionGridArea.Team2_Agent:
						c = m_ColorDictionary[VisionGridArea.Team2_Agent];
						break;
					default:
						c = m_ColorDictionary[VisionGridArea.Neutral];
						break;
				}

				SetColor(xIdx, yIdx, c);
			}
		}

		m_Mesh.colors = m_Colors;
	}

	public void CreateMesh(Vector3 _bottomLeftPos, float _gridSize)
	{
		m_Mesh = new Mesh();
		m_Mesh.name = name;
		m_MeshFilter = gameObject.AddComponent<MeshFilter>();
		m_MeshRenderer = gameObject.AddComponent<MeshRenderer>();

		m_MeshFilter.mesh = m_Mesh;
		m_MeshRenderer.material = m_Material;

		float objectHeight = transform.position.y;
		float startX = 0;
		float startZ = 0;

		List<Vector3> vertices = new List<Vector3>();
		for (int yIdx = 0; yIdx < m_VisionGridData.Height; ++yIdx)
		{
			for (int xIdx = 0; xIdx < m_VisionGridData.Width; ++xIdx)
			{
				//int idx = (yIdx * m_GridData.Width) + xIdx;

				Vector3 bl = new Vector3(startX + (xIdx * _gridSize), objectHeight, startZ + (yIdx * _gridSize));
				Vector3 br = new Vector3(startX + ((xIdx + 1) * _gridSize), objectHeight, startZ + (yIdx * _gridSize));
				Vector3 tl = new Vector3(startX + (xIdx * _gridSize), objectHeight, startZ + ((yIdx + 1) * _gridSize));
				Vector3 tr = new Vector3(startX + ((xIdx + 1) * _gridSize), objectHeight, startZ + ((yIdx + 1) * _gridSize));

				//Debug.Log("verts: " + xIdx + ", " + yIdx + ": " + idx + " " + bl.ToString("0.000"));

				vertices.Add(bl);
				vertices.Add(br);
				vertices.Add(tl);
				vertices.Add(tr);
			}
		}
		//Debug.Log("verts: " + vertices.Count);

		List<Color> colors = new List<Color>();
		for (int yIdx = 0; yIdx < m_VisionGridData.Height; ++yIdx)
		{
			for (int xIdx = 0; xIdx < m_VisionGridData.Width; ++xIdx)
			{
				colors.Add(Color.white);
				colors.Add(Color.white);
				colors.Add(Color.white);
				colors.Add(Color.white);
			}
		}
		m_Colors = colors.ToArray();

		List<Vector3> normals = new List<Vector3>();
		for (int yIdx = 0; yIdx < m_VisionGridData.Height; ++yIdx)
		{
			for (int xIdx = 0; xIdx < m_VisionGridData.Width; ++xIdx)
			{
				normals.Add(Vector3.up);
				normals.Add(Vector3.up);
				normals.Add(Vector3.up);
				normals.Add(Vector3.up);
			}
		}

		List<Vector2> uvs = new List<Vector2>();
		for (int yIdx = 0; yIdx < m_VisionGridData.Height; ++yIdx)
		{
			for (int xIdx = 0; xIdx < m_VisionGridData.Width; ++xIdx)
			{
				uvs.Add(new Vector2(0, 0));
				uvs.Add(new Vector2(1, 0));
				uvs.Add(new Vector2(0, 1));
				uvs.Add(new Vector2(1, 1));
			}
		}

		List<int> triangles = new List<int>();
		for (int idx = 0; idx < vertices.Count; idx += 4)
		{

			//int idx = (yIdx * m_GridData.Width) + xIdx;

			int bl = idx;
			int br = idx + 1;
			int tl = idx + 2;
			int tr = idx + 3;

			//Debug.Log(bl + ", " + br + ", " + tl + ", " + tr);

			triangles.Add(bl);
			triangles.Add(tl);
			triangles.Add(br);

			triangles.Add(tl);
			triangles.Add(tr);
			triangles.Add(br);

		}
		//Debug.Log("tris: " + triangles.Count);

		m_Mesh.vertices = vertices.ToArray();
		m_Mesh.normals = normals.ToArray();
		m_Mesh.uv = uvs.ToArray();
		m_Mesh.colors = m_Colors;
		m_Mesh.triangles = triangles.ToArray();
	}

	public void SetGridData(VisionGridData _gridData)
	{
		m_VisionGridData = _gridData;
	}

	void SetColor(int _x, int _y, Color _c)
	{
		int idx = ((_y * m_VisionGridData.Width) + _x) * 4;
		m_Colors[idx] = _c;
		m_Colors[idx + 1] = _c;
		m_Colors[idx + 2] = _c;
		m_Colors[idx + 3] = _c;
	}
}
