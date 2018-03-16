using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEditor;
using System.Runtime.Serialization.Formatters.Binary;

[Serializable]
struct Point2D
{
    public int x;
    public int y;
}

[System.Serializable]
class HeightCoordData
{    
    public Point2D Position;
    public float[,] Heights;
    //public float[,,] Alphamaps;
}

[CustomEditor(typeof(DynamicTerrainMaster))]
public class DynamicTerrainMasterEditor : Editor {
    

    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        string saveFilePath = Path.Combine(Application.persistentDataPath, "RawData.map");
        DynamicTerrainMaster self = (DynamicTerrainMaster)target;        
        
        if(GUILayout.Button("Clear terrain data"))
        {
            DynamicTerrainChunk[,] chunks = self.GetTerrainChunks();
            for (int x = 0; x < chunks.GetLength(0); x++)
            {
                for (int y = 0; y < chunks.GetLength(1); y++)
                {
                    chunks[x, y].ClearTerrainDataHeights();
                }
            }
        }

        if(GUILayout.Button("Save terrain data"))
        {
            DynamicTerrainChunk[,] chunks = self.GetTerrainChunks();            
            List<HeightCoordData> heightList = new List<HeightCoordData>();

            for (int x = 0; x < chunks.GetLength(0); x++)
            {
                for (int y = 0; y < chunks.GetLength(1); y++)
                {
                    HeightCoordData hc = new HeightCoordData();
                    Point2D coord = new Point2D();
                    coord.x = x;
                    coord.y = y;
                    hc.Position = coord;
                    hc.Heights = chunks[x, y].GetTerrainDataHeights();
                    //hc.Alphamaps = chunks[x, y].GetAlphamaps();
                    heightList.Add(hc);
                }
            }
                        

            if (File.Exists(saveFilePath))
            {
                File.Delete(saveFilePath);
            }

            FileStream fStream = File.Create(saveFilePath);
            BinaryFormatter bf = new BinaryFormatter();

            try
            {
                bf.Serialize(fStream, heightList);
                Debug.Log("Saved info at " + saveFilePath);
            }
            catch (Exception e)
            {
                Debug.LogError("Couldn't serialize data: " + e.Message);                
            }
            finally
            {
                fStream.Close();
            }                        
        }

        if(GUILayout.Button("Load terrain data"))
        {
            List<HeightCoordData> heightCoords;

            BinaryFormatter bf = new BinaryFormatter();
            FileStream fStream;
            System.Diagnostics.Stopwatch sw = new System.Diagnostics.Stopwatch();
            if (File.Exists(saveFilePath))
            {
                sw.Start();
                fStream = File.OpenRead(saveFilePath);
            }
            else
            {
                return;
            }            

            try
            {
                heightCoords = (List<HeightCoordData>)bf.Deserialize(fStream);
                DynamicTerrainChunk[,] chunks = self.GetTerrainChunks();

                foreach (HeightCoordData heightCoord in heightCoords)
                {
                    chunks[(int)heightCoord.Position.x, (int)heightCoord.Position.y].SetTerrainDataHeigths(heightCoord.Heights);
                    //chunks[(int)heightCoord.Position.x, (int)heightCoord.Position.y].SetAlphamaps(heightCoord.Alphamaps);
                }
                Debug.Log("Loaded info at " + saveFilePath);
            }
            catch(Exception e)
            {
                Debug.LogError("Couldn't deserialize data: " + e.Message);
            }
            finally
            {
                fStream.Close();
                sw.Stop();
                Debug.Log("Terrain generation time: " + (sw.ElapsedMilliseconds / 1000f));
            }
        }
    }
}
