
using System;
using System.Collections.Generic;
using Unity.Collections.LowLevel.Unsafe;
using Unity.VisualScripting;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;


//note everything is done from the perspective of white
namespace MG
{

    public enum Type
    {
        PAWN,
        BISHOP,
        KNIGHT,
        ROOK,
        QUEEN,
        KING,
        NONE
    }

    public enum Team
    {
        WHITE,
        BLACK,
        NONE
    }

    //within the chess world, this is a struct to represent coordinates, it ca convert to conventional chess square names
    //but works typically within 0-7, 0-7
    public struct Coord
    {
       public Coord(int x, int y)
        {
            this.x = x;
            this.y = y;
        }
        public int x; 
        public int y;

        public void Add(int x, int y)
        {
            this.x += x;
            this.y += y;
        }

        //returns if coord is on the board
        public bool Exists()
        {
            return (x >= 0 && x <= 7 && y >= 0 && y <= 7);
        }

        public bool IsDiagonal()
        {
            return (Math.Abs(x) == 1 && Math.Abs(y) == 1);
        }

        //note 0, 0 isnt really handled here, this is really just for vector dirs
        public bool IsHorzVert()
        {
            return (x == 0 || y == 0);
        }

        public static bool operator== (Coord a, Coord b)
        {
            return (a.x == b.x && a.y == b.y);
        }

        public static bool operator !=(Coord a, Coord b)
        {
            return !(a.x == b.x && a.y == b.y);
        }


        //public override bool Equals(object obj)
        //{
        //    return base.Equals(obj);
        //}

        public static Coord operator -(Coord a, Coord b)
        {
            return new Coord(a.x-b.x, a.y-b.y);
        }

        public static Coord operator +(Coord a, Coord b)
        {
            return new Coord(a.x + b.x, a.y + b.y);
        }

        public static Coord operator *(int x, Coord a)
        {
            return new Coord(a.x * x, a.y * x);
        }

        public static Coord operator *(Coord a, int x)
        {
            return new Coord(a.x * x, a.y * x);
        }
    }




    public struct Piece
    {
        public Piece(Type type, Team team)
        {
            this.type = type;
            this.team = team;
        }

      
        public Type type;
        public Team team;
    }

    public struct Square
    {
        public Square(int x, int y)
        {
            pos = new Coord(x, y);
            piece = new Piece(Type.NONE, Team.NONE);
        }

        public Square(Coord pos)
        {
            this.pos = pos;
            piece = new Piece(Type.NONE, Team.NONE);
        }

        public Square(int x, int y, Piece piece)
        {
            pos = new Coord(x, y);
            this.piece = piece;
        }

        public Square(Coord pos, Piece piece)
        {
            this.pos = pos;
            this.piece = piece;
        }

        public bool empty()
        {
            return piece.type == Type.NONE;   
        }

        public Piece piece;
        public Coord pos;
    }

    public struct Action
    {
        public Square start;
        public Square end;
        public Piece piece;

        //special move data to make our life easier

        public Coord enPassantTarget;

    }

    public class Board
    {

        //assumed move has been 
        public Board(Board previous, Action move)
        {

        }

        //default board at the start of a chess game
        public Board()
        {
            whosTurn = Team.WHITE;

            for (int i = 0; i < 8; i++)
            {
                List <Square> squareRow = new List <Square>();
                for(int j = 0; j < 8; j++)
                {
                    Square sqr = createPieceAtDefaultPos(j, i);

                    if (sqr.piece.type != Type.NONE)
                        storePiece(sqr);
                    
                    squareRow.Add(sqr);
                }
                squares.Add(squareRow);
            }

        }


        public int boardSize()
        {
            return squares[0].Count * squares.Count;
        }


        List<List <Square>> squares = new List<List <Square>>();    
        Action previousAction = new Action();
        List<Square> whitePieceSquares = new List<Square>();
        List<Square> blackPieceSquares = new List<Square>();
        List<Coord> whiteInfluence = new List<Coord>();
        List<Coord> blackInfluence = new List<Coord>();
        Square whiteKing;
        Square blackKing;
        Team whosTurn;
        //can be determined when creating enemy threats which are actually friendly threats after a movement is made when creating a board
        bool inCheck;
        //check blockable squares? might be worth
        //these are gonna have to be used if in check so might as well make them when we have the data
        List<Coord> checkBlockableCoords = new List<Coord>();
        public bool getInCheck()
        {
            return inCheck;
        }

