using System;

namespace Mini_Othello
{
	public enum GamePlayer
	{
		DynamicProgramming,
		QLearning,
		Human,
		None
	}

	public class GameManager
	{
		public GamePlayer BlackPlayer;
		public GamePlayer WhitePlayer;

		public void PlayGame()
		{
			// 게임을 시작하는 함수
			while (true)
			{
				// 선공 플레이어를 정하고
				BlackPlayer = GetBlackPlayer();
				if (BlackPlayer == GamePlayer.None)
					return;

				// 후공 플레이어를 정한 후
				WhitePlayer = GetWhitePlayer();
				if (WhitePlayer == GamePlayer.None)
					return;

				// 게임 진행
				ManageGame();
			}
		}

		public void ManageGame()
		{
			// 게임 진행을 관리하는 함수

			// 초기 게임 상태를 생성한 후
			var gameState = new GameState();
			var gameTurnCount = 0;
			var gameMove = 0;
			var isGameFinished = gameState.isFinalState();

			while (!isGameFinished) // 게임이 종료될 때까지 루프 진행
			{
				// 현재 게임 상태 화면 표시
				gameState.DisplayBoard(gameTurnCount, gameMove, BlackPlayer, WhitePlayer);

				isGameFinished = gameState.isFinalState();

				Console.WriteLine(Environment.NewLine);

				if (isGameFinished)
				{
					// 게임이 끝난 경우
					Console.Write("게임이 끝났습니다. 아무 키나 누르세요:");
					Console.ReadLine();
				}
				else
				{
					// 게임이 끝나지 않은 경우

					var playerforNextTurn = GetGamePlayer(gameState.NextTurn);
					if (playerforNextTurn == GamePlayer.Human)
					{
						// 이번 차례가 인간 차례인 경우 인간의 행동을 입력을 받음
						gameMove = GetHumanGameMove(gameState);
					}
					else
					{
						// 이번 차례가 동적프로그래밍, SARSA, Q 러닝 에이전트의 차례이면 해당 가치함수 관리자로부터 현 게임 상태에 대한 행동을 선택받아옴
						Console.Write("아무 키나 누르세요:");
						Console.ReadLine();

						if (playerforNextTurn == GamePlayer.DynamicProgramming)
							gameMove = MainProgram.ValueFunctionManager.GetNextMove(gameState.BoardStateKey);
						else if (playerforNextTurn == GamePlayer.QLearning)
							gameMove = MainProgram.QLearningValueFunctionManager.GetNextMove(gameState.BoardStateKey);
					}

					// 게임 보드에 행동 적용
					gameState.MakeMove(gameMove);
					gameTurnCount++;
				}
			}
		}

		public int GetHumanGameMove(GameState gameState)
		{
			if(gameState.CountValidMoves() == 0)
			{
				Console.Write("둘 곳에 없어서 패스합니다. 아무 키나 입력하세요");
				Console.ReadLine();
				return 0;
			}
			// 인간 플레이어의 행동을 입력받는 함수. 1부터 16까지의 숫자가 입력되어야 행동을 반환함

			Console.Write("다음 행동을 입력하세요 (1-16):");
			var humanMove = Console.ReadLine();

			while (true)
			{
				try
				{
					var gameMove = Int32.Parse(humanMove);

					if (gameMove >= 1 && gameMove <= 16 && gameState.IsValidMove(gameMove))
						return gameMove;
					else
					{
						Console.Write("잘못된 행동입니다. 다음 행동을 입력하세요 (1-16):");
						humanMove = Console.ReadLine();
					}
				}
				catch
				{
					Console.Write("잘못된 행동입니다. 다음 행동을 입력하세요 (1-16):");
					humanMove = Console.ReadLine();
				}
			}
		}

		public GamePlayer GetGamePlayer(int Turn)
		{
			if (Turn == 1)
				return BlackPlayer;
			else
				return WhitePlayer;
		}

		public GamePlayer GetBlackPlayer()
		{
			// X 플레이어를 선택하는 함수
			return PlayerSelection("X 플레이어를 선택해 주세요.");
		}

		public GamePlayer GetWhitePlayer()
		{
			// O 플레이어를 선택하는 함수
			return PlayerSelection("O 플레이어를 선택해 주세요.");
		}

		public GamePlayer PlayerSelection(string menuLabel)
		{
			// 플레이어를 선택하는 함수

			while (true)
			{
				Console.Clear();
				Console.WriteLine(menuLabel);
				Console.WriteLine(Environment.NewLine);
				Console.WriteLine("1) 동적프로그래밍");
				Console.WriteLine("2) Q-러닝");
				Console.WriteLine("3) 사람");
				Console.WriteLine("4) 게임 종료");
				Console.Write("선택 (1-4):");

				switch (Console.ReadLine())
				{
					case "1":
						if (MainProgram.ValueFunctionManager.StateValueFunction.Count > 0)
						{
							return GamePlayer.DynamicProgramming;
						}
						else
						{
							Console.Write("상태 가치 함수가 정의되어 있지 않습니다. 동적 프로그래밍을 수행하거나, 가치 함수를 읽어오세요.");
							Console.WriteLine(Environment.NewLine);
							Console.Write("아무 키나 누르세요:");
							Console.ReadLine();
						}
						break;
					case "2":
						if (MainProgram.QLearningValueFunctionManager.ActionValueFunction.Count > 0)
						{
							return GamePlayer.QLearning;
						}
						else
						{
							Console.Write("상태 가치 함수가 정의되어 있지 않습니다. Q-러닝을 수행하거나, 가치 함수를 읽어오세요.");
							Console.WriteLine(Environment.NewLine);
							Console.Write("아무 키나 누르세요:");
							Console.ReadLine();
						}
						break;
					case "3":
						Console.Write("사람을 선택하셨습니다..");
						Console.WriteLine(Environment.NewLine);
						Console.Write("아무 키나 누르세요:");
						Console.ReadLine();
						return GamePlayer.Human;
					case "4":
						Console.Write("메인 메뉴로 돌아갑니다..");
						Console.WriteLine(Environment.NewLine);
						Console.Write("아무 키나 누르세요:");
						Console.ReadLine();
						return GamePlayer.None;
					default:
						break;
				}
			}
		}
	}
}
