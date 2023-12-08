using System;
using System.Collections.Generic;
using System.Linq;

namespace Mini_Othello
{
	public class GameParameters
	{
		public static int ActionMinIndex = 1;
		public static int ActionMaxIndex = 16;
		public static int BoardRowCount = 4;
		public static int BoardColCount = 4;
		public static int BoardMaxIndex = 15;
	}

	public class GameState
	{
		public int[,] BoardState;
		public int NextTurn;
		public int BoardStateKey;
		public int GameWinner;
		public int NumOfBlack;
		public int NumOfWhite;

		public GameState()
		{
			// 초기 게임 상태로 클래스를 초기화
			// 보드의 중앙에 흑돌과 백돌이 초기상태로 2개씩 놓여있고 흑돌 차례인 상태

			BoardState = new int[,] { { 0, 0, 0, 0 }, { 0, 1, 2, 0 }, { 0, 2, 1, 0 }, { 0, 0, 0, 0} };
			NextTurn = 1;

			var boardStateKey = 0;
			for (var i = 0; i <= GameParameters.BoardMaxIndex; i++)
			{
				boardStateKey = boardStateKey * 3;
				boardStateKey = boardStateKey + BoardState[GetRowFromIndex(i), GetColFromIndex(i)];
			}

			BoardStateKey = boardStateKey* 3 + NextTurn;
			GameWinner = 0;
			NumOfBlack = 2;
			NumOfWhite = 2;
		}

		public GameState(int boardStateKey)
		{
			// 주어진 게임 상태 키로부터 게임 상태를 생성하면서 클래스를 초기화

			BoardState = new int[4, 4];
			BoardStateKey = boardStateKey;
			NextTurn = boardStateKey % 3;
			GameWinner = 0;
			PopulateBoard(boardStateKey / 3);
		}

		public void PopulateBoard(int boardState)
		{
			// 주어진 보드 상태 값을 3진수로 변환시키면서 보드 상태 생성

			var boardValueProcessing = boardState;
			NumOfBlack = 0;
			NumOfWhite = 0;

			for (var i = GameParameters.BoardMaxIndex; i >= 0; i--)
			{
				var boardValue = boardValueProcessing % 3;
				boardValueProcessing = boardValueProcessing / 3;

				BoardState[GetRowFromIndex(i), GetColFromIndex(i)] = boardValue;

				if (boardValue == 1)
					NumOfBlack++;
				if (boardValue == 2)
					NumOfWhite++;
			}
		}

		public bool isFinalState()
		{
			// 게임이 끝난 상태인지 확인
			GameWinner = 0;
			NumOfWhite = 0;
			NumOfBlack = 0;

			for(var i = 0; i < GameParameters.BoardRowCount; i++)
			{
				for (var j = 0; j < GameParameters.BoardColCount; j++)
				{
					switch(BoardState[i, j])
					{
						case 1:
							NumOfBlack++;
							break;
						case 2:
							NumOfWhite++;
							break;
						default:
							break;
					}
				}
			}

			// 흑돌, 백돌 모두 더 이상 수가 없는 당태이면 종료 상태로 판단
			if(CountValidMoves(1) == 0 && CountValidMoves(2) == 0)
			{
				if (NumOfBlack > NumOfWhite) // 흑돌이 더 많으면 흑돌이 승자
				{
					GameWinner = 1;
				}
				else if (NumOfBlack < NumOfWhite) // 백돌이 더 많으면 백돌이 승자
				{
					GameWinner = 2;
				}
				return true;
			}

			return false;
		}

		public float GetReward()
		{
			// 보상값 함수
			// 게임을 1이 이긴 상태이면 100, 2가 이긴 상태이면 -100, 그렇지 않으면 0 반환

			if (isFinalState())
			{
				if (GameWinner == 1)
					return 100.0f;
				else if (GameWinner == 2)
					return -100.0f;
			}

			return 0.0f;
		}

		public int CountValidMoves()
		{
			return CountValidMoves(NextTurn);
		}

		private int CountValidMoves(int turn)
		{
			// 현재 게임 상태에 대해 올바른 행동의 개수를 찾아보는 함수

			var count = 0;
			for (var i = GameParameters.ActionMinIndex; i <= GameParameters.ActionMaxIndex; i++)
			{
				// 모든 위치에 대해 올바른 행동인지 판단
				if (IsValidMove(i, turn))
					count++;
			}
			return count;
		}

		public bool IsValidMove(int move)
		{
			return IsValidMove(move, NextTurn);
		}

