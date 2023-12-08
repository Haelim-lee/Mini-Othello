using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mini_Othello
{
	class MainProgram
	{
		public static DynamicProgrammingManager ValueFunctionManager;
		public static QLearningManager QLearningValueFunctionManager;
		public static GameManager OthelloGameManager;

		static void Main(string[] args)
		{
			ValueFunctionManager = new DynamicProgrammingManager();
			QLearningValueFunctionManager = new QLearningManager();
			OthelloGameManager = new GameManager();

			var showMenu = true;

			while (showMenu)
			{
				showMenu = MainMenu();
			}
		}

		private static bool MainMenu()
		{
			Console.Clear();
			Console.WriteLine("원하는 동작을 선택하세요:");
			Console.WriteLine(Environment.NewLine);
			Console.WriteLine("1) 동적프로그래밍 진행");
			Console.WriteLine("2) 동적 프로그래밍 가치 함수 저장");
			Console.WriteLine("3) 동적 프로그래밍 가치 함수 읽어오기");
			Console.WriteLine("4) Q-러닝 진행");
			Console.WriteLine("5) Q-러닝 가치 함수 저장");
			Console.WriteLine("6) Q-러닝 가치 함수 읽어오기");
			Console.WriteLine("7) 게임 하기");
			Console.WriteLine("8) 프로그램 종료");
			Console.WriteLine(Environment.NewLine);
			Console.Write("동작 선택:");

			switch (Console.ReadLine())
			{
				case "1":
					ValueFunctionManager.UpdateByDynamicProgramming();
					return true;
				case "2":
					ValueFunctionManager.SaveStateValueFunction();
					return true;
				case "3":
					ValueFunctionManager.LoadStateValueFunction();
					return true;
				case "4":
					QLearningValueFunctionManager.UpdateByQLearning();
					return true;
				case "5":
					QLearningValueFunctionManager.SaveStateValueFunction();
					return true;
				case "6":
					QLearningValueFunctionManager.LoadStateValueFunction();
					return true;
				case "7":
					OthelloGameManager.PlayGame();
					return true;
				case "8":
					return false;
				default:
					return true;
			}
		}
	}
}