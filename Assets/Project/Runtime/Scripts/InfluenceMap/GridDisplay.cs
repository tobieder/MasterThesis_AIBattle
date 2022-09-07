using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface GridData
{
    int Width { get; }
    int Height { get; }
    float GetValue(int _x, int _y);
	int GetLastInfluenceValue(int _x, int _y);
}

public enum GridDisplayMode
{
	InfluenceMap,
	LastInfluenceMap
}

public class GridDisplay : MonoBehaviour
{
    MeshRenderer m_MeshRenderer;
    MeshFilter m_MeshFilter;
    Mesh m_Mesh;

	GridData m_GridData;

	[SerializeField]
	GridDisplayMode m_GridDisplayMode;

	[SerializeField]
	Material m_Material;

	[SerializeField]
	Color m_NeutralColor = Color.white;
	[SerializeField]
	Color m_PositiveColor = Color.red;
	[SerializeField]
	Color m_Positive2Color = Color.red;
	[SerializeField]
	Color m_NegativeColor = Color.blue;
	[SerializeField]
	Color m_Negative2Color = Color.blue;
	

    Color[] m_Colors;

    private void Update()
	{
		for (int yIdx = 0; yIdx < m_GridData.Height; ++yIdx)
		{
			for (int xIdx = 0; xIdx < m_GridData.Width; ++xIdx)
            {
                Color c = m_NeutralColor;

                switch (m_GridDisplayMode)
				{
					case GridDisplayMode.InfluenceMap:
                        float value = m_GridData.GetValue(xIdx, yIdx);
                        if (value > 0.5f)
                        {
                            c = Color.Lerp(m_PositiveColor, m_Positive2Color, (value - 0.5f) / 0.5f);
                        }
                        else if (value > 0)
                        {
                            c = Color.Lerp(m_NeutralColor, m_PositiveColor, value / 0.5f);
                        }
                        else if (value < -0.5f)
                        {
                            c = Color.Lerp(m_NegativeColor, m_Negative2Color, -(value + 0.5f) / 0.5f);
                        }
                        else
                        {
                            c = Color.Lerp(m_NeutralColor, m_NegativeColor, -value / 0.5f);
                        }
						break;
					case GridDisplayMode.LastInfluenceMap:
						float livValue = m_GridData.GetLastInfluenceValue(xIdx, yIdx);
						if(livValue > 0)
						{
							c = m_Positive2Color;
						}
						else if(livValue < 0)
						{
							c = m_Negative2Color;
						}
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
		m_Mesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
        m_MeshFilter = gameObject.AddComponent<MeshFilter>();
        m_MeshRenderer = gameObject.AddComponent<MeshRenderer>();

        m_MeshFilter.mesh = m_Mesh;
        m_MeshRenderer.material = m_Material;

        float objectHeight = transform.position.y;
        float startX = 0;
        float startZ = 0;

		List<Vector3> vertices = new List<Vector3>();
		for (int yIdx = 0; yIdx < m_GridData.Height; ++yIdx)
		{
			for (int xIdx = 0; xIdx < m_GridData.Width; ++xIdx)
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
		for (int yIdx = 0; yIdx < m_GridData.Height; ++yIdx)
		{
			for (int xIdx = 0; xIdx < m_GridData.Width; ++xIdx)
			{
				colors.Add(Color.white);
				colors.Add(Color.white);
				colors.Add(Color.white);
				colors.Add(Color.white);
			}
		}
		m_Colors = colors.ToArray();

		List<Vector3> normals = new List<Vector3>();
		for (int yIdx = 0; yIdx < m_GridData.Height; ++yIdx)
		{
			for (int xIdx = 0; xIdx < m_GridData.Width; ++xIdx)
			{
				normals.Add(Vector3.up);
				normals.Add(Vector3.up);
				normals.Add(Vector3.up);
				normals.Add(Vector3.up);
			}
		}

		List<Vector2> uvs = new List<Vector2>();
		for (int yIdx = 0; yIdx < m_GridData.Height; ++yIdx)
		{
			for (int xIdx = 0; xIdx < m_GridData.Width; ++xIdx)
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

	public void SetGridData(GridData _gridData)
	{
		m_GridData = _gridData;
	}

	void SetColor(int _x, int _y, Color _c)
    {
		int idx = ((_y * m_GridData.Width) + _x) * 4;
		m_Colors[idx] = _c;
		m_Colors[idx + 1] = _c;
		m_Colors[idx + 2] = _c;
		m_Colors[idx + 3] = _c;
	}

	public GridData GetGridData()
    {
		return m_GridData;
    }

}