        public ref List <Coord> getCheckBlockableCoords()
        {
            return ref checkBlockableCoords;
        }

        public Team getWhosTurn()
        {
            return whosTurn;
        }

        public Action getPreviousAction()
        {
            return previousAction;
        }

        
        public Square getSquare(ref Square s)
        {
            return squares[s.pos.x][s.pos.y];
        }

        public Square getSquare(Coord p)
        {
            return squares[p.x][p.y];
        }


        public ref List<Square> readFriendlyPieces()
        {
            if(whosTurn == Team.WHITE)
                return ref whitePieceSquares;
            else return ref blackPieceSquares;

        }

        public ref List<Square> readEnemyPieces()
        {
            if (whosTurn != Team.WHITE)
                return ref whitePieceSquares;
            else return ref blackPieceSquares;

        }

        //these should only be used if the lists want be coppied and used in another board
        public List<Square> getFriendlyPieces()
        {
            if (whosTurn == Team.WHITE)
                return whitePieceSquares;
            else return blackPieceSquares;

        }

        public  List<Square> getEnemyPieces()
        {
            if (whosTurn != Team.WHITE)
                return whitePieceSquares;
            else return blackPieceSquares;

        }

        public ref Square getFriendlyKing()
        {
            if (whosTurn == Team.WHITE)
                return ref whiteKing;
            else return ref blackKing;

        }

        public ref Square getEnemyKing()
        {
            if (whosTurn != Team.WHITE)
                return ref whiteKing;
            else return ref blackKing;

        }

        public ref List <Coord> getEnemyInfluence()
        {
            if (whosTurn != Team.WHITE)
                return ref whiteInfluence;
            else return ref blackInfluence;

        }

        public ref List<Coord> getFriendlyInfluence()
        {
            if (whosTurn == Team.WHITE)
                return ref whiteInfluence;
            else return ref blackInfluence;

        }




        Square setupSquareWithTeamAndCoord(int x, int y, Team team)
        {
            Square square = new Square(x, y);
            Type pieceType = Type.NONE;
            if (x == 0 || x == 7)
            {
                pieceType = Type.ROOK;
            }
            else if (x == 1 || x == 6)
            {
                pieceType = Type.KNIGHT;
            }
            else if (x == 2 || x == 5)
            {
                pieceType = Type.BISHOP;
            }
            else if (x == 3)
            {
                pieceType = Type.QUEEN;
            }
            else if (x == 4)
            {
                pieceType = Type.KING;
            }


            square.piece = new Piece(pieceType, team);
            return square;
        }

        Square createPieceAtDefaultPos(int x, int y)
        {
            //if defaulrt start we placing pieces and creating pieces according to standard game staring loadout
            Square square = new Square(x, y);
                //we are in tye center, its empty here so just return NONE
                if (y > 1 && y < 6)
                    return square;

                //check out of bounds

                if (y > 7 || y < 0 || x > 7 || x < 0)
                    return square;

                if(y == 1)
                {
                    //we spitting out white pawns
                    square.piece = new Piece(Type.PAWN, Team.WHITE);
                    return square;
                }
                else if(y == 6)
                {
                    //black pawns
                    square.piece = new Piece(Type.PAWN, Team.BLACK);
                    return square;
                }
                else if(y == 0)
                {

                    return setupSquareWithTeamAndCoord(x,y, Team.WHITE);
                    
                }
                else if (y == 7)
                {
                    return setupSquareWithTeamAndCoord(x, y, Team.BLACK);
                }

                return square; 
        }


        void storePiece(Square square)
        {
            if (square.piece.team == Team.WHITE)
            {
                whitePieceSquares.Add(square);
                if (square.piece.type == Type.KING)
                    whiteKing = square;

            }
            else
            {
                blackPieceSquares.Add(square);
                if (square.piece.type == Type.KING)
                    blackKing = square;
            }

        }
    }

    public class MoveGenerator : MonoBehaviour
    {
        //how this class works

