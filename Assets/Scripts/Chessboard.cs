using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Chessboard : MonoBehaviour
{

    [Header("Art")]
    [SerializeField] private float tileSize = 1.0f;
    [SerializeField] private float yOffset = 1.0f;
    [SerializeField] private Vector3 boardCenter = Vector3.zero;
    [SerializeField] private float dragOffset = 2.5f;

    [Header("Prefabs and Materials")]
    [SerializeField] private GameObject[] prefabs;
    [SerializeField] private Material[] teamMaterials;
    [SerializeField] private Material tileMaterial;
    [SerializeField] private Material hoverMaterial;
    [SerializeField] private Material movesMaterial;
    [SerializeField] private GameObject victoryScreen;

    private const int TILE_COUNT_X = 8;
    private const int TILE_COUNT_Y = 8;
    private GameObject[,] tiles;
    private Camera currentCamera;
    private Vector2Int currentHover;
    private Vector3 bounds;
    private ChessPiece[,] chessPieces;
    private ChessPiece pieceDragging;
    private List<Vector2> availableMoves = new List<Vector2>();
    private int turn = 0;

    private int whitePiecesTaken = 0;
    private int blackPiecesTaken = 0;


    private void Awake()
    {
        GenerateTiles(tileSize, TILE_COUNT_X, TILE_COUNT_Y);
        SpawnAllPieces();
        PositionAllPieces();

    }


    private void Update()
    {
        if (!currentCamera)
        {
            currentCamera = Camera.main;
            return;
        }

        RaycastHit info;
        Ray ray = currentCamera.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out info, 100, LayerMask.GetMask("Tile", "Hover", "Moves")))
        {
            Vector2Int hitPosition = LookupTileIndex(info.transform.gameObject);

            //sets currentHover if hasnt been set before
            if (currentHover == -Vector2Int.one)
            {
                currentHover = hitPosition;
                tiles[hitPosition.x, hitPosition.y].layer = LayerMask.NameToLayer("Hover");
            }

            //sets currentHover to new tile

            if (currentHover != hitPosition)
            {
                tiles[currentHover.x, currentHover.y].layer = IsValidMove(ref availableMoves, currentHover) ? LayerMask.NameToLayer("Moves") : LayerMask.NameToLayer("Tile");
                currentHover = hitPosition;
                tiles[hitPosition.x, hitPosition.y].layer = LayerMask.NameToLayer("Hover");
            }

            if (Input.GetMouseButtonDown(0))
            {
                if (chessPieces[hitPosition.x, hitPosition.y] != null)
                {
                    if (chessPieces[hitPosition.x, hitPosition.y].team == turn % 2)
                    {
                        pieceDragging = chessPieces[hitPosition.x, hitPosition.y];

                        availableMoves = pieceDragging.GetPossibleMoves(ref chessPieces, TILE_COUNT_X, TILE_COUNT_Y);

                        highlightMoves(ref availableMoves);
                    }
                }
            }
            if (pieceDragging != null && Input.GetMouseButtonUp(0))
            {
                Vector2Int previousPosition = new Vector2Int(pieceDragging.currentX, pieceDragging.currentY);
                bool validMove;
                if (IsValidMove(ref availableMoves, new Vector2(hitPosition.x, hitPosition.y)))
                {
                    validMove = MoveTo(pieceDragging, hitPosition.x, hitPosition.y);
                }
                else
                {
                    validMove = false;
                }
                if (!validMove)
                {
                    pieceDragging.SetPosition(getTileCenter(previousPosition.x, previousPosition.y));

                }
                else
                {
                    turn++;
                }
                pieceDragging = null;
                unHighlightMoves(ref availableMoves);
                availableMoves = new List<Vector2>();

            }
        }
        else
        //mouse hovering off board
        {
            if (currentHover != -Vector2Int.one)
            {
                tiles[currentHover.x, currentHover.y].layer = LayerMask.NameToLayer("Tile");
                currentHover = -Vector2Int.one;
            }
            if (pieceDragging && Input.GetMouseButtonUp(0))
            {
                pieceDragging.SetPosition(getTileCenter(pieceDragging.currentX, pieceDragging.currentY));
                pieceDragging = null;
                unHighlightMoves(ref availableMoves);
                availableMoves = new List<Vector2>();

            }
        }
        ChangeMaterial();


        //moves pieces while dragging them
        if (pieceDragging)
        {
            Plane horizontalPlane = new Plane(Vector3.up, Vector3.up * yOffset);
            float distance = 0.0f;
            if (horizontalPlane.Raycast(ray, out distance))
            {
                pieceDragging.SetPosition(ray.GetPoint(distance) + Vector3.up * dragOffset);
            }
        }

    }



    //Piece Generation
    private void SpawnAllPieces()
    {
        chessPieces = new ChessPiece[TILE_COUNT_X, TILE_COUNT_Y];

        int whiteTeam = 0;
        int blackTeam = 1;

        //White Team
        chessPieces[0, 0] = SpawnSinglePiece(ChessPieceType.Rook, whiteTeam);
        chessPieces[1, 0] = SpawnSinglePiece(ChessPieceType.Knight, whiteTeam);
        chessPieces[2, 0] = SpawnSinglePiece(ChessPieceType.Bishop, whiteTeam);
        chessPieces[3, 0] = SpawnSinglePiece(ChessPieceType.Queen, whiteTeam);
        chessPieces[4, 0] = SpawnSinglePiece(ChessPieceType.King, whiteTeam);
        chessPieces[5, 0] = SpawnSinglePiece(ChessPieceType.Bishop, whiteTeam);
        chessPieces[6, 0] = SpawnSinglePiece(ChessPieceType.Knight, whiteTeam);
        chessPieces[7, 0] = SpawnSinglePiece(ChessPieceType.Rook, whiteTeam);
        //Pawns
        for (int i = 0; i < TILE_COUNT_X; i++)
        {
            chessPieces[i, 1] = SpawnSinglePiece(ChessPieceType.Pawn, whiteTeam);
        }

        //Black Team
        chessPieces[0, 7] = SpawnSinglePiece(ChessPieceType.Rook, blackTeam);
        chessPieces[1, 7] = SpawnSinglePiece(ChessPieceType.Knight, blackTeam);
        chessPieces[2, 7] = SpawnSinglePiece(ChessPieceType.Bishop, blackTeam);
        chessPieces[3, 7] = SpawnSinglePiece(ChessPieceType.Queen, blackTeam);
        chessPieces[4, 7] = SpawnSinglePiece(ChessPieceType.King, blackTeam);
        chessPieces[5, 7] = SpawnSinglePiece(ChessPieceType.Bishop, blackTeam);
        chessPieces[6, 7] = SpawnSinglePiece(ChessPieceType.Knight, blackTeam);
        chessPieces[7, 7] = SpawnSinglePiece(ChessPieceType.Rook, blackTeam);
        //Pawns
        for (int i = 0; i < TILE_COUNT_X; i++)
        {
            chessPieces[i, 6] = SpawnSinglePiece(ChessPieceType.Pawn, blackTeam);
        }

    }

    private void PositionAllPieces()
    {
        for (int x = 0; x < TILE_COUNT_X; x++)
        {
            for (int y = 0; y < TILE_COUNT_Y; y++)
            {
                if (chessPieces[x, y] != null)
                {
                    PositionSinglePiece(x, y, true);
                }
            }
        }

    }

    private void PositionSinglePiece(int x, int y, bool force = false)
    {
        chessPieces[x, y].currentX = x;
        chessPieces[x, y].currentY = y;
        chessPieces[x, y].SetPosition(getTileCenter(x, y), force);

    }

    private ChessPiece SpawnSinglePiece(ChessPieceType type, int team)
    {
        ChessPiece cp = Instantiate(prefabs[(int)type - 1], transform).GetComponent<ChessPiece>();
        cp.type = type;
        cp.team = team;
        cp.GetComponent<MeshRenderer>().material = teamMaterials[team];
        //rotate black team to face board
        if (team == 1)
        {
            cp.transform.rotation = new Quaternion(0, 180, 0, 0);
        }
        return cp;
    }

    private Vector3 getTileCenter(int x, int y)
    {
        return new Vector3(x * tileSize, yOffset, y * tileSize) - bounds + new Vector3(tileSize / 2, 0, tileSize / 2);
    }

    //Board Generation
    private void GenerateTiles(float tileSize, int tileCountX, int tileCountY)
    {
        yOffset += transform.position.y;
        bounds = new Vector3((tileCountX / 2) * tileSize, 0, (tileCountY / 2) * tileSize) + boardCenter;
        tiles = new GameObject[tileCountX, tileCountY];
        for (int x = 0; x < tileCountX; x++)
        {
            for (int y = 0; y < tileCountY; y++)
            {
                tiles[x, y] = GenerateSingleTile(tileSize, x, y);
            }
        }
    }

    private GameObject GenerateSingleTile(float tileSize, int x, int y)
    {


        GameObject tileObject = new GameObject(string.Format("X:{0}, Y:{1}", x, y));
        tileObject.transform.parent = transform;

        Mesh mesh = new Mesh();
        tileObject.AddComponent<MeshFilter>().mesh = mesh;
        tileObject.AddComponent<MeshRenderer>().material = tileMaterial;

        Vector3[] vertices = new Vector3[4];
        vertices[0] = new Vector3(x * tileSize, yOffset, y * tileSize) - bounds;
        vertices[1] = new Vector3(x * tileSize, yOffset, (y + 1) * tileSize) - bounds;
        vertices[2] = new Vector3((x + 1) * tileSize, yOffset, y * tileSize) - bounds;
        vertices[3] = new Vector3((x + 1) * tileSize, yOffset, (y + 1) * tileSize) - bounds;

        int[] triangles = new int[] { 0, 1, 2, 1, 3, 2 };
        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.RecalculateNormals();
        tileObject.layer = LayerMask.NameToLayer("Tile");

        tileObject.AddComponent<BoxCollider>();

        return tileObject;
    }

    //Operations
    private Vector2Int LookupTileIndex(GameObject hitInfo)
    {
        for (int x = 0; x < TILE_COUNT_X; x++)
        {
            for (int y = 0; y < TILE_COUNT_Y; y++)
            {
                if (tiles[x, y] == hitInfo)
                    return new Vector2Int(x, y);
            }
        }
        return -Vector2Int.one; //off board
    }

    private void ChangeMaterial()
    {
        for (int x = 0; x < TILE_COUNT_X; x++)
        {
            for (int y = 0; y < TILE_COUNT_Y; y++)
            {
                if (tiles[x, y] != null)
                {
                    try
                    {
                        if (tiles[x, y].layer == 6)
                        {
                            tiles[x, y].GetComponent<MeshRenderer>().material = tileMaterial;
                        }
                        else if(tiles[x,y ].layer ==7)
                        {
                            tiles[x, y].GetComponent<MeshRenderer>().material = hoverMaterial;
                        }
                        else
                        {
                            tiles[x, y].GetComponent<MeshRenderer>().material = movesMaterial;

                        }
                    }
                    catch
                    {
                        print("null object referenced");
                    }
                }
            }
        }
    }

    private bool MoveTo(ChessPiece piece, int x, int y)
    {
        Vector2Int previousPosition = new Vector2Int(piece.currentX, piece.currentY);

        if (chessPieces[x, y] != null)
        {
            ChessPiece other = chessPieces[x, y];

            if (piece.team == other.team)
            {
                return false;
            }
            else
            {
                Taken(x, y);
                chessPieces[x, y] = null;
            }
        }
        chessPieces[x, y] = piece;
        chessPieces[previousPosition.x, previousPosition.y] = null;

        PositionSinglePiece(x, y);

        return true;
    }

    private void Taken(int x, int y)
    {

        ChessPiece piece = chessPieces[x, y];
        if (piece.team == 0)
        {
            whitePiecesTaken++;
            piece.SetScale(Vector3.zero);
            //piece.SetPosition(new Vector3(-8 * tileSize, yOffset,( -8 * tileSize)+ whitePiecesTaken*tileSize)+bounds);
        }
        else
        {
            blackPiecesTaken++;
            piece.SetScale(Vector3.zero);
            blackPiecesTaken++;
        }
        if(piece.type == ChessPieceType.King)
        {
            if(piece.team == 0)
            {
                Checkmate(1);
            }
            else
            {
                Checkmate(0);
            }
                
        }
        //piece.SetScale(new Vector3(0.7f, 0.7f, 0.7f));
    }

    private bool IsValidMove(ref List<Vector2> moves, Vector2 pos)
    {
        for (int i = 0; i < moves.Count; i++)
        {
            if (moves[i].x == pos.x && moves[i].y == pos.y)
            {
                return true;
            }
        }
        return false;
    }

    private void highlightMoves(ref List<Vector2> moves)
    {
        for (int i = 0; i < moves.Count; i++)
        {
            tiles[(int)moves[i].x, (int)moves[i].y].layer = LayerMask.NameToLayer("Moves");
        }
    }

    private void unHighlightMoves(ref List<Vector2> moves)
    {
        for (int i = 0; i < moves.Count; i++)
        {
            tiles[(int)moves[i].x, (int)moves[i].y].layer = LayerMask.NameToLayer("Tile");
        }
    }

    //UI/Checkmate

    private void Checkmate(int team)
    {
        DisplayVictory(team);
    }

    private void DisplayVictory(int team)
    {
        victoryScreen.SetActive(true);
        victoryScreen.transform.GetChild(team).gameObject.SetActive(true);
    }

    public void Reset()
    {
        victoryScreen.SetActive(false);
        victoryScreen.transform.GetChild(0).gameObject.SetActive(false);
        victoryScreen.transform.GetChild(1).gameObject.SetActive(false);

        for(int x = 0; x < TILE_COUNT_X; x++)
        {
            for (int y = 0; y <TILE_COUNT_Y; y++)
            {
                if(chessPieces[x,y]!=null)
                {
                    Destroy(chessPieces[x, y].gameObject);
                    chessPieces[x, y] = null;
                }
            }
        }
        turn = 0;
        SpawnAllPieces();
        PositionAllPieces();
    }

    public void Exit()
    {
        Application.Quit();
    }
}