		public bool IsValidMove(int move, int turn)
		{
			// 현재 주어진 게임 상태에 대해 주어진 행동을 적용할 수 있는지 판단하는 함수
			var index = move - 1;
			var row = GetRowFromIndex(index);
			var col = GetColFromIndex(index);

			// 보드상에 주어진 위치가 비어 있는 곳이어야 하며
			if (BoardState[row, col] == 0)
			{
				for(var incRow = -1; incRow <= 1; incRow++)
				{
					for (var incCol = -1; incCol <= 1; incCol++)
					{
						if (incRow != 0 || incCol != 0)
						{
							// 그 위치로부터 가로, 세로, 대각선 8개 방향으로 상대방의 돌을 넘실 수 있는 수가 있어야 한다
							if (IsMoveValidInDirection(row, col, incRow, incCol, turn))
								return true;
						}
					}
				}
				
			}

			return false;
		}

		public bool IsMoveValidInDirection(int targetRow, int targetCol, int incRow, int incCol, int turn)
		{
			int testingRow;
			int testingCol;
			int enemyStoneCount = 0;
			int increment = 1;

			// 주어진 위치 (targetRow, TargetCol)에서 주어진 방향 (incRow, incCol) 으로 진행하면서 상대방 돌을 넘길수 있는지 판단
			while(true)
			{
				testingRow = targetRow + incRow * increment;
				testingCol = targetCol + incCol * increment;

				if (testingRow < 0 || testingRow >= GameParameters.BoardRowCount) // 인덱스가 보드 밖으로 나가면 안됨
					return false;
				if (testingCol < 0 || testingCol >= GameParameters.BoardColCount) // 인덱스가 보드 밖으로 나가면 안됨
					return false;

				int testingPlaceStone = BoardState[testingRow, testingCol];

				if (testingPlaceStone == 0) // 진행 중에 빈칸이 있어서도 안됨
					return false;
				if (testingPlaceStone != turn) // 상대방 돌을 만나면 그 개수를 기록함
					enemyStoneCount++;
				if (testingPlaceStone == turn) // 인덱스가 보드 밖으로 나가거나 빈칸을 만나기 전에, 상대방 돌을 1개 이상 연속으로 만난 후 같은편 돌을 만나야 함
				{
					if (enemyStoneCount == 0)
						return false;
					else
						return true;
				}
				increment++;
			}
		}

		public void MakeMoveInDirection(int targetRow, int targetCol, int incRow, int incCol, int turn)
		{
			int testingRow;
			int testingCol;
			int increment = 1;

			// 주어진 위치 (targetRow, TargetCol)에서 주어진 방향 (incRow, incCol) 으로 진행하면서 우리편 돌을 만날 때까지 상대방 돌을 우리돌로 바꿈
			while (true)
			{
				testingRow = targetRow + incRow * increment;
				testingCol = targetCol + incCol * increment;

				int testingPlaceStone = BoardState[testingRow, testingCol];

				if (testingPlaceStone != turn)
				{
					BoardState[testingRow, testingCol] = turn;
					if(turn == 1)
					{
						NumOfBlack++;
						NumOfWhite--;
					}
					else if (turn == 2)
					{
						NumOfBlack--;
						NumOfWhite++;
					}
				}
				if (testingPlaceStone == turn)
					return;
				increment++;
			}
		}

		public GameState GetNextState(int move)
		{
			// 현재 게임 상태에 대해 주어진 행동을 취하여 전이되어 가는 게임 상태를 생성해서 반환
			var nextState = new GameState(BoardStateKey);
			nextState.MakeMove(move);
			return nextState;
		}

		public void MakeMove(int move)
		{
			var boardStateKey = 0;

			if (move != 0) // pass 가 아닌 경우
			{
				// 현재 게임 상태에 행동을 적용하는 함수
				var index = move - 1;
				var row = GetRowFromIndex(index);
				var col = GetColFromIndex(index);

				// 주어진 위치에 다음 차례에 둘 돌을 놓은 후
				BoardState[row, col] = NextTurn;
				if (NextTurn == 1)
					NumOfBlack++;
				else if (NextTurn == 2)
					NumOfWhite++;

				// 가로, 세로, 대각선 8개 방향으로 진행하면서
				for (var incRow = -1; incRow <= 1; incRow++)
				{
					for (var incCol = -1; incCol <= 1; incCol++)
					{
						if (incRow != 0 || incCol != 0)
							if (IsMoveValidInDirection(row, col, incRow, incCol, NextTurn)) // 상대방 돌을 우리 돌로 바꿀 수 있는 경우가 존재하면
								MakeMoveInDirection(row, col, incRow, incCol, NextTurn); // 우리 돌로 바꿔줌
					}
				}

				for (int i = 0; i <= GameParameters.BoardMaxIndex; i++) // 게임 보드 상태키 재설정하기 위한 보드 상태의 3진수화
				{
					boardStateKey = boardStateKey * 3;
					boardStateKey = boardStateKey + BoardState[GetRowFromIndex(i), GetColFromIndex(i)];
				}
			}
			else // Pass 한 경우에는 턴 정보만 바꾸기 위해 보드 상태키에서 턴정보를 뺀 보드 상태 3진수값 구해두기
			{
				boardStateKey = BoardStateKey / 3;
			}

			// 턴 변경
			if (NextTurn == 1)
				NextTurn = 2;
			else if (NextTurn == 2)
				NextTurn = 1;

			// 턴정보 추가하여 보드 상태키 업데이트
			BoardStateKey = boardStateKey * 3 + NextTurn;
		}

