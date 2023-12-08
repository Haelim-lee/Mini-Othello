using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;

namespace Mini_Othello
{
	public class DynamicProgrammingManager
	{
		public Dictionary<int, float> StateValueFunction { get; set; } // 상태 가치 함수값들을 저장하기 위한 Dictionary
		public float DiscountFactor = 0.9f;
		public string stateValueFunctionFilePath = "StateValueFunction.json";

		public DynamicProgrammingManager()
		{
			StateValueFunction = new Dictionary<int, float>();
		}

		public void UpdateByDynamicProgramming()
		{
			InitializeValueFunction();

			ApplyDynamicProgramming();
		}

		public void InitializeValueFunction()
		{
			Console.Clear();
			Console.WriteLine("동적 프로그래밍 시작");
			Console.WriteLine("가치 함수 초기화");

			StateValueFunction.Clear();
			
			var gameState = new GameState(); // 초기 게임 상태 생성
			var boardStateKeyList = new List<int>(); // 게임 상태 후보 리스트 선언
			boardStateKeyList.Add(gameState.BoardStateKey); // 초기 상태 게임 상태키를 후보 리스트에 추가
			var mergedChildStateList = new List<int>();

			while (true) // 루프 시작
			{
				mergedChildStateList.Clear(); // 처리가 끝난 자식 상태 리스트 비우기
				foreach (int gameBoardKey in boardStateKeyList) // 게임 상태 후보 리스트에 있는 상태들에 대해
				{
					if (!StateValueFunction.ContainsKey(gameBoardKey)) // 가치 함수에 아직 포함되지 않았으면
					{
						StateValueFunction.Add(gameBoardKey, 0.0f); // 가치 함수에 추가
						var processingGameState = new GameState(gameBoardKey); // 게임 상태 생성

						if (!processingGameState.isFinalState()) // 게임 종료 상태가 아니면
						{
							var childStateList = new List<int>();
							for (var i = GameParameters.ActionMinIndex; i <= GameParameters.ActionMaxIndex; i++)  // 모든 행동에 대해 루프를 수행
							{
								if (processingGameState.IsValidMove(i)) // 이 행동이 올바른 행동이면
								{
									var nextState = processingGameState.GetNextState(i); // 행동을 통해 전이해 간 다음 상태 구성
									childStateList.Add(nextState.BoardStateKey); // 자식 상태 임시 후보 리스트에 추가
								}
							}
							if (childStateList.Count == 0) // 만일 후보 리스트에 포함된 상태가 없으면
							{
								var nextState = processingGameState.GetNextState(0); // Pass 로 턴만 바뀐 상태 생성
								childStateList.Add(nextState.BoardStateKey); // 임시 후보 리스트에 추가
							}

							// 임시 후보 리스트의 상태 중 자식 상태 리스트에 포함되어 있지 않은 상태를 자식 상태 리스트에 추가
							mergedChildStateList.AddRange(childStateList.Where(e => !mergedChildStateList.Contains(e)));
						}
					}
				}
				if (mergedChildStateList.Count == 0) // 자식 상태 리스트에 상태가 없으면
					break; // 루프 종료
				else
					boardStateKeyList = new List<int>(mergedChildStateList); // 게임 상태 후보 리스트를 자식 상태로 치환하고 루프 지속
			}

			Console.WriteLine(Environment.NewLine);
			Console.WriteLine($"가치 함수 초기화 완료, 상태 {StateValueFunction.Count} 개");
			Console.WriteLine(Environment.NewLine);
			Console.Write("아무 키나 누르세요:");
			Console.ReadLine();
		}

		public void ApplyDynamicProgramming()
		{
			Console.Clear();
			Console.WriteLine("동적 프로그래밍 적용");
			Console.WriteLine(Environment.NewLine);

			var loopCount = 0;
			var terminateLoop = false;

			while (!terminateLoop)
			{
				var nextStateValueFunction = new Dictionary<int, float>(); // 업데이트되는 가치 함수값을 임시로 저장하기 위한 dictionary
				var valueFunctionUpdateAmount = 0.0f; // 동적 프로그래밍 각 단계에서 함수값이 업데이트된 크기

				foreach (KeyValuePair<int, float> valueFunctionEntry in StateValueFunction) // 가치 함수 업데이트 루프
				{
					var updatedValue = UpdateValueFunction(valueFunctionEntry.Key); // 가치 함수 업데이트 계산
					var updatedAmount = Math.Abs(valueFunctionEntry.Value - updatedValue); // 업데이트 크기
					nextStateValueFunction[valueFunctionEntry.Key] = updatedValue; // 가치 함수 업데이트

					if (updatedAmount > valueFunctionUpdateAmount) // 루프를 돌면서 함수가 업데이트된 크기를 기록
						valueFunctionUpdateAmount = updatedAmount;
				}

				StateValueFunction = new Dictionary<int, float>(nextStateValueFunction); // 가치 함수값을 임시 저장 가치 함수로 변경

				loopCount++;
				Console.WriteLine($"동적 프로그래밍 {loopCount}회 수행, 업데이트 오차 {valueFunctionUpdateAmount}");

				if (valueFunctionUpdateAmount < 0.01f) // 업데이트 크기가 충분히 작으면 루프 종료
					terminateLoop = true;
			}

			Console.WriteLine(Environment.NewLine);
			Console.Write("아무 키나 누르세요:");
			Console.ReadLine();
		}

