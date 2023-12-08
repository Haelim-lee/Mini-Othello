using System;
using System.Collections.Generic;
using System.Linq;

namespace Mini_Othello
{
	public class Utilities
	{
		public static Random random = new Random();
		public static Dictionary<int, Dictionary<int, float>> CreateActionValueFunction()
		{
			// SARSA, Q 러닝에서 사용되는 행동 가치 함수를 초기화하는 함수

			Dictionary<int, Dictionary<int, float>> actionValueFunction = new Dictionary<int, Dictionary<int, float>>();

			var gameState = new GameState(); // 초기 게임 상태 생성
			var boardStateKeyList = new List<int>(); // 게임 상태 후보 리스트 선언
			boardStateKeyList.Add(gameState.BoardStateKey); // 초기 상태 게임 상태키를 후보 리스트에 추가
			var mergedChildStateList = new List<int>();

			while (true) // 루프 시작
			{
				mergedChildStateList.Clear(); // 처리가 끝난 자식 상태 리스트 비우기
				foreach (int gameBoardKey in boardStateKeyList) // 게임 상태 후보 리스트에 있는 상태들에 대해
				{
					if (!actionValueFunction.ContainsKey(gameBoardKey)) // 가치 함수에 아직 포함되지 않았으면
					{
						
						var actionValues = new Dictionary<int, float>();
						var processingGameState = new GameState(gameBoardKey); // 게임 상태 생성

						if (!processingGameState.isFinalState()) // 게임 종료 상태가 아니면
						{
							var childStateList = new List<int>();
							for (int i = GameParameters.ActionMinIndex; i <= GameParameters.ActionMaxIndex; i++)  // 모든 행동에 대해 루프를 수행
							{
								if (processingGameState.IsValidMove(i)) // 이 행동이 올바른 행동이면
								{
									var nextState = processingGameState.GetNextState(i); // 행동을 통해 전이해 간 다음 상태 구성
									childStateList.Add(nextState.BoardStateKey); // 자식 상태 임시 후보 리스트에 추가
									actionValues.Add(i, 0.0f); // 가치 함수값을 0으로 초기화 한후 dictionary에 추가
								}
							}
							if (childStateList.Count == 0) // 만일 후보 리스트에 포함된 상태가 없으면
							{
								var nextState = processingGameState.GetNextState(0); // Pass 로 턴만 바뀐 상태 생성
								childStateList.Add(nextState.BoardStateKey); // 임시 후보 리스트에 추가
								actionValues.Add(0, 0.0f); // Pass 행동
							}

							// 임시 후보 리스트의 상태 중 자식 상태 리스트에 포함되어 있지 않은 상태를 자식 상태 리스트에 추가
							mergedChildStateList.AddRange(childStateList.Where(e => !mergedChildStateList.Contains(e)));
						}
						actionValueFunction.Add(gameBoardKey, actionValues); // 가치 함수에 추가
					}
				}
				if (mergedChildStateList.Count == 0) // 자식 상태 리스트에 상태가 없으면
					break; // 루프 종료
				else
					boardStateKeyList = new List<int>(mergedChildStateList); // 게임 상태 후보 리스트를 자식 상태로 치환하고 루프 지속
			}

			return actionValueFunction;
		}

		public static int GetEpsilonGreedyAction(int turn, Dictionary<int, float> actionValues)
		{
			// Epsilon 탐욕 정책으로 행동을 선택하는 함수
			var greedyActionValue = 0.0f;
			var epsilon = 10;

			if (actionValues.Count == 0)
				return 0;

			if (turn == 1) // 흑돌 차례인 경우 가치 함수 최대값 선택
			{
				greedyActionValue = actionValues.Select(e => e.Value).Max();
			}
			else if (turn == 2) // 백돌 차례인 경우 가치 함수 최소값 선택
			{
				greedyActionValue = actionValues.Select(e => e.Value).Min();
			}

			var exploitRandom = random.Next(0, 100); // 랜덤값 발생
			IEnumerable<int> actionCandidates;

			if (exploitRandom < epsilon) // 탐험을 하는 경우
			{
				// 선택되지 않은 가치 함수값을 가지는 행동들을 선택
				actionCandidates = actionValues.Where(e => e.Value != greedyActionValue).Select(e => e.Key);
				if (actionCandidates.Count() == 0) // 만일 선택된 행동이 없으면 (가치함수값이 모두 똑같은 경우), 전체 행동 고려
					actionCandidates = actionValues.Where(e => e.Value == greedyActionValue).Select(e => e.Key);
			}
			else // 탐험하지 않는 경우
			{
				// 선택된 가치 함수값을 가지는 행동들을 선택
				actionCandidates = actionValues.Where(e => e.Value == greedyActionValue).Select(e => e.Key);
			}

			// 선택된 행동들 중 하나를 랜덤하게 선택해서 반환
			return actionCandidates.ElementAt(random.Next(0, actionCandidates.Count()));
		}