		public void DisplayBoard(int turnCount, int lastMove, GamePlayer blackPlayer, GamePlayer whitePlayer)
		{
			// 화면에 현재 게임 상태를 출력하는 함수. 게임 진행 과정에서 사용됨
			Console.Clear();

			Console.WriteLine($"X: {blackPlayer}, O: {whitePlayer}");
			Console.WriteLine();
			Console.WriteLine($"게임 턴: {turnCount}, {GetTurnMark()} 차례입니다.");
			Console.Write($"Black:{NumOfBlack}, White:{NumOfWhite}, ");

			if (turnCount != 0)
			{
				if (lastMove != 0)
					Console.WriteLine($" 지난 행동, Row: {GetRowFromIndex(lastMove - 1)}, Column: {GetColFromIndex(lastMove - 1)}");
				else
					Console.WriteLine($" 지난 행동, Pass");
			}
			else
			{
				Console.WriteLine();
			}
			Console.WriteLine();

			Console.WriteLine("    +-------+-------+-------+-------+");
			Console.WriteLine("    |       |       |       |       |");
			Console.WriteLine($"    |   {GetGameBoardValue(0, 0)}   |   {GetGameBoardValue(0, 1)}   |   {GetGameBoardValue(0, 2)}   |   {GetGameBoardValue(0, 3)}   |");
			Console.WriteLine("    |       |       |       |       |");
			Console.WriteLine("    +-------+-------+-------+-------+");
			Console.WriteLine("    |       |       |       |       |");
			Console.WriteLine($"    |   {GetGameBoardValue(1, 0)}   |   {GetGameBoardValue(1, 1)}   |   {GetGameBoardValue(1, 2)}   |   {GetGameBoardValue(1, 3)}   |");
			Console.WriteLine("    |       |       |       |       |");
			Console.WriteLine("    +-------+-------+-------+-------+");
			Console.WriteLine("    |       |       |       |       |");
			Console.WriteLine($"    |   {GetGameBoardValue(2, 0)}   |   {GetGameBoardValue(2, 1)}   |   {GetGameBoardValue(2, 2)}   |   {GetGameBoardValue(2, 3)}   |");
			Console.WriteLine("    |       |       |       |       |");
			Console.WriteLine("    +-------+-------+-------+-------+");
			Console.WriteLine("    |       |       |       |       |");
			Console.WriteLine($"    |   {GetGameBoardValue(3, 0)}   |   {GetGameBoardValue(3, 1)}   |   {GetGameBoardValue(3, 2)}   |   {GetGameBoardValue(3, 3)}   |");
			Console.WriteLine("    |       |       |       |       |");
			Console.WriteLine("    +-------+-------+-------+-------+");


			isFinalState();
			Console.WriteLine(Environment.NewLine);
			if(isFinalState())
			{
				switch (GameWinner)
				{
					case 1:
						Console.WriteLine("X 가 이겼습니다!!");
						break;
					case 2:
						Console.WriteLine("O 가 이겼습니다!!");
						break;
					default:
						Console.WriteLine("게임이 비겼습니다!!");
						break;
				}
			}
			else
			{
				Console.WriteLine("게임 진행중입니다!!");
			}
		}

		private string GetTurnMark()
		{
			return NextTurn == 1 ? "X" : "O";
		}

		private string GetGameBoardValue(int row, int col)
		{
			switch (BoardState[row, col])
			{
				case 1:
					return "X";
				case 2:
					return "O";
				default:
					return "+";
			}
		}

		private int GetRowFromIndex(int index)
		{
			return index / GameParameters.BoardRowCount;
		}

		private int GetColFromIndex(int index)
		{
			return index % GameParameters.BoardColCount;
		}
	}
}