        //singelton that holds a series of functions for making chess move genration easy

        Board currentBoard = null;



        List <Board> generatePiecesMoves(Square pieceOnSquare)
        {
            List <Board> futureBoards = new List <Board>();
            //first do easy targets
            List<Coord> basicTargets = generatePiecesTargets(pieceOnSquare);

            //more complicated targets like special moves and pawn movement will be handled here because they aren't threats and can only be done as actions

            if(pieceOnSquare.piece.type == Type.KING)
            {
                kingSpecialMoveGenerate(pieceOnSquare, ref futureBoards);
            }
            else if(pieceOnSquare.piece.type == Type.PAWN)
            {
               pawnSpecialMoveGenerate(pieceOnSquare, ref futureBoards);
            }

            

            normalMoveGenerate(pieceOnSquare, ref basicTargets, ref futureBoards);

            return futureBoards;
        }


        void normalMoveGenerate(Square pieceOnSquare, ref List<Coord> basicTargets, ref List<Board> boards)
        {
            //check legality of targets and cull them

            //don't start making a board until we have affirmed a target locs legality
            basicTargetLegalityCull(pieceOnSquare, ref basicTargets);

            //coords should be legal moves now, so we can now make each of them into a board

        }

        void kingSpecialMoveGenerate(Square pieceOnSquare, ref List <Board> boards)
        {

        }



        void pawnSpecialMoveGenerate(Square pieceOnSquare, ref List<Board> boards)
        {

        }


        void basicTargetLegalityCull(Square pieceOnSquare, ref List<Coord> targets)
        {
            //a few ways a move is illegal

            //taking afriendy piece?
            noTakeFriendlyPieceCull(pieceOnSquare, ref targets);

            underCheckPieceMovementCull(pieceOnSquare, ref targets);

            underPinPieceMovementCull(pieceOnSquare, ref targets);
            
        }

        void noTakeFriendlyPieceCull(Square pieceOnSquare, ref List<Coord> targets)
        {
            for(int i = 0; i < targets.Count; i++)
            {
                Square targetSquare = currentBoard.getSquare(targets[i]);
                if (!targetSquare.empty() && targetSquare.piece.team == pieceOnSquare.piece.team)
                {
                    targets.RemoveAt(i);
                    i--;
                }
            }
        }

        void underCheckPieceMovementCull(Square pieceOnSquare, ref List<Coord> targets)
        {
            //check king under check
            if(currentBoard.getInCheck() && pieceOnSquare.piece.type != Type.KING)
            {
                List<Coord> blockCheckCoords = currentBoard.getCheckBlockableCoords();
                //we have check blockable squares generated, so lets see if targets reside in any of these as ong as we are not king
               removeAllCoordsInFirstThatDontExistInSecond(ref targets, ref blockCheckCoords);
            }
        }

        void underPinPieceMovementCull(Square pieceOnSquare, ref List<Coord> targets)
        {
            //we want to do the conidtionals that will most oikely let us avoid doing this first
            if (!coordUnderthreat(pieceOnSquare.pos) || pieceOnSquare.piece.type == Type.KING)
                return;

            //now determine whether there is an unobstructed ray to our king
            Square king = currentBoard.getFriendlyKing();

            Coord vec = king.pos - pieceOnSquare.pos;
            //vec is the displacement to the king from our pos
            //if this isnt a 45 or 90 degree line we can immeditely just quit
            Coord kingDir = new Coord(vec.x, vec.y);
            if (vec.x == 0)// straight line of either 45 or 90 degree
            {
                kingDir.y /= Math.Abs(kingDir.y);
            }
            else if(vec.y == 0)
            {
                kingDir.x /= Math.Abs(kingDir.y);
            }
            else if(Math.Abs(vec.x) == Math.Abs(vec.y))
            {
                kingDir.x /= Math.Abs(kingDir.x);
                kingDir.y /= Math.Abs(kingDir.x);
            }
            else
                return;

            //kingDir is now a directional unit vector towards the king

            //king is on an unobstructed path
            if(getFirstPieceSquareAlongDir(kingDir, pieceOnSquare.pos).piece.type == Type.KING)
            {
                //now check the path in the opposite direction
                Square sqr = getFirstPieceSquareAlongDir(-1 * kingDir, pieceOnSquare.pos);
                //now the piece captured needs to have aray that matches the one that aims towards it

                //diagonal = bishop or queen
                if(((kingDir.IsDiagonal() && sqr.piece.type == Type.BISHOP)
                    || (kingDir.IsHorzVert() && sqr.piece.type == Type.ROOK)
                    || sqr.piece.type == Type.QUEEN)
                    && sqr.piece.team != pieceOnSquare.piece.team)
                {
                    //ok so we have a pin, now create a list of squares in region
                    //we can actually just use direction target gen

                    List<Coord> validPinBlockCoords = new List<Coord>();
                    directionalTargetGeneration(-kingDir.x, -kingDir.y, pieceOnSquare.pos, ref validPinBlockCoords);
                    //now remove unvalid targets
                    removeAllCoordsInFirstThatDontExistInSecond(ref targets, ref validPinBlockCoords);
                }
            }
        }