		public static int GetGreedyAction(int turn, Dictionary<int, float> actionValues)
		{
			// 주어진 가치함수 dictionary로부터 turn을 고려하여 행동을 선택. 흑돌 차례이면 가치함수값이 최대값인 행동들을, 백돌 차례이면 최소값인 행동들을 선택
			var actionCandidates = GetGreedyActionCandidate(turn, actionValues);

			if (actionCandidates.Count() == 0)
				return 0;

			// 선택된 행동 중 랜덤하게 하나를 선택하여 반환
			return actionCandidates.ElementAt(random.Next(0, actionCandidates.Count()));
		}

		public static IEnumerable<int> GetGreedyActionCandidate(int turn, Dictionary<int, float> actionValues)
		{
			var greedyActionValue = 0.0f;

			if (actionValues.Count == 0)
				return new List<int>();

			if (turn == 1) // 흑돌 차례이면 주어진 가치 함수값 중 최대값 선택
			{
				greedyActionValue = actionValues.Select(e => e.Value).Max();
			}
			else if (turn == 2) // 백돌 차례이면 주어진 가치 함수값 중 최소값 선택
			{
				greedyActionValue = actionValues.Select(e => e.Value).Min();
			}

			// 선택된 가치 함수값을 가지는 행동들을 선택해서 반환
			return actionValues.Where(e => e.Value == greedyActionValue).Select(e => e.Key);
		}

		public static float GetGreedyActionValue(int turn, Dictionary<int, float> actionValues)
		{
			// 주어진 dictionary에 element가 없으면 0 반환
			if (actionValues.Count == 0)
				return 0.0f;

			if (turn == 1) // 흑돌의 차례이면 주어진 dictionary의 가치 함수값 중 최대값을 반환
			{
				return actionValues.Select(e => e.Value).Max();
			}
			else if (turn == 2) // 백돌의 차례이면 주어진 dictionary의 가치 함수값 중 최소값을 반환
			{
				return actionValues.Select(e => e.Value).Min();
			}

			return 0.0f;
		}

		public static float EvaluateValueFunction()
		{
			// 모든 게임 상태에 대해 SARSA나 Q 러닝 에이전트가 동적 프로그래밍 에이전트와 같은 행동을 선택하는 비율을 계산하여 반환하는 함수

			if (MainProgram.ValueFunctionManager.StateValueFunction.Count == 0)
				return 0.0f;

			var totalStateCount = 0;
			var matchingStateCount = 0;

			foreach (KeyValuePair<int, Dictionary<int, float>> valueFunctionEntry in MainProgram.QLearningValueFunctionManager.ActionValueFunction)
			{
				var gameState = new GameState(valueFunctionEntry.Key);
				if (!gameState.isFinalState() && gameState.CountValidMoves() > 0)
				{
					if (CompareActionCandidate(valueFunctionEntry.Key))
					{
						matchingStateCount++;
					}
					totalStateCount++;
				}
			}
			return ((float)matchingStateCount) / ((float)totalStateCount) * 100.0f;
		}

		public static bool CompareActionCandidate(int boardStateKey)
		{
			// 주어진 상태에 대해서 SARSA나 Q 러닝 에이전트가 선택하게 되는 행동을 동적 프로그래밍 에이전트도 
			// 마찬가지로 선택하는지를 판단하는 함수

			var DPActionCandidate = MainProgram.ValueFunctionManager.GetNextMoveCandidate(boardStateKey);
			var QActionCandidate = MainProgram.QLearningValueFunctionManager.GetNextMoveCandidate(boardStateKey);

			if (QActionCandidate.Count() == 0 && DPActionCandidate.Count() > 0)
				return false;

			var UnmatchedActionList = QActionCandidate.Where(e => !DPActionCandidate.Contains(e));

			if (UnmatchedActionList.Count() == 0)
				return true;

			return false;
		}
	}
}
