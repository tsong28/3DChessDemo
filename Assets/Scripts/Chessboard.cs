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
    [SerializeField] private GameObject promoteScreen;

    private const int TILE_COUNT_X = 8;
    private const int TILE_COUNT_Y = 8;
    private GameObject[,] tiles;
    private Camera currentCamera;
    private Vector2Int currentHover;
    private Vector3 bounds;
    private ChessPiece[,] chessPieces;
    private ChessPiece pieceDragging;
    private ChessPiece whiteKing;
    private ChessPiece blackKing;
    private List<Vector2Int> availableMoves = new List<Vector2Int>();
    private int turn = 0;
    private ChessPiece pieceToPromote = null;
    private ChessPiece enPassantPiece = null;

    bool kingInCheck = false;



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

                        if (pieceDragging.type == ChessPieceType.King) {
                            availableMoves = KingMoves(pieceDragging);
                        }
                        else
                        {

                            availableMoves = KingInCheckMoves(pieceDragging);
                            if (enPassantPiece != null && pieceDragging.type == ChessPieceType.Pawn && enPassantPiece.team != pieceDragging.team)
                            {
                                if(enPassantPiece.currentY==pieceDragging.currentY && (enPassantPiece.currentX +1 == pieceDragging.currentX
                                    || enPassantPiece.currentX -1 == pieceDragging.currentX))
                                {
                                    int direction = pieceDragging.team == 0 ? 1 : -1;
                                    Vector2Int move = new Vector2Int(enPassantPiece.currentX, pieceDragging.currentY+direction);
                                    availableMoves.Add(move);
                                }
                            }
                        }
                        highlightMoves(ref availableMoves);
                    }
                }
            }
            if (pieceDragging != null && Input.GetMouseButtonUp(0))
            {
                Vector2Int previousPosition = new Vector2Int(pieceDragging.currentX, pieceDragging.currentY);
                bool validMove;
                if (pieceDragging.type != ChessPieceType.King)
                {
                    if (IsValidMove(ref availableMoves, new Vector2Int(hitPosition.x, hitPosition.y)))
                    {
                        if(pieceDragging.type == ChessPieceType.Pawn && enPassantPiece != null)
                        {
                            int direction = pieceDragging.team == 0 ? -1 : 1;
                            if (hitPosition.y == enPassantPiece.currentY -direction &&
                                (hitPosition.x == enPassantPiece.currentX))
                            {
                                Debug.Log("en passant piece at: " + enPassantPiece.currentX + " " + enPassantPiece.currentY);
                                Debug.Log("taking moving to: " + hitPosition.x + " " + hitPosition.y);
                                Taken(enPassantPiece.currentX, enPassantPiece.currentY, ref chessPieces);
                                enPassantPiece = null;
                            }
                        }
                        if(pieceDragging.type == ChessPieceType.Pawn && (Math.Abs(pieceDragging.currentY-hitPosition.y) == 2))
                        {
                            enPassantPiece = pieceDragging;
                        } else
                        {
                            enPassantPiece = null;
                        }

                        validMove = true;
                        MoveTo(pieceDragging, hitPosition.x, hitPosition.y, ref chessPieces);
                        if (pieceDragging.type == ChessPieceType.Pawn && (pieceDragging.currentY == 0 || pieceDragging.currentY == 7))
                        {
                            Promote(pieceDragging);
                        }

                        //check if move put king in check
                        List<Vector2Int> newSquares = getSquaresAttacking(pieceDragging.team, ref chessPieces);
                        if (newSquares.Contains(pieceDragging.team == 0 ? new Vector2Int(blackKing.currentX, blackKing.currentY) :
                                                                         new Vector2Int(whiteKing.currentX, whiteKing.currentY)))
                        {
                            kingInCheck = true;
                            Debug.Log("KingInCheck!!!!");
                            KingInCheckmate((turn + 1) % 2);
                        }
                        else
                        {
                            kingInCheck = false;
                            Debug.Log("king not in check!!!!");
                        }
                        pieceDragging.hasMoved = true;
                    }
                    else
                    {
                        validMove = false;
                    }
                }
                else
                {
                    if (IsValidMove(ref availableMoves, new Vector2Int(hitPosition.x, hitPosition.y)))
                    {
                        validMove = true;
                        MoveTo(pieceDragging, hitPosition.x, hitPosition.y, ref chessPieces);

                        

                        //check if move put king in check
                        List<Vector2Int> newSquares = getSquaresAttacking(pieceDragging.team, ref chessPieces);
                        if (newSquares.Contains(pieceDragging.team == 0 ? new Vector2Int(blackKing.currentX, blackKing.currentY) :
                                                                         new Vector2Int(whiteKing.currentX, whiteKing.currentY)))
                        {
                            kingInCheck = true;
                            Debug.Log("KingInCheck!!!!");
                            KingInCheckmate(turn + 1 % 2);
                        }
                        else
                        {
                            kingInCheck = false;
                            Debug.Log("king not in check!!!!");
                        }
                        pieceDragging.hasMoved = true;
                    }
                    else
                    {
                        validMove = false;
                    }
                }
                if (!validMove)
                {
                    pieceDragging.SetPosition(getTileCenter(previousPosition.x, previousPosition.y));

                }
                else
                {
                    turn++;
                    Quaternion rotation = Quaternion.Euler(55, turn %2 == 0 ? 0 : 180, 0);
                    Camera.main.transform.SetLocalPositionAndRotation(new Vector3Int(0, 20, (turn % 2 == 0 ? -15 : 15)),
                        rotation);
                    
                }

                pieceDragging = null;
                unHighlightMoves(ref availableMoves);
                
                availableMoves = new List<Vector2Int>();

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
                availableMoves = new List<Vector2Int>();

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
        whiteKing = chessPieces[4, 0];
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
        blackKing = chessPieces[4, 7];

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
                    PositionSinglePiece(x, y, ref chessPieces, true);
                }
            }
        }

    }

    private void PositionSinglePiece(int x, int y, ref ChessPiece[,] board, bool force = false)
    {
        board[x, y].currentX = x;
        board[x, y].currentY = y;
        board[x, y].SetPosition(getTileCenter(x, y), force);

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
                        Debug.Log("null object referenced");
                    }
                }
            }
        }
    }

    private bool MoveTo(ChessPiece piece, int x, int y, ref ChessPiece[,] board)
    {
        Vector2Int previousPosition = new Vector2Int(piece.currentX, piece.currentY);

        if (board[x, y] != null)
        {
            ChessPiece other = board[x, y];
            if(other.type == ChessPieceType.Rook && piece.type == ChessPieceType.King)
            {
                Castle(piece, other);
                return true;
            }
            else if (piece.team == other.team)
            {
                return false;
            }
            else
            {
                Taken(x, y, ref board);
                board[x, y] = null;
            }
        }
        board[x, y] = piece;
        board[previousPosition.x, previousPosition.y] = null;
        PositionSinglePiece(x, y, ref board);
        //Debug.Log("Moved to " + x + " " + y);
        return true;
    }

    private void Taken(int x, int y, ref ChessPiece[,] board)
    {

        ChessPiece piece = board[x, y];
        if (piece.team == 0)
        {

            piece.SetScale(Vector3.zero);
        }
        else
        {

            piece.SetScale(Vector3.zero);

        }
        
    }

    private bool IsValidMove(ref List<Vector2Int> moves, Vector2Int pos)
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

    private void highlightMoves(ref List<Vector2Int> moves)
    {
        for (int i = 0; i < moves.Count; i++)
        {
            tiles[(int)moves[i].x, (int)moves[i].y].layer = LayerMask.NameToLayer("Moves");
        }
    }

    private void unHighlightMoves(ref List<Vector2Int> moves)
    {
        for (int i = 0; i < moves.Count; i++)
        {
            tiles[(int)moves[i].x, (int)moves[i].y].layer = LayerMask.NameToLayer("Tile");
        }
    }

    private List<Vector2Int> getSquaresAttacking(int currTeam, ref ChessPiece[,] board)
    {
        var squaresAttacked = new Dictionary<Vector2Int, Vector2Int>();
        for(int x =0; x < TILE_COUNT_X; x++)
        {
            for(int y= 0; y< TILE_COUNT_Y; y++)
            {
                if(board[x,y]!=null && board[x,y].team == currTeam)
                {
                    foreach(Vector2Int square in board[x,y].GetPossibleMoves(ref board))
                    {
                        try
                        {
                            squaresAttacked[square] = square;
                        }
                        catch { }
                    }
                }
            }

        }
        List<Vector2Int> squaresAttackedList = new List<Vector2Int>();
        foreach (KeyValuePair<Vector2Int, Vector2Int> kvp in squaresAttacked)
        {
            squaresAttackedList.Add(kvp.Key);
        }
        return squaresAttackedList;
    }

    private Dictionary<ChessPiece, List<Vector2Int>> getAllPossibleMoves(int currTeam, ref ChessPiece[,] board)
    {
        Debug.Log("getallpossiblemoves called");
        var squaresAttacked = new Dictionary<ChessPiece, List<Vector2Int>>();
        for (int x = 0; x < TILE_COUNT_X; x++)
        {
            for (int y = 0; y < TILE_COUNT_Y; y++)
            {
                if (board[x, y] != null && board[x, y].team == currTeam)
                {
                    squaresAttacked[board[x, y]] = new List<Vector2Int>(board[x,y].GetPossibleMoves(ref chessPieces));
                }
            }

        }

        return squaresAttacked;
    }
    private List<Vector2Int> KingMoves(ChessPiece king)
    {
        List<Vector2Int> attackedSquares = getSquaresAttacking(king.team == 0 ? 1 : 0, ref chessPieces);

        List<Vector2Int> possibleMoves = king.GetPossibleMoves(ref chessPieces);
        for(int i = 0; i < attackedSquares.Count; i++)
        {
            if(possibleMoves.Contains(attackedSquares[i])) {
                possibleMoves.Remove(attackedSquares[i]);
            } 
        }

        //castling
        List<ChessPiece> rooks = new List<ChessPiece>();

        for(int x = 0; x <8; x++)
        {
            if (chessPieces[x, king.currentY] != null)
            {
                if (chessPieces[x, king.currentY].type == ChessPieceType.Rook)
                {
                    rooks.Add(chessPieces[x, king.currentY]);
                }
            }
        }

        if(rooks.Count != 0)
        {
            foreach (ChessPiece rook in rooks)
            {
                if(CanCastle(king, rook))
                {
                    possibleMoves.Add(new Vector2Int(rook.currentX, rook.currentY));
                }
            }
            
        }

        return possibleMoves;
    }

    private void KingInCheckmate(int teamInCheck)
    {
        Dictionary<ChessPiece, List<Vector2Int>> possibleMoves = getAllPossibleMoves(teamInCheck, ref chessPieces);

        Debug.Log("possible moves count: " + possibleMoves.Count);
        foreach (KeyValuePair<ChessPiece, List<Vector2Int>> kvp in possibleMoves)
        {
            Debug.Log("kvp in possible moves: "+kvp);
            foreach(Vector2Int move in kvp.Value)
            {
                Debug.Log(kvp.Key.type + ": " + move);
                if(SimulateSingleMove(ref chessPieces, kvp.Key, move)) {
                    return;
                }
            }
        }

        Checkmate(teamInCheck == 0 ? 1 : 0);

    }

    private List<Vector2Int> KingInCheckMoves(ChessPiece piece)
    {

        List<Vector2Int> simulatedMoves = piece.GetPossibleMoves(ref chessPieces);
        List<Vector2Int> possibleMoves = new List<Vector2Int>();

        foreach (Vector2Int move in simulatedMoves)
        {

            if (SimulateSingleMove(ref chessPieces, piece, move))
            {
                possibleMoves.Add(move);
                //Debug.Log("added move" + move);
            }

        }


        return possibleMoves;
    }

    //returns false if king is still in check after simulated move, true otherwise
    private bool SimulateSingleMove(ref ChessPiece[,] board, ChessPiece piece, Vector2Int move)
    {
        ChessPiece[,] copy = new ChessPiece[8, 8];
        for (int x = 0; x < 8; x++)
        {
            for (int y = 0; y < 8; y++)
            {
                if (chessPieces[x, y] != null)
                {
                    copy[x, y] = Instantiate(chessPieces[x, y]);
                    copy[x, y].SetScale(Vector3.zero, true);
                }
            }
        }
        MoveTo(copy[piece.currentX, piece.currentY], move.x, move.y, ref copy);

        List<Vector2Int> attackedSquares = getSquaresAttacking(piece.team == 0 ? 1: 0, ref copy);
        Debug.Log("Simulated Move: " + move);
        //foreach (Vector2Int a in attackedSquares)
        //{
        //    Debug.Log(a);
        //}
        for (int x = 0; x < 8; x++)
        {
            for (int y = 0; y < 8; y++)
            {
                if (copy[x, y] != null)
                {
                    //Debug.Log("Destroying:" + copy[x, y]);
                    Destroy(copy[x, y].gameObject);

                    copy[x, y] = null;
                }
            }
        }
        if (piece.team == 0)
        {
            return !(attackedSquares.Contains(new Vector2Int(whiteKing.currentX, whiteKing.currentY)));
        } else
        {
            return !(attackedSquares.Contains(new Vector2Int(blackKing.currentX, blackKing.currentY)));
        }

        
    }

    private bool CanCastle(ChessPiece king, ChessPiece rook)
    {
        Debug.Log("Checking can castle");
        if(!king.hasMoved&&!kingInCheck&&!rook.hasMoved)    
        {
            Debug.Log("Checking attacked squares and piece involvement");
            List<Vector2Int> moves = getSquaresAttacking(king.team == 0 ? 1 : 0, ref chessPieces);
            if(rook.currentX == 0)
            {
                for(int x = 0; x <=4; x++) {
                    if(moves.Contains(new Vector2Int(x, king.currentY)))
                    {
                        return false;
                    }
                }
                for(int x = 1; x<4; x++)
                {
                    if(chessPieces[x, king.currentY] != null)
                    {
                        return false;
                    }
                }
            } else
            {
                for (int x = 7; x >= 4; x--)
                {
                    if (moves.Contains(new Vector2Int(x, king.currentY)))
                    {
                        //Debug.Log("squares between are attacked");
                        return false;
                    }
                }
                for (int x = 6; x > 4; x--)
                {
                    if (chessPieces[x, king.currentY] != null)
                    {
                        //Debug.Log("piece in between");
                        return false;
                    }
                }
            }
            return true;
        }
        return false;
        
    }

    private void Castle(ChessPiece king, ChessPiece rook)
    {
        //need to move king, move rook, then set old positions to null
        int kingX = king.currentX;
        int kingY = king.currentY;
        int rookX = rook.currentX;
        int rookY = rook.currentY;
        if (rook.currentX == 0)
        {
            chessPieces[kingX - 2, kingY] = rook;
            chessPieces[rook.currentX, rook.currentY] = null;

            chessPieces[rook.currentX + 1, rook.currentY] = king;
            chessPieces[kingX, kingY] = null;

            PositionSinglePiece(kingX - 2, kingY, ref chessPieces);
            PositionSinglePiece(rookX + 1, rookY, ref chessPieces);
        } else
        {
            chessPieces[kingX +1, kingY] = rook;
            chessPieces[rook.currentX, rook.currentY] = null;

            chessPieces[rook.currentX -1, rook.currentY] = king;
            chessPieces[kingX, kingY] = null;

            PositionSinglePiece(kingX + 1, kingY, ref chessPieces);

            PositionSinglePiece(rookX - 1, rookY, ref chessPieces);


        }

    }


    //UI/Checkmate

    private void Checkmate(int team)
    {
        DisplayVictory(team);
    }

    private void Promote(ChessPiece pawn)
    {
        pieceToPromote = pawn;
        promoteScreen.SetActive(true);
    }
    public void setRook()
    {
        int team = pieceToPromote.team;
        Vector2Int pos = new Vector2Int(pieceToPromote.currentX, pieceToPromote.currentY);
        chessPieces[pos.x, pos.y] = SpawnSinglePiece(ChessPieceType.Rook, team);
        chessPieces[pos.x, pos.y].hasMoved = true;
        Destroy(pieceToPromote.gameObject);
        promoteScreen.SetActive(false);
        PositionSinglePiece(pos.x, pos.y, ref chessPieces, true);
    }
    public void setQueen()
    {
        int team = pieceToPromote.team;
        Vector2Int pos = new Vector2Int(pieceToPromote.currentX, pieceToPromote.currentY);
        chessPieces[pos.x, pos.y] = SpawnSinglePiece(ChessPieceType.Queen, team);
        chessPieces[pos.x, pos.y].hasMoved = true;
        Destroy(pieceToPromote.gameObject);
        promoteScreen.SetActive(false);
        PositionSinglePiece(pos.x, pos.y, ref chessPieces, true);
    }
    public void setBishop()
    {
        int team = pieceToPromote.team;
        Vector2Int pos = new Vector2Int(pieceToPromote.currentX, pieceToPromote.currentY);
        chessPieces[pos.x, pos.y] = SpawnSinglePiece(ChessPieceType.Bishop, team);
        chessPieces[pos.x, pos.y].hasMoved = true;
        Destroy(pieceToPromote.gameObject);
        promoteScreen.SetActive(false);
        PositionSinglePiece(pos.x, pos.y, ref chessPieces, true);
    }
    public void setKnight()
    {
        int team = pieceToPromote.team;
        Vector2Int pos = new Vector2Int(pieceToPromote.currentX, pieceToPromote.currentY);
        chessPieces[pos.x, pos.y] = SpawnSinglePiece(ChessPieceType.Knight, team);
        chessPieces[pos.x, pos.y].hasMoved = true;
        Destroy(pieceToPromote.gameObject);
        promoteScreen.SetActive(false);
        PositionSinglePiece(pos.x, pos.y, ref chessPieces, true);
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