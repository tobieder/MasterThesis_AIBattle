using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class SaveLoadArray
{
    public void SaveInfluenceMap(string aFileName, float[,] _influenceMap)
    {
        int fileVersion = 1;
        using (var fileStream = System.IO.File.OpenWrite(aFileName))
        using (var writer = new System.IO.BinaryWriter(fileStream))
        {
            writer.Write("InfuenceMapFile");
            writer.Write(fileVersion);
            writer.Write(_influenceMap.GetLength(0));
            writer.Write(_influenceMap.GetLength(1));
            //writer.Write(regionsize);
            /*
            for (int regionX = 0; regionX < regionMapSize.x; regionX++)
            {
                for (int regionY = 0; regionY < regionMapSize.y; regionY++)
                {
                    for (int tilemapX = 0; tilemapX < regionsize; tilemapX++)
                    {
                        for (int tilemapY = 0; tilemapY < regionsize; tilemapY++)
                        {
                            float value = tilemapRegionValues[regionX, regionY, tilemapX, tilemapY];
                            writer.Write(value);
                        }
                    }
                }
            }
            */

            for(int x = 0; x < _influenceMap.GetLength(0); x++)
            {
                for(int y = 0; y < _influenceMap.GetLength(1); y++)
                {
                    float value = _influenceMap[x, y];
                    writer.Write(value);
                }
            }
        }
    }
    public void SaveLastInfluenceMap(string aFileName, int[,] _influenceMap)
    {
        int fileVersion = 1;
        using (var fileStream = System.IO.File.OpenWrite(aFileName))
        using (var writer = new System.IO.BinaryWriter(fileStream))
        {
            writer.Write("InfuenceMapFile");
            writer.Write(fileVersion);
            writer.Write(_influenceMap.GetLength(0));
            writer.Write(_influenceMap.GetLength(1));
            //writer.Write(regionsize);
            /*
            for (int regionX = 0; regionX < regionMapSize.x; regionX++)
            {
                for (int regionY = 0; regionY < regionMapSize.y; regionY++)
                {
                    for (int tilemapX = 0; tilemapX < regionsize; tilemapX++)
                    {
                        for (int tilemapY = 0; tilemapY < regionsize; tilemapY++)
                        {
                            float value = tilemapRegionValues[regionX, regionY, tilemapX, tilemapY];
                            writer.Write(value);
                        }
                    }
                }
            }
            */

            for (int x = 0; x < _influenceMap.GetLength(0); x++)
            {
                for (int y = 0; y < _influenceMap.GetLength(1); y++)
                {
                    float value = _influenceMap[x, y];
                    writer.Write(value);
                }
            }
        }
    }
    public float[,] LoadInfluenceMap(string aFileName)
    {
        float[,] influenceMap = null;

        try
        {
            using (var fileStream = System.IO.File.OpenRead(aFileName))
            using (var reader = new System.IO.BinaryReader(fileStream))
            {
                var magic = reader.ReadString();
                // check your file magic to identify your file, so you can be sure
                // you access the right file
                if (magic != "InfuenceMapFile")
                    throw new System.Exception("Wrong file format");
                // check your file version in order to be future proof
                var version = reader.ReadInt32();
                if (version != 1)
                    throw new System.Exception("Not supported file version");
                // read our own 
                int sizeX = reader.ReadInt32();
                int sizeY = reader.ReadInt32();
                //regionsize = reader.ReadInt32();
                //tilemapRegionValues = new float[regionMapSize.x, regionMapSize.y, regionsize, regionsize];
                influenceMap = new float[sizeX, sizeY];
                /*
                for (int regionX = 0; regionX < regionMapSize.x; regionX++)
                {
                    for (int regionY = 0; regionY < regionMapSize.y; regionY++)
                    {
                        for (int tilemapX = 0; tilemapX < regionsize; tilemapX++)
                        {
                            for (int tilemapY = 0; tilemapY < regionsize; tilemapY++)
                            {
                                float value = reader.ReadSingle();
                                tilemapRegionValues[regionX, regionY, tilemapX, tilemapY] = value;
                            }
                        }
                    }
                }
                */
                for (int x = 0; x < influenceMap.GetLength(0); x++)
                {
                    for (int y = 0; y < influenceMap.GetLength(1); y++)
                    {
                        float value = reader.ReadSingle();
                        influenceMap[x, y] = value;
                    }
                }
            }
        }
        catch (System.Exception e)
        {
            // handle errors here.
            Debug.LogError("Errer on loading Influence Map.");
        }

        return influenceMap;
    }
    public int[,] LoadLastInfluenceMap(string aFileName)
    {
        int[,] influenceMap = null;

        try
        {
            using (var fileStream = System.IO.File.OpenRead(aFileName))
            using (var reader = new System.IO.BinaryReader(fileStream))
            {
                var magic = reader.ReadString();
                // check your file magic to identify your file, so you can be sure
                // you access the right file
                if (magic != "InfuenceMapFile")
                    throw new System.Exception("Wrong file format");
                // check your file version in order to be future proof
                var version = reader.ReadInt32();
                if (version != 1)
                    throw new System.Exception("Not supported file version");
                // read our own 
                int sizeX = reader.ReadInt32();
                int sizeY = reader.ReadInt32();
                //regionsize = reader.ReadInt32();
                //tilemapRegionValues = new float[regionMapSize.x, regionMapSize.y, regionsize, regionsize];
                influenceMap = new int[sizeX, sizeY];
                /*
                for (int regionX = 0; regionX < regionMapSize.x; regionX++)
                {
                    for (int regionY = 0; regionY < regionMapSize.y; regionY++)
                    {
                        for (int tilemapX = 0; tilemapX < regionsize; tilemapX++)
                        {
                            for (int tilemapY = 0; tilemapY < regionsize; tilemapY++)
                            {
                                float value = reader.ReadSingle();
                                tilemapRegionValues[regionX, regionY, tilemapX, tilemapY] = value;
                            }
                        }
                    }
                }
                */
                for (int x = 0; x < influenceMap.GetLength(0); x++)
                {
                    for (int y = 0; y < influenceMap.GetLength(1); y++)
                    {
                        int value = (int)reader.ReadSingle();
                        influenceMap[x, y] = value;
                    }
                }
            }
        }
        catch (System.Exception e)
        {
            // handle errors here.
            Debug.LogError("Errer on loading Influence Map.");
        }

        return influenceMap;
    }
}