        void removeAllCoordsInFirstThatDontExistInSecond(ref List<Coord> removables, ref List<Coord> constants) 
        {
            for (int i = 0; i < removables.Count; i++)
            {
                //target is not among acceptable targets, so remove it
                if (!coordContainedWithinCoords(ref constants, removables[i]))
                {
                    removables.RemoveAt(i);
                    --i;
                }

            }
        }

        Square getFirstPieceSquareAlongDir(Coord dir, Coord pos)
        {
            //8 is max length of a move
            for (int i = 0; i < 8; i++)
            {
               pos.Add(dir.x, dir.y);
                //if the move failed to add or hit a piece, stop adding in that direction
                if (!pos.Exists())
                    break;
                Square checkSquare = currentBoard.getSquare(pos);
                if (!checkSquare.empty())
                    return checkSquare;        
            }
            return new Square(pos.x, pos.y);
        }

     


        //square influence doesn't require legality checks because even if a piece is pinned, a king cannot move there, so the square is under threat
        List <Coord> generatePiecesTargets(Square pieceOnSquare)
        {
            //moves depend on the pieces type
            List<Coord> targets = new List<Coord>();
            switch (pieceOnSquare.piece.type)
            {
                case Type.PAWN:
                    {
                        generatePawnTargets(pieceOnSquare, ref targets);
                        break;
                    }
                case Type.ROOK:
                    {
                        generateRookTargets(pieceOnSquare.pos, ref targets);
                        break;
                    }
                case Type.BISHOP:
                    {
                        generateBishopTargets(pieceOnSquare.pos, ref targets);
                        break;
                    }
                case Type.QUEEN:
                    {
                        generateQueenTargets(pieceOnSquare.pos, ref targets);
                        break;
                    }
                case Type.KING:
                    {
                        generateKingTargets(pieceOnSquare.pos, ref targets);
                        break;
                    }
                case Type.KNIGHT:
                    {
                        generateKnightTargets(pieceOnSquare.pos, ref targets);
                        break;
                    }
                default:
                    {

                        break;
                    }
            }
            return targets;
        }




        void generateRookTargets(Coord pos, ref List<Coord> targets)
        {
          
            directionalTargetGeneration(0, 1, pos, ref targets);
            directionalTargetGeneration(0, -1, pos, ref targets);
            directionalTargetGeneration(1, 0, pos, ref targets);
            directionalTargetGeneration(-1, 0, pos, ref targets);
        }

        void generateQueenTargets(Coord pos, ref List<Coord> targets)
        {
            directionalTargetGeneration(1, 1, pos, ref targets);
            directionalTargetGeneration(1, -1, pos, ref targets);
            directionalTargetGeneration(-1, 1, pos, ref targets);
            directionalTargetGeneration(-1, -1, pos, ref targets);
            directionalTargetGeneration(0, 1, pos, ref targets);
            directionalTargetGeneration(0, -1, pos, ref targets);
            directionalTargetGeneration(1, 0, pos, ref targets);
            directionalTargetGeneration(-1, 0, pos, ref targets);
        }

        void generateBishopTargets(Coord pos, ref List<Coord> targets)
        {
            directionalTargetGeneration(1, 1, pos, ref targets);
            directionalTargetGeneration(1, -1, pos, ref targets);
            directionalTargetGeneration(-1, 1, pos, ref targets);
            directionalTargetGeneration(-1, -1, pos, ref targets);
        }

