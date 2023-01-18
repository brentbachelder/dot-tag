using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class TilemapController : MonoBehaviour
{
    [SerializeField] GameNetwork NetworkController;
    [SerializeField] Tile[] tiles = new Tile[7];
    [SerializeField] Tilemap tileMap;
    [SerializeField] GridLayout gridLayout;
    
    
    // Start is called before the first frame update
    void Start()
    {
        /*Vector3Int currentCell = tileMap.WorldToCell(transform.position);
        Debug.Log(currentCell);
        Vector3Int cellpos = new Vector3Int(-2, 2, 0);
        tileMap.SetTile(cellpos, tiles[0]);*/
    }

    public void CreateRoom(string map)
	{
        string tileNumbers = "";
        string tileRotate = "";
        for(var t = 0; t < map.Length; t += 2) tileNumbers += map[t];
        for(var r = 1; r < map.Length; r += 2) tileRotate += map[r];

        for(var i = 0; i < tileNumbers.Length; i++)
		{
            int counter, column, row, tileNum, rotateNum;
            row = 10 - (Mathf.FloorToInt(i / 14));
            counter = Mathf.FloorToInt(i / 14);
            column = i - (14 * counter);
            
            char tileBlock = tileNumbers[i];
            char rotateBlock = tileRotate[i];
            tileNum = (int)char.GetNumericValue(tileBlock);
            rotateNum = (int)char.GetNumericValue(rotateBlock);
            float rotation = rotateNum * -90;
            //if(i == 1) Debug.Log(tileNum * rotateNum * rotation);

            Vector3Int cellpos = new Vector3Int(column - 7, row - 6, 0);
            if(tileMap) tileMap.SetTile(cellpos, tiles[tileNum]); 
            if(tileMap) tileMap.SetTransformMatrix(cellpos, Matrix4x4.Rotate(Quaternion.Euler(0, 0, rotation)));
        }
	}

    public Vector2 FindSpaces()
	{
        List<Vector2> openSpaces = new List<Vector2>();
        int counter = 0;
        foreach(var position in tileMap.cellBounds.allPositionsWithin)
        {
            if(!tileMap.HasTile(position))
            {
                continue;
            }

            if(tileMap.GetTile(position).name == "allBlocks_5" || tileMap.GetTile(position).name == "allBlocks_6")
			{
                Vector3 cellPosition = tileMap.GetCellCenterWorld(position);
                openSpaces.Add(new Vector2(cellPosition.x + 1, cellPosition.y + 1));
                counter++;
            }
        }

        int randNum = Random.Range(0, openSpaces.Count);
        return openSpaces[randNum];
    }
}
