using System;
using System.IO;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace Mini_Othello
{
	public class QLearningManager
	{
		public Dictionary<int, Dictionary<int, float>> ActionValueFunction { get; set; }
		public Dictionary<int, float> FunctionAccuracyForEpisodeCount { get; set; }
		public float DiscountFactor = 0.9f;
		public float UpdateStep = 0.01f;
		public string ActionValueFunctionFilePath = "QLearningActionValueFunction.json";
		public string FunctionAccuracyFilePath = "QLearningActionValueFunctionAccuracy.csv";

		public QLearningManager()
		{
			// 상태 가치 함수
			ActionValueFunction = new Dictionary<int, Dictionary<int, float>>();
			// 동적 프로그래밍 결과와 비교한 구해진 가치 함수의 정확도를 저장하는 dictionary
			FunctionAccuracyForEpisodeCount = new Dictionary<int, float>();
		}

		public void UpdateByQLearning()
		{
			InitializeValueFunction();

			ApplyQLearning();
		}

		public void InitializeValueFunction()
		{
			Console.Clear();
			Console.WriteLine("Q  러닝 시작");
			Console.WriteLine("가치 함수 초기화");

			ActionValueFunction.Clear();
			ActionValueFunction = Utilities.CreateActionValueFunction();

			Console.WriteLine(Environment.NewLine);
			Console.WriteLine($"가치 함수 초기화 완료, 상태 {ActionValueFunction.Count} 개");

			Console.WriteLine(Environment.NewLine);
			Console.Write("아무 키나 누르세요:");
			Console.ReadLine();
		}

		public void ApplyQLearning()
		{
			Console.Clear();
			Console.WriteLine("가치 함수 업데이트 시작");
			Console.WriteLine(Environment.NewLine);

			var episodeCount = 0;
			var keepUpdating = true;
			var isDPFunctionAvailable = MainProgram.ValueFunctionManager.StateValueFunction.Count > 0;
			FunctionAccuracyForEpisodeCount.Clear();

			while (keepUpdating)
			{
				var firstState = new GameState(); // 초기 게임 상태 생성
				var episodeFinished = false; // 게임 종료 여부
				while (!episodeFinished)
				{
					// Epsilon 탐욕 정책으로 첫번째 행동 선택
					var firstAction = Utilities.GetEpsilonGreedyAction(firstState.NextTurn, ActionValueFunction[firstState.BoardStateKey]);
					// 선택된 행동을 통해 전이해 간 두번째 상태 생성
					var secondState = firstState.GetNextState(firstAction);
					// 두번째 상태에 대한 보상 계산
					var reward = secondState.GetReward();

					// 첫번째 상태, 행동에 대한 가치 함수값
					var firstStateActionValue = ActionValueFunction[firstState.BoardStateKey][firstAction];
					// 두번째 상태에 대한 가치 함수값
					var secondStateActionValue = Utilities.GetGreedyActionValue(secondState.NextTurn, ActionValueFunction[secondState.BoardStateKey]);

					// 가치 함수 업데이트
					var updatedActionValue = firstStateActionValue + UpdateStep * (reward + DiscountFactor * secondStateActionValue - firstStateActionValue);
					ActionValueFunction[firstState.BoardStateKey][firstAction] = updatedActionValue;

					if (secondState.isFinalState() || ActionValueFunction[secondState.BoardStateKey].Count == 0) // 에피소드가 종료된 경우
					{
						episodeFinished = true;
						episodeCount++;
					}
					else // 에피소드가 계속 진행되는 경우. 두번째 상태를 첫번째 상태로 재설정
					{
						firstState = secondState;
					}
				}

				if (episodeCount % 1000 == 0) // 에피소드 1000개 수행할 때 마다 상태 출력
				{
					var valueFunctionAccuracy = 0.0f;
					var functionAccuracy = "";

					if (isDPFunctionAvailable)
					{
						valueFunctionAccuracy = Utilities.EvaluateValueFunction();
						FunctionAccuracyForEpisodeCount.Add(episodeCount, valueFunctionAccuracy);
						functionAccuracy = $"함수 정확도 {valueFunctionAccuracy}%.";
					}
					Console.WriteLine($"에피소드를 {episodeCount}개 처리했습니다. {functionAccuracy}");
				}

				if (episodeCount > 1000000) // 에피소드 100만개 처리 후 종료
				{
					if (isDPFunctionAvailable)
					{
						FunctionAccuracyForEpisodeCount.Add(episodeCount, Utilities.EvaluateValueFunction());
					}
					keepUpdating = false;
				}
			}

			Console.WriteLine(Environment.NewLine);
			Console.Write("Q-러닝을 종료합니다. 아무 키나 누르세요:");
			Console.ReadLine();
		}

		public int GetNextMove(int boardStateKey)
		{
			// 주어진 게임 상태에 대한 행동 결정
			GameState gameState = new GameState(boardStateKey);
			return Utilities.GetGreedyAction(gameState.NextTurn, ActionValueFunction[boardStateKey]);
		}

		public IEnumerable<int> GetNextMoveCandidate(int boardStateKey)
		{
			// 주어진 게임 상태에 대해 취할 수 있는 행동들을 모두 선택
			GameState gameState = new GameState(boardStateKey);
			return Utilities.GetGreedyActionCandidate(gameState.NextTurn, ActionValueFunction[boardStateKey]);
		}

		public void SaveStateValueFunction()
		{
			// 가치 함수 저장
			var settings = new JsonSerializerSettings();
			settings.Formatting = Formatting.Indented;
			var actionValueFunctionInJson = JsonConvert.SerializeObject(ActionValueFunction, settings);
			File.WriteAllText(ActionValueFunctionFilePath, actionValueFunctionInJson);

			if (FunctionAccuracyForEpisodeCount.Count > 0)
			{
				var dataString = "";
				foreach (KeyValuePair<int, float> dataPair in FunctionAccuracyForEpisodeCount)
				{
					dataString = dataString + Environment.NewLine + $"{dataPair.Key},{dataPair.Value}";
				}
				File.WriteAllText(FunctionAccuracyFilePath, dataString);
			}

			Console.Clear();
			Console.WriteLine($"가치 함수가 파일 {ActionValueFunctionFilePath}에 저장되었습니다.");
			Console.WriteLine(Environment.NewLine);
			Console.Write("아무 키나 누르세요:");
			Console.ReadLine();
		}

		public void LoadStateValueFunction()
		{
			// 가치 함수 로드
			if (File.Exists(ActionValueFunctionFilePath))
			{
				var actionValueFunctionInJson = File.ReadAllText(ActionValueFunctionFilePath);
				var settings = new JsonSerializerSettings();
				settings.Formatting = Formatting.Indented;
				ActionValueFunction = JsonConvert.DeserializeObject<Dictionary<int, Dictionary<int, float>>>(actionValueFunctionInJson, settings);

				Console.Clear();
				Console.WriteLine($"가치 함수가 파일 {ActionValueFunctionFilePath}에서 로드되었습니다.");
			}
			else
			{
				Console.Clear();
				Console.WriteLine($"가치 함수 파일 {ActionValueFunctionFilePath}이 존재하지 않습니다.");
			}
			Console.WriteLine(Environment.NewLine);
			Console.Write("아무 키나 누르세요:");
			Console.ReadLine();
		}
	}
}