        void generateKingTargets(Coord pos, ref List<Coord> targets)
        {
            singularTargetGeneration(1, 1, pos, ref targets);
            singularTargetGeneration(1, -1, pos, ref targets);
            singularTargetGeneration(-1, 1, pos, ref targets);
            singularTargetGeneration(-1, -1, pos, ref targets);
            singularTargetGeneration(0, 1, pos, ref targets);
            singularTargetGeneration(0, -1, pos, ref targets);
            singularTargetGeneration(1, 0, pos, ref targets);
            singularTargetGeneration(-1, 0, pos, ref targets);
        }

        void generateKnightTargets(Coord pos, ref List<Coord> targets)
        {
            singularTargetGeneration(1, 2, pos, ref targets);
            singularTargetGeneration(1, -2, pos, ref targets);
            singularTargetGeneration(-1, 2, pos, ref targets);
            singularTargetGeneration(-1, -2, pos, ref targets);
            singularTargetGeneration(2, 1, pos, ref targets);
            singularTargetGeneration(2, -1, pos, ref targets);
            singularTargetGeneration(-2, 1, pos, ref targets);
            singularTargetGeneration(-2, -1, pos, ref targets);
        }

        //note: targets are conventionally controlled squares so for the pawn, these are diagonal squares, excluding en pessant because its reactionary
        void generatePawnTargets(Square pieceSquare, ref List<Coord> targets)
        {
            //always from the perspective of white on bottom, so a white pawn is positive and blavk pawn is negative diection
            if(pieceSquare.piece.team == Team.WHITE)
            {
                singularTargetGeneration(1, 1, pieceSquare.pos, ref targets);
                singularTargetGeneration(-1, 1, pieceSquare.pos, ref targets);
            }
            else
            {
                singularTargetGeneration(1, -1, pieceSquare.pos, ref targets);
                singularTargetGeneration(-1, -1, pieceSquare.pos, ref targets);
            }
            
        }



        //we set a current board that describes the current game

        //then we loop throup all pieces who's team matches whos turn and generate the legal moves for every piece, then we return that list of Boards

        public List<Board> generateMoves(Board currentBoard)
        {
            this.currentBoard = currentBoard;
            List <Board> generatedBoards = new List<Board> ();
            for(int i= 0; i < currentBoard.readFriendlyPieces().Count; i++) 
            {
                List <Board> thisPiecesBoards = generatePiecesMoves(currentBoard.readFriendlyPieces()[i]);
                for(int j = 0; j <  thisPiecesBoards.Count; j++) 
                {
                    generatedBoards.Add(thisPiecesBoards[i]);
                }
            }

            this.currentBoard = null;
            return generatedBoards; 
        }



        void singularTargetGeneration(int x, int y, Coord pos, ref List<Coord> targets)
        {
            //8 is max length of a move

            pos.Add(x, y);
            //if the move failed to add or hit a piece, stop adding in that direction
            determineValidTarget(ref targets, pos);
        }



       void directionalTargetGeneration(int x, int y, Coord pos, ref List<Coord> targets)
        {      
            //8 is max length of a move
            for (int i = 0; i < 8; i++)
            {
                pos.Add(x, y);
                //if the move failed to add or hit a piece, stop adding in that direction
                if (!determineValidTarget(ref targets, pos))
                    break;
            }
        }

        //determines whether a given coordinate is on the board and returns false if the target is a piece
        //returns true if empty and false if off board target
        bool determineValidTarget(ref List <Coord> targets,  Coord target)
        {
            if (!target.Exists())
                return false;
            targets.Add(target);
            if (!currentBoard.getSquare(target).empty())
                return false;
            else return true;
        }
       

       bool coordUnderthreat(Coord coord)
        {
            return coordContainedWithinCoords(ref currentBoard.getEnemyInfluence(), coord);
        }


        bool coordContainedWithinCoords(ref List<Coord> coords, Coord coord)
        {
            for(int i = 0; i < coords.Count; i++) 
            {
                if (coords[i] == coord)
                    return true;
            }
            return false;
        }
    }




   

    
}