		public float UpdateValueFunction(int gameStateKey)
		{
			var gameState = new GameState(gameStateKey); // 주어진 게임 상태 키에 대해 게임 상태 생성

			if (gameState.isFinalState()) // 게임 종료 상태이면 함수값 0 반환
				return 0.0f;

			var actionExpectationList = new List<float>();

			for (int i = GameParameters.ActionMinIndex; i <= GameParameters.ActionMaxIndex; i++)
			{
				if (gameState.IsValidMove(i)) // 이 행동이 올바른 행동이면
				{
					var nextState = gameState.GetNextState(i); // 행동을 통해 전이해 간 다음 상태 구성
					var reward = nextState.GetReward(); // 다음 상태에서의 보상값 확인

					var actionExpectation = reward + DiscountFactor * StateValueFunction[nextState.BoardStateKey]; // 행동 가치 함수값 계산

					actionExpectationList.Add(actionExpectation); // 계산된 가치 함수값을 저장
				}
			}
			if (actionExpectationList.Count == 0) // 만일 올바른 행동이 하나도 없는 상태라면
			{
				var nextState = gameState.GetNextState(0); // Pass를 통해 턴만 바뀐 상태에 대한 가치함수를 가져올 수 있도록 변경
				var reward = nextState.GetReward(); // 다음 상태에서의 보상값 확인

				var actionExpectation = reward + DiscountFactor * StateValueFunction[nextState.BoardStateKey]; // 행동 가치 함수값 계산

				actionExpectationList.Add(actionExpectation); // 계산된 가치 함수값을 저장
			}

			if (gameState.NextTurn == 1) // 흑돌이 둘 차례이면 저장된 가치 함수값 중 최대값 반환
				return actionExpectationList.Max();
			else if (gameState.NextTurn == 2) // 백돌이 둘 차례이면 저장된 가치 함수값 중 최소값 반환
				return actionExpectationList.Min();
			return 0.0f;
		}

		public int GetNextMove(int boardStateKey)
		{
			// 주어진 게임 상태에 대해서 선택할 수 있는 행동 후보값을 구한 후,
			var actionCandidates = GetNextMoveCandidate(boardStateKey);

			if (actionCandidates.Count() == 0) // pass
				return 0;

			// 그 중 한 값을 랜덤하게 선택해서 반환
			return actionCandidates.ElementAt(Utilities.random.Next(0, actionCandidates.Count()));
		}

		public IEnumerable<int> GetNextMoveCandidate(int boardStateKey)
		{
			var selectedExpectation = 0.0f;

			var gameState = new GameState(boardStateKey); // 주어진 상태에 대한 게임 상태 생성
			var actionCandidateDictionary = new Dictionary<int, float>();

			for (var i = GameParameters.ActionMinIndex; i <= GameParameters.ActionMaxIndex; i++)
			{
				if (gameState.IsValidMove(i)) // 이 행동에 이 상태에 적용 가능한 올바른 행동인 경우
				{
					var nextState = gameState.GetNextState(i); //그 행동을 통해 전이해가는 상태를 구하고
					var reward = nextState.GetReward(); // 그 전이해 간 상태에서의 보상값

					var actionExpectation = reward + DiscountFactor * StateValueFunction[nextState.BoardStateKey]; // 행동 가치 함수값 계산

					actionCandidateDictionary.Add(i, actionExpectation); // 행동과 그 행동에 대한 행동 가치 함수값을 저장
				}
			}

			if (actionCandidateDictionary.Count == 0)
				return new List<int>();

			if (gameState.NextTurn == 1) // 흑돌 차례인 경우 저장된 행동 가치 함수값 중 최대값을 선택
			{
				selectedExpectation = actionCandidateDictionary.Select(e => e.Value).Max();
			}
			else if (gameState.NextTurn == 2) // 백돌 차례인 경우 저장된 행동 가치 함수값 중 최소값 선택
			{
				selectedExpectation = actionCandidateDictionary.Select(e => e.Value).Min();
			}

			// 선택한 가치 함수값을 가지는 행동들을 모두 모아서 반환
			return actionCandidateDictionary.Where(e => e.Value == selectedExpectation).Select(e => e.Key);
		}

		public void SaveStateValueFunction()
		{
			// 가치 함수 저장
			// Json Serilizer 적용 후 text 형태로 저장
			var settings = new JsonSerializerSettings();
			settings.Formatting = Formatting.Indented;

			var stateValueFunctionInJson = JsonConvert.SerializeObject(StateValueFunction, settings);

			File.WriteAllText(stateValueFunctionFilePath, stateValueFunctionInJson);

			Console.Clear();
			Console.WriteLine($"가치 함수가 파일 {stateValueFunctionFilePath}에 저장되었습니다.");
			Console.WriteLine(Environment.NewLine);
			Console.Write("아무 키나 누르세요:");
			Console.ReadLine();
		}

		public void LoadStateValueFunction()
		{
			// 가치 함수 로드
			// 텍스트 형태로 읽어온 후 Json Deserialize 적용
			if (File.Exists(stateValueFunctionFilePath))
			{
				var stateValueFunctionInJson = File.ReadAllText(stateValueFunctionFilePath);

				var settings = new JsonSerializerSettings();
				settings.Formatting = Formatting.Indented;

				StateValueFunction = JsonConvert.DeserializeObject<Dictionary<int, float>>(stateValueFunctionInJson, settings);

				Console.Clear();
				Console.WriteLine($"가치 함수가 파일 {stateValueFunctionFilePath}에서 로드되었습니다.");
			}
			else
			{
				Console.Clear();
				Console.WriteLine($"가치 함수 파일 {stateValueFunctionFilePath}이 존재하지 않습니다.");
			}
			Console.WriteLine(Environment.NewLine);
			Console.Write("아무 키나 누르세요:");
			Console.ReadLine();
		}
	}
